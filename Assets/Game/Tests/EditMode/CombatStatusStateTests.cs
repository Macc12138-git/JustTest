using JustTest.Game.Combat;
using NUnit.Framework;

namespace JustTest.Game.Tests
{
    public sealed class CombatStatusStateTests
    {
        [Test]
        public void Apply_ActivatesStatusAndAssignsApplicationId()
        {
            CombatStatusState state = new CombatStatusState();

            bool applied = state.Apply(
                new CombatStatusApplication(CombatStatusType.Unbalanced, 1.2f),
                10f,
                out CombatStatusEvent statusEvent);

            Assert.That(applied, Is.True);
            Assert.That(state.IsActive(CombatStatusType.Unbalanced), Is.True);
            Assert.That(state.ActiveCount, Is.EqualTo(1));
            Assert.That(statusEvent.ApplicationId, Is.Not.Zero);
            Assert.That(statusEvent.WasRefresh, Is.False);
        }

        [Test]
        public void Apply_SameStatusRefreshesWithoutIncreasingActiveCount()
        {
            CombatStatusState state = new CombatStatusState();
            state.Apply(
                new CombatStatusApplication(CombatStatusType.Stunned, 1f),
                10f,
                out CombatStatusEvent firstEvent);

            state.Apply(
                new CombatStatusApplication(CombatStatusType.Stunned, 2f),
                10.5f,
                out CombatStatusEvent refreshedEvent);

            Assert.That(state.ActiveCount, Is.EqualTo(1));
            Assert.That(refreshedEvent.WasRefresh, Is.True);
            Assert.That(refreshedEvent.ApplicationId, Is.Not.EqualTo(firstEvent.ApplicationId));
            Assert.That(state.GetRemainingDuration(CombatStatusType.Stunned, 10.5f), Is.EqualTo(2f));
        }

        [Test]
        public void Apply_DifferentStatusesRemainActiveTogether()
        {
            CombatStatusState state = new CombatStatusState();
            state.Apply(
                new CombatStatusApplication(CombatStatusType.Unbalanced, 1.2f),
                10f,
                out _);
            state.Apply(
                new CombatStatusApplication(CombatStatusType.Airborne, 1.5f),
                10f,
                out _);
            state.Apply(
                new CombatStatusApplication(CombatStatusType.Stunned, 1f),
                10f,
                out _);

            Assert.That(state.ActiveCount, Is.EqualTo(3));
            Assert.That(state.IsActive(CombatStatusType.Unbalanced), Is.True);
            Assert.That(state.IsActive(CombatStatusType.Airborne), Is.True);
            Assert.That(state.IsActive(CombatStatusType.Stunned), Is.True);
        }

        [Test]
        public void TryExpireNext_ExpiresOnlyStatusesWhoseDurationEnded()
        {
            CombatStatusState state = new CombatStatusState();
            state.Apply(
                new CombatStatusApplication(CombatStatusType.Unbalanced, 0.5f),
                10f,
                out _);
            state.Apply(
                new CombatStatusApplication(CombatStatusType.Stunned, 1f),
                10f,
                out _);

            bool expired = state.TryExpireNext(10.5f, out CombatStatusEvent endedEvent);

            Assert.That(expired, Is.True);
            Assert.That(endedEvent.StatusType, Is.EqualTo(CombatStatusType.Unbalanced));
            Assert.That(state.IsActive(CombatStatusType.Stunned), Is.True);
            Assert.That(state.ActiveCount, Is.EqualTo(1));
        }

        [Test]
        public void Remove_RejectsStaleApplicationId()
        {
            CombatStatusState state = new CombatStatusState();
            state.Apply(
                new CombatStatusApplication(CombatStatusType.Airborne, 1.5f),
                10f,
                out CombatStatusEvent firstEvent);
            state.Apply(
                new CombatStatusApplication(CombatStatusType.Airborne, 1.5f),
                10.2f,
                out CombatStatusEvent refreshedEvent);

            bool removed = state.Remove(
                CombatStatusType.Airborne,
                firstEvent.ApplicationId,
                out _);

            Assert.That(removed, Is.False);
            Assert.That(state.GetApplicationId(CombatStatusType.Airborne),
                Is.EqualTo(refreshedEvent.ApplicationId));
        }

        [Test]
        public void TryClearNext_RemovesAllStatusesIndependently()
        {
            CombatStatusState state = new CombatStatusState();
            state.Apply(
                new CombatStatusApplication(CombatStatusType.Unbalanced, 1f),
                10f,
                out _);
            state.Apply(
                new CombatStatusApplication(CombatStatusType.Stunned, 1f),
                10f,
                out _);

            int clearedCount = 0;
            while (state.TryClearNext(out _))
            {
                clearedCount++;
            }

            Assert.That(clearedCount, Is.EqualTo(2));
            Assert.That(state.ActiveCount, Is.Zero);
        }

        [Test]
        public void Apply_RejectsEmptyStatusApplication()
        {
            CombatStatusState state = new CombatStatusState();

            bool applied = state.Apply(default, 10f, out _);

            Assert.That(applied, Is.False);
            Assert.That(state.ActiveCount, Is.Zero);
        }
    }
}
