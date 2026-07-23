namespace JustTest.Game.Player
{
    internal sealed class PlayerControlLockState
    {
        internal PlayerControlLockSource ActiveSources { get; private set; }

        internal bool IsLocked => ActiveSources != PlayerControlLockSource.None;

        internal bool Set(PlayerControlLockSource source, bool active)
        {
            if (source == PlayerControlLockSource.None)
            {
                return false;
            }

            PlayerControlLockSource previous = ActiveSources;
            ActiveSources = active
                ? ActiveSources | source
                : ActiveSources & ~source;
            return previous != ActiveSources;
        }

        internal bool Clear()
        {
            if (ActiveSources == PlayerControlLockSource.None)
            {
                return false;
            }

            ActiveSources = PlayerControlLockSource.None;
            return true;
        }
    }
}
