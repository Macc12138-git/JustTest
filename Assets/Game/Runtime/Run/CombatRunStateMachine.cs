namespace JustTest.Game.Run
{
    internal sealed class CombatRunStateMachine
    {
        internal CombatRunState State { get; private set; } = CombatRunState.Active;

        internal bool TryMarkPlayerDefeated()
        {
            if (State != CombatRunState.Active)
            {
                return false;
            }

            State = CombatRunState.PlayerDefeated;
            return true;
        }

        internal bool TryBeginRestart(bool allowWhileActive)
        {
            if (State == CombatRunState.Restarting ||
                (State == CombatRunState.Active && !allowWhileActive))
            {
                return false;
            }

            State = CombatRunState.Restarting;
            return true;
        }
    }
}
