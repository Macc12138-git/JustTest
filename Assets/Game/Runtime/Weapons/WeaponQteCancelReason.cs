namespace JustTest.Game.Weapons
{
    internal enum WeaponQteCancelReason
    {
        None = 0,
        InvalidRequest = 1,
        TargetUnavailable = 2,
        Obstructed = 3,
        ApproachTimeout = 4,
        PlayerHit = 5,
        PlayerDied = 6,
        CombatReset = 7,
        HitboxActivationFailed = 8,
        Disabled = 9
    }
}
