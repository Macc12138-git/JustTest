using System;
using JustTest.Game.Combat;
using UnityEngine;

namespace JustTest.Game.Enemies
{
    public sealed class RangedEnemyAttackRunner : MonoBehaviour
    {
        [SerializeField] private Transform projectileOrigin;

        private AttackInstanceFactory attackFactory;
        private AttackTimeline timeline;
        private AttackDefinition activeDefinition;
        private EnemyProjectile2D activeProjectile;
        private CombatProjectilePool2D projectilePool;
        private bool timelineCompleted;
        private bool projectileLaunched;
        private bool cancelling;
        private bool internalReferencesValid;

        public event Action<AttackPhase> PhaseChanged;
        public event Action<bool> AttackEnded;

        public AttackPhase Phase => timeline?.Phase ?? AttackPhase.Idle;
        public bool IsAttacking => activeDefinition != null;
        public float PhaseProgress => timeline?.PhaseProgress ?? 0f;
        public AttackDefinition CurrentDefinition => activeDefinition;
        public int FacingDirection { get; private set; } = -1;
        internal bool HasActiveProjectile => activeProjectile != null;

        private void Awake()
        {
            internalReferencesValid = projectileOrigin != null;
            if (!internalReferencesValid)
            {
                Debug.LogError($"{nameof(RangedEnemyAttackRunner)} is missing an Inspector reference.", this);
                enabled = false;
                return;
            }

            attackFactory = new AttackInstanceFactory();
        }

        private void Update()
        {
            timeline?.Tick(Time.deltaTime);
        }

        private void OnDisable()
        {
            CancelAttack();
        }

        internal bool BindProjectilePool(CombatProjectilePool2D pool)
        {
            if (projectileOrigin == null || pool == null)
            {
                return false;
            }

            projectilePool = pool;
            return true;
        }

        internal bool TryStart(AttackDefinition definition, int direction)
        {
            if (projectilePool == null ||
                definition == null ||
                IsAttacking ||
                (direction != -1 && direction != 1))
            {
                return false;
            }

            FacingDirection = direction;
            activeDefinition = definition;
            timelineCompleted = false;
            projectileLaunched = false;
            cancelling = false;
            timeline = new AttackTimeline(
                definition.WindupDuration,
                definition.ActiveDuration,
                definition.RecoveryDuration);
            timeline.PhaseChanged += OnTimelinePhaseChanged;
            return timeline.Start();
        }

        internal void CancelAttack()
        {
            if (!IsAttacking)
            {
                return;
            }

            cancelling = true;
            if (timeline != null && timeline.IsRunning)
            {
                timeline.Cancel();
            }
            else
            {
                timelineCompleted = true;
                ReleaseTimeline();
            }

            if (activeProjectile != null)
            {
                EnemyProjectile2D projectile = activeProjectile;
                activeProjectile = null;
                projectile.Completed -= OnProjectileCompleted;
                projectilePool?.Release(projectile);
            }

            TryFinalizeAttack();
        }

        private void OnTimelinePhaseChanged(AttackPhase previousPhase, AttackPhase nextPhase)
        {
            if (nextPhase == AttackPhase.Active && !TryLaunchProjectile())
            {
                cancelling = true;
                timeline.Cancel();
                return;
            }

            PhaseChanged?.Invoke(nextPhase);
            if (nextPhase != AttackPhase.Idle)
            {
                return;
            }

            timelineCompleted = true;
            ReleaseTimeline();
            TryFinalizeAttack();
        }

        private bool TryLaunchProjectile()
        {
            AttackInstance attack = attackFactory.Create(
                GetInstanceID(),
                CombatFaction.Enemy,
                activeDefinition.Damage,
                FacingDirection,
                activeDefinition.HitReaction,
                activeDefinition.StatusApplication,
                activeDefinition.AllowFriendlyFire,
                activeDefinition.IgnorePostHitInvulnerability,
                activeDefinition.FeedbackTier);
            if (!projectilePool.TryLaunch(
                    attack,
                    projectileOrigin.position,
                    FacingDirection,
                    out EnemyProjectile2D projectile))
            {
                return false;
            }

            activeProjectile = projectile;
            activeProjectile.Completed += OnProjectileCompleted;
            projectileLaunched = true;
            return true;
        }

        private void OnProjectileCompleted(EnemyProjectile2D projectile)
        {
            if (projectile != activeProjectile)
            {
                return;
            }

            projectile.Completed -= OnProjectileCompleted;
            activeProjectile = null;
            TryFinalizeAttack();
        }

        private void TryFinalizeAttack()
        {
            if (!timelineCompleted || activeProjectile != null || activeDefinition == null)
            {
                return;
            }

            bool completed = !cancelling && projectileLaunched;
            activeDefinition = null;
            timelineCompleted = false;
            projectileLaunched = false;
            cancelling = false;
            AttackEnded?.Invoke(completed);
        }

        private void ReleaseTimeline()
        {
            if (timeline == null)
            {
                return;
            }

            timeline.PhaseChanged -= OnTimelinePhaseChanged;
            timeline = null;
        }
    }
}
