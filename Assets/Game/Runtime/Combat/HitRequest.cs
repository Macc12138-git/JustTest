namespace JustTest.Game.Combat
{
    public readonly struct HitRequest
    {
        public HitRequest(
            int attackInstanceId,
            int sourceId,
            CombatFaction sourceFaction,
            float damage,
            bool allowFriendlyFire = false)
            : this(
                attackInstanceId,
                sourceId,
                sourceFaction,
                damage,
                1,
                default,
                default,
                allowFriendlyFire,
                false)
        {
        }

        public HitRequest(
            int attackInstanceId,
            int sourceId,
            CombatFaction sourceFaction,
            float damage,
            int attackDirection,
            HitReactionData reaction,
            bool allowFriendlyFire = false)
            : this(
                attackInstanceId,
                sourceId,
                sourceFaction,
                damage,
                attackDirection,
                reaction,
                default,
                allowFriendlyFire,
                false)
        {
        }

        public HitRequest(
            int attackInstanceId,
            int sourceId,
            CombatFaction sourceFaction,
            float damage,
            int attackDirection,
            HitReactionData reaction,
            CombatStatusApplication status,
            bool allowFriendlyFire = false,
            bool ignorePostHitInvulnerability = false)
        {
            AttackInstanceId = attackInstanceId;
            SourceId = sourceId;
            SourceFaction = sourceFaction;
            Damage = damage;
            AttackDirection = attackDirection;
            Reaction = reaction;
            Status = status;
            AllowFriendlyFire = allowFriendlyFire;
            IgnorePostHitInvulnerability = ignorePostHitInvulnerability;
        }

        public int AttackInstanceId { get; }

        public int SourceId { get; }

        public CombatFaction SourceFaction { get; }

        public float Damage { get; }

        public int AttackDirection { get; }

        public HitReactionData Reaction { get; }

        public CombatStatusApplication Status { get; }

        public bool AllowFriendlyFire { get; }

        public bool IgnorePostHitInvulnerability { get; }

        internal bool IsValid =>
            AttackInstanceId != 0 &&
            SourceId != 0 &&
            SourceFaction != CombatFaction.None &&
            (AttackDirection == -1 || AttackDirection == 1) &&
            Damage > 0f &&
            !float.IsNaN(Damage) &&
            !float.IsInfinity(Damage) &&
            Reaction.IsValid &&
            Status.IsValid;
    }
}
