using JustTest.Game.Run;
using NUnit.Framework;

namespace JustTest.Game.Tests.EditMode
{
    public sealed class CombatRunStateMachineTests
    {
        [Test]
        public void PlayerDefeatTransitionsFromActive()
        {
            CombatRunStateMachine stateMachine = new CombatRunStateMachine();

            bool changed = stateMachine.TryMarkPlayerDefeated();

            Assert.That(changed, Is.True);
            Assert.That(stateMachine.State, Is.EqualTo(CombatRunState.PlayerDefeated));
        }

        [Test]
        public void RepeatedPlayerDefeatIsIgnored()
        {
            CombatRunStateMachine stateMachine = new CombatRunStateMachine();
            stateMachine.TryMarkPlayerDefeated();

            bool changed = stateMachine.TryMarkPlayerDefeated();

            Assert.That(changed, Is.False);
            Assert.That(stateMachine.State, Is.EqualTo(CombatRunState.PlayerDefeated));
        }

        [Test]
        public void VictoryTransitionsFromActive()
        {
            CombatRunStateMachine stateMachine = new CombatRunStateMachine();

            bool changed = stateMachine.TryMarkVictory();

            Assert.That(changed, Is.True);
            Assert.That(stateMachine.State, Is.EqualTo(CombatRunState.Victory));
        }

        [Test]
        public void VictoryCannotReplacePlayerDefeat()
        {
            CombatRunStateMachine stateMachine = new CombatRunStateMachine();
            stateMachine.TryMarkPlayerDefeated();

            bool changed = stateMachine.TryMarkVictory();

            Assert.That(changed, Is.False);
            Assert.That(stateMachine.State, Is.EqualTo(CombatRunState.PlayerDefeated));
        }

        [Test]
        public void ActiveRunCanRestartWhenEnabled()
        {
            CombatRunStateMachine stateMachine = new CombatRunStateMachine();

            bool changed = stateMachine.TryBeginRestart(true);

            Assert.That(changed, Is.True);
            Assert.That(stateMachine.State, Is.EqualTo(CombatRunState.Restarting));
        }

        [Test]
        public void ActiveRunCannotRestartWhenDisabled()
        {
            CombatRunStateMachine stateMachine = new CombatRunStateMachine();

            bool changed = stateMachine.TryBeginRestart(false);

            Assert.That(changed, Is.False);
            Assert.That(stateMachine.State, Is.EqualTo(CombatRunState.Active));
        }

        [Test]
        public void DefeatedRunCanRestart()
        {
            CombatRunStateMachine stateMachine = new CombatRunStateMachine();
            stateMachine.TryMarkPlayerDefeated();

            bool changed = stateMachine.TryBeginRestart(false);

            Assert.That(changed, Is.True);
            Assert.That(stateMachine.State, Is.EqualTo(CombatRunState.Restarting));
        }

        [Test]
        public void VictoriousRunCanRestart()
        {
            CombatRunStateMachine stateMachine = new CombatRunStateMachine();
            stateMachine.TryMarkVictory();

            bool changed = stateMachine.TryBeginRestart(false);

            Assert.That(changed, Is.True);
            Assert.That(stateMachine.State, Is.EqualTo(CombatRunState.Restarting));
        }

        [Test]
        public void RestartCannotBeginTwice()
        {
            CombatRunStateMachine stateMachine = new CombatRunStateMachine();
            stateMachine.TryBeginRestart(true);

            bool changed = stateMachine.TryBeginRestart(true);

            Assert.That(changed, Is.False);
            Assert.That(stateMachine.State, Is.EqualTo(CombatRunState.Restarting));
        }
    }
}
