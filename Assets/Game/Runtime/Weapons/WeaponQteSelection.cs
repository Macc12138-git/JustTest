using JustTest.Game.Combat;

namespace JustTest.Game.Weapons
{
    internal readonly struct WeaponQteSelection
    {
        internal WeaponQteSelection(
            CombatStatusController target,
            CombatStatusType statusType,
            int applicationId,
            int slotIndex,
            WeaponDefinition weapon)
        {
            Target = target;
            StatusType = statusType;
            ApplicationId = applicationId;
            SlotIndex = slotIndex;
            Weapon = weapon;
        }

        internal CombatStatusController Target { get; }

        internal CombatStatusType StatusType { get; }

        internal int ApplicationId { get; }

        internal int SlotIndex { get; }

        internal WeaponDefinition Weapon { get; }
    }
}
