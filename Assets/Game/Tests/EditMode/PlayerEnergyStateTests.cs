using JustTest.Game.Player;
using NUnit.Framework;

namespace JustTest.Game.Tests
{
    public sealed class PlayerEnergyStateTests
    {
        [Test]
        public void TrySpend_DeductsEnergyOnlyWhenAffordable()
        {
            PlayerEnergyState state = new PlayerEnergyState(100f, 60f);

            bool firstSpend = state.TrySpend(40f);
            bool secondSpend = state.TrySpend(30f);

            Assert.That(firstSpend, Is.True);
            Assert.That(secondSpend, Is.False);
            Assert.That(state.CurrentEnergy, Is.EqualTo(20f));
        }

        [Test]
        public void Restore_ClampsAtMaximumEnergy()
        {
            PlayerEnergyState state = new PlayerEnergyState(100f, 40f);

            float restored = state.Restore(80f);

            Assert.That(restored, Is.EqualTo(60f));
            Assert.That(state.CurrentEnergy, Is.EqualTo(100f));
        }

        [Test]
        public void Reset_RestoresConfiguredStartingEnergy()
        {
            PlayerEnergyState state = new PlayerEnergyState(100f, 75f);
            state.TrySpend(50f);

            bool changed = state.Reset();

            Assert.That(changed, Is.True);
            Assert.That(state.CurrentEnergy, Is.EqualTo(75f));
        }

        [TestCase(0f, 0f)]
        [TestCase(-1f, 0f)]
        [TestCase(100f, -1f)]
        [TestCase(100f, 101f)]
        public void Constructor_RejectsInvalidConfiguration(float maximum, float starting)
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() =>
                new PlayerEnergyState(maximum, starting));
        }
    }
}
