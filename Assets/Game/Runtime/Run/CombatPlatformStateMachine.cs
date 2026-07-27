namespace JustTest.Game.Run
{
    internal sealed class CombatPlatformStateMachine
    {
        internal CombatPlatformState State { get; private set; } = CombatPlatformState.Dormant;

        internal bool TryBeginAppearance()
        {
            if (State != CombatPlatformState.Dormant)
            {
                return false;
            }

            State = CombatPlatformState.Appearing;
            return true;
        }

        internal bool TryActivate()
        {
            if (State != CombatPlatformState.Appearing)
            {
                return false;
            }

            State = CombatPlatformState.Active;
            return true;
        }

        internal bool TryComplete()
        {
            if (State != CombatPlatformState.Active)
            {
                return false;
            }

            State = CombatPlatformState.Completed;
            return true;
        }

        internal bool TryInterrupt()
        {
            if (State == CombatPlatformState.Completed || State == CombatPlatformState.Interrupted)
            {
                return false;
            }

            State = CombatPlatformState.Interrupted;
            return true;
        }
    }
}
