using System.Reflection;
using JustTest.Game.Combat;
using JustTest.Game.Weapons;
using NUnit.Framework;
using UnityEngine;

namespace JustTest.Game.Tests
{
    public sealed class WeaponQteDefinitionTests
    {
        [Test]
        public void DefaultDefinition_IsInvalidWithoutStrikes()
        {
            WeaponQteDefinition definition = ScriptableObject.CreateInstance<WeaponQteDefinition>();

            Assert.That(definition.IsValid, Is.False);

            Object.DestroyImmediate(definition);
        }

        [Test]
        public void DefinitionWithValidStrike_IsValid()
        {
            AttackDefinition attack = ScriptableObject.CreateInstance<AttackDefinition>();
            WeaponQteDefinition definition = ScriptableObject.CreateInstance<WeaponQteDefinition>();
            WeaponQteStrikeDefinition strike = CreateStrike(attack, Vector2.one);
            SetPrivateField(definition, "strikes", new[] { strike });

            Assert.That(definition.IsValid, Is.True);
            Assert.That(definition.StrikeCount, Is.EqualTo(1));
            Assert.That(definition.GetStrike(0).Attack, Is.SameAs(attack));

            Object.DestroyImmediate(definition);
            Object.DestroyImmediate(attack);
        }

        [Test]
        public void DefinitionWithNonPositiveHitboxSize_IsInvalid()
        {
            AttackDefinition attack = ScriptableObject.CreateInstance<AttackDefinition>();
            WeaponQteDefinition definition = ScriptableObject.CreateInstance<WeaponQteDefinition>();
            WeaponQteStrikeDefinition strike = CreateStrike(attack, new Vector2(1f, 0f));
            SetPrivateField(definition, "strikes", new[] { strike });

            Assert.That(definition.IsValid, Is.False);

            Object.DestroyImmediate(definition);
            Object.DestroyImmediate(attack);
        }

        private static WeaponQteStrikeDefinition CreateStrike(
            AttackDefinition attack,
            Vector2 hitboxSize)
        {
            object boxedStrike = default(WeaponQteStrikeDefinition);
            SetPrivateField(boxedStrike, "attack", attack);
            SetPrivateField(boxedStrike, "hitboxSize", hitboxSize);
            return (WeaponQteStrikeDefinition)boxedStrike;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"Missing field: {fieldName}");
            field.SetValue(target, value);
        }
    }
}
