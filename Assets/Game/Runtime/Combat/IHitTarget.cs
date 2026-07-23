namespace JustTest.Game.Combat
{
    internal interface IHitTarget
    {
        int TargetId { get; }

        float CurrentHealth { get; }

        HitResult ReceiveHit(in HitRequest request);
    }
}
