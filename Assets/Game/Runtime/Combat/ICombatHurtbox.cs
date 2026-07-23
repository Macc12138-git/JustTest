namespace JustTest.Game.Combat
{
    internal interface ICombatHurtbox
    {
        bool IsAvailable { get; }

        HitResult ReceiveHit(AttackInstance attackInstance);
    }
}
