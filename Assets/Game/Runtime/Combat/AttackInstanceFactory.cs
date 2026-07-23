using System;

namespace JustTest.Game.Combat
{
    internal sealed class AttackInstanceFactory
    {
        private int nextInstanceId = 1;

        internal AttackInstance Create(
            int sourceId,
            CombatFaction sourceFaction,
            float damage,
            bool allowFriendlyFire = false)
        {
            return Create(
                sourceId,
                sourceFaction,
                damage,
                1,
                default,
                allowFriendlyFire);
        }

        internal AttackInstance Create(
            int sourceId,
            CombatFaction sourceFaction,
            float damage,
            int attackDirection,
            HitReactionData reaction,
            bool allowFriendlyFire = false)
        {
            if (sourceId == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sourceId));
            }

            if (sourceFaction == CombatFaction.None)
            {
                throw new ArgumentOutOfRangeException(nameof(sourceFaction));
            }

            if (damage <= 0f || float.IsNaN(damage) || float.IsInfinity(damage))
            {
                throw new ArgumentOutOfRangeException(nameof(damage));
            }

            if (attackDirection != -1 && attackDirection != 1)
            {
                throw new ArgumentOutOfRangeException(nameof(attackDirection));
            }

            if (!reaction.IsValid)
            {
                throw new ArgumentOutOfRangeException(nameof(reaction));
            }

            int instanceId = nextInstanceId;
            nextInstanceId = nextInstanceId == int.MaxValue ? 1 : nextInstanceId + 1;
            return new AttackInstance(
                instanceId,
                sourceId,
                sourceFaction,
                damage,
                attackDirection,
                reaction,
                allowFriendlyFire);
        }
    }
}
