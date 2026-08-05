using JustTest.Game.Enemies;
using NUnit.Framework;

namespace JustTest.Game.Tests.EditMode
{
    public sealed class EnemyProjectileLifetimeStateTests
    {
        [Test]
        public void LifetimeExpiresOnlyAfterConfiguredDuration()
        {
            EnemyProjectileLifetimeState state = new EnemyProjectileLifetimeState(1.6f);

            Assert.That(state.Start(), Is.True);
            Assert.That(state.Tick(1f), Is.False);
            Assert.That(state.RemainingLifetime, Is.EqualTo(0.6f).Within(0.0001f));
            Assert.That(state.Tick(0.6f), Is.True);
            Assert.That(state.TryComplete(), Is.True);
            Assert.That(state.IsActive, Is.False);
        }

        [Test]
        public void CompletionAndRestartAreProtectedAgainstDuplicateSignals()
        {
            EnemyProjectileLifetimeState state = new EnemyProjectileLifetimeState(1f);

            Assert.That(state.Start(), Is.True);
            Assert.That(state.Start(), Is.False);
            Assert.That(state.TryComplete(), Is.True);
            Assert.That(state.TryComplete(), Is.False);
            Assert.That(state.Start(), Is.True);
            Assert.That(state.IsActive, Is.True);
        }

        [Test]
        public void ResetClearsActiveProjectileWithoutCompletion()
        {
            EnemyProjectileLifetimeState state = new EnemyProjectileLifetimeState(1f);
            state.Start();
            state.Tick(0.25f);

            state.Reset();

            Assert.That(state.IsActive, Is.False);
            Assert.That(state.RemainingLifetime, Is.Zero);
            Assert.That(state.TryComplete(), Is.False);
        }
    }
}
