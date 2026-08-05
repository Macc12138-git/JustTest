using System.Collections.Generic;
using JustTest.Game.Combat;
using UnityEngine;
using UnityEngine.Pool;

namespace JustTest.Game.Enemies
{
    public sealed class CombatProjectilePool2D : MonoBehaviour
    {
        [SerializeField] private EnemyProjectile2D prefab;
        [SerializeField] private Transform poolRoot;
        [SerializeField, Min(0)] private int initialCapacity = 4;
        [SerializeField, Min(1)] private int maximumCapacity = 8;
        [SerializeField] private bool collectionCheck = true;

        private readonly HashSet<EnemyProjectile2D> leasedProjectiles =
            new HashSet<EnemyProjectile2D>();
        private ObjectPool<EnemyProjectile2D> pool;
        private bool ready;

        internal int ActiveCount => leasedProjectiles.Count;

        internal bool Initialize()
        {
            if (ready)
            {
                return true;
            }

            if (prefab == null ||
                poolRoot == null ||
                initialCapacity < 0 ||
                maximumCapacity <= 0 ||
                initialCapacity > maximumCapacity)
            {
                return false;
            }

            pool = new ObjectPool<EnemyProjectile2D>(
                CreateInstance,
                null,
                PrepareInstanceForPool,
                DestroyInstance,
                collectionCheck,
                initialCapacity,
                maximumCapacity);
            ready = true;
            Prewarm();
            return true;
        }

        internal bool TryLaunch(
            AttackInstance attack,
            Vector3 position,
            int direction,
            out EnemyProjectile2D projectile)
        {
            projectile = null;
            if (!ready ||
                attack == null ||
                (direction != -1 && direction != 1) ||
                (pool.CountInactive == 0 && pool.CountAll >= maximumCapacity))
            {
                return false;
            }

            EnemyProjectile2D candidate = pool.Get();
            if (candidate == null || !leasedProjectiles.Add(candidate))
            {
                if (candidate != null)
                {
                    pool.Release(candidate);
                }

                return false;
            }

            candidate.RecycleRequested += OnProjectileRecycleRequested;
            if (!candidate.Launch(attack, position, direction))
            {
                candidate.RecycleRequested -= OnProjectileRecycleRequested;
                leasedProjectiles.Remove(candidate);
                pool.Release(candidate);
                return false;
            }

            projectile = candidate;
            return true;
        }

        internal bool Release(EnemyProjectile2D projectile)
        {
            if (!ready || projectile == null || !leasedProjectiles.Remove(projectile))
            {
                return false;
            }

            projectile.RecycleRequested -= OnProjectileRecycleRequested;
            pool.Release(projectile);
            return true;
        }

        internal void ReleaseAll()
        {
            if (!ready || leasedProjectiles.Count == 0)
            {
                return;
            }

            EnemyProjectile2D[] projectiles =
                new EnemyProjectile2D[leasedProjectiles.Count];
            leasedProjectiles.CopyTo(projectiles);
            for (int index = 0; index < projectiles.Length; index++)
            {
                Release(projectiles[index]);
            }
        }

        private void OnDestroy()
        {
            if (pool == null)
            {
                return;
            }

            ReleaseAll();
            pool.Dispose();
            pool = null;
            ready = false;
        }

        private void Prewarm()
        {
            EnemyProjectile2D[] projectiles = new EnemyProjectile2D[initialCapacity];
            for (int index = 0; index < projectiles.Length; index++)
            {
                projectiles[index] = pool.Get();
            }

            for (int index = 0; index < projectiles.Length; index++)
            {
                pool.Release(projectiles[index]);
            }
        }

        private EnemyProjectile2D CreateInstance()
        {
            EnemyProjectile2D instance = Instantiate(prefab, poolRoot);
            instance.name = prefab.name;
            instance.gameObject.SetActive(false);
            return instance;
        }

        private void OnProjectileRecycleRequested(EnemyProjectile2D projectile)
        {
            Release(projectile);
        }

        private static void PrepareInstanceForPool(EnemyProjectile2D projectile)
        {
            projectile?.PrepareForPool();
        }

        private static void DestroyInstance(EnemyProjectile2D projectile)
        {
            if (projectile != null)
            {
                Destroy(projectile.gameObject);
            }
        }

        private void OnValidate()
        {
            initialCapacity = Mathf.Max(0, initialCapacity);
            maximumCapacity = Mathf.Max(1, maximumCapacity);
            initialCapacity = Mathf.Min(initialCapacity, maximumCapacity);
        }
    }
}
