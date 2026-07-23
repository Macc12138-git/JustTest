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

            int instanceId = nextInstanceId;
            nextInstanceId = nextInstanceId == int.MaxValue ? 1 : nextInstanceId + 1;
            return new AttackInstance(
                instanceId,
                sourceId,
                sourceFaction,
                damage,
                allowFriendlyFire);
        }
    }
}
