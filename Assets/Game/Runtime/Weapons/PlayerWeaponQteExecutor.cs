using System;
using JustTest.Game.Combat;
using JustTest.Game.Player;
using UnityEngine;

namespace JustTest.Game.Weapons
{
    [DefaultExecutionOrder(-20)]
    public sealed class PlayerWeaponQteExecutor : MonoBehaviour
    {
        [SerializeField] private PlayerMovementController movementController;
        [SerializeField] private PlayerAttackRunner attackRunner;
        [SerializeField] private DamageReceiver playerDamageReceiver;
        [SerializeField] private HealthComponent playerHealth;
        [SerializeField] private CombatReactionReceiver playerReaction;
        [SerializeField] private InvulnerabilityController playerInvulnerability;
        [SerializeField] private Rigidbody2D body;
        [SerializeField] private Transform qteAttackAnchor;
        [SerializeField] private Hitbox2D qteHitbox;
        [SerializeField] private BoxCollider2D qteHitboxCollider;

        private AttackInstanceFactory attackFactory;
        private AttackTimeline strikeTimeline;
        private AttackInstance activeAttack;
        private WeaponQteSelection activeSelection;
        private WeaponQteDefinition activeDefinition;
        private WeaponQteStrikeDefinition activeStrike;
        private Vector3 attackAnchorBaseLocalPosition;
        private Vector2 hitboxBaseSize;
        private float approachElapsed;
        private int attackDirection = 1;
        private int currentStrikeIndex = -1;
        private bool selectedTargetKilledByQte;
        private bool ready;

        internal event Action<WeaponQteSelection> Completed;

        internal event Action<WeaponQteSelection, WeaponQteCancelReason> Cancelled;

        internal event Action<HitResult> HitResolved;

        internal event Action ExecutionStateChanged;

        internal bool IsExecuting => Phase != WeaponQteExecutionPhase.Idle;

        internal WeaponQteExecutionPhase Phase { get; private set; }

        internal AttackPhase StrikePhase => strikeTimeline?.Phase ?? AttackPhase.Idle;

        internal float StrikePhaseProgress => strikeTimeline?.PhaseProgress ?? 0f;

        internal AttackDefinition CurrentAttackDefinition => activeStrike.Attack;

        internal int CurrentAttackDirection => attackDirection;

        internal int CurrentStrikeIndex => currentStrikeIndex;

        internal WeaponDefinition PendingWeapon => IsExecuting ? activeSelection.Weapon : null;

        internal int PendingSlotIndex => IsExecuting ? activeSelection.SlotIndex : -1;

        internal WeaponQteCancelReason LastCancelReason { get; private set; }

        private void Awake()
        {
            ready =
                movementController != null &&
                attackRunner != null &&
                playerDamageReceiver != null &&
                playerHealth != null &&
                playerReaction != null &&
                playerInvulnerability != null &&
                body != null &&
                qteAttackAnchor != null &&
                qteHitbox != null &&
                qteHitboxCollider != null &&
                qteHitboxCollider.isTrigger;
            if (!ready)
            {
                Debug.LogError($"{nameof(PlayerWeaponQteExecutor)} is missing an Inspector reference.", this);
                enabled = false;
                return;
            }

            attackFactory = new AttackInstanceFactory();
            attackAnchorBaseLocalPosition = qteAttackAnchor.localPosition;
            hitboxBaseSize = qteHitboxCollider.size;
            qteHitbox.EndAttack();
        }

        private void OnEnable()
        {
            if (!ready)
            {
                return;
            }

            playerReaction.ReactionStarted += OnPlayerReactionStarted;
            playerHealth.Died += OnPlayerDied;
            playerDamageReceiver.CombatStateReset += OnCombatStateReset;
        }

        private void Update()
        {
            if (!IsExecuting)
            {
                return;
            }

            if (!IsTargetAvailable())
            {
                if (selectedTargetKilledByQte)
                {
                    CompleteAction();
                }
                else
                {
                    CancelAction(WeaponQteCancelReason.TargetUnavailable);
                }

                return;
            }

            if (Phase == WeaponQteExecutionPhase.Strike)
            {
                strikeTimeline?.Tick(Time.deltaTime);
            }
        }

        private void FixedUpdate()
        {
            if (Phase == WeaponQteExecutionPhase.Approach)
            {
                TickApproach(Time.fixedDeltaTime);
            }
            else if (Phase == WeaponQteExecutionPhase.Strike)
            {
                ApplyStrikeMotion();
            }
        }

        private void OnDisable()
        {
            if (playerReaction != null)
            {
                playerReaction.ReactionStarted -= OnPlayerReactionStarted;
            }

            if (playerHealth != null)
            {
                playerHealth.Died -= OnPlayerDied;
            }

            if (playerDamageReceiver != null)
            {
                playerDamageReceiver.CombatStateReset -= OnCombatStateReset;
            }

            CancelAction(WeaponQteCancelReason.Disabled);
        }

        internal bool CanStart(in WeaponQteSelection selection)
        {
            WeaponQteDefinition definition = selection.Weapon?.QteAction;
            if (!ready ||
                IsExecuting ||
                playerHealth.IsDead ||
                !movementController.CanStartAction ||
                selection.Target == null ||
                !selection.Target.isActiveAndEnabled ||
                selection.Target.OwnerHealth == null ||
                selection.Target.OwnerHealth.IsDead ||
                definition == null ||
                !definition.IsValid ||
                !selection.Target.IsApplicationActive(selection.StatusType, selection.ApplicationId))
            {
                return false;
            }

            Vector2 targetPosition = selection.Target.transform.position;
            if (Vector2.Distance(body.position, targetPosition) > definition.MaximumTriggerDistance)
            {
                return false;
            }

            return !HasObstacle(body.position, targetPosition, definition.ObstacleLayers);
        }

        internal bool TryStart(in WeaponQteSelection selection)
        {
            if (!CanStart(selection))
            {
                LastCancelReason = WeaponQteCancelReason.InvalidRequest;
                return false;
            }

            activeSelection = selection;
            activeDefinition = selection.Weapon.QteAction;
            attackDirection = selection.Target.transform.position.x >= body.position.x ? 1 : -1;
            approachElapsed = 0f;
            currentStrikeIndex = -1;
            selectedTargetKilledByQte = false;
            LastCancelReason = WeaponQteCancelReason.None;

            attackRunner.CancelAttack();
            movementController.CancelRoll();
            movementController.SetControlLock(PlayerControlLockSource.Qte, true);
            movementController.ResetMotion();
            movementController.SetFacingDirection(attackDirection);
            if (activeDefinition.GrantInvulnerability)
            {
                playerInvulnerability.SetSource(InvulnerabilitySource.Qte, true);
            }

            Phase = WeaponQteExecutionPhase.Approach;
            ExecutionStateChanged?.Invoke();
            return true;
        }

        internal void CancelAction(WeaponQteCancelReason reason)
        {
            if (!IsExecuting)
            {
                return;
            }

            WeaponQteSelection cancelledSelection = activeSelection;
            LastCancelReason = reason;
            CleanupAction();
            movementController.ResetMotion();
            ExecutionStateChanged?.Invoke();
            Cancelled?.Invoke(cancelledSelection, reason);
        }

        private void TickApproach(float deltaTime)
        {
            if (!IsTargetAvailable())
            {
                CancelAction(WeaponQteCancelReason.TargetUnavailable);
                return;
            }

            approachElapsed += deltaTime;
            if (approachElapsed > activeDefinition.ApproachTimeout)
            {
                CancelAction(WeaponQteCancelReason.ApproachTimeout);
                return;
            }

            Vector2 destination = GetApproachDestination();
            if (HasObstacle(body.position, destination, activeDefinition.ObstacleLayers))
            {
                CancelAction(WeaponQteCancelReason.Obstructed);
                return;
            }

            body.velocity = Vector2.zero;
            Vector2 nextPosition = Vector2.MoveTowards(
                body.position,
                destination,
                activeDefinition.ApproachSpeed * deltaTime);
            body.MovePosition(nextPosition);
            if (Vector2.Distance(nextPosition, destination) <= activeDefinition.StoppingDistance)
            {
                movementController.ResetMotion();
                BeginStrike(0);
            }
        }

        private Vector2 GetApproachDestination()
        {
            Vector2 targetPosition = activeSelection.Target.transform.position;
            float destinationX =
                targetPosition.x - attackDirection * activeDefinition.HorizontalTargetDistance;
            float destinationY = activeDefinition.MotionMode == WeaponQteMotionMode.DirectApproach
                ? targetPosition.y + activeDefinition.VerticalTargetOffset
                : body.position.y;
            return new Vector2(destinationX, destinationY);
        }

        private void BeginStrike(int strikeIndex)
        {
            if (strikeIndex < 0 || strikeIndex >= activeDefinition.StrikeCount)
            {
                CompleteAction();
                return;
            }

            currentStrikeIndex = strikeIndex;
            activeStrike = activeDefinition.GetStrike(strikeIndex);
            AttackDefinition attackDefinition = activeStrike.Attack;

            Vector3 localPosition = attackAnchorBaseLocalPosition;
            localPosition.x = activeStrike.HitboxOffset.x * attackDirection;
            localPosition.y = activeStrike.HitboxOffset.y;
            qteAttackAnchor.localPosition = localPosition;
            qteHitboxCollider.size = activeStrike.HitboxSize;

            activeAttack = attackFactory.Create(
                GetInstanceID(),
                CombatFaction.Player,
                attackDefinition.Damage,
                attackDirection,
                attackDefinition.HitReaction,
                attackDefinition.StatusApplication,
                attackDefinition.AllowFriendlyFire,
                attackDefinition.IgnorePostHitInvulnerability,
                attackDefinition.FeedbackTier);
            activeAttack.HitResolved += OnAttackHitResolved;

            strikeTimeline = new AttackTimeline(
                attackDefinition.WindupDuration,
                attackDefinition.ActiveDuration,
                attackDefinition.RecoveryDuration);
            strikeTimeline.PhaseChanged += OnStrikePhaseChanged;
            Phase = WeaponQteExecutionPhase.Strike;
            strikeTimeline.Start();
        }

        private void OnStrikePhaseChanged(AttackPhase previousPhase, AttackPhase nextPhase)
        {
            if (previousPhase == AttackPhase.Active)
            {
                qteHitbox.EndAttack();
                bool finalStrike = currentStrikeIndex == activeDefinition.StrikeCount - 1;
                if (finalStrike && activeDefinition.GrantInvulnerability)
                {
                    playerInvulnerability.SetSource(InvulnerabilitySource.Qte, false);
                }
            }

            if (nextPhase == AttackPhase.Active && !qteHitbox.BeginAttack(activeAttack))
            {
                CancelAction(WeaponQteCancelReason.HitboxActivationFailed);
                return;
            }

            if (nextPhase != AttackPhase.Idle)
            {
                return;
            }

            EndCurrentStrike();
            if (selectedTargetKilledByQte)
            {
                CompleteAction();
                return;
            }

            BeginStrike(currentStrikeIndex + 1);
        }

        private void ApplyStrikeMotion()
        {
            Vector2 velocity = activeStrike.MovementVelocity;
            velocity.x = Mathf.Abs(velocity.x) * attackDirection;
            movementController.ApplyExternalVelocity(velocity);
        }

        private void OnAttackHitResolved(HitResult result)
        {
            if (result.KilledTarget &&
                activeSelection.Target != null &&
                activeSelection.Target.OwnerDamageReceiver != null &&
                result.TargetId == activeSelection.Target.OwnerDamageReceiver.GetInstanceID())
            {
                selectedTargetKilledByQte = true;
            }

            HitResolved?.Invoke(result);
        }

        private void EndCurrentStrike()
        {
            qteHitbox.EndAttack();
            if (strikeTimeline != null)
            {
                strikeTimeline.PhaseChanged -= OnStrikePhaseChanged;
                strikeTimeline = null;
            }

            if (activeAttack != null)
            {
                activeAttack.HitResolved -= OnAttackHitResolved;
                activeAttack = null;
            }
        }

        private void CompleteAction()
        {
            if (!IsExecuting)
            {
                return;
            }

            WeaponQteSelection completedSelection = activeSelection;
            Vector2 completionVelocity = activeDefinition.CompletionVelocity;
            completionVelocity.x = Mathf.Abs(completionVelocity.x) * attackDirection;
            CleanupAction();
            movementController.ApplyExternalVelocity(completionVelocity);
            ExecutionStateChanged?.Invoke();
            Completed?.Invoke(completedSelection);
        }

        private void CleanupAction()
        {
            EndCurrentStrike();
            playerInvulnerability.SetSource(InvulnerabilitySource.Qte, false);
            movementController.SetControlLock(PlayerControlLockSource.Qte, false);
            qteAttackAnchor.localPosition = attackAnchorBaseLocalPosition;
            qteHitboxCollider.size = hitboxBaseSize;
            activeSelection = default;
            activeDefinition = null;
            activeStrike = default;
            approachElapsed = 0f;
            attackDirection = 1;
            currentStrikeIndex = -1;
            selectedTargetKilledByQte = false;
            Phase = WeaponQteExecutionPhase.Idle;
        }

        private bool IsTargetAvailable()
        {
            return activeSelection.Target != null &&
                   activeSelection.Target.isActiveAndEnabled &&
                   activeSelection.Target.OwnerHealth != null &&
                   !activeSelection.Target.OwnerHealth.IsDead;
        }

        private static bool HasObstacle(Vector2 start, Vector2 end, LayerMask obstacleLayers)
        {
            return obstacleLayers.value != 0 &&
                   Physics2D.Linecast(start, end, obstacleLayers).collider != null;
        }

        private void OnPlayerReactionStarted(HitReactionData _)
        {
            CancelAction(WeaponQteCancelReason.PlayerHit);
        }

        private void OnPlayerDied()
        {
            CancelAction(WeaponQteCancelReason.PlayerDied);
        }

        private void OnCombatStateReset()
        {
            CancelAction(WeaponQteCancelReason.CombatReset);
        }
    }
}
