using System.Reflection;
using JustTest.Game.Combat;
using JustTest.Game.Weapons;
using NUnit.Framework;
using UnityEngine;

namespace JustTest.Game.Tests
{
    public sealed class WeaponSkillDefinitionTests
    {
        [Test]
        public void DefaultDefinition_IsInvalidWithoutAttack()
        {
            WeaponSkillDefinition definition = ScriptableObject.CreateInstance<WeaponSkillDefinition>();

            Assert.That(definition.IsValid, Is.False);

            Object.DestroyImmediate(definition);
        }

        [Test]
        public void DefinitionWithAttack_IsValid()
        {
            AttackDefinition attack = ScriptableObject.CreateInstance<AttackDefinition>();
            WeaponSkillDefinition definition = ScriptableObject.CreateInstance<WeaponSkillDefinition>();
            SetPrivateField(definition, "attack", attack);

            Assert.That(definition.IsValid, Is.True);
            Assert.That(definition.Attack, Is.SameAs(attack));
            Assert.That(definition.EnergyCost, Is.EqualTo(30f));

            Object.DestroyImmediate(definition);
            Object.DestroyImmediate(attack);
        }

        [Test]
        public void DefinitionWithNonPositiveHitboxSize_IsInvalid()
        {
            AttackDefinition attack = ScriptableObject.CreateInstance<AttackDefinition>();
            WeaponSkillDefinition definition = ScriptableObject.CreateInstance<WeaponSkillDefinition>();
            SetPrivateField(definition, "attack", attack);
            SetPrivateField(definition, "hitboxSize", new Vector2(1f, 0f));

            Assert.That(definition.IsValid, Is.False);

            Object.DestroyImmediate(definition);
            Object.DestroyImmediate(attack);
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
