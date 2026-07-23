using JustTest.Game.Combat;
using JustTest.Game.Weapons;
using NUnit.Framework;

namespace JustTest.Game.Tests
{
    public sealed class WeaponQteOpportunityStateTests
    {
        [Test]
        public void Open_MakesEveryCandidateAvailable()
        {
            WeaponQteOpportunityState state = new WeaponQteOpportunityState();

            bool opened = state.Open(
                100,
                CreateStatusEvent(CombatStatusType.Unbalanced, 5),
                0b110);

            Assert.That(opened, Is.True);
            Assert.That(state.IsCandidate(0), Is.False);
            Assert.That(state.IsCandidate(1), Is.True);
            Assert.That(state.IsCandidate(2), Is.True);
        }

        [Test]
        public void TrySelect_ClosesOpportunityAndClearsOtherCandidates()
        {
            WeaponQteOpportunityState state = new WeaponQteOpportunityState();
            state.Open(100, CreateStatusEvent(CombatStatusType.Unbalanced, 5), 0b110);

            bool selected = state.TrySelect(1);

            Assert.That(selected, Is.True);
            Assert.That(state.IsOpen, Is.False);
            Assert.That(state.CandidateMask, Is.Zero);
        }

        [Test]
        public void Open_NewStatusReplacesPreviousOpportunity()
        {
            WeaponQteOpportunityState state = new WeaponQteOpportunityState();
            state.Open(100, CreateStatusEvent(CombatStatusType.Unbalanced, 5), 0b010);

            state.Open(200, CreateStatusEvent(CombatStatusType.Stunned, 6), 0b100);

            Assert.That(state.TargetInstanceId, Is.EqualTo(200));
            Assert.That(state.StatusType, Is.EqualTo(CombatStatusType.Stunned));
            Assert.That(state.ApplicationId, Is.EqualTo(6));
            Assert.That(state.CandidateMask, Is.EqualTo(0b100));
        }

        [Test]
        public void TryEnd_StaleApplicationDoesNotCloseRefreshedOpportunity()
        {
            WeaponQteOpportunityState state = new WeaponQteOpportunityState();
            state.Open(100, CreateStatusEvent(CombatStatusType.Airborne, 8), 0b010);

            bool ended = state.TryEnd(100, CombatStatusType.Airborne, 7);

            Assert.That(ended, Is.False);
            Assert.That(state.IsOpen, Is.True);
        }

        [Test]
        public void TryEnd_MatchingApplicationClosesOpportunity()
        {
            WeaponQteOpportunityState state = new WeaponQteOpportunityState();
            state.Open(100, CreateStatusEvent(CombatStatusType.Stunned, 9), 0b100);

            bool ended = state.TryEnd(100, CombatStatusType.Stunned, 9);

            Assert.That(ended, Is.True);
            Assert.That(state.IsOpen, Is.False);
        }

        private static CombatStatusEvent CreateStatusEvent(
            CombatStatusType statusType,
            int applicationId)
        {
            return new CombatStatusEvent(statusType, applicationId, 1f, false);
        }
    }
}
