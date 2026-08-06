using JustTest.Game.Combat;
using JustTest.Game.Enemies;
using NUnit.Framework;

namespace JustTest.Game.Tests.EditMode
{
    public sealed class EliteEnemyDecisionResolverTests
    {
        private static readonly EliteEnemyDecisionParameters Parameters =
            new EliteEnemyDecisionParameters(
                1.15f,
                1.75f,
                1.95f,
                3.2f,
                6f,
                1.4f,
                2.4f,
                0.7f);

        [Test]
        public void PlayerRecoveryAtCloseRangeSelectsQuickSlashFirst()
        {
            EliteEnemyDecision decision = Resolve(
                1.5f,
                AttackPhase.Recovery,
                recoveryOpportunityAvailable: true,
                quickAttackReady: true,
                heavyAttackReady: true,
                dashAttackReady: true,
                passiveAttackDue: true,
                closePresenceDuration: 1f);

            Assert.That(decision, Is.EqualTo(EliteEnemyDecision.QuickSlash));
        }

        [Test]
        public void DistantTargetWithinDashRangeSelectsDashCleave()
        {
            EliteEnemyDecision decision = Resolve(
                4f,
                AttackPhase.Idle,
                dashAttackReady: true);

            Assert.That(decision, Is.EqualTo(EliteEnemyDecision.DashCleave));
        }

        [Test]
        public void CloseTargetHeldLongEnoughSelectsHeavySmash()
        {
            EliteEnemyDecision decision = Resolve(
                1.7f,
                AttackPhase.Idle,
                heavyAttackReady: true,
                closePresenceDuration: 0.7f);

            Assert.That(decision, Is.EqualTo(EliteEnemyDecision.HeavySmash));
        }

        [Test]
        public void PassiveDeadlineSelectsQuickSlash()
        {
            EliteEnemyDecision decision = Resolve(
                1.7f,
                AttackPhase.Idle,
                quickAttackReady: true,
                passiveAttackDue: true);

            Assert.That(decision, Is.EqualTo(EliteEnemyDecision.QuickSlash));
        }

        [Test]
        public void PassiveDeadlineOutsideQuickRangeRequestsReposition()
        {
            EliteEnemyDecision decision = Resolve(
                2f,
                AttackPhase.Idle,
                quickAttackReady: true,
                passiveAttackDue: true);

            Assert.That(decision, Is.EqualTo(EliteEnemyDecision.Reposition));
        }

        [Test]
        public void RollingTargetIsObservedWithoutAttacking()
        {
            EliteEnemyDecision decision = Resolve(
                1.5f,
                AttackPhase.Idle,
                targetRolling: true,
                recoveryOpportunityAvailable: true,
                quickAttackReady: true,
                heavyAttackReady: true,
                dashAttackReady: true,
                passiveAttackDue: true,
                closePresenceDuration: 1f);

            Assert.That(decision, Is.EqualTo(EliteEnemyDecision.Observe));
        }

        [TestCase(AttackPhase.Windup)]
        [TestCase(AttackPhase.Active)]
        public void ActivePlayerAttackIsObservedWithoutCounterAttacking(AttackPhase phase)
        {
            EliteEnemyDecision decision = Resolve(
                1.5f,
                phase,
                recoveryOpportunityAvailable: true,
                quickAttackReady: true,
                heavyAttackReady: true,
                dashAttackReady: true,
                passiveAttackDue: true,
                closePresenceDuration: 1f);

            Assert.That(decision, Is.EqualTo(EliteEnemyDecision.Observe));
        }

        [Test]
        public void TargetBeyondDashRangeRequestsReposition()
        {
            EliteEnemyDecision decision = Resolve(
                6.1f,
                AttackPhase.Idle,
                dashAttackReady: true);

            Assert.That(decision, Is.EqualTo(EliteEnemyDecision.Reposition));
        }

        [Test]
        public void TargetAtPreferredDistanceWithoutOpportunityIsObserved()
        {
            EliteEnemyDecision decision = Resolve(2f, AttackPhase.Idle);

            Assert.That(decision, Is.EqualTo(EliteEnemyDecision.Observe));
        }

        private static EliteEnemyDecision Resolve(
            float horizontalDistance,
            AttackPhase targetAttackPhase,
            float verticalDistance = 0f,
            bool targetRolling = false,
            bool recoveryOpportunityAvailable = false,
            bool quickAttackReady = false,
            bool heavyAttackReady = false,
            bool dashAttackReady = false,
            bool passiveAttackDue = false,
            float closePresenceDuration = 0f)
        {
            EliteEnemyDecisionResolver resolver = new EliteEnemyDecisionResolver(Parameters);
            EliteEnemyDecisionInput input = new EliteEnemyDecisionInput(
                horizontalDistance,
                verticalDistance,
                targetAttackPhase,
                targetRolling,
                recoveryOpportunityAvailable,
                quickAttackReady,
                heavyAttackReady,
                dashAttackReady,
                passiveAttackDue,
                closePresenceDuration);

            return resolver.Resolve(input);
        }
    }
}
