using System;

namespace JustTest.Game.Player
{
    [Flags]
    internal enum PlayerControlLockSource
    {
        None = 0,
        HitReaction = 1 << 0,
        Death = 1 << 1,
        External = 1 << 2,
        Qte = 1 << 3,
        WeaponSkill = 1 << 4
    }
}
