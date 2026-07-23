using JustTest.Game.Combat;
using NUnit.Framework;

namespace JustTest.Game.Tests
{
    public sealed class DamageResolverTests
    {
        private DamageResolver resolver;

        [SetUp]
        public void SetUp()
        {
            resolver = new DamageResolver(new CombatFactionRules());
        }

        [Test]
        public void Resolve_AppliesDamageAndReportsRemainingHealth()
        {
            HealthState health = new HealthState(100f);
            HitRequest request = CreateRequest(25f);

            HitResult result = resolver.Resolve(
                request,
                200,
                CombatFaction.Enemy,
                health,
                false);

            Assert.That(result.Outcome, Is.EqualTo(HitOutcome.Applied));
            Assert.That(result.AppliedDamage, Is.EqualTo(25f));
            Assert.That(result.RemainingHealth, Is.EqualTo(75f));
            Assert.That(result.KilledTarget, Is.False);
        }

        [Test]
        public void Resolve_ClampsOverkillDamageAndReportsDeath()
        {
            HealthState health = new HealthState(20f);
            HitRequest request = CreateRequest(50f);

            HitResult result = resolver.Resolve(
                request,
                200,
                CombatFaction.Enemy,
                health,
                false);

            Assert.That(result.Outcome, Is.EqualTo(HitOutcome.Applied));
            Assert.That(result.AppliedDamage, Is.EqualTo(20f));
            Assert.That(result.RemainingHealth, Is.Zero);
            Assert.That(result.KilledTarget, Is.True);
        }

        [Test]
        public void Resolve_BlocksFriendlyFireByDefault()
        {
            HealthState health = new HealthState(100f);
            HitRequest request = CreateRequest(25f, CombatFaction.Player);

            HitResult result = resolver.Resolve(
                request,
                200,
                CombatFaction.Player,
                health,
                false);

            Assert.That(result.Outcome, Is.EqualTo(HitOutcome.FriendlyFireBlocked));
            Assert.That(health.CurrentHealth, Is.EqualTo(100f));
        }

        [Test]
        public void Resolve_AllowsExplicitFriendlyFire()
        {
            HealthState health = new HealthState(100f);
            HitRequest request = new HitRequest(10, 100, CombatFaction.Player, 25f, true);

            HitResult result = resolver.Resolve(
                request,
                200,
                CombatFaction.Player,
                health,
                false);

            Assert.That(result.Outcome, Is.EqualTo(HitOutcome.Applied));
            Assert.That(health.CurrentHealth, Is.EqualTo(75f));
        }

        [Test]
        public void Resolve_BlocksDamageWhileTargetIsInvulnerable()
        {
            HealthState health = new HealthState(100f);
            HitRequest request = CreateRequest(25f);

            HitResult result = resolver.Resolve(
                request,
                200,
                CombatFaction.Enemy,
                health,
                true);

            Assert.That(result.Outcome, Is.EqualTo(HitOutcome.Invulnerable));
            Assert.That(health.CurrentHealth, Is.EqualTo(100f));
        }

        [Test]
        public void Resolve_RejectsFurtherHitsAfterDeath()
        {
            HealthState health = new HealthState(10f);
            HitRequest request = CreateRequest(10f);

            resolver.Resolve(request, 200, CombatFaction.Enemy, health, false);
            HitResult secondResult = resolver.Resolve(
                request,
                200,
                CombatFaction.Enemy,
                health,
                false);

            Assert.That(secondResult.Outcome, Is.EqualTo(HitOutcome.TargetDead));
            Assert.That(secondResult.AppliedDamage, Is.Zero);
        }

        [TestCase(0, 100, 10f)]
        [TestCase(10, 0, 10f)]
        [TestCase(10, 100, 0f)]
        [TestCase(10, 100, -1f)]
        public void Resolve_RejectsInvalidRequests(int attackId, int sourceId, float damage)
        {
            HealthState health = new HealthState(100f);
            HitRequest request = new HitRequest(
                attackId,
                sourceId,
                CombatFaction.Player,
                damage);

            HitResult result = resolver.Resolve(
                request,
                200,
                CombatFaction.Enemy,
                health,
                false);

            Assert.That(result.Outcome, Is.EqualTo(HitOutcome.InvalidRequest));
            Assert.That(health.CurrentHealth, Is.EqualTo(100f));
        }

        private static HitRequest CreateRequest(
            float damage,
            CombatFaction sourceFaction = CombatFaction.Player)
        {
            return new HitRequest(10, 100, sourceFaction, damage);
        }
    }
}
