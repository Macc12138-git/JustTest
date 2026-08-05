using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace JustTest.Game.Enemies
{
    public sealed class CombatEnemyPool2D : MonoBehaviour
    {
        private sealed class PoolBucket
        {
            internal PoolBucket(CombatEnemyArchetype archetype)
            {
                Archetype = archetype;
            }

            internal CombatEnemyArchetype Archetype { get; }
            internal ObjectPool<CombatEnemyRuntime2D> Pool { get; set; }
        }

        [SerializeField] private CombatEnemyArchetype[] archetypes;
        [SerializeField] private Transform poolRoot;
        [SerializeField] private bool collectionCheck = true;

        private readonly Dictionary<CombatEnemyArchetype, PoolBucket> buckets =
            new Dictionary<CombatEnemyArchetype, PoolBucket>();
        private readonly Dictionary<CombatEnemyRuntime2D, PoolBucket> leasedInstances =
            new Dictionary<CombatEnemyRuntime2D, PoolBucket>();

        private CombatEnemySceneContext sceneContext;
        private bool ready;

        internal int ActiveCount => leasedInstances.Count;

        internal bool Initialize(in CombatEnemySceneContext context)
        {
            if (ready)
            {
                return true;
            }

            if (poolRoot == null ||
                !context.IsValid ||
                archetypes == null ||
                archetypes.Length == 0)
            {
                return false;
            }

            sceneContext = context;
            for (int index = 0; index < archetypes.Length; index++)
            {
                CombatEnemyArchetype archetype = archetypes[index];
                if (archetype == null || !archetype.IsValid || buckets.ContainsKey(archetype))
                {
                    ClearBuckets();
                    return false;
                }

                PoolBucket bucket = new PoolBucket(archetype);
                bucket.Pool = new ObjectPool<CombatEnemyRuntime2D>(
                    () => CreateInstance(bucket),
                    null,
                    PrepareInstanceForPool,
                    DestroyInstance,
                    collectionCheck,
                    archetype.InitialCapacity,
                    archetype.MaximumCapacity);
                buckets.Add(archetype, bucket);
            }

            foreach (PoolBucket bucket in buckets.Values)
            {
                if (!Prewarm(bucket))
                {
                    ClearBuckets();
                    return false;
                }
            }

            ready = true;
            return true;
        }

        internal bool TryAcquire(
            CombatEnemyArchetype archetype,
            Vector3 position,
            out CombatEnemyRuntime2D instance)
        {
            instance = null;
            if (!ready ||
                archetype == null ||
                !buckets.TryGetValue(archetype, out PoolBucket bucket) ||
                (bucket.Pool.CountInactive == 0 &&
                 bucket.Pool.CountAll >= archetype.MaximumCapacity))
            {
                return false;
            }

            CombatEnemyRuntime2D candidate;
            try
            {
                candidate = bucket.Pool.Get();
            }
            catch (InvalidOperationException exception)
            {
                Debug.LogError(exception.Message, this);
                return false;
            }

            if (candidate == null ||
                leasedInstances.ContainsKey(candidate) ||
                !candidate.PrepareForSpawn(position))
            {
                if (candidate != null)
                {
                    bucket.Pool.Release(candidate);
                }

                return false;
            }

            leasedInstances.Add(candidate, bucket);
            instance = candidate;
            return true;
        }

        internal bool Release(CombatEnemyRuntime2D instance)
        {
            if (!ready ||
                instance == null ||
                !leasedInstances.TryGetValue(instance, out PoolBucket bucket))
            {
                return false;
            }

            leasedInstances.Remove(instance);
            bucket.Pool.Release(instance);
            return true;
        }

        internal void ReleaseAll()
        {
            if (!ready || leasedInstances.Count == 0)
            {
                return;
            }

            CombatEnemyRuntime2D[] instances =
                new CombatEnemyRuntime2D[leasedInstances.Count];
            leasedInstances.Keys.CopyTo(instances, 0);
            for (int index = 0; index < instances.Length; index++)
            {
                Release(instances[index]);
            }
        }

        private void OnDestroy()
        {
            ReleaseAll();
            ClearBuckets();
            ready = false;
        }

        private bool Prewarm(PoolBucket bucket)
        {
            int count = bucket.Archetype.InitialCapacity;
            if (count <= 0)
            {
                return true;
            }

            CombatEnemyRuntime2D[] instances = new CombatEnemyRuntime2D[count];
            int acquiredCount = 0;
            try
            {
                for (; acquiredCount < count; acquiredCount++)
                {
                    instances[acquiredCount] = bucket.Pool.Get();
                }
            }
            catch (InvalidOperationException exception)
            {
                Debug.LogError(exception.Message, this);
            }

            for (int index = 0; index < acquiredCount; index++)
            {
                bucket.Pool.Release(instances[index]);
            }

            return acquiredCount == count;
        }

        private CombatEnemyRuntime2D CreateInstance(PoolBucket bucket)
        {
            CombatEnemyRuntime2D instance = Instantiate(
                bucket.Archetype.Prefab,
                poolRoot);
            instance.name = bucket.Archetype.Prefab.name;
            instance.gameObject.SetActive(false);
            if (instance.BindSceneContext(sceneContext))
            {
                return instance;
            }

            Destroy(instance.gameObject);
            throw new InvalidOperationException(
                $"{nameof(CombatEnemyPool2D)} could not bind archetype '{bucket.Archetype.name}'.");
        }

        private void ClearBuckets()
        {
            foreach (PoolBucket bucket in buckets.Values)
            {
                bucket.Pool?.Dispose();
            }

            buckets.Clear();
        }

        private static void PrepareInstanceForPool(CombatEnemyRuntime2D instance)
        {
            instance?.PrepareForPool();
        }

        private static void DestroyInstance(CombatEnemyRuntime2D instance)
        {
            if (instance != null)
            {
                Destroy(instance.gameObject);
            }
        }
    }
}
