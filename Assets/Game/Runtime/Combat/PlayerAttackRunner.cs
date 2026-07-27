using System;
using JustTest.Game.Input;
using JustTest.Game.Player;
using JustTest.Game.Weapons;
using UnityEngine;

namespace JustTest.Game.Combat
{
    [DefaultExecutionOrder(-10)]
    public sealed class PlayerAttackRunner : MonoBehaviour
    {
        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private PlayerMovementController movementController;
        [SerializeField] private Hitbox2D hitbox;
        [SerializeField] private BoxCollider2D hitboxCollider;
        [SerializeField] private Transform attackAnchor;
        [SerializeField] private PlayerWeaponLoadout weaponLoadout;

        private readonly PlayerAttackComboState comboState = new PlayerAttackComboState();

        private AttackInstanceFactory attackFactory;
        private AttackTimeline timeline;
        private AttackInstance activeAttack;
        private AttackDefinition activeDefinition;
        private WeaponBasicComboDefinition activeCombo;
        private WeaponBasicComboStep activeStep;
        private Vector3 attackAnchorBaseLocalPosition;
        private Vector2 hitboxBaseSize;
        private HitResult lastHitResult;
        private int currentComboStepIndex = -1;
        private int attackDirection = 1;
        private bool hasLastHitResult;
        private bool resettingCombo;
        private bool transitioningCombo;
        private bool continueAfterTimelineTick;
        private bool ready;

        public event Action<HitResult> HitResolved;

        public AttackPhase Phase => timeline?.Phase ?? AttackPhase.Idle;

        public bool IsAttacking => timeline != null && timeline.IsRunning;

        public float PhaseProgress => timeline?.PhaseProgress ?? 0f;

        public AttackDefinition CurrentDefinition => activeDefinition;

        public int CurrentAttackInstanceId => activeAttack?.InstanceId ?? 0;

        public int CurrentComboStepIndex => currentComboStepIndex;

        public int CurrentComboStepCount => activeCombo?.StepCount ??
                                            weaponLoadout.ActiveWeapon?.BasicCombo?.StepCount ?? 0;

        public bool IsComboContinuationQueued => comboState.IsContinuationQueued;

        public int CurrentAttackDirection => attackDirection;

        public bool HasLastHitResult => hasLastHitResult;

        public HitResult LastHitResult => lastHitResult;

        private void Awake()
        {
            ready =
                inputReader != null &&
                movementController != null &&
                hitbox != null &&
                hitboxCollider != null &&
                hitboxCollider.isTrigger &&
                attackAnchor != null &&
                weaponLoadout != null;
            if (!ready)
            {
                Debug.LogError($"{nameof(PlayerAttackRunner)} is missing an Inspector reference.", this);
                enabled = false;
                return;
            }

            attackFactory = new AttackInstanceFactory();
            attackAnchorBaseLocalPosition = attackAnchor.localPosition;
            hitboxBaseSize = hitboxCollider.size;
            hitbox.EndAttack();
        }

        private void OnEnable()
        {
            if (ready)
            {
                weaponLoadout.ActiveWeaponChanged += OnActiveWeaponChanged;
            }
        }

        private void Update()
        {
            if (!ready)
            {
                return;
            }

            if (movementController.IsRolling && IsAttacking)
            {
                CancelAttack();
            }

            timeline?.Tick(Time.deltaTime);
            if (continueAfterTimelineTick)
            {
                continueAfterTimelineTick = false;
                TryStartPendingComboStep(Time.time);
                return;
            }

            if (IsAttacking)
            {
                TryQueueContinuation(Time.time);
                TryAdvanceQueuedCombo(Time.time);
                return;
            }

            TryStartBufferedAttack(Time.time);
        }

        private void FixedUpdate()
        {
            if (!ready || !IsAttacking || activeStep == null ||
                !activeStep.UsesMovement(Phase))
            {
                return;
            }

            if (movementController.IsRolling)
            {
                CancelAttack();
                return;
            }

            movementController.ApplyActionHorizontalVelocity(
                activeStep.ForwardSpeed * attackDirection);
        }

        private void OnDisable()
        {
            if (weaponLoadout != null)
            {
                weaponLoadout.ActiveWeaponChanged -= OnActiveWeaponChanged;
            }

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
            comboState.Reset();
            continueAfterTimelineTick = false;
            resettingCombo = true;
            if (timeline == null || !timeline.Cancel())
            {
                ClearActiveAttack();
            }
            resettingCombo = false;
        }

        private void TryStartBufferedAttack(float timestamp)
        {
            WeaponBasicComboDefinition combo = weaponLoadout.ActiveWeapon?.BasicCombo;
            if (combo == null || !combo.IsValid || !movementController.CanStartAction)
            {
                return;
            }

            int stepIndex = comboState.ResolveStartStep(timestamp);
            WeaponBasicComboStep step = combo.GetStep(stepIndex);
            if (step == null ||
                !inputReader.HasBufferedPrimaryAttack(timestamp, step.InputBufferDuration))
            {
                return;
            }

            inputReader.ConsumePrimaryAttack();
            StartStep(combo, stepIndex, step);
        }

        private void TryQueueContinuation(float timestamp)
        {
            if (comboState.IsContinuationQueued || activeCombo == null ||
                !activeCombo.TryGetNextStepIndex(currentComboStepIndex, out int nextStepIndex))
            {
                return;
            }

            WeaponBasicComboStep nextStep = activeCombo.GetStep(nextStepIndex);
            if (nextStep == null ||
                !inputReader.HasBufferedPrimaryAttack(timestamp, nextStep.InputBufferDuration))
            {
                return;
            }

            inputReader.ConsumePrimaryAttack();
            comboState.QueueContinuation();
        }

        private void TryAdvanceQueuedCombo(float timestamp)
        {
            if (!comboState.IsContinuationQueued ||
                Phase != AttackPhase.Recovery ||
                activeStep == null ||
                PhaseProgress < activeStep.ChainStartProgress)
            {
                return;
            }

            WeaponBasicComboDefinition combo = activeCombo;
            int completedStepIndex = currentComboStepIndex;
            comboState.MarkStepCompleted(
                completedStepIndex,
                combo.StepCount,
                combo.LoopAfterFinalStep,
                timestamp,
                combo.ComboResetDelay);

            transitioningCombo = true;
            timeline.Cancel();
            transitioningCombo = false;
            TryStartPendingComboStep(timestamp);
        }

        private void TryStartPendingComboStep(float timestamp)
        {
            WeaponBasicComboDefinition combo = weaponLoadout.ActiveWeapon?.BasicCombo;
            if (combo == null || !combo.IsValid || !movementController.CanStartAction)
            {
                comboState.Reset();
                return;
            }

            int nextStepIndex = comboState.ResolveStartStep(timestamp);
            WeaponBasicComboStep nextStep = combo.GetStep(nextStepIndex);
            if (nextStep == null)
            {
                comboState.Reset();
                return;
            }

            StartStep(combo, nextStepIndex, nextStep);
        }

        private void StartStep(
            WeaponBasicComboDefinition combo,
            int stepIndex,
            WeaponBasicComboStep step)
        {
            comboState.MarkStepStarted();
            activeCombo = combo;
            activeStep = step;
            currentComboStepIndex = stepIndex;
            activeDefinition = step.Attack;
            attackDirection = movementController.FacingDirection == -1 ? -1 : 1;

            Vector3 localPosition = attackAnchorBaseLocalPosition;
            localPosition.x = Mathf.Abs(step.HitboxOffset.x) * attackDirection;
            localPosition.y = step.HitboxOffset.y;
            attackAnchor.localPosition = localPosition;
            hitboxCollider.size = step.HitboxSize;

            activeAttack = attackFactory.Create(
                GetInstanceID(),
                CombatFaction.Player,
                activeDefinition.Damage,
                attackDirection,
                activeDefinition.HitReaction,
                activeDefinition.StatusApplication,
                activeDefinition.AllowFriendlyFire,
                activeDefinition.IgnorePostHitInvulnerability,
                activeDefinition.FeedbackTier);
            activeAttack.HitResolved += OnAttackHitResolved;

            timeline = new AttackTimeline(
                activeDefinition.WindupDuration,
                activeDefinition.ActiveDuration,
                activeDefinition.RecoveryDuration);
            timeline.PhaseChanged += OnPhaseChanged;
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
                CancelAttack();
                return;
            }

            if (nextPhase != AttackPhase.Idle)
            {
                return;
            }

            bool queuedContinuation = comboState.IsContinuationQueued;
            WeaponBasicComboDefinition completedCombo = activeCombo;
            int completedStepIndex = currentComboStepIndex;
            bool completedNaturally = !resettingCombo && !transitioningCombo;
            if (completedNaturally && completedCombo != null)
            {
                comboState.MarkStepCompleted(
                    completedStepIndex,
                    completedCombo.StepCount,
                    completedCombo.LoopAfterFinalStep,
                    Time.time,
                    completedCombo.ComboResetDelay);
            }

            ClearActiveAttack();
            continueAfterTimelineTick = completedNaturally && queuedContinuation;
        }

        private void OnAttackHitResolved(HitResult result)
        {
            lastHitResult = result;
            hasLastHitResult = true;
            HitResolved?.Invoke(result);
        }

        private void OnActiveWeaponChanged(int _, WeaponDefinition __)
        {
            CancelAttack();
        }

        private void ClearActiveAttack()
        {
            hitbox?.EndAttack();
            if (timeline != null)
            {
                timeline.PhaseChanged -= OnPhaseChanged;
                timeline = null;
            }

            if (activeAttack != null)
            {
                activeAttack.HitResolved -= OnAttackHitResolved;
                activeAttack = null;
            }

            if (attackAnchor != null)
            {
                attackAnchor.localPosition = attackAnchorBaseLocalPosition;
            }

            if (hitboxCollider != null)
            {
                hitboxCollider.size = hitboxBaseSize;
            }

            activeDefinition = null;
            activeCombo = null;
            activeStep = null;
            currentComboStepIndex = -1;
            attackDirection = 1;
        }
    }
}
