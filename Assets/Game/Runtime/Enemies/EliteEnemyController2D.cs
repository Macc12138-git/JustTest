using System;
using JustTest.Game.Combat;
using JustTest.Game.Player;
using JustTest.Game.Run;
using UnityEngine;

namespace JustTest.Game.Enemies
{
    [DefaultExecutionOrder(-20)]
    public sealed class EliteEnemyController2D : MonoBehaviour
    {
        [SerializeField] private EliteEnemyConfig config;
        [SerializeField] private HealthComponent health;
        [SerializeField] private DamageReceiver damageReceiver;
        [SerializeField] private CombatReactionReceiver reactionReceiver;
        [SerializeField] private CombatStatusController statusController;
        [SerializeField] private EliteEnemyMotor2D motor;
        [SerializeField] private EnemyAttackRunner attackRunner;
        [SerializeField] private AttackDefinition quickSlash;
        [SerializeField] private AttackDefinition heavySmash;
        [SerializeField] private AttackDefinition dashCleave;

        private Transform target;
        private HealthComponent targetHealth;
        private PlayerAttackRunner targetAttackRunner;
        private PlayerRollController targetRollController;
        private CombatPlatformController2D combatPlatform;
        private CombatEnemyRuntime2D runtimeOwner;
        private EliteEnemyDecisionResolver decisionResolver;

        private float nextDecisionAt;
        private float nextQuickAttackAt;
        private float nextHeavyAttackAt;
        private float nextDashAttackAt;
        private float nextPassiveAttackAt;
        private float repositionEndsAt;
        private float repositionTargetX;
        private float closePresenceDuration;
        private float recoveryOpportunityAvailableAt = float.PositiveInfinity;
        private int repositionDirection;
        private AttackPhase lastTargetAttackPhase;
        private bool targetWasRolling;
        private bool encounterActive;
        private bool interruptingAttack;
        private bool defeatNotified;
        private bool internalReferencesValid;
        private bool sceneContextBound;
        private bool ready;

        internal event Action<EliteEnemyController2D> Defeated;

        public EliteEnemyDecisionState State { get; private set; } =
            EliteEnemyDecisionState.Dormant;
        internal EliteEnemyAttackType CurrentAttackType { get; private set; } =
            EliteEnemyAttackType.None;

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
                quickSlash != null &&
                heavySmash != null &&
                dashCleave != null;
            if (internalReferencesValid)
            {
                decisionResolver = new EliteEnemyDecisionResolver(config.DecisionParameters);
            }

            RefreshReadiness();
            if (!internalReferencesValid)
            {
                Debug.LogError($"{nameof(EliteEnemyController2D)} is missing an Inspector reference.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            RefreshReadiness();
            if (!ready)
            {
                Debug.LogError($"{nameof(EliteEnemyController2D)} has not received its scene context.", this);
                enabled = false;
                return;
            }

            health.Died += OnDied;
            targetHealth.Died += OnTargetDied;
            damageReceiver.CombatStateReset += OnCombatStateReset;
            attackRunner.AttackEnded += OnAttackEnded;
            attackRunner.PhaseChanged += OnAttackPhaseChanged;
        }

        private void Update()
        {
            if (!ready || !encounterActive)
            {
                return;
            }

            if (health.IsDead)
            {
                EnterInterruptedState(EliteEnemyDecisionState.Dead, false);
                return;
            }

            if (targetHealth.IsDead)
            {
                EnterInterruptedState(EliteEnemyDecisionState.PlayerDefeated, true);
                return;
            }

            if (reactionReceiver.IsReacting || statusController.ActiveStatusCount > 0)
            {
                EnterInterruptedState(EliteEnemyDecisionState.Controlled, false);
                return;
            }

            motor.SetControlEnabled(true);
            UpdateTargetSignals();
            if (attackRunner.IsAttacking)
            {
                TickAttack();
                return;
            }

            if (State == EliteEnemyDecisionState.Reposition && TickReposition())
            {
                return;
            }

            EvaluateDecision();
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
                attackRunner.PhaseChanged -= OnAttackPhaseChanged;
                attackRunner.CancelAttack();
            }

            combatPlatform?.ReleaseAttack(runtimeOwner);
            CurrentAttackType = EliteEnemyAttackType.None;
            motor?.ResetMotion();
        }

        internal bool BindSceneContext(
            in CombatEnemySceneContext context,
            CombatEnemyRuntime2D owner)
        {
            if (!context.IsValid || owner == null)
            {
                return false;
            }

            target = context.Target;
            targetHealth = context.TargetHealth;
            targetAttackRunner = context.TargetAttackRunner;
            targetRollController = context.TargetRollController;
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
            interruptingAttack = false;
            CurrentAttackType = EliteEnemyAttackType.None;
            closePresenceDuration = 0f;
            recoveryOpportunityAvailableAt = float.PositiveInfinity;
            State = EliteEnemyDecisionState.Dormant;
        }

        internal void ActivateEncounter()
        {
            if (!ready || health.IsDead)
            {
                return;
            }

            float now = Time.time;
            encounterActive = true;
            defeatNotified = false;
            CurrentAttackType = EliteEnemyAttackType.None;
            closePresenceDuration = 0f;
            nextDecisionAt = now + config.InitialObservationDuration;
            nextQuickAttackAt = now;
            nextHeavyAttackAt = now;
            nextDashAttackAt = now;
            nextPassiveAttackAt = now + config.MaximumPassiveDuration;
            lastTargetAttackPhase = targetAttackRunner.Phase;
            targetWasRolling = targetRollController.IsRolling;
            recoveryOpportunityAvailableAt = float.PositiveInfinity;
            motor.ResetMotion();
            FaceTarget();
            State = EliteEnemyDecisionState.Observe;
        }

        internal void InterruptEncounter()
        {
            encounterActive = false;
            CancelCurrentAttack();
            combatPlatform?.ReleaseAttack(runtimeOwner);
            closePresenceDuration = 0f;
            recoveryOpportunityAvailableAt = float.PositiveInfinity;
            if (motor != null)
            {
                motor.ResetMotion();
                motor.SetControlEnabled(false);
            }

            if (State != EliteEnemyDecisionState.Dead)
            {
                State = EliteEnemyDecisionState.Dormant;
            }
        }

        private void UpdateTargetSignals()
        {
            AttackPhase targetPhase = targetAttackRunner.Phase;
            if (targetPhase != lastTargetAttackPhase)
            {
                recoveryOpportunityAvailableAt = targetPhase == AttackPhase.Recovery
                    ? Time.time + config.OpportunityReactionDelay
                    : float.PositiveInfinity;
                lastTargetAttackPhase = targetPhase;
            }

            bool targetRolling = targetRollController.IsRolling;
            if (targetWasRolling && !targetRolling)
            {
                nextDecisionAt = Mathf.Max(
                    nextDecisionAt,
                    Time.time + config.RollExitObservationDuration);
            }

            targetWasRolling = targetRolling;
        }

        private void TickAttack()
        {
            motor.Stop();
            if (CurrentAttackType == EliteEnemyAttackType.DashCleave &&
                attackRunner.Phase == AttackPhase.Active)
            {
                if (CanContinueDash())
                {
                    TryBeginPlatformDash();
                }
                else
                {
                    motor.StopDash();
                }
            }
            else
            {
                motor.StopDash();
            }

            State = attackRunner.Phase == AttackPhase.Recovery
                ? EliteEnemyDecisionState.AttackRecovery
                : EliteEnemyDecisionState.Attack;
        }

        private bool CanContinueDash()
        {
            int direction = attackRunner.FacingDirection;
            if (!combatPlatform.CanMoveWithinPositionSlot(
                    runtimeOwner,
                    transform.position.x,
                    direction,
                    config.PositionTargetTolerance))
            {
                return false;
            }

            float desiredX =
                transform.position.x + direction * config.DashSpeed * Time.fixedDeltaTime;
            return combatPlatform.TryGetPositionTarget(runtimeOwner, desiredX, out float targetX) &&
                   Mathf.Abs(targetX - desiredX) <= config.PositionTargetTolerance;
        }

        private bool TryBeginPlatformDash()
        {
            int direction = attackRunner.FacingDirection;
            if (!combatPlatform.CanMoveWithinPositionSlot(
                    runtimeOwner,
                    transform.position.x,
                    direction,
                    config.PositionTargetTolerance))
            {
                return false;
            }

            float desiredStopX =
                transform.position.x + direction * config.DashMaximumDistance;
            if (!combatPlatform.TryGetPositionTarget(
                    runtimeOwner,
                    desiredStopX,
                    out float stopX) ||
                (stopX - transform.position.x) * direction <=
                config.PositionTargetTolerance)
            {
                return false;
            }

            motor.BeginDash(direction, stopX);
            return motor.IsDashing;
        }

        private void EvaluateDecision()
        {
            Vector2 offset = target.position - transform.position;
            float horizontalDistance = Mathf.Abs(offset.x);
            float verticalDistance = Mathf.Abs(offset.y);
            int targetDirection = offset.x >= 0f ? 1 : -1;
            motor.Face(targetDirection);

            bool closeForHeavy =
                horizontalDistance <= config.HeavyAttackRange &&
                verticalDistance <= config.AttackVerticalTolerance &&
                targetAttackRunner.Phase == AttackPhase.Idle &&
                !targetRollController.IsRolling;
            closePresenceDuration = closeForHeavy
                ? closePresenceDuration + Time.deltaTime
                : 0f;

            if (Time.time < nextDecisionAt)
            {
                Observe();
                return;
            }

            EliteEnemyDecisionInput input = new EliteEnemyDecisionInput(
                horizontalDistance,
                verticalDistance,
                targetAttackRunner.Phase,
                targetRollController.IsRolling,
                targetAttackRunner.Phase == AttackPhase.Recovery &&
                Time.time >= recoveryOpportunityAvailableAt,
                Time.time >= nextQuickAttackAt,
                Time.time >= nextHeavyAttackAt,
                Time.time >= nextDashAttackAt,
                Time.time >= nextPassiveAttackAt,
                closePresenceDuration);
            EliteEnemyDecision decision = decisionResolver.Resolve(input);

            switch (decision)
            {
                case EliteEnemyDecision.QuickSlash:
                    TryStartAttack(quickSlash, EliteEnemyAttackType.QuickSlash, targetDirection);
                    break;
                case EliteEnemyDecision.HeavySmash:
                    TryStartAttack(heavySmash, EliteEnemyAttackType.HeavySmash, targetDirection);
                    break;
                case EliteEnemyDecision.DashCleave:
                    TryStartAttack(dashCleave, EliteEnemyAttackType.DashCleave, targetDirection);
                    break;
                case EliteEnemyDecision.Reposition:
                    TryBeginReposition(horizontalDistance, targetDirection);
                    break;
                default:
                    Observe();
                    nextDecisionAt = Time.time + config.DecisionRetryInterval;
                    break;
            }
        }

        private void TryStartAttack(
            AttackDefinition definition,
            EliteEnemyAttackType attackType,
            int direction)
        {
            motor.Stop();
            motor.StopDash();
            motor.Face(direction);
            if (!combatPlatform.TryAcquireAttack(runtimeOwner))
            {
                Observe();
                nextDecisionAt = Time.time + config.AttackRequestRetryInterval;
                return;
            }

            CurrentAttackType = attackType;
            if (attackRunner.TryStart(definition, direction))
            {
                State = EliteEnemyDecisionState.Attack;
                return;
            }

            CurrentAttackType = EliteEnemyAttackType.None;
            combatPlatform.ReleaseAttack(runtimeOwner);
            nextDecisionAt = Time.time + config.DecisionRetryInterval;
        }

        private void TryBeginReposition(float horizontalDistance, int targetDirection)
        {
            float desiredX = horizontalDistance < config.PreferredMinimumDistance
                ? target.position.x - targetDirection * config.PreferredMinimumDistance
                : target.position.x - targetDirection * config.PreferredMaximumDistance;
            if (!combatPlatform.TryGetPositionTarget(runtimeOwner, desiredX, out float targetX))
            {
                Observe();
                nextDecisionAt = Time.time + config.DecisionRetryInterval;
                return;
            }

            float offset = targetX - transform.position.x;
            if (Mathf.Abs(offset) <= config.PositionTargetTolerance)
            {
                Observe();
                nextDecisionAt = Time.time + config.ObservationDuration;
                return;
            }

            int direction = offset > 0f ? 1 : -1;
            if (!combatPlatform.CanMoveWithinPositionSlot(
                    runtimeOwner,
                    transform.position.x,
                    direction,
                    config.PositionTargetTolerance))
            {
                Observe();
                nextDecisionAt = Time.time + config.DecisionRetryInterval;
                return;
            }

            repositionDirection = direction;
            repositionTargetX = targetX;
            repositionEndsAt = Time.time + config.RepositionDuration;
            motor.SetHorizontalDirection(direction);
            State = EliteEnemyDecisionState.Reposition;
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

            Observe();
            nextDecisionAt = Time.time + config.ObservationDuration;
            return false;
        }

        private void Observe()
        {
            motor.Stop();
            motor.StopDash();
            FaceTarget();
            State = EliteEnemyDecisionState.Observe;
        }

        private void FaceTarget()
        {
            if (target != null)
            {
                motor.Face(target.position.x >= transform.position.x ? 1 : -1);
            }
        }

        private void EnterInterruptedState(EliteEnemyDecisionState state, bool allowBraking)
        {
            CancelCurrentAttack();
            combatPlatform.ReleaseAttack(runtimeOwner);
            motor.Stop();
            motor.StopDash();
            motor.SetControlEnabled(allowBraking);
            State = state;
        }

        private void CancelCurrentAttack()
        {
            if (attackRunner != null && attackRunner.IsAttacking)
            {
                interruptingAttack = true;
                attackRunner.CancelAttack();
                interruptingAttack = false;
            }

            motor?.StopDash();
            CurrentAttackType = EliteEnemyAttackType.None;
        }

        private void OnAttackPhaseChanged(AttackPhase phase)
        {
            if (CurrentAttackType == EliteEnemyAttackType.DashCleave &&
                phase == AttackPhase.Active)
            {
                TryBeginPlatformDash();
            }
            else if (phase == AttackPhase.Recovery || phase == AttackPhase.Idle)
            {
                motor.StopDash();
            }

            State = phase == AttackPhase.Recovery
                ? EliteEnemyDecisionState.AttackRecovery
                : EliteEnemyDecisionState.Attack;
        }

        private void OnAttackEnded(bool completed)
        {
            combatPlatform.ReleaseAttack(runtimeOwner);
            motor.StopDash();
            EliteEnemyAttackType completedAttack = CurrentAttackType;
            CurrentAttackType = EliteEnemyAttackType.None;
            if (!encounterActive || !completed || interruptingAttack)
            {
                return;
            }

            float now = Time.time;
            switch (completedAttack)
            {
                case EliteEnemyAttackType.QuickSlash:
                    nextQuickAttackAt = now + config.QuickAttackCooldown;
                    break;
                case EliteEnemyAttackType.HeavySmash:
                    nextHeavyAttackAt = now + config.HeavyAttackCooldown;
                    break;
                case EliteEnemyAttackType.DashCleave:
                    nextDashAttackAt = now + config.DashAttackCooldown;
                    break;
            }

            nextPassiveAttackAt = now + config.MaximumPassiveDuration;
            nextDecisionAt = now + config.PostAttackObservationDuration;
            closePresenceDuration = 0f;
            State = EliteEnemyDecisionState.Observe;
        }

        private void OnDied()
        {
            encounterActive = false;
            EnterInterruptedState(EliteEnemyDecisionState.Dead, false);
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
            EnterInterruptedState(EliteEnemyDecisionState.PlayerDefeated, true);
        }

        private void OnCombatStateReset()
        {
            CancelCurrentAttack();
            combatPlatform.ReleaseAttack(runtimeOwner);
            motor.ResetMotion();
            defeatNotified = false;
            closePresenceDuration = 0f;
            recoveryOpportunityAvailableAt = float.PositiveInfinity;
            State = encounterActive
                ? EliteEnemyDecisionState.Observe
                : EliteEnemyDecisionState.Dormant;
        }

        private void RefreshReadiness()
        {
            ready =
                internalReferencesValid &&
                sceneContextBound &&
                target != null &&
                targetHealth != null &&
                targetAttackRunner != null &&
                targetRollController != null &&
                combatPlatform != null &&
                runtimeOwner != null;
        }
    }
}
