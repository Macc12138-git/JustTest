namespace JustTest.Game.Combat
{
    internal sealed class DamageResolver
    {
        private readonly CombatFactionRules factionRules;

        internal DamageResolver(CombatFactionRules factionRules)
        {
            this.factionRules = factionRules;
        }

        internal HitResult Resolve(
            in HitRequest request,
            int targetId,
            CombatFaction targetFaction,
            HealthState targetHealth,
            bool targetInvulnerable)
        {
            float currentHealth = targetHealth?.CurrentHealth ?? 0f;
            if (!request.IsValid || targetId == 0 || targetFaction == CombatFaction.None || targetHealth == null)
            {
                return Rejected(HitOutcome.InvalidRequest, request, targetId, currentHealth);
            }

            if (targetHealth.IsDead)
            {
                return Rejected(HitOutcome.TargetDead, request, targetId, currentHealth);
            }

            if (!factionRules.CanDamage(
                    request.SourceFaction,
                    targetFaction,
                    request.AllowFriendlyFire))
            {
                return Rejected(HitOutcome.FriendlyFireBlocked, request, targetId, currentHealth);
            }

            if (targetInvulnerable)
            {
                return Rejected(HitOutcome.Invulnerable, request, targetId, currentHealth);
            }

            float appliedDamage = targetHealth.ApplyDamage(request.Damage);
            return new HitResult(
                HitOutcome.Applied,
                request.AttackInstanceId,
                targetId,
                request.Damage,
                appliedDamage,
                targetHealth.CurrentHealth,
                targetHealth.IsDead);
        }

        private HitResult Rejected(
            HitOutcome outcome,
            in HitRequest request,
            int targetId,
            float remainingHealth)
        {
            return new HitResult(
                outcome,
                request.AttackInstanceId,
                targetId,
                request.Damage,
                0f,
                remainingHealth,
                false);
        }
    }
}
