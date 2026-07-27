using System;
using JustTest.Game.Combat;
using JustTest.Game.Player;
using JustTest.Game.Run;
using UnityEngine;

namespace JustTest.Game.Enemies
{
    [DefaultExecutionOrder(-20)]
    public sealed class MeleeEnemyController2D : MonoBehaviour
    {
        [SerializeField] private MeleeEnemyConfig config;
        [SerializeField] private Transform target;
        [SerializeField] private HealthComponent targetHealth;
        [SerializeField] private PlayerAttackRunner targetAttackRunner;
        [SerializeField] private PlayerRollController targetRollController;
        [SerializeField] private HealthComponent health;
        [SerializeField] private DamageReceiver damageReceiver;
        [SerializeField] private CombatReactionReceiver reactionReceiver;
        [SerializeField] private CombatStatusController statusController;
        [SerializeField] private MeleeEnemyMotor2D motor;
        [SerializeField] private EnemyAttackRunner attackRunner;
        [SerializeField] private CombatPlatformController2D combatPlatform;
        [SerializeField] private AttackDefinition normalAttack;
        [SerializeField] private AttackDefinition heavyAttack;

        private float anchorX;
        private float nextDecisionAt;
        private float nextAttackAt;
        private float nextProbeAttackAt;
        private float nextHeavyAttackAt;
        private float repositionEndsAt;
        private float closePresenceDuration;
        private float openingAvailableAt = float.PositiveInfinity;
        private int repositionDirection;
        private AttackPhase lastTargetAttackPhase;
        private bool targetWasRolling;
        private bool encounterActive;
        private bool currentAttackIsHeavy;
        private bool interruptingAttack;
        private bool defeatNotified;
        private bool ready;

        internal event Action<MeleeEnemyController2D> Defeated;

        public MeleeEnemyDecisionState State { get; private set; } = MeleeEnemyDecisionState.Dormant;
        internal bool IsCurrentAttackHeavy => currentAttackIsHeavy && attackRunner.IsAttacking;

        private void Awake()
        {
            ready =
                config != null &&
                config.IsValid &&
                target != null &&
                targetHealth != null &&
                targetAttackRunner != null &&
                targetRollController != null &&
                health != null &&
                damageReceiver != null &&
                reactionReceiver != null &&
                statusController != null &&
                motor != null &&
                attackRunner != null &&
                combatPlatform != null &&
                normalAttack != null &&
                heavyAttack != null;
            if (!ready)
            {
                Debug.LogError($"{nameof(MeleeEnemyController2D)} is missing an Inspector reference.", this);
                enabled = false;
                return;
            }

            anchorX = transform.position.x;
            lastTargetAttackPhase = targetAttackRunner.Phase;
        }

        private void OnEnable()
        {
            if (!ready)
            {
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
                EnterInterruptedState(MeleeEnemyDecisionState.Dead, false);
                return;
            }

            if (targetHealth.IsDead)
            {
                EnterInterruptedState(MeleeEnemyDecisionState.PlayerDefeated, true);
                return;
            }

            if (reactionReceiver.IsReacting)
            {
                EnterInterruptedState(MeleeEnemyDecisionState.Controlled, false);
                return;
            }

            if (statusController.ActiveStatusCount > 0)
            {
                EnterInterruptedState(MeleeEnemyDecisionState.Controlled, true);
                return;
            }

            motor.SetControlEnabled(true);
            UpdateTargetSignals();
            if (attackRunner.IsAttacking)
            {
                motor.Stop();
                State = MeleeEnemyDecisionState.Attack;
                return;
            }

            if (State == MeleeEnemyDecisionState.Reposition && TickReposition())
            {
                return;
            }

            EvaluateOpportunity();
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

            combatPlatform?.ReleaseAttack(this);
            motor?.Stop();
        }

        internal void PrepareForEncounter()
        {
            encounterActive = false;
            State = MeleeEnemyDecisionState.Dormant;
        }

        internal void ActivateEncounter()
        {
            if (!ready || health.IsDead)
            {
                return;
            }

            encounterActive = true;
            defeatNotified = false;
            currentAttackIsHeavy = false;
            closePresenceDuration = 0f;
            anchorX = transform.position.x;
            nextDecisionAt = Time.time + config.InitialObservationDuration;
            nextProbeAttackAt = Time.time + config.MaximumPassiveDuration;
            nextHeavyAttackAt = Time.time + config.InitialHeavyAttackDelay;
            lastTargetAttackPhase = targetAttackRunner.Phase;
            targetWasRolling = targetRollController.IsRolling;
            motor.ResetMotion();
            FaceTarget();
            State = MeleeEnemyDecisionState.Observe;
        }

        internal void InterruptEncounter()
        {
            encounterActive = false;
            interruptingAttack = true;
            attackRunner?.CancelAttack();
            interruptingAttack = false;
            combatPlatform?.ReleaseAttack(this);
            currentAttackIsHeavy = false;
            if (motor != null)
            {
                motor.ResetMotion();
                motor.SetControlEnabled(false);
            }

            if (State != MeleeEnemyDecisionState.Dead)
            {
                State = MeleeEnemyDecisionState.Dormant;
            }
        }

        private void UpdateTargetSignals()
        {
            AttackPhase targetPhase = targetAttackRunner.Phase;
            if (targetPhase != lastTargetAttackPhase)
            {
                openingAvailableAt = targetPhase == AttackPhase.Recovery
                    ? Time.time + config.OpportunityReactionDelay
                    : float.PositiveInfinity;
                lastTargetAttackPhase = targetPhase;
            }

            bool targetRolling = targetRollController.IsRolling;
            if (targetWasRolling && !targetRolling)
            {
                nextDecisionAt = Mathf.Max(nextDecisionAt, Time.time + config.RollExitObservationDuration);
            }

            targetWasRolling = targetRolling;
        }

        private void EvaluateOpportunity()
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

            if (targetRollController.IsRolling ||
                targetAttackRunner.Phase == AttackPhase.Windup ||
                targetAttackRunner.Phase == AttackPhase.Active ||
                verticalDistance > config.AttackVerticalTolerance)
            {
                Observe();
                nextDecisionAt = Time.time + config.DecisionRetryInterval;
                return;
            }

            bool canAttack = Time.time >= nextAttackAt;
            bool punishOpportunity =
                targetAttackRunner.Phase == AttackPhase.Recovery &&
                Time.time >= openingAvailableAt;
            if (canAttack &&
                punishOpportunity &&
                horizontalDistance <= config.NormalAttackRange)
            {
                TryStartAttack(normalAttack, false, targetDirection);
                return;
            }

            bool heavyOpportunity =
                Time.time >= nextHeavyAttackAt &&
                closePresenceDuration >= config.HeavyOpportunityDuration;
            if (canAttack && heavyOpportunity)
            {
                TryStartAttack(heavyAttack, true, targetDirection);
                return;
            }

            bool probeOpportunity =
                Time.time >= nextProbeAttackAt &&
                horizontalDistance <= config.NormalAttackRange;
            if (canAttack && probeOpportunity)
            {
                TryStartAttack(normalAttack, false, targetDirection);
                return;
            }

            if (horizontalDistance > config.PreferredMaximumDistance)
            {
                BeginReposition(targetDirection);
                return;
            }

            if (horizontalDistance < config.PreferredMinimumDistance)
            {
                BeginReposition(-targetDirection);
                return;
            }

            Observe();
            State = canAttack
                ? MeleeEnemyDecisionState.Observe
                : MeleeEnemyDecisionState.WaitingForTurn;
            nextDecisionAt = Time.time + config.DecisionRetryInterval;
        }

        private void TryStartAttack(AttackDefinition definition, bool heavy, int direction)
        {
            motor.Stop();
            motor.Face(direction);
            if (!combatPlatform.TryAcquireAttack(this))
            {
                State = MeleeEnemyDecisionState.WaitingForTurn;
                nextDecisionAt = Time.time + config.AttackRequestRetryInterval;
                return;
            }

            currentAttackIsHeavy = heavy;
            if (attackRunner.TryStart(definition, direction))
            {
                State = MeleeEnemyDecisionState.Attack;
                return;
            }

            currentAttackIsHeavy = false;
            combatPlatform.ReleaseAttack(this);
            nextDecisionAt = Time.time + config.DecisionRetryInterval;
        }

        private void BeginReposition(int direction)
        {
            float distanceFromAnchor = transform.position.x - anchorX;
            bool movingPastLeftLimit = direction < 0 && distanceFromAnchor <= -config.MaximumRoamDistance;
            bool movingPastRightLimit = direction > 0 && distanceFromAnchor >= config.MaximumRoamDistance;
            if (direction == 0 || movingPastLeftLimit || movingPastRightLimit)
            {
                Observe();
                nextDecisionAt = Time.time + config.ObservationDuration;
                return;
            }

            repositionDirection = direction;
            repositionEndsAt = Time.time + config.RepositionDuration;
            motor.SetHorizontalDirection(direction);
            State = MeleeEnemyDecisionState.Reposition;
        }

        private bool TickReposition()
        {
            float distanceFromAnchor = transform.position.x - anchorX;
            bool reachedLimit =
                (repositionDirection < 0 && distanceFromAnchor <= -config.MaximumRoamDistance) ||
                (repositionDirection > 0 && distanceFromAnchor >= config.MaximumRoamDistance);
            if (Time.time < repositionEndsAt && !reachedLimit)
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
            FaceTarget();
            State = MeleeEnemyDecisionState.Observe;
        }

        private void FaceTarget()
        {
            if (target != null)
            {
                motor.Face(target.position.x >= transform.position.x ? 1 : -1);
            }
        }

        private void EnterInterruptedState(MeleeEnemyDecisionState state, bool allowBraking)
        {
            if (attackRunner.IsAttacking)
            {
                interruptingAttack = true;
                attackRunner.CancelAttack();
                interruptingAttack = false;
            }

            combatPlatform.ReleaseAttack(this);
            currentAttackIsHeavy = false;
            motor.Stop();
            motor.SetControlEnabled(allowBraking);
            State = state;
        }

        private void OnAttackEnded(bool completed)
        {
            combatPlatform.ReleaseAttack(this);
            bool wasHeavy = currentAttackIsHeavy;
            currentAttackIsHeavy = false;
            if (!encounterActive || !completed || interruptingAttack)
            {
                return;
            }

            nextAttackAt = Time.time + config.AttackCooldown;
            nextDecisionAt = Time.time + config.PostAttackObservationDuration;
            nextProbeAttackAt = Time.time + config.MaximumPassiveDuration;
            closePresenceDuration = 0f;
            if (wasHeavy)
            {
                nextHeavyAttackAt = Time.time + config.HeavyAttackCooldown;
            }

            int retreatDirection = target.position.x >= transform.position.x ? -1 : 1;
            BeginReposition(retreatDirection);
        }

        private void OnDied()
        {
            encounterActive = false;
            EnterInterruptedState(MeleeEnemyDecisionState.Dead, false);
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
            EnterInterruptedState(MeleeEnemyDecisionState.PlayerDefeated, true);
        }

        private void OnCombatStateReset()
        {
            interruptingAttack = true;
            attackRunner.CancelAttack();
            interruptingAttack = false;
            combatPlatform.ReleaseAttack(this);
            motor.ResetMotion();
            currentAttackIsHeavy = false;
            defeatNotified = false;
            closePresenceDuration = 0f;
            State = encounterActive
                ? MeleeEnemyDecisionState.Observe
                : MeleeEnemyDecisionState.Dormant;
        }
    }
}
