using System;
using System.Collections;
using System.Collections.Generic;
using JustTest.Game.Combat;
using JustTest.Game.Enemies;
using JustTest.Game.Player;
using UnityEngine;

namespace JustTest.Game.Run
{
    [DefaultExecutionOrder(-100)]
    public sealed class CombatPlatformController2D : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private CombatPlatformConfig config;
        [SerializeField] private CombatEncounterConfig encounterConfig;

        [Header("Platform")]
        [SerializeField] private Collider2D entryTrigger;
        [SerializeField] private Collider2D combatSurface;
        [SerializeField] private Collider2D[] boundaries;
        [SerializeField] private SpriteRenderer[] boundaryVisuals;
        [SerializeField] private Transform[] spawnPoints;

        [Header("Player")]
        [SerializeField] private Transform playerTarget;
        [SerializeField] private Collider2D playerCollider;
        [SerializeField] private PlayerGroundProbe2D playerGroundProbe;
        [SerializeField] private HealthComponent playerHealth;
        [SerializeField] private PlayerAttackRunner playerAttackRunner;
        [SerializeField] private PlayerRollController playerRollController;

        [Header("Encounter Services")]
        [SerializeField] private CombatEnemyPool2D enemyPool;
        [SerializeField] private CombatProjectilePool2D projectilePool;
        [SerializeField] private CombatFeedbackCoordinator feedbackCoordinator;

        private readonly CombatPlatformStateMachine stateMachine = new CombatPlatformStateMachine();
        private readonly HashSet<CombatEnemyRuntime2D> leasedEnemies =
            new HashSet<CombatEnemyRuntime2D>();
        private readonly HashSet<CombatEnemyRuntime2D> livingEnemies =
            new HashSet<CombatEnemyRuntime2D>();
        private readonly Dictionary<CombatEnemyRuntime2D, CombatFeedbackRegistration>
            feedbackRegistrations =
                new Dictionary<CombatEnemyRuntime2D, CombatFeedbackRegistration>();
        private readonly Dictionary<CombatEnemyRuntime2D, Coroutine> recycleRoutines =
            new Dictionary<CombatEnemyRuntime2D, Coroutine>();

        private CombatWaveStateMachine waveStateMachine;
        private CombatPositionSlotAllocator positionSlotAllocator;
        private Coroutine encounterRoutine;
        private CombatEnemyRuntime2D activeAttacker;
        private float nextAttackAllowedAt;
        private int nextSpawnPointIndex;
        private bool ready;

        internal event Action Completed;

        internal CombatPlatformState State => stateMachine.State;
        internal bool IsCombatActive => stateMachine.State == CombatPlatformState.Active;
        internal int CurrentWaveIndex => waveStateMachine?.CurrentWaveIndex ?? -1;
        internal int LivingEnemyCount => livingEnemies.Count;

        private void Awake()
        {
            ready = ValidateReferences() && InitializeRuntimeServices();
            if (!ready)
            {
                Debug.LogError(
                    $"{nameof(CombatPlatformController2D)} has an invalid Inspector binding or combat surface.",
                    this);
                enabled = false;
                return;
            }

            SetBoundariesClosed(false);
        }

        private void OnEnable()
        {
            if (ready)
            {
                playerHealth.Died += OnPlayerDied;
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!ready ||
                stateMachine.State != CombatPlatformState.Dormant ||
                other != playerCollider ||
                playerHealth.IsDead ||
                !playerGroundProbe.IsGrounded ||
                playerGroundProbe.GroundCollider != combatSurface)
            {
                return;
            }

            BeginEncounter();
        }

        private void OnDisable()
        {
            if (playerHealth != null)
            {
                playerHealth.Died -= OnPlayerDied;
            }

            StopEncounterRoutine();
            StopRecycleRoutines();
            waveStateMachine?.TryInterrupt();
            CleanupAllEnemies();
            activeAttacker = null;
            SetBoundariesClosed(false);
        }

        internal bool TryAcquireAttack(CombatEnemyRuntime2D requester)
        {
            if (!ready ||
                !IsCombatActive ||
                requester == null ||
                !livingEnemies.Contains(requester) ||
                activeAttacker != null ||
                Time.time < nextAttackAllowedAt)
            {
                return false;
            }

            activeAttacker = requester;
            return true;
        }

        internal void ReleaseAttack(CombatEnemyRuntime2D requester)
        {
            if (activeAttacker != requester)
            {
                return;
            }

            activeAttacker = null;
            nextAttackAllowedAt = Time.time + config.SharedAttackInterval;
        }

        internal bool TryGetPositionTarget(
            CombatEnemyRuntime2D requester,
            float desiredX,
            out float targetX)
        {
            targetX = desiredX;
            return ready &&
                   requester != null &&
                   positionSlotAllocator != null &&
                   positionSlotAllocator.TryGetTarget(
                       requester.ParticipantId,
                       desiredX,
                       out targetX);
        }

        internal bool CanMoveWithinPositionSlot(
            CombatEnemyRuntime2D requester,
            float currentX,
            int direction,
            float tolerance)
        {
            return ready &&
                   requester != null &&
                   positionSlotAllocator != null &&
                   positionSlotAllocator.CanMove(
                       requester.ParticipantId,
                       currentX,
                       direction,
                       tolerance);
        }

        private bool ValidateReferences()
        {
            if (config == null ||
                !config.IsValid ||
                encounterConfig == null ||
                !encounterConfig.IsValid ||
                entryTrigger == null ||
                !entryTrigger.isTrigger ||
                !config.IsValidCombatSurface(combatSurface) ||
                playerTarget == null ||
                playerCollider == null ||
                playerGroundProbe == null ||
                playerHealth == null ||
                playerAttackRunner == null ||
                playerRollController == null ||
                enemyPool == null ||
                projectilePool == null ||
                feedbackCoordinator == null ||
                boundaries == null ||
                boundaries.Length == 0 ||
                spawnPoints == null ||
                spawnPoints.Length == 0)
            {
                return false;
            }

            for (int index = 0; index < boundaries.Length; index++)
            {
                Collider2D boundary = boundaries[index];
                if (boundary == null || boundary.isTrigger || boundary == combatSurface)
                {
                    return false;
                }
            }

            for (int index = 0; index < spawnPoints.Length; index++)
            {
                if (spawnPoints[index] == null)
                {
                    return false;
                }
            }

            return true;
        }

        private bool InitializeRuntimeServices()
        {
            Bounds surfaceBounds = combatSurface.bounds;
            positionSlotAllocator = new CombatPositionSlotAllocator(
                surfaceBounds.min.x + config.PlatformEdgePadding,
                surfaceBounds.max.x - config.PlatformEdgePadding,
                config.SlotInnerPadding);
            if (!positionSlotAllocator.IsValid)
            {
                return false;
            }

            waveStateMachine = new CombatWaveStateMachine(
                encounterConfig.BuildWaveEnemyCounts(),
                encounterConfig.MaximumConcurrentEnemies);
            CombatEnemySceneContext sceneContext = new CombatEnemySceneContext(
                playerTarget,
                playerHealth,
                playerAttackRunner,
                playerRollController,
                this,
                projectilePool);
            return projectilePool.Initialize() && enemyPool.Initialize(sceneContext);
        }

        private void BeginEncounter()
        {
            if (!stateMachine.TryBeginAppearance())
            {
                return;
            }

            SetBoundariesClosed(true);
            encounterRoutine = StartCoroutine(RunEncounter());
        }

        private IEnumerator RunEncounter()
        {
            if (config.AppearanceDelay > 0f)
            {
                yield return new WaitForSeconds(config.AppearanceDelay);
            }

            if (!stateMachine.TryActivate() || !waveStateMachine.TryBegin())
            {
                encounterRoutine = null;
                yield break;
            }

            while (stateMachine.State == CombatPlatformState.Active)
            {
                switch (waveStateMachine.State)
                {
                    case CombatWaveState.Spawning:
                        yield return RunSpawnStep();
                        break;
                    case CombatWaveState.WaitingForDefeat:
                        yield return null;
                        break;
                    case CombatWaveState.InterWaveDelay:
                        if (encounterConfig.InterWaveDelay > 0f)
                        {
                            yield return new WaitForSeconds(encounterConfig.InterWaveDelay);
                        }

                        waveStateMachine.TryBeginNextWave();
                        break;
                    case CombatWaveState.Completed:
                        CompleteEncounter();
                        encounterRoutine = null;
                        yield break;
                    default:
                        encounterRoutine = null;
                        yield break;
                }
            }

            encounterRoutine = null;
        }

        private IEnumerator RunSpawnStep()
        {
            if (!waveStateMachine.CanSpawn)
            {
                yield return null;
                yield break;
            }

            if (!TrySelectSpawnPosition(out Vector3 spawnPosition) ||
                !encounterConfig.GetWave(waveStateMachine.CurrentWaveIndex)
                    .TryGetArchetypeAt(
                        waveStateMachine.SpawnedCount,
                        out CombatEnemyArchetype archetype) ||
                !TrySpawnEnemy(
                    archetype,
                    spawnPosition,
                    out CombatEnemyRuntime2D enemy))
            {
                if (encounterConfig.SpawnRetryInterval > 0f)
                {
                    yield return new WaitForSeconds(encounterConfig.SpawnRetryInterval);
                }
                else
                {
                    yield return null;
                }

                yield break;
            }

            int expectedLeaseId = enemy.LeaseId;
            if (encounterConfig.EnemyAppearanceDelay > 0f)
            {
                yield return new WaitForSeconds(encounterConfig.EnemyAppearanceDelay);
            }

            if (stateMachine.State == CombatPlatformState.Active &&
                livingEnemies.Contains(enemy) &&
                enemy.LeaseId == expectedLeaseId)
            {
                enemy.ActivateEncounter();
            }

            if (encounterConfig.SpawnInterval > 0f)
            {
                yield return new WaitForSeconds(encounterConfig.SpawnInterval);
            }
        }

        private bool TrySpawnEnemy(
            CombatEnemyArchetype archetype,
            Vector3 position,
            out CombatEnemyRuntime2D enemy)
        {
            enemy = null;
            if (!enemyPool.TryAcquire(
                    archetype,
                    position,
                    out CombatEnemyRuntime2D candidate))
            {
                return false;
            }

            if (!feedbackCoordinator.TryRegisterRuntime(
                    candidate.DamageReceiver,
                    candidate.HitFlash,
                    candidate.ImpactAnchor,
                    candidate.FeedbackSources,
                    candidate.AttackRecoil,
                    out CombatFeedbackRegistration feedbackRegistration))
            {
                enemyPool.Release(candidate);
                return false;
            }

            int participantId = candidate.ParticipantId;
            if (!positionSlotAllocator.Register(participantId, position.x) ||
                !waveStateMachine.TryRecordSpawn(participantId))
            {
                positionSlotAllocator.Unregister(participantId);
                feedbackCoordinator.UnregisterRuntime(feedbackRegistration);
                enemyPool.Release(candidate);
                return false;
            }

            candidate.Defeated += OnEnemyDefeated;
            feedbackRegistrations.Add(candidate, feedbackRegistration);
            leasedEnemies.Add(candidate);
            livingEnemies.Add(candidate);
            enemy = candidate;
            return true;
        }

        private bool TrySelectSpawnPosition(out Vector3 position)
        {
            position = default;
            for (int offset = 0; offset < spawnPoints.Length; offset++)
            {
                int index = (nextSpawnPointIndex + offset) % spawnPoints.Length;
                Vector3 candidate = spawnPoints[index].position;
                if (!IsSpawnPositionSafe(candidate))
                {
                    continue;
                }

                nextSpawnPointIndex = (index + 1) % spawnPoints.Length;
                position = candidate;
                return true;
            }

            return false;
        }

        private bool IsSpawnPositionSafe(Vector3 position)
        {
            if (Vector2.Distance(position, playerTarget.position) <
                encounterConfig.MinimumDistanceFromPlayer)
            {
                return false;
            }

            foreach (CombatEnemyRuntime2D enemy in livingEnemies)
            {
                if (enemy != null &&
                    Vector2.Distance(position, enemy.transform.position) <
                    encounterConfig.MinimumEnemySeparation)
                {
                    return false;
                }
            }

            return true;
        }

        private void OnEnemyDefeated(CombatEnemyRuntime2D enemy)
        {
            if (enemy == null || !livingEnemies.Remove(enemy))
            {
                return;
            }

            enemy.Defeated -= OnEnemyDefeated;
            ReleaseAttack(enemy);
            positionSlotAllocator.Unregister(enemy.ParticipantId);
            Coroutine recycleRoutine = StartCoroutine(
                RecycleAfterDelay(enemy, enemy.LeaseId));
            recycleRoutines[enemy] = recycleRoutine;
            waveStateMachine.TryRecordDefeat(enemy.ParticipantId);
        }

        private IEnumerator RecycleAfterDelay(
            CombatEnemyRuntime2D enemy,
            int expectedLeaseId)
        {
            if (encounterConfig.CorpseLifetime > 0f)
            {
                yield return new WaitForSeconds(encounterConfig.CorpseLifetime);
            }

            recycleRoutines.Remove(enemy);
            if (enemy != null && enemy.LeaseId == expectedLeaseId)
            {
                UnregisterFeedback(enemy);
                leasedEnemies.Remove(enemy);
                enemyPool.Release(enemy);
            }
        }

        private void CompleteEncounter()
        {
            if (!stateMachine.TryComplete())
            {
                return;
            }

            activeAttacker = null;
            projectilePool.ReleaseAll();
            SetBoundariesClosed(false);
            Completed?.Invoke();
        }

        private void OnPlayerDied()
        {
            bool encounterStarted = stateMachine.State != CombatPlatformState.Dormant;
            if (!stateMachine.TryInterrupt())
            {
                return;
            }

            StopEncounterRoutine();
            waveStateMachine.TryInterrupt();
            activeAttacker = null;
            projectilePool.ReleaseAll();
            foreach (CombatEnemyRuntime2D enemy in livingEnemies)
            {
                enemy?.InterruptEncounter();
            }

            SetBoundariesClosed(encounterStarted);
        }

        private void StopEncounterRoutine()
        {
            if (encounterRoutine == null)
            {
                return;
            }

            StopCoroutine(encounterRoutine);
            encounterRoutine = null;
        }

        private void StopRecycleRoutines()
        {
            foreach (Coroutine routine in recycleRoutines.Values)
            {
                if (routine != null)
                {
                    StopCoroutine(routine);
                }
            }

            recycleRoutines.Clear();
        }

        private void CleanupAllEnemies()
        {
            if (leasedEnemies.Count == 0)
            {
                livingEnemies.Clear();
                feedbackRegistrations.Clear();
                return;
            }

            CombatEnemyRuntime2D[] enemies =
                new CombatEnemyRuntime2D[leasedEnemies.Count];
            leasedEnemies.CopyTo(enemies);
            for (int index = 0; index < enemies.Length; index++)
            {
                CombatEnemyRuntime2D enemy = enemies[index];
                if (enemy == null)
                {
                    continue;
                }

                enemy.Defeated -= OnEnemyDefeated;
                ReleaseAttack(enemy);
                positionSlotAllocator?.Unregister(enemy.ParticipantId);
                UnregisterFeedback(enemy);
                enemy.InterruptEncounter();
                enemyPool?.Release(enemy);
            }

            livingEnemies.Clear();
            leasedEnemies.Clear();
            feedbackRegistrations.Clear();
        }

        private void UnregisterFeedback(CombatEnemyRuntime2D enemy)
        {
            if (feedbackRegistrations.TryGetValue(
                    enemy,
                    out CombatFeedbackRegistration registration))
            {
                feedbackCoordinator.UnregisterRuntime(registration);
                feedbackRegistrations.Remove(enemy);
            }
        }

        private void SetBoundariesClosed(bool closed)
        {
            if (boundaries != null)
            {
                for (int index = 0; index < boundaries.Length; index++)
                {
                    if (boundaries[index] != null)
                    {
                        boundaries[index].enabled = closed;
                    }
                }
            }

            if (boundaryVisuals == null)
            {
                return;
            }

            for (int index = 0; index < boundaryVisuals.Length; index++)
            {
                if (boundaryVisuals[index] != null)
                {
                    boundaryVisuals[index].enabled = closed;
                }
            }
        }
    }
}
