using JustTest.Game.Combat;
using NUnit.Framework;

namespace JustTest.Game.Tests
{
    public sealed class HealthStateTests
    {
        [Test]
        public void Restore_ClampsAtMaximumHealth()
        {
            HealthState health = new HealthState(100f);
            health.ApplyDamage(30f);

            float restored = health.Restore(50f);

            Assert.That(restored, Is.EqualTo(30f));
            Assert.That(health.CurrentHealth, Is.EqualTo(100f));
        }

        [Test]
        public void Restore_DoesNotReviveDeadTarget()
        {
            HealthState health = new HealthState(100f);
            health.ApplyDamage(100f);

            float restored = health.Restore(20f);

            Assert.That(restored, Is.Zero);
            Assert.That(health.IsDead, Is.True);
        }

        [Test]
        public void RestoreToFull_RevivesForExplicitCombatReset()
        {
            HealthState health = new HealthState(100f);
            health.ApplyDamage(100f);

            health.RestoreToFull();

            Assert.That(health.CurrentHealth, Is.EqualTo(100f));
            Assert.That(health.IsDead, Is.False);
        }
    }
}
