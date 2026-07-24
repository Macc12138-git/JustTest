using System;
using JustTest.Game.Combat;
using JustTest.Game.Input;
using JustTest.Game.Player;
using UnityEngine;

namespace JustTest.Game.Weapons
{
    [DefaultExecutionOrder(-25)]
    public sealed class PlayerWeaponQteController : MonoBehaviour
    {
        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private PlayerMovementController movementController;
        [SerializeField] private PlayerAttackRunner attackRunner;
        [SerializeField] private PlayerWeaponLoadout weaponLoadout;
        [SerializeField] private PlayerWeaponQteExecutor qteExecutor;
        [SerializeField] private CombatStatusEventChannel statusEventChannel;

        private readonly WeaponQteOpportunityState opportunity = new WeaponQteOpportunityState();
        private CombatStatusController currentTarget;
        private bool ready;

        internal event Action OpportunityChanged;

        internal event Action<WeaponQteSelection> QteSelected;

        internal bool HasOpportunity => opportunity.IsOpen;

        internal CombatStatusType OpportunityStatus => opportunity.StatusType;

        internal int OpportunityApplicationId => opportunity.ApplicationId;

        private void Awake()
        {
            ready =
                inputReader != null &&
                movementController != null &&
                attackRunner != null &&
                weaponLoadout != null &&
                qteExecutor != null &&
                statusEventChannel != null;
            if (ready)
            {
                return;
            }

            Debug.LogError($"{nameof(PlayerWeaponQteController)} is missing an Inspector reference.", this);
            enabled = false;
        }

        private void OnEnable()
        {
            if (!ready)
            {
                return;
            }

            statusEventChannel.StatusApplied += OnStatusApplied;
            statusEventChannel.StatusEnded += OnStatusEnded;
            qteExecutor.Completed += OnQteCompleted;
        }

        private void Update()
        {
            if (!ready || !weaponLoadout.IsReady)
            {
                return;
            }

            if (inputReader.ResetPressedThisFrame)
            {
                ClearOpportunity();
                weaponLoadout.ResetActiveSlot();
                return;
            }

            int requestedSlot = inputReader.WeaponSlotPressedThisFrame;
            if (requestedSlot < 0)
            {
                return;
            }

            if (opportunity.IsOpen)
            {
                TrySelectQteCandidate(requestedSlot);
                return;
            }

            if (movementController.CanStartAction && !attackRunner.IsAttacking)
            {
                weaponLoadout.TrySelectSlot(requestedSlot);
            }
        }

        private void OnDisable()
        {
            if (statusEventChannel != null)
            {
                statusEventChannel.StatusApplied -= OnStatusApplied;
                statusEventChannel.StatusEnded -= OnStatusEnded;
            }

            if (qteExecutor != null)
            {
                qteExecutor.Completed -= OnQteCompleted;
            }

            ClearOpportunity();
        }

        internal bool IsCandidate(int slotIndex)
        {
            return opportunity.IsCandidate(slotIndex);
        }

        private void OnStatusApplied(CombatStatusSignal signal)
        {
            int candidateMask = weaponLoadout.BuildQteCandidateMask(signal.StatusEvent.StatusType);
            bool opened = opportunity.Open(
                signal.Target.GetInstanceID(),
                signal.StatusEvent,
                candidateMask);
            currentTarget = opened ? signal.Target : null;
            OpportunityChanged?.Invoke();
        }

        private void OnStatusEnded(CombatStatusSignal signal)
        {
            if (!opportunity.TryEnd(
                    signal.Target.GetInstanceID(),
                    signal.StatusEvent.StatusType,
                    signal.StatusEvent.ApplicationId))
            {
                return;
            }

            currentTarget = null;
            OpportunityChanged?.Invoke();
        }

        private void TrySelectQteCandidate(int slotIndex)
        {
            if (!movementController.CanStartAction || !opportunity.IsCandidate(slotIndex))
            {
                return;
            }

            CombatStatusController selectedTarget = currentTarget;
            CombatStatusType selectedStatus = opportunity.StatusType;
            int selectedApplicationId = opportunity.ApplicationId;
            WeaponDefinition selectedWeapon = weaponLoadout.GetWeapon(slotIndex);
            WeaponQteSelection selection = new WeaponQteSelection(
                selectedTarget,
                selectedStatus,
                selectedApplicationId,
                slotIndex,
                selectedWeapon);

            if (!qteExecutor.TryStart(selection))
            {
                return;
            }

            if (!opportunity.TrySelect(slotIndex))
            {
                qteExecutor.CancelAction(WeaponQteCancelReason.InvalidRequest);
                return;
            }

            currentTarget = null;
            OpportunityChanged?.Invoke();
            QteSelected?.Invoke(selection);
        }

        private void OnQteCompleted(WeaponQteSelection selection)
        {
            if (!weaponLoadout.TrySelectSlot(selection.SlotIndex))
            {
                Debug.LogError(
                    $"{nameof(PlayerWeaponQteController)} could not equip the completed QTE weapon.",
                    this);
            }
        }

        private void ClearOpportunity()
        {
            if (!opportunity.Clear())
            {
                currentTarget = null;
                return;
            }

            currentTarget = null;
            OpportunityChanged?.Invoke();
        }
    }
}
