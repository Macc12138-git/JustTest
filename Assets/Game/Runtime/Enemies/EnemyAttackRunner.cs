using System;
using JustTest.Game.Combat;
using UnityEngine;

namespace JustTest.Game.Enemies
{
    public sealed class EnemyAttackRunner : MonoBehaviour
    {
        [SerializeField] private Hitbox2D hitbox;
        [SerializeField] private Transform attackAnchor;

        private AttackInstanceFactory attackFactory;
        private AttackTimeline timeline;
        private AttackInstance activeAttack;
        private AttackDefinition activeDefinition;
        private Vector3 attackAnchorBaseLocalPosition;
        private bool cancelling;
        private bool ready;

        public event Action<AttackPhase> PhaseChanged;
        public event Action<bool> AttackEnded;

        public AttackPhase Phase => timeline?.Phase ?? AttackPhase.Idle;
        public bool IsAttacking => timeline != null && timeline.IsRunning;
        public float PhaseProgress => timeline?.PhaseProgress ?? 0f;
        public AttackDefinition CurrentDefinition => activeDefinition;
        public int FacingDirection { get; private set; } = -1;

        private void Awake()
        {
            ready = hitbox != null && attackAnchor != null;
            if (!ready)
            {
                Debug.LogError($"{nameof(EnemyAttackRunner)} is missing an Inspector reference.", this);
                enabled = false;
                return;
            }

            attackFactory = new AttackInstanceFactory();
            attackAnchorBaseLocalPosition = attackAnchor.localPosition;
        }

        private void Update()
        {
            timeline?.Tick(Time.deltaTime);
        }

        private void OnDisable()
        {
            CancelAttack();
        }

        public bool TryStart(AttackDefinition definition, int direction)
        {
            if (!ready || definition == null || IsAttacking || (direction != -1 && direction != 1))
            {
                return false;
            }

            FacingDirection = direction;
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
                definition.IgnorePostHitInvulnerability);
            activeDefinition = definition;
            timeline = new AttackTimeline(
                definition.WindupDuration,
                definition.ActiveDuration,
                definition.RecoveryDuration);
            timeline.PhaseChanged += OnTimelinePhaseChanged;
            cancelling = false;
            return timeline.Start();
        }

        public void CancelAttack()
        {
            if (timeline == null)
            {
                hitbox?.EndAttack();
                activeAttack = null;
                activeDefinition = null;
                return;
            }

            if (timeline.IsRunning)
            {
                cancelling = true;
                timeline.Cancel();
            }
            else
            {
                ReleaseTimeline();
            }

            hitbox?.EndAttack();
        }

        private void OnTimelinePhaseChanged(AttackPhase previousPhase, AttackPhase nextPhase)
        {
            if (previousPhase == AttackPhase.Active)
            {
                hitbox.EndAttack();
            }

            if (nextPhase == AttackPhase.Active && !hitbox.BeginAttack(activeAttack))
            {
                Debug.LogError($"{nameof(EnemyAttackRunner)} could not activate its Hitbox2D.", this);
                cancelling = true;
                timeline.Cancel();
                return;
            }

            PhaseChanged?.Invoke(nextPhase);
            if (nextPhase != AttackPhase.Idle)
            {
                return;
            }

            bool completed = !cancelling;
            ReleaseTimeline();
            AttackEnded?.Invoke(completed);
        }

        private void ReleaseTimeline()
        {
            if (timeline != null)
            {
                timeline.PhaseChanged -= OnTimelinePhaseChanged;
                timeline = null;
            }

            activeAttack = null;
            activeDefinition = null;
            cancelling = false;
        }
    }
}
