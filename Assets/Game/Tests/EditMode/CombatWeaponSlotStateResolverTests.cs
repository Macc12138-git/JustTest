using JustTest.Game.UI;
using NUnit.Framework;

namespace JustTest.Game.Tests.EditMode
{
    public sealed class CombatWeaponSlotStateResolverTests
    {
        private CombatWeaponSlotStateResolver resolver;

        [SetUp]
        public void SetUp()
        {
            resolver = new CombatWeaponSlotStateResolver();
        }

        [Test]
        public void EmptyWeaponAlwaysResolvesToEmpty()
        {
            CombatWeaponSlotVisualState result = resolver.Resolve(false, true, true, true);

            Assert.That(result, Is.EqualTo(CombatWeaponSlotVisualState.Empty));
        }

        [Test]
        public void ExecutingTakesPriorityOverCandidateAndActive()
        {
            CombatWeaponSlotVisualState result = resolver.Resolve(true, true, true, true);

            Assert.That(result, Is.EqualTo(CombatWeaponSlotVisualState.QteExecuting));
        }

        [Test]
        public void CandidateTakesPriorityOverActive()
        {
            CombatWeaponSlotVisualState result = resolver.Resolve(true, true, true, false);

            Assert.That(result, Is.EqualTo(CombatWeaponSlotVisualState.QteCandidate));
        }

        [TestCase(true, (int)CombatWeaponSlotVisualState.Active)]
        [TestCase(false, (int)CombatWeaponSlotVisualState.Inactive)]
        public void RegularWeaponUsesActiveState(bool isActive, int expectedValue)
        {
            CombatWeaponSlotVisualState result = resolver.Resolve(true, isActive, false, false);

            Assert.That(result, Is.EqualTo((CombatWeaponSlotVisualState)expectedValue));
        }
    }
}
