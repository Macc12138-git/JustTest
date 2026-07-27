using JustTest.Game.Run;
using NUnit.Framework;

namespace JustTest.Game.Tests.EditMode
{
    public sealed class CombatPlatformStateMachineTests
    {
        [Test]
        public void EncounterFollowsExpectedStateSequence()
        {
            CombatPlatformStateMachine stateMachine = new CombatPlatformStateMachine();

            Assert.That(stateMachine.TryBeginAppearance(), Is.True);
            Assert.That(stateMachine.State, Is.EqualTo(CombatPlatformState.Appearing));
            Assert.That(stateMachine.TryActivate(), Is.True);
            Assert.That(stateMachine.State, Is.EqualTo(CombatPlatformState.Active));
            Assert.That(stateMachine.TryComplete(), Is.True);
            Assert.That(stateMachine.State, Is.EqualTo(CombatPlatformState.Completed));
        }

        [Test]
        public void EncounterCannotCompleteBeforeActivation()
        {
            CombatPlatformStateMachine stateMachine = new CombatPlatformStateMachine();

            Assert.That(stateMachine.TryComplete(), Is.False);
            Assert.That(stateMachine.State, Is.EqualTo(CombatPlatformState.Dormant));
        }

        [Test]
        public void InterruptedEncounterCannotResumeOrComplete()
        {
            CombatPlatformStateMachine stateMachine = new CombatPlatformStateMachine();
            stateMachine.TryBeginAppearance();

            Assert.That(stateMachine.TryInterrupt(), Is.True);
            Assert.That(stateMachine.State, Is.EqualTo(CombatPlatformState.Interrupted));
            Assert.That(stateMachine.TryActivate(), Is.False);
            Assert.That(stateMachine.TryComplete(), Is.False);
        }

        [Test]
        public void CompletedEncounterCannotBeInterrupted()
        {
            CombatPlatformStateMachine stateMachine = new CombatPlatformStateMachine();
            stateMachine.TryBeginAppearance();
            stateMachine.TryActivate();
            stateMachine.TryComplete();

            Assert.That(stateMachine.TryInterrupt(), Is.False);
            Assert.That(stateMachine.State, Is.EqualTo(CombatPlatformState.Completed));
        }
    }
}
