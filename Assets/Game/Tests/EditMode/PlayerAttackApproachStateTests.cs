using JustTest.Game.Combat;
using NUnit.Framework;

namespace JustTest.Game.Tests
{
    public sealed class PlayerAttackApproachStateTests
    {
        [Test]
        public void CalculateWarpVelocity_UsesPhaseStrengthAsSpeedLimit()
        {
            PlayerAttackApproachState state = new PlayerAttackApproachState();

            float velocity = state.CalculateWarpVelocity(
                8f,
                1,
                1f,
                0f,
                0.5f,
                0.02f);

            Assert.That(velocity, Is.EqualTo(4f).Within(0.001f));
        }

        [Test]
        public void CalculateWarpVelocity_LimitsFinalStepToPreventOvershoot()
        {
            PlayerAttackApproachState state = new PlayerAttackApproachState();

            float velocity = state.CalculateWarpVelocity(
                8f,
                -1,
                1f,
                0.97f,
                1f,
                0.02f);

            Assert.That(velocity, Is.EqualTo(-1.5f).Within(0.001f));
        }

        [Test]
        public void CalculateWarpVelocity_StopsWhenCurveTravelHasBeenConsumed()
        {
            PlayerAttackApproachState state = new PlayerAttackApproachState();

            float velocity = state.CalculateWarpVelocity(
                8f,
                1,
                0.6f,
                0.6f,
                1f,
                0.02f);

            Assert.That(velocity, Is.Zero);
        }

        [Test]
        public void TryResolveWarpVelocity_WithoutActiveTargetDoesNotProvideFallbackMovement()
        {
            PlayerAttackApproachState state = new PlayerAttackApproachState();

            bool handled = state.TryResolveWarpVelocity(
                null,
                null,
                null,
                null,
                AttackPhase.Active,
                0.5f,
                0.02f,
                out float velocity);

            Assert.That(handled, Is.False);
            Assert.That(velocity, Is.Zero);
        }

        [Test]
        public void ShouldSwitchTarget_RequiresMeaningfulScoreImprovement()
        {
            PlayerAttackApproachState state = new PlayerAttackApproachState();

            Assert.That(
                state.ShouldSwitchTarget(2f, 1.4f, 0.65f, false),
                Is.False);
            Assert.That(
                state.ShouldSwitchTarget(2f, 1.2f, 0.65f, false),
                Is.True);
        }

        [Test]
        public void ShouldSwitchTarget_DirectionalOverrideAlwaysWins()
        {
            PlayerAttackApproachState state = new PlayerAttackApproachState();

            Assert.That(
                state.ShouldSwitchTarget(1f, 5f, 0.5f, true),
                Is.True);
        }
    }
}
