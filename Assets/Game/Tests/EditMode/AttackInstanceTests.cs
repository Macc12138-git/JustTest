using JustTest.Game.Combat;
using NUnit.Framework;
using UnityEngine;

namespace JustTest.Game.Tests
{
    public sealed class AttackInstanceTests
    {
        private AttackInstanceFactory factory;

        [SetUp]
        public void SetUp()
        {
            factory = new AttackInstanceFactory();
        }

        [Test]
        public void TryHit_ProcessesSameTargetOnlyOnce()
        {
            AttackInstance attack = CreateAttack();
            FakeHitTarget target = new FakeHitTarget(200, 100f);

            HitResult firstResult = attack.TryHit(target);
            HitResult secondResult = attack.TryHit(target);

            Assert.That(firstResult.Outcome, Is.EqualTo(HitOutcome.Applied));
            Assert.That(secondResult.Outcome, Is.EqualTo(HitOutcome.DuplicateHit));
            Assert.That(target.ReceiveCount, Is.EqualTo(1));
            Assert.That(attack.ContactedTargetCount, Is.EqualTo(1));
        }

        [Test]
        public void TryHit_UsesTargetIdToDeduplicateMultipleHurtboxes()
        {
            AttackInstance attack = CreateAttack();
            FakeHitTarget firstHurtboxTarget = new FakeHitTarget(200, 100f);
            FakeHitTarget secondHurtboxTarget = new FakeHitTarget(200, 75f);

            HitResult firstResult = attack.TryHit(firstHurtboxTarget);
            HitResult secondResult = attack.TryHit(secondHurtboxTarget);

            Assert.That(firstResult.Outcome, Is.EqualTo(HitOutcome.Applied));
            Assert.That(secondResult.Outcome, Is.EqualTo(HitOutcome.DuplicateHit));
            Assert.That(firstHurtboxTarget.ReceiveCount, Is.EqualTo(1));
            Assert.That(secondHurtboxTarget.ReceiveCount, Is.Zero);
        }

        [Test]
        public void TryHit_ProcessesDifferentTargetsIndependently()
        {
            AttackInstance attack = CreateAttack();
            FakeHitTarget firstTarget = new FakeHitTarget(200, 100f);
            FakeHitTarget secondTarget = new FakeHitTarget(201, 100f);

            HitResult firstResult = attack.TryHit(firstTarget);
            HitResult secondResult = attack.TryHit(secondTarget);

            Assert.That(firstResult.Outcome, Is.EqualTo(HitOutcome.Applied));
            Assert.That(secondResult.Outcome, Is.EqualTo(HitOutcome.Applied));
            Assert.That(attack.ContactedTargetCount, Is.EqualTo(2));
        }

        [Test]
        public void TryHit_DoesNotRetryRejectedContactWithinSameAttack()
        {
            AttackInstance attack = CreateAttack();
            FakeHitTarget target = new FakeHitTarget(200, 100f, HitOutcome.Invulnerable);

            HitResult firstResult = attack.TryHit(target);
            target.Outcome = HitOutcome.Applied;
            HitResult secondResult = attack.TryHit(target);

            Assert.That(firstResult.Outcome, Is.EqualTo(HitOutcome.Invulnerable));
            Assert.That(secondResult.Outcome, Is.EqualTo(HitOutcome.DuplicateHit));
            Assert.That(target.ReceiveCount, Is.EqualTo(1));
        }

        [Test]
        public void NewAttackInstance_CanHitPreviousTargetAgain()
        {
            FakeHitTarget target = new FakeHitTarget(200, 100f);
            AttackInstance firstAttack = CreateAttack();
            AttackInstance secondAttack = CreateAttack();

            HitResult firstResult = firstAttack.TryHit(target);
            HitResult secondResult = secondAttack.TryHit(target);

            Assert.That(firstResult.Outcome, Is.EqualTo(HitOutcome.Applied));
            Assert.That(secondResult.Outcome, Is.EqualTo(HitOutcome.Applied));
            Assert.That(target.ReceiveCount, Is.EqualTo(2));
        }

        [Test]
        public void Create_ProducesSequentialNonZeroIds()
        {
            AttackInstance firstAttack = CreateAttack();
            AttackInstance secondAttack = CreateAttack();

            Assert.That(firstAttack.InstanceId, Is.Not.Zero);
            Assert.That(secondAttack.InstanceId, Is.EqualTo(firstAttack.InstanceId + 1));
        }

        [Test]
        public void TryHit_TransformsKnockbackIntoAttackDirection()
        {
            HitReactionData reaction = new HitReactionData(0.35f, new Vector2(7f, 3f));
            AttackInstance attack = factory.Create(
                100,
                CombatFaction.Player,
                25f,
                -1,
                reaction);
            FakeHitTarget target = new FakeHitTarget(200, 100f);

            attack.TryHit(target);

            Assert.That(target.LastRequest.AttackDirection, Is.EqualTo(-1));
            Assert.That(target.LastRequest.Reaction.HitStunDuration, Is.EqualTo(0.35f));
            Assert.That(target.LastRequest.Reaction.KnockbackVelocity, Is.EqualTo(new Vector2(-7f, 3f)));
        }

        [Test]
        public void TryHit_PropagatesDirectStatusApplication()
        {
            CombatStatusApplication status = new CombatStatusApplication(
                CombatStatusType.Airborne,
                1.5f);
            AttackInstance attack = factory.Create(
                100,
                CombatFaction.Player,
                1f,
                1,
                new HitReactionData(0.3f, new Vector2(3f, 9f)),
                status);
            FakeHitTarget target = new FakeHitTarget(200, 100f);

            attack.TryHit(target);

            Assert.That(target.LastRequest.Status.StatusType, Is.EqualTo(CombatStatusType.Airborne));
            Assert.That(target.LastRequest.Status.Duration, Is.EqualTo(1.5f));
        }

        [Test]
        public void TryHit_PropagatesPostHitInvulnerabilityOverride()
        {
            AttackInstance attack = factory.Create(
                100,
                CombatFaction.Player,
                1f,
                1,
                default,
                default,
                false,
                true);
            FakeHitTarget target = new FakeHitTarget(200, 100f);

            attack.TryHit(target);

            Assert.That(target.LastRequest.IgnorePostHitInvulnerability, Is.True);
        }

        [Test]
        public void TryHit_PropagatesCombatFeedbackTier()
        {
            AttackInstance attack = factory.Create(
                100,
                CombatFaction.Player,
                1f,
                1,
                default,
                default,
                false,
                false,
                CombatFeedbackTier.Heavy);
            FakeHitTarget target = new FakeHitTarget(200, 100f);

            attack.TryHit(target);

            Assert.That(target.LastRequest.FeedbackTier, Is.EqualTo(CombatFeedbackTier.Heavy));
        }

        [TestCase(0, CombatFaction.Player, 10f)]
        [TestCase(100, CombatFaction.None, 10f)]
        [TestCase(100, CombatFaction.Player, 0f)]
        public void Create_RejectsInvalidAttackData(
            int sourceId,
            CombatFaction faction,
            float damage)
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() =>
                factory.Create(sourceId, faction, damage));
        }

        private AttackInstance CreateAttack()
        {
            return factory.Create(100, CombatFaction.Player, 25f);
        }

        private sealed class FakeHitTarget : IHitTarget
        {
            internal FakeHitTarget(
                int targetId,
                float currentHealth,
                HitOutcome outcome = HitOutcome.Applied)
            {
                TargetId = targetId;
                CurrentHealth = currentHealth;
                Outcome = outcome;
            }

            public int TargetId { get; }

            public float CurrentHealth { get; private set; }

            internal HitOutcome Outcome { get; set; }

            internal int ReceiveCount { get; private set; }

            internal HitRequest LastRequest { get; private set; }

            public HitResult ReceiveHit(in HitRequest request)
            {
                ReceiveCount++;
                LastRequest = request;
                float appliedDamage = Outcome == HitOutcome.Applied
                    ? System.Math.Min(CurrentHealth, request.Damage)
                    : 0f;
                CurrentHealth -= appliedDamage;
                return new HitResult(
                    Outcome,
                    request.AttackInstanceId,
                    TargetId,
                    request.Damage,
                    appliedDamage,
                    CurrentHealth,
                    CurrentHealth <= 0f);
            }
        }
    }
}
