namespace JustTest.Game.Weapons
{
    internal enum WeaponSkillCancelReason
    {
        None = 0,
        InsufficientEnergy = 1,
        InvalidDefinition = 2,
        PlayerHit = 3,
        PlayerDied = 4,
        CombatReset = 5,
        HitboxActivationFailed = 6,
        Disabled = 7
    }
}
