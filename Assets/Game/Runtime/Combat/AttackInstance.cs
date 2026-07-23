using System;
using System.Collections.Generic;

namespace JustTest.Game.Combat
{
    internal sealed class AttackInstance
    {
        private readonly HashSet<int> contactedTargetIds = new();
        private readonly int sourceId;
        private readonly CombatFaction sourceFaction;
        private readonly float damage;
        private readonly bool allowFriendlyFire;

        internal AttackInstance(
            int instanceId,
            int sourceId,
            CombatFaction sourceFaction,
            float damage,
            bool allowFriendlyFire)
        {
            InstanceId = instanceId;
            this.sourceId = sourceId;
            this.sourceFaction = sourceFaction;
            this.damage = damage;
            this.allowFriendlyFire = allowFriendlyFire;
        }

        internal event Action<HitResult> HitResolved;

        internal int InstanceId { get; }

        internal int ContactedTargetCount => contactedTargetIds.Count;

        internal HitResult TryHit(IHitTarget target)
        {
            if (target == null || target.TargetId == 0)
            {
                return Publish(new HitResult(
                    HitOutcome.InvalidRequest,
                    InstanceId,
                    0,
                    damage,
                    0f,
                    0f,
                    false));
            }

            int targetId = target.TargetId;
            if (!contactedTargetIds.Add(targetId))
            {
                return Publish(new HitResult(
                    HitOutcome.DuplicateHit,
                    InstanceId,
                    targetId,
                    damage,
                    0f,
                    target.CurrentHealth,
                    false));
            }

            HitRequest request = new HitRequest(
                InstanceId,
                sourceId,
                sourceFaction,
                damage,
                allowFriendlyFire);
            return Publish(target.ReceiveHit(request));
        }

        private HitResult Publish(HitResult result)
        {
            HitResolved?.Invoke(result);
            return result;
        }
    }
}
