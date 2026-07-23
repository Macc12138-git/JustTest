using JustTest.Game.Player;
using NUnit.Framework;

namespace JustTest.Game.Tests
{
    public sealed class PlayerControlLockStateTests
    {
        [Test]
        public void RemovingOneSource_DoesNotReleaseOtherSource()
        {
            PlayerControlLockState state = new PlayerControlLockState();
            state.Set(PlayerControlLockSource.HitReaction, true);
            state.Set(PlayerControlLockSource.Death, true);

            state.Set(PlayerControlLockSource.HitReaction, false);

            Assert.That(state.IsLocked, Is.True);
            Assert.That(state.ActiveSources, Is.EqualTo(PlayerControlLockSource.Death));
        }

        [Test]
        public void Clear_RemovesEveryLockSource()
        {
            PlayerControlLockState state = new PlayerControlLockState();
            state.Set(PlayerControlLockSource.HitReaction, true);
            state.Set(PlayerControlLockSource.External, true);

            bool changed = state.Clear();

            Assert.That(changed, Is.True);
            Assert.That(state.IsLocked, Is.False);
            Assert.That(state.ActiveSources, Is.EqualTo(PlayerControlLockSource.None));
        }

        [Test]
        public void NoneSource_DoesNotChangeState()
        {
            PlayerControlLockState state = new PlayerControlLockState();

            bool changed = state.Set(PlayerControlLockSource.None, true);

            Assert.That(changed, Is.False);
            Assert.That(state.IsLocked, Is.False);
        }
    }
}
