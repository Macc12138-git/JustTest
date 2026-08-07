using JustTest.Game.Combat;
using JustTest.Game.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace JustTest.Game.Tests
{
    public sealed class AttackAnimationBindingTests
    {
        [Test]
        public void EvaluateNormalizedTime_MapsCombatPhasesIntoConfiguredRanges()
        {
            AttackDefinition attack = ScriptableObject.CreateInstance<AttackDefinition>();
            AttackAnimationBinding binding = new AttackAnimationBinding(
                attack,
                "SwordAttack",
                0.25f,
                0.7f);

            Assert.That(binding.EvaluateNormalizedTime(AttackPhase.Windup, 0.5f), Is.EqualTo(0.125f));
            Assert.That(binding.EvaluateNormalizedTime(AttackPhase.Active, 0.5f), Is.EqualTo(0.475f));
            Assert.That(binding.EvaluateNormalizedTime(AttackPhase.Recovery, 0.5f), Is.EqualTo(0.85f));

            Object.DestroyImmediate(attack);
        }

        [Test]
        public void EvaluateNormalizedTime_ClampsPhaseProgress()
        {
            AttackDefinition attack = ScriptableObject.CreateInstance<AttackDefinition>();
            AttackAnimationBinding binding = new AttackAnimationBinding(
                attack,
                "SwordAttack",
                0.3f,
                0.65f);

            Assert.That(binding.EvaluateNormalizedTime(AttackPhase.Windup, -1f), Is.EqualTo(0f));
            Assert.That(binding.EvaluateNormalizedTime(AttackPhase.Recovery, 2f), Is.EqualTo(1f));

            Object.DestroyImmediate(attack);
        }
    }
}
