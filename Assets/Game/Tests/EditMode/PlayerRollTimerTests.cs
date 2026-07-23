using JustTest.Game.Player;
using NUnit.Framework;

namespace JustTest.Game.Tests
{
    public sealed class PlayerRollTimerTests
    {
        [Test]
        public void Invulnerability_UsesConfiguredWindow()
        {
            PlayerRollTimer timer = new PlayerRollTimer(0.35f, 0.05f, 0.2f, 0.45f);

            timer.Start(1f);
            Assert.That(timer.IsInvulnerable, Is.False);

            timer.Tick(0.05f);
            Assert.That(timer.IsInvulnerable, Is.True);

            timer.Tick(0.2f);
            Assert.That(timer.IsInvulnerable, Is.False);
        }

        [Test]
        public void CanStart_UsesMinimumIntervalBetweenStartTimes()
        {
            PlayerRollTimer timer = new PlayerRollTimer(0.35f, 0f, 0.2f, 0.45f);

            timer.Start(1f);
            timer.Tick(0.35f);

            Assert.That(timer.CanStart(1.44f), Is.False);
            Assert.That(timer.CanStart(1.45f), Is.True);
        }

        [Test]
        public void Tick_CompletesAtConfiguredDuration()
        {
            PlayerRollTimer timer = new PlayerRollTimer(0.35f, 0f, 0.2f, 0.45f);
            timer.Start(1f);

            Assert.That(timer.Tick(0.34f), Is.False);
            Assert.That(timer.Tick(0.01f), Is.True);
            Assert.That(timer.IsRolling, Is.False);
        }
    }
}
