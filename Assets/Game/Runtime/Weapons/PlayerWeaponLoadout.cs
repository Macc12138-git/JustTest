using System;
using UnityEngine;

namespace JustTest.Game.Weapons
{
    [DefaultExecutionOrder(-100)]
    public sealed class PlayerWeaponLoadout : MonoBehaviour
    {
        [SerializeField] private WeaponLoadoutDefinition definition;

        private readonly WeaponLoadoutState state = new WeaponLoadoutState();
        private bool ready;

        internal event Action<int, WeaponDefinition> ActiveWeaponChanged;

        internal int ActiveSlotIndex => state.ActiveSlotIndex;

        internal WeaponDefinition ActiveWeapon => state.ActiveWeapon;

        internal bool IsReady => ready;

        private void Awake()
        {
            ready =
                definition != null &&
                definition.IsValid &&
                state.Initialize(definition.StartingWeapons, definition.DefaultWeapon);
            if (ready)
            {
                return;
            }

            Debug.LogError($"{nameof(PlayerWeaponLoadout)} has an invalid loadout definition.", this);
            enabled = false;
        }

        internal WeaponDefinition GetWeapon(int slotIndex)
        {
            return state.GetWeapon(slotIndex);
        }

        internal int BuildQteCandidateMask(JustTest.Game.Combat.CombatStatusType statusType)
        {
            return state.BuildQteCandidateMask(statusType);
        }

        internal bool TrySelectSlot(int slotIndex)
        {
            if (!ready || !state.TrySelectSlot(slotIndex))
            {
                return false;
            }

            ActiveWeaponChanged?.Invoke(state.ActiveSlotIndex, state.ActiveWeapon);
            return true;
        }

        internal bool ResetActiveSlot()
        {
            if (!ready || !state.ResetActiveSlot())
            {
                return false;
            }

            ActiveWeaponChanged?.Invoke(state.ActiveSlotIndex, state.ActiveWeapon);
            return true;
        }
    }
}
