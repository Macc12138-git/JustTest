namespace JustTest.Game.Combat
{
    internal sealed class CombatFactionRules
    {
        internal bool CanDamage(
            CombatFaction source,
            CombatFaction target,
            bool allowFriendlyFire)
        {
            if (source == CombatFaction.None || target == CombatFaction.None)
            {
                return false;
            }

            if (allowFriendlyFire || source == CombatFaction.Neutral || target == CombatFaction.Neutral)
            {
                return true;
            }

            return source != target;
        }
    }
}
