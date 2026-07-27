using System;
using UnityEngine;

namespace JustTest.Game.Combat
{
    public sealed class CombatSandboxAttackDriver : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Transform attackAnchor;
        [SerializeField] private Hitbox2D hitbox;
        [SerializeField] private AttackDefinition normalAttack;
        [SerializeField] private AttackDefinition heavyAttack;
        [SerializeField] private CombatDebugConfig config;

        private AttackInstanceFactory attackFactory;
        private AttackTimeline timeline;
        private AttackInstance activeAttack;
        private Vector3 attackAnchorBaseLocalPosition;
        private bool ready;

        private void Awake()
        {
            ready =
                target != null &&
                attackAnchor != null &&
                hitbox != null &&
                normalAttack != null &&
                heavyAttack != null &&
                config != null;
            if (!ready)
            {
                Debug.LogError($"{nameof(CombatSandboxAttackDriver)} is missing an Inspector reference.", this);
                enabled = false;
                return;
            }

            attackFactory = new AttackInstanceFactory();
            attackAnchorBaseLocalPosition = attackAnchor.localPosition;
        }

        private void Update()
        {
            if (!ready)
            {
                return;
            }

            timeline?.Tick(Time.deltaTime);
            if (timeline != null && timeline.IsRunning)
            {
                return;
            }

            if (UnityEngine.Input.GetKeyDown(config.NormalEnemyAttackKey))
            {
                StartAttack(normalAttack);
            }
            else if (UnityEngine.Input.GetKeyDown(config.HeavyEnemyAttackKey))
            {
                StartAttack(heavyAttack);
            }
        }

        private void OnDisable()
        {
            CancelAttack();
        }

        private void StartAttack(AttackDefinition definition)
        {
            ReleaseTimeline();

            int direction = target.position.x >= transform.position.x ? 1 : -1;
            Vector3 localPosition = attackAnchorBaseLocalPosition;
            localPosition.x = Mathf.Abs(localPosition.x) * direction;
            attackAnchor.localPosition = localPosition;

            activeAttack = attackFactory.Create(
                GetInstanceID(),
                CombatFaction.Enemy,
                definition.Damage,
                direction,
                definition.HitReaction,
                definition.StatusApplication,
                definition.AllowFriendlyFire,
                definition.IgnorePostHitInvulnerability,
                definition.FeedbackTier);
            timeline = new AttackTimeline(
                definition.WindupDuration,
                definition.ActiveDuration,
                definition.RecoveryDuration);
            timeline.PhaseChanged += OnPhaseChanged;
            timeline.Start();
        }

        private void CancelAttack()
        {
            if (timeline != null && timeline.IsRunning)
            {
                timeline.Cancel();
            }

            hitbox?.EndAttack();
            ReleaseTimeline();
        }

        private void OnPhaseChanged(AttackPhase previousPhase, AttackPhase nextPhase)
        {
            if (previousPhase == AttackPhase.Active)
            {
                hitbox.EndAttack();
            }

            if (nextPhase == AttackPhase.Active && !hitbox.BeginAttack(activeAttack))
            {
                Debug.LogError($"{nameof(CombatSandboxAttackDriver)} could not activate its Hitbox2D.", this);
                timeline.Cancel();
            }

            if (nextPhase == AttackPhase.Idle)
            {
                activeAttack = null;
            }
        }

        private void ReleaseTimeline()
        {
            if (timeline != null)
            {
                timeline.PhaseChanged -= OnPhaseChanged;
                timeline = null;
            }

            activeAttack = null;
        }
    }
}
