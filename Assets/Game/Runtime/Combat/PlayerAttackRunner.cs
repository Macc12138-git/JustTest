using System;
using JustTest.Game.Input;
using JustTest.Game.Player;
using UnityEngine;

namespace JustTest.Game.Combat
{
    public sealed class PlayerAttackRunner : MonoBehaviour
    {
        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private PlayerMovementController movementController;
        [SerializeField] private Hitbox2D hitbox;
        [SerializeField] private Transform attackAnchor;
        [SerializeField] private AttackDefinition attackDefinition;

        private AttackInstanceFactory attackFactory;
        private AttackTimeline timeline;
        private AttackInstance activeAttack;
        private Vector3 attackAnchorBaseLocalPosition;
        private HitResult lastHitResult;
        private bool hasLastHitResult;
        private bool ready;

        public event Action<HitResult> HitResolved;

        public AttackPhase Phase => timeline?.Phase ?? AttackPhase.Idle;

        public bool IsAttacking => timeline != null && timeline.IsRunning;

        public int CurrentAttackInstanceId => activeAttack?.InstanceId ?? 0;

        public bool HasLastHitResult => hasLastHitResult;

        public HitResult LastHitResult => lastHitResult;

        private void Awake()
        {
            ready =
                inputReader != null &&
                movementController != null &&
                hitbox != null &&
                attackAnchor != null &&
                attackDefinition != null;
            if (!ready)
            {
                Debug.LogError($"{nameof(PlayerAttackRunner)} is missing an Inspector reference.", this);
                enabled = false;
                return;
            }

            attackFactory = new AttackInstanceFactory();
            timeline = new AttackTimeline(
                attackDefinition.WindupDuration,
                attackDefinition.ActiveDuration,
                attackDefinition.RecoveryDuration);
            timeline.PhaseChanged += OnPhaseChanged;
            attackAnchorBaseLocalPosition = attackAnchor.localPosition;
            UpdateAttackAnchorFacing();
        }

        private void Update()
        {
            if (!ready)
            {
                return;
            }

            if (movementController.IsRolling && timeline.IsRunning)
            {
                CancelAttack();
            }

            timeline.Tick(Time.deltaTime);
            TryStartBufferedAttack(Time.time);
        }

        private void OnDisable()
        {
            CancelAttack();
            hitbox?.EndAttack();
        }

        private void OnDestroy()
        {
            if (timeline != null)
            {
                timeline.PhaseChanged -= OnPhaseChanged;
            }

            ClearActiveAttack();
        }

        public void CancelAttack()
        {
            timeline?.Cancel();
        }

        private void TryStartBufferedAttack(float timestamp)
        {
            if (timeline.IsRunning ||
                !movementController.CanStartAction ||
                !inputReader.HasBufferedPrimaryAttack(
                    timestamp,
                    attackDefinition.InputBufferDuration))
            {
                return;
            }

            inputReader.ConsumePrimaryAttack();
            UpdateAttackAnchorFacing();
            activeAttack = attackFactory.Create(
                GetInstanceID(),
                CombatFaction.Player,
                attackDefinition.Damage,
                movementController.FacingDirection,
                attackDefinition.HitReaction,
                attackDefinition.AllowFriendlyFire);
            activeAttack.HitResolved += OnAttackHitResolved;
            timeline.Start();
        }

        private void OnPhaseChanged(AttackPhase previousPhase, AttackPhase nextPhase)
        {
            if (previousPhase == AttackPhase.Active)
            {
                hitbox.EndAttack();
            }

            if (nextPhase == AttackPhase.Active && !hitbox.BeginAttack(activeAttack))
            {
                Debug.LogError($"{nameof(PlayerAttackRunner)} could not activate its Hitbox2D.", this);
                timeline.Cancel();
                return;
            }

            if (nextPhase == AttackPhase.Idle)
            {
                ClearActiveAttack();
            }
        }

        private void OnAttackHitResolved(HitResult result)
        {
            lastHitResult = result;
            hasLastHitResult = true;
            HitResolved?.Invoke(result);
        }

        private void UpdateAttackAnchorFacing()
        {
            int facingDirection = movementController.FacingDirection == 0
                ? 1
                : movementController.FacingDirection;
            Vector3 localPosition = attackAnchorBaseLocalPosition;
            localPosition.x = Mathf.Abs(localPosition.x) * facingDirection;
            attackAnchor.localPosition = localPosition;
        }

        private void ClearActiveAttack()
        {
            if (activeAttack == null)
            {
                return;
            }

            activeAttack.HitResolved -= OnAttackHitResolved;
            activeAttack = null;
        }
    }
}
