namespace JustTest.Game.Combat
{
    public readonly struct HitResult
    {
        internal HitResult(
            HitOutcome outcome,
            int attackInstanceId,
            int targetId,
            float requestedDamage,
            float appliedDamage,
            float remainingHealth,
            bool killedTarget)
        {
            Outcome = outcome;
            AttackInstanceId = attackInstanceId;
            TargetId = targetId;
            RequestedDamage = requestedDamage;
            AppliedDamage = appliedDamage;
            RemainingHealth = remainingHealth;
            KilledTarget = killedTarget;
        }

        public HitOutcome Outcome { get; }

        public int AttackInstanceId { get; }

        public int TargetId { get; }

        public float RequestedDamage { get; }

        public float AppliedDamage { get; }

        public float RemainingHealth { get; }

        public bool KilledTarget { get; }

        public bool WasApplied => Outcome == HitOutcome.Applied;
    }
}
