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
        {
            AttackInstanceId = attackInstanceId;
            SourceId = sourceId;
            SourceFaction = sourceFaction;
            Damage = damage;
            AllowFriendlyFire = allowFriendlyFire;
        }

        public int AttackInstanceId { get; }

        public int SourceId { get; }

        public CombatFaction SourceFaction { get; }

        public float Damage { get; }

        public bool AllowFriendlyFire { get; }

        internal bool IsValid =>
            AttackInstanceId != 0 &&
            SourceId != 0 &&
            SourceFaction != CombatFaction.None &&
            Damage > 0f &&
            !float.IsNaN(Damage) &&
            !float.IsInfinity(Damage);
    }
}
