using JustTest.Game.Combat;
using JustTest.Game.Input;
using NUnit.Framework;
using UnityEngine;

namespace JustTest.Game.Tests
{
    public sealed class CombatAttackConfigTests
    {
        [Test]
        public void AttackDefinition_DefaultsMatchPrototypeTuning()
        {
            AttackDefinition definition = ScriptableObject.CreateInstance<AttackDefinition>();

            Assert.That(definition.Damage, Is.EqualTo(20f));
            Assert.That(definition.WindupDuration, Is.EqualTo(0.08f));
            Assert.That(definition.ActiveDuration, Is.EqualTo(0.12f));
            Assert.That(definition.RecoveryDuration, Is.EqualTo(0.2f));
            Assert.That(definition.InputBufferDuration, Is.EqualTo(0.1f));
            Assert.That(definition.AllowFriendlyFire, Is.False);

            Object.DestroyImmediate(definition);
        }

        [Test]
        public void PlayerInputConfig_DefaultPrimaryAttackKeyIsJ()
        {
            PlayerInputConfig config = ScriptableObject.CreateInstance<PlayerInputConfig>();

            Assert.That(config.PrimaryAttackKey, Is.EqualTo(KeyCode.J));

            Object.DestroyImmediate(config);
        }

        [Test]
        public void CombatDebugConfig_DefaultOverlayUsesReadableFontSize()
        {
            CombatDebugConfig config = ScriptableObject.CreateInstance<CombatDebugConfig>();

            Assert.That(config.OverlayFontSize, Is.EqualTo(18));
            Assert.That(config.OverlaySize.x, Is.GreaterThanOrEqualTo(340f));
            Assert.That(config.OverlaySize.y, Is.GreaterThanOrEqualTo(140f));

            Object.DestroyImmediate(config);
        }
    }
}
