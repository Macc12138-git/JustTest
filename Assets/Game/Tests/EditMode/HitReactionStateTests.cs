using JustTest.Game.Combat;
using NUnit.Framework;
using UnityEngine;

namespace JustTest.Game.Tests
{
    public sealed class HitReactionStateTests
    {
        [Test]
        public void Start_ActivatesReactionWithConfiguredValues()
        {
            HitReactionState state = new HitReactionState();
            HitReactionData reaction = new HitReactionData(0.2f, new Vector2(4f, 1.5f));

            bool started = state.Start(reaction);

            Assert.That(started, Is.True);
            Assert.That(state.IsActive, Is.True);
            Assert.That(state.RemainingDuration, Is.EqualTo(0.2f));
            Assert.That(state.Current.KnockbackVelocity, Is.EqualTo(new Vector2(4f, 1.5f)));
        }

        [Test]
        public void Tick_EndsReactionWhenDurationExpires()
        {
            HitReactionState state = new HitReactionState();
            state.Start(new HitReactionData(0.2f, Vector2.right));

            bool endedEarly = state.Tick(0.1f);
            bool ended = state.Tick(0.1f);

            Assert.That(endedEarly, Is.False);
            Assert.That(ended, Is.True);
            Assert.That(state.IsActive, Is.False);
            Assert.That(state.RemainingDuration, Is.Zero);
        }

        [Test]
        public void Start_ReplacesExistingReactionWithoutAccumulatingDuration()
        {
            HitReactionState state = new HitReactionState();
            state.Start(new HitReactionData(0.2f, new Vector2(4f, 1f)));
            state.Tick(0.1f);

            state.Start(new HitReactionData(0.35f, new Vector2(-7f, 3f)));

            Assert.That(state.RemainingDuration, Is.EqualTo(0.35f));
            Assert.That(state.Current.KnockbackVelocity, Is.EqualTo(new Vector2(-7f, 3f)));
        }

        [Test]
        public void Start_RejectsEmptyReaction()
        {
            HitReactionState state = new HitReactionState();

            bool started = state.Start(default);

            Assert.That(started, Is.False);
            Assert.That(state.IsActive, Is.False);
        }
    }
}
