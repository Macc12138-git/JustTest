using System;
using JustTest.Game.Combat;
using JustTest.Game.Run;
using UnityEngine;

namespace JustTest.Game.Enemies
{
    [DefaultExecutionOrder(-20)]
    public sealed class RangedEnemyController2D : MonoBehaviour
    {
        [SerializeField] private RangedEnemyConfig config;
        [SerializeField] private HealthComponent health;
        [SerializeField] private DamageReceiver damageReceiver;
        [SerializeField] private CombatReactionReceiver reactionReceiver;
        [SerializeField] private CombatStatusController statusController;
        [SerializeField] private RangedEnemyMotor2D motor;
        [SerializeField] private RangedEnemyAttackRunner attackRunner;
        [SerializeField] private AttackDefinition rangedAttack;

        private Transform target;
        private HealthComponent targetHealth;
        private CombatPlatformController2D combatPlatform;
        private CombatEnemyRuntime2D runtimeOwner;
        private float nextDecisionAt;
        private float nextAttackAt;
        private float repositionEndsAt;
        private float repositionTargetX;
        private float retreatBlockedSince = float.PositiveInfinity;
        private int repositionDirection;
        private RangedEnemyDecisionState repositionState;
        private bool encounterActive;
        private bool interruptingAttack;
        private bool defeatNotified;
        private bool internalReferencesValid;
        private bool sceneContextBound;
        private bool ready;

        internal event Action<RangedEnemyController2D> Defeated;

        public RangedEnemyDecisionState State { get; private set; } =
            RangedEnemyDecisionState.Dormant;

        private void Awake()
        {
            internalReferencesValid =
                config != null &&
                config.IsValid &&
                health != null &&
                damageReceiver != null &&
                reactionReceiver != null &&
                statusController != null &&
                motor != null &&
                attackRunner != null &&
                rangedAttack != null;
            RefreshReadiness();
            if (!internalReferencesValid)
            {
                Debug.LogError($"{nameof(RangedEnemyController2D)} is missing an Inspector reference.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            RefreshReadiness();
            if (!ready)
            {
                Debug.LogError($"{nameof(RangedEnemyController2D)} has not received its scene context.", this);
                enabled = false;
                return;
            }

            health.Died += OnDied;
            targetHealth.Died += OnTargetDied;
            damageReceiver.CombatStateReset += OnCombatStateReset;
            attackRunner.AttackEnded += OnAttackEnded;
        }

        private void Update()
        {
            if (!ready || !encounterActive)
            {
                return;
            }

            if (health.IsDead)
            {
                EnterInterruptedState(RangedEnemyDecisionState.Dead, false);
                return;
            }

            if (targetHealth.IsDead)
            {
                EnterInterruptedState(RangedEnemyDecisionState.PlayerDefeated, true);
                return;
            }

            if (reactionReceiver.IsReacting || statusController.ActiveStatusCount > 0)
            {
                EnterInterruptedState(RangedEnemyDecisionState.Controlled, false);
                return;
            }

            motor.SetControlEnabled(true);
            if (attackRunner.IsAttacking)
            {
                motor.Stop();
                State = RangedEnemyDecisionState.Attack;
                return;
            }

            if ((State == RangedEnemyDecisionState.Reposition ||
                 State == RangedEnemyDecisionState.Retreat) &&
                TickReposition())
            {
                return;
            }

            EvaluatePositionAndAttack();
        }

        private void OnDisable()
        {
            encounterActive = false;
            if (health != null)
            {
                health.Died -= OnDied;
            }

            if (targetHealth != null)
            {
                targetHealth.Died -= OnTargetDied;
            }

            if (damageReceiver != null)
            {
                damageReceiver.CombatStateReset -= OnCombatStateReset;
            }

            if (attackRunner != null)
            {
                attackRunner.AttackEnded -= OnAttackEnded;
                attackRunner.CancelAttack();
            }

            combatPlatform?.ReleaseAttack(runtimeOwner);
            motor?.Stop();
        }

        internal bool BindSceneContext(
            in CombatEnemySceneContext context,
            CombatEnemyRuntime2D owner)
        {
            if (!context.IsValid ||
                context.ProjectilePool == null ||
                owner == null ||
                !attackRunner.BindProjectilePool(context.ProjectilePool))
            {
                return false;
            }

            target = context.Target;
            targetHealth = context.TargetHealth;
            combatPlatform = context.CombatPlatform;
            runtimeOwner = owner;
            sceneContextBound = true;
            RefreshReadiness();
            return true;
        }

        internal void PrepareForEncounter()
        {
            encounterActive = false;
            defeatNotified = false;
            retreatBlockedSince = float.PositiveInfinity;
            State = RangedEnemyDecisionState.Dormant;
        }

        internal void ActivateEncounter()
        {
            if (!ready || health.IsDead)
            {
                return;
            }

            encounterActive = true;
            defeatNotified = false;
            retreatBlockedSince = float.PositiveInfinity;
            nextDecisionAt = Time.time + config.InitialObservationDuration;
            motor.ResetMotion();
            FaceTarget();
            State = RangedEnemyDecisionState.Observe;
        }

        internal void InterruptEncounter()
        {
            encounterActive = false;
            interruptingAttack = true;
            attackRunner?.CancelAttack();
            interruptingAttack = false;
            combatPlatform?.ReleaseAttack(runtimeOwner);
            retreatBlockedSince = float.PositiveInfinity;
            if (motor != null)
            {
                motor.ResetMotion();
                motor.SetControlEnabled(false);
            }

            if (State != RangedEnemyDecisionState.Dead)
            {
                State = RangedEnemyDecisionState.Dormant;
            }
        }

        private void EvaluatePositionAndAttack()
        {
            Vector2 offset = target.position - transform.position;
            float horizontalDistance = Mathf.Abs(offset.x);
            float verticalDistance = Mathf.Abs(offset.y);
            int targetDirection = offset.x >= 0f ? 1 : -1;
            motor.Face(targetDirection);

            if (Time.time < nextDecisionAt || verticalDistance > config.AttackVerticalTolerance)
            {
                Observe();
                return;
            }

            bool canShootFromBlockedRetreat = false;
            if (horizontalDistance < config.PreferredMinimumDistance)
            {
                float desiredRetreatX =
                    target.position.x - targetDirection * config.PreferredMinimumDistance;
                if (TryBeginReposition(desiredRetreatX, RangedEnemyDecisionState.Retreat))
                {
                    retreatBlockedSince = float.PositiveInfinity;
                    return;
                }

                if (float.IsPositiveInfinity(retreatBlockedSince))
                {
                    retreatBlockedSince = Time.time;
                }

                canShootFromBlockedRetreat =
                    Time.time - retreatBlockedSince >= config.BlockedRetreatGraceDuration;
                if (!canShootFromBlockedRetreat)
                {
                    Observe();
                    nextDecisionAt = Time.time + config.DecisionRetryInterval;
                    return;
                }
            }
            else
            {
                retreatBlockedSince = float.PositiveInfinity;
            }

            if (horizontalDistance > config.PreferredMaximumDistance)
            {
                float desiredApproachX =
                    target.position.x - targetDirection * config.PreferredMaximumDistance;
                if (TryBeginReposition(desiredApproachX, RangedEnemyDecisionState.Reposition))
                {
                    return;
                }

                Observe();
                nextDecisionAt = Time.time + config.DecisionRetryInterval;
                return;
            }

            bool inPreferredRange =
                horizontalDistance >= config.PreferredMinimumDistance &&
                horizontalDistance <= config.PreferredMaximumDistance;
            if ((inPreferredRange || canShootFromBlockedRetreat) && Time.time >= nextAttackAt)
            {
                TryStartAttack(targetDirection);
                return;
            }

            Observe();
            State = Time.time < nextAttackAt
                ? RangedEnemyDecisionState.WaitingForTurn
                : RangedEnemyDecisionState.Observe;
            nextDecisionAt = Time.time + config.DecisionRetryInterval;
        }

        private bool TryBeginReposition(float desiredX, RangedEnemyDecisionState state)
        {
            if (!combatPlatform.TryGetPositionTarget(runtimeOwner, desiredX, out float targetX))
            {
                return false;
            }

            float offset = targetX - transform.position.x;
            if (Mathf.Abs(offset) <= config.PositionTargetTolerance)
            {
                return false;
            }

            int direction = offset > 0f ? 1 : -1;
            if (!combatPlatform.CanMoveWithinPositionSlot(
                    runtimeOwner,
                    transform.position.x,
                    direction,
                    config.PositionTargetTolerance))
            {
                return false;
            }

            repositionDirection = direction;
            repositionTargetX = targetX;
            repositionEndsAt = Time.time + config.RepositionDuration;
            repositionState = state;
            motor.SetHorizontalDirection(direction);
            State = state;
            return true;
        }

        private bool TickReposition()
        {
            float remainingOffset = repositionTargetX - transform.position.x;
            bool reachedTarget = Mathf.Abs(remainingOffset) <= config.PositionTargetTolerance;
            bool canMove = combatPlatform.CanMoveWithinPositionSlot(
                runtimeOwner,
                transform.position.x,
                repositionDirection,
                config.PositionTargetTolerance);
            if (Time.time < repositionEndsAt && !reachedTarget && canMove)
            {
                motor.SetHorizontalDirection(repositionDirection);
                return true;
            }

            if (repositionState == RangedEnemyDecisionState.Retreat && !reachedTarget)
            {
                retreatBlockedSince = float.IsPositiveInfinity(retreatBlockedSince)
                    ? Time.time
                    : retreatBlockedSince;
            }

            Observe();
            nextDecisionAt = Time.time + config.ObservationDuration;
            return false;
        }

        private void TryStartAttack(int direction)
        {
            motor.Stop();
            motor.Face(direction);
            if (!combatPlatform.TryAcquireAttack(runtimeOwner))
            {
                State = RangedEnemyDecisionState.WaitingForTurn;
                nextDecisionAt = Time.time + config.AttackRequestRetryInterval;
                return;
            }

            if (attackRunner.TryStart(rangedAttack, direction))
            {
                State = RangedEnemyDecisionState.Attack;
                return;
            }

            combatPlatform.ReleaseAttack(runtimeOwner);
            nextDecisionAt = Time.time + config.DecisionRetryInterval;
        }

        private void Observe()
        {
            motor.Stop();
            FaceTarget();
            State = RangedEnemyDecisionState.Observe;
        }

        private void FaceTarget()
        {
            if (target != null)
            {
                motor.Face(target.position.x >= transform.position.x ? 1 : -1);
            }
        }

        private void EnterInterruptedState(RangedEnemyDecisionState state, bool allowBraking)
        {
            if (attackRunner.IsAttacking)
            {
                interruptingAttack = true;
                attackRunner.CancelAttack();
                interruptingAttack = false;
            }

            combatPlatform.ReleaseAttack(runtimeOwner);
            motor.Stop();
            motor.SetControlEnabled(allowBraking);
            State = state;
        }

        private void OnAttackEnded(bool completed)
        {
            combatPlatform.ReleaseAttack(runtimeOwner);
            if (!encounterActive || !completed || interruptingAttack)
            {
                return;
            }

            nextAttackAt = Time.time + config.AttackCooldown;
            nextDecisionAt = Time.time + config.PostAttackObservationDuration;
            State = RangedEnemyDecisionState.Observe;
        }

        private void OnDied()
        {
            encounterActive = false;
            EnterInterruptedState(RangedEnemyDecisionState.Dead, false);
            if (defeatNotified)
            {
                return;
            }

            defeatNotified = true;
            Defeated?.Invoke(this);
        }

        private void OnTargetDied()
        {
            encounterActive = false;
            EnterInterruptedState(RangedEnemyDecisionState.PlayerDefeated, true);
        }

        private void OnCombatStateReset()
        {
            interruptingAttack = true;
            attackRunner.CancelAttack();
            interruptingAttack = false;
            combatPlatform.ReleaseAttack(runtimeOwner);
            motor.ResetMotion();
            defeatNotified = false;
            retreatBlockedSince = float.PositiveInfinity;
            State = encounterActive
                ? RangedEnemyDecisionState.Observe
                : RangedEnemyDecisionState.Dormant;
        }

        private void RefreshReadiness()
        {
            ready =
                internalReferencesValid &&
                sceneContextBound &&
                target != null &&
                targetHealth != null &&
                combatPlatform != null &&
                runtimeOwner != null;
        }
    }
}
