namespace JustTest.Game.UI
{
    internal enum CombatWeaponSlotVisualState
    {
        Empty = 0,
        Inactive = 1,
        Active = 2,
        QteCandidate = 3,
        QteExecuting = 4
    }

    internal sealed class CombatWeaponSlotStateResolver
    {
        internal CombatWeaponSlotVisualState Resolve(
            bool hasWeapon,
            bool isActive,
            bool isQteCandidate,
            bool isQteExecuting)
        {
            if (!hasWeapon)
            {
                return CombatWeaponSlotVisualState.Empty;
            }

            if (isQteExecuting)
            {
                return CombatWeaponSlotVisualState.QteExecuting;
            }

            if (isQteCandidate)
            {
                return CombatWeaponSlotVisualState.QteCandidate;
            }

            return isActive
                ? CombatWeaponSlotVisualState.Active
                : CombatWeaponSlotVisualState.Inactive;
        }
    }
}
