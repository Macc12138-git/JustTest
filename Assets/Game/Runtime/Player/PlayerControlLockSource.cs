using System;

namespace JustTest.Game.Player
{
    [Flags]
    internal enum PlayerControlLockSource
    {
        None = 0,
        HitReaction = 1 << 0,
        Death = 1 << 1,
        External = 1 << 2
    }
}
