using System.Collections.Generic;
using JustTest.Game.Combat;
using NUnit.Framework;

namespace JustTest.Game.Tests
{
    public sealed class AttackTimelineTests
    {
        [Test]
        public void Tick_AdvancesThroughConfiguredPhases()
        {
            AttackTimeline timeline = new AttackTimeline(0.08f, 0.12f, 0.2f);

            timeline.Start();
            Assert.That(timeline.Phase, Is.EqualTo(AttackPhase.Windup));

            timeline.Tick(0.08f);
            Assert.That(timeline.Phase, Is.EqualTo(AttackPhase.Active));

            timeline.Tick(0.12f);
            Assert.That(timeline.Phase, Is.EqualTo(AttackPhase.Recovery));

            timeline.Tick(0.2f);
            Assert.That(timeline.Phase, Is.EqualTo(AttackPhase.Idle));
        }

        [Test]
        public void Tick_LargeDeltaTimeCrossesMultiplePhases()
        {
            AttackTimeline timeline = new AttackTimeline(0.08f, 0.12f, 0.2f);

            timeline.Start();
            timeline.Tick(0.4f);

            Assert.That(timeline.Phase, Is.EqualTo(AttackPhase.Idle));
            Assert.That(timeline.IsRunning, Is.False);
        }

        [Test]
        public void Cancel_ReturnsImmediatelyToIdle()
        {
            AttackTimeline timeline = new AttackTimeline(0.08f, 0.12f, 0.2f);
            timeline.Start();
            timeline.Tick(0.08f);

            bool cancelled = timeline.Cancel();

            Assert.That(cancelled, Is.True);
            Assert.That(timeline.Phase, Is.EqualTo(AttackPhase.Idle));
        }

        [Test]
        public void Start_DoesNotRestartRunningAttack()
        {
            AttackTimeline timeline = new AttackTimeline(0.08f, 0.12f, 0.2f);

            Assert.That(timeline.Start(), Is.True);
            timeline.Tick(0.04f);
            Assert.That(timeline.Start(), Is.False);
            timeline.Tick(0.04f);

            Assert.That(timeline.Phase, Is.EqualTo(AttackPhase.Active));
        }

        [Test]
        public void PhaseChanged_ReportsEachTransitionInOrder()
        {
            AttackTimeline timeline = new AttackTimeline(0.08f, 0.12f, 0.2f);
            List<AttackPhase> phases = new List<AttackPhase>();
            timeline.PhaseChanged += (_, next) => phases.Add(next);

            timeline.Start();
            timeline.Tick(0.4f);

            Assert.That(phases, Is.EqualTo(new[]
            {
                AttackPhase.Windup,
                AttackPhase.Active,
                AttackPhase.Recovery,
                AttackPhase.Idle
            }));
        }

        [TestCase(0f, 0.1f, 0.1f)]
        [TestCase(0.1f, 0f, 0.1f)]
        [TestCase(0.1f, 0.1f, 0f)]
        public void Constructor_RejectsNonPositiveDurations(
            float windup,
            float active,
            float recovery)
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() =>
                new AttackTimeline(windup, active, recovery));
        }
    }
}
