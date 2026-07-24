using JustTest.Game.Combat;
using NUnit.Framework;

namespace JustTest.Game.Tests
{
    public sealed class PlayerAttackComboStateTests
    {
        [Test]
        public void ResolveStartStep_FreshStateStartsAtFirstStep()
        {
            PlayerAttackComboState state = new PlayerAttackComboState();

            Assert.That(state.ResolveStartStep(10f), Is.Zero);
        }

        [Test]
        public void MarkStepCompleted_ContinuesWithinResetDelay()
        {
            PlayerAttackComboState state = new PlayerAttackComboState();
            state.MarkStepCompleted(0, 3, true, 10f, 0.35f);

            Assert.That(state.ResolveStartStep(10.35f), Is.EqualTo(1));
        }

        [Test]
        public void ResolveStartStep_ExpiredContinuationRestartsCombo()
        {
            PlayerAttackComboState state = new PlayerAttackComboState();
            state.MarkStepCompleted(1, 3, true, 10f, 0.35f);

            Assert.That(state.ResolveStartStep(10.36f), Is.Zero);
        }

        [Test]
        public void MarkStepCompleted_FinalStepLoopsWhenConfigured()
        {
            PlayerAttackComboState state = new PlayerAttackComboState();
            state.MarkStepCompleted(2, 3, true, 10f, 0.35f);

            Assert.That(state.ResolveStartStep(10.1f), Is.Zero);
        }

        [Test]
        public void MarkStepCompleted_FinalStepResetsWhenLoopDisabled()
        {
            PlayerAttackComboState state = new PlayerAttackComboState();
            state.MarkStepCompleted(2, 3, false, 10f, 0.35f);

            Assert.That(state.ResolveStartStep(10.1f), Is.Zero);
        }

        [Test]
        public void QueueContinuation_OnlyQueuesOneInput()
        {
            PlayerAttackComboState state = new PlayerAttackComboState();

            Assert.That(state.QueueContinuation(), Is.True);
            Assert.That(state.QueueContinuation(), Is.False);
            Assert.That(state.IsContinuationQueued, Is.True);
        }
    }
}
