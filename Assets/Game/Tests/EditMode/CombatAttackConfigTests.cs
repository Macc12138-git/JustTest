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
            Assert.That(definition.HitReaction.HitStunDuration, Is.EqualTo(0.2f));
            Assert.That(definition.HitReaction.KnockbackVelocity, Is.EqualTo(new Vector2(4f, 1.5f)));
            Assert.That(definition.StatusApplication.StatusType, Is.EqualTo(CombatStatusType.None));
            Assert.That(definition.StatusApplication.Duration, Is.Zero);
            Assert.That(definition.AllowFriendlyFire, Is.False);

            Object.DestroyImmediate(definition);
        }

        [Test]
        public void PlayerInputConfig_DefaultPrimaryAttackKeyIsJ()
        {
            PlayerInputConfig config = ScriptableObject.CreateInstance<PlayerInputConfig>();

            Assert.That(config.PrimaryAttackKey, Is.EqualTo(KeyCode.J));
            Assert.That(config.WeaponSlotOneKey, Is.EqualTo(KeyCode.Alpha1));
            Assert.That(config.WeaponSlotTwoKey, Is.EqualTo(KeyCode.Alpha2));
            Assert.That(config.WeaponSlotThreeKey, Is.EqualTo(KeyCode.Alpha3));

            Object.DestroyImmediate(config);
        }

        [Test]
        public void CombatDebugConfig_DefaultOverlayUsesReadableFontSize()
        {
            CombatDebugConfig config = ScriptableObject.CreateInstance<CombatDebugConfig>();

            Assert.That(config.OverlayFontSize, Is.EqualTo(18));
            Assert.That(config.OverlaySize.x, Is.GreaterThanOrEqualTo(680f));
            Assert.That(config.OverlaySize.y, Is.GreaterThanOrEqualTo(360f));
            Assert.That(config.NormalEnemyAttackKey, Is.EqualTo(KeyCode.U));
            Assert.That(config.HeavyEnemyAttackKey, Is.EqualTo(KeyCode.I));
            Assert.That(config.ApplyUnbalancedKey, Is.EqualTo(KeyCode.F1));
            Assert.That(config.ApplyAirborneKey, Is.EqualTo(KeyCode.F2));
            Assert.That(config.ApplyStunnedKey, Is.EqualTo(KeyCode.F3));
            Assert.That(config.BypassPostHitInvulnerabilityForStatusTests, Is.True);

            Object.DestroyImmediate(config);
        }
    }
}
