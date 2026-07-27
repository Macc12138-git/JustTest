using JustTest.Game.Combat;
using UnityEngine;

namespace JustTest.Game.Enemies
{
    [DefaultExecutionOrder(-20)]
    public sealed class MeleeEnemyController2D : MonoBehaviour
    {
        [SerializeField] private MeleeEnemyConfig config;
        [SerializeField] private Transform target;
        [SerializeField] private HealthComponent targetHealth;
        [SerializeField] private HealthComponent health;
        [SerializeField] private DamageReceiver damageReceiver;
        [SerializeField] private CombatReactionReceiver reactionReceiver;
        [SerializeField] private CombatStatusController statusController;
        [SerializeField] private MeleeEnemyMotor2D motor;
        [SerializeField] private MeleeEnemyPathFollower2D pathFollower;
        [SerializeField] private EnemyAttackRunner attackRunner;
        [SerializeField] private AttackDefinition[] attackSequence;

        private int attackSequenceIndex;
        private float nextAttackTime;
        private bool ready;

        public MeleeEnemyDecisionState State { get; private set; } = MeleeEnemyDecisionState.Idle;

        private void Awake()
        {
            ready =
                config != null &&
                target != null &&
                targetHealth != null &&
                health != null &&
                damageReceiver != null &&
                reactionReceiver != null &&
                statusController != null &&
                motor != null &&
                pathFollower != null &&
                attackRunner != null &&
                attackSequence != null &&
                attackSequence.Length > 0 &&
                HasCompleteAttackSequence();
            if (ready)
            {
                return;
            }

            Debug.LogError($"{nameof(MeleeEnemyController2D)} is missing an Inspector reference.", this);
            enabled = false;
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
            if (!ready)
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
            if (attackRunner.IsAttacking)
            {
                pathFollower.Stop();
                motor.SetHorizontalDirection(0f);
                State = MeleeEnemyDecisionState.Attack;
                return;
            }

            if (Time.time < nextAttackTime)
            {
                pathFollower.Stop();
                motor.SetHorizontalDirection(0f);
                State = MeleeEnemyDecisionState.AttackCooldown;
                return;
            }

            Vector2 targetPosition = target.position;
            Vector2 offset = targetPosition - (Vector2)transform.position;
            if (offset.magnitude > config.DetectionRange)
            {
                pathFollower.Stop();
                motor.SetHorizontalDirection(0f);
                State = MeleeEnemyDecisionState.Idle;
                return;
            }

            bool samePlatform = pathFollower.IsOnSamePlatform(targetPosition);
            if (samePlatform &&
                Mathf.Abs(offset.x) <= config.AttackRange &&
                Mathf.Abs(offset.y) <= config.AttackVerticalTolerance)
            {
                TryStartAttack(offset.x >= 0f ? 1 : -1);
                return;
            }

            bool following = pathFollower.Tick(targetPosition);
            State = following
                ? (samePlatform ? MeleeEnemyDecisionState.DirectChase : MeleeEnemyDecisionState.PathChase)
                : MeleeEnemyDecisionState.Idle;
        }

        private void OnDisable()
        {
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

            pathFollower?.Stop();
        }

        private bool HasCompleteAttackSequence()
        {
            for (int index = 0; index < attackSequence.Length; index++)
            {
                if (attackSequence[index] == null)
                {
                    return false;
                }
            }

            return true;
        }

        private void TryStartAttack(int direction)
        {
            pathFollower.Stop();
            motor.SetHorizontalDirection(0f);
            motor.Face(direction);
            if (attackRunner.TryStart(attackSequence[attackSequenceIndex], direction))
            {
                State = MeleeEnemyDecisionState.Attack;
            }
        }

        private void EnterInterruptedState(MeleeEnemyDecisionState state, bool allowBraking)
        {
            if (attackRunner.IsAttacking)
            {
                attackRunner.CancelAttack();
            }

            pathFollower.Stop();
            motor.SetControlEnabled(allowBraking);
            if (allowBraking)
            {
                motor.SetHorizontalDirection(0f);
            }
            State = state;
        }

        private void OnAttackEnded(bool completed)
        {
            if (!completed)
            {
                return;
            }

            attackSequenceIndex = (attackSequenceIndex + 1) % attackSequence.Length;
            nextAttackTime = Time.time + config.AttackCooldown;
        }

        private void OnDied()
        {
            EnterInterruptedState(MeleeEnemyDecisionState.Dead, false);
        }

        private void OnTargetDied()
        {
            EnterInterruptedState(MeleeEnemyDecisionState.PlayerDefeated, true);
        }

        private void OnCombatStateReset()
        {
            attackRunner.CancelAttack();
            pathFollower.ResetPath();
            motor.ResetMotion();
            attackSequenceIndex = 0;
            nextAttackTime = 0f;
            State = MeleeEnemyDecisionState.Idle;
        }
    }
}
