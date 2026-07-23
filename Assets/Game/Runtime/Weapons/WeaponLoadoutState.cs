using System.Collections.Generic;
using JustTest.Game.Combat;

namespace JustTest.Game.Weapons
{
    internal sealed class WeaponLoadoutState
    {
        internal const int Capacity = 3;

        private readonly WeaponDefinition[] slots = new WeaponDefinition[Capacity];
        private int initialActiveSlotIndex = -1;

        internal int ActiveSlotIndex { get; private set; } = -1;

        internal WeaponDefinition ActiveWeapon => GetWeapon(ActiveSlotIndex);

        internal bool Initialize(
            IReadOnlyList<WeaponDefinition> startingWeapons,
            WeaponDefinition defaultWeapon)
        {
            for (int index = 0; index < slots.Length; index++)
            {
                slots[index] = startingWeapons != null && index < startingWeapons.Count
                    ? startingWeapons[index]
                    : null;
            }

            int firstWeaponSlot = FindFirstWeaponSlot();
            if (firstWeaponSlot < 0)
            {
                if (defaultWeapon == null)
                {
                    ActiveSlotIndex = -1;
                    initialActiveSlotIndex = -1;
                    return false;
                }

                slots[0] = defaultWeapon;
                firstWeaponSlot = 0;
            }

            ActiveSlotIndex = firstWeaponSlot;
            initialActiveSlotIndex = firstWeaponSlot;
            return true;
        }

        internal WeaponDefinition GetWeapon(int slotIndex)
        {
            return IsValidSlot(slotIndex) ? slots[slotIndex] : null;
        }

        internal bool TrySelectSlot(int slotIndex)
        {
            if (!IsValidSlot(slotIndex) || slots[slotIndex] == null || slotIndex == ActiveSlotIndex)
            {
                return false;
            }

            ActiveSlotIndex = slotIndex;
            return true;
        }

        internal bool ResetActiveSlot()
        {
            if (initialActiveSlotIndex < 0 || ActiveSlotIndex == initialActiveSlotIndex)
            {
                return false;
            }

            ActiveSlotIndex = initialActiveSlotIndex;
            return true;
        }

        internal int BuildQteCandidateMask(CombatStatusType statusType)
        {
            if (statusType == CombatStatusType.None)
            {
                return 0;
            }

            int candidateMask = 0;
            for (int index = 0; index < slots.Length; index++)
            {
                WeaponDefinition weapon = slots[index];
                if (index != ActiveSlotIndex &&
                    weapon != null &&
                    weapon.QteTriggerStatus == statusType)
                {
                    candidateMask |= 1 << index;
                }
            }

            return candidateMask;
        }

        private int FindFirstWeaponSlot()
        {
            for (int index = 0; index < slots.Length; index++)
            {
                if (slots[index] != null)
                {
                    return index;
                }
            }

            return -1;
        }

        private static bool IsValidSlot(int slotIndex)
        {
            return slotIndex >= 0 && slotIndex < Capacity;
        }
    }
}
