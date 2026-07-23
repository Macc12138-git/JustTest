using System.Reflection;
using JustTest.Game.Combat;
using JustTest.Game.Weapons;
using NUnit.Framework;
using UnityEngine;

namespace JustTest.Game.Tests
{
    public sealed class WeaponLoadoutStateTests
    {
        [Test]
        public void Initialize_EmptyLoadoutUsesDefaultWeapon()
        {
            WeaponDefinition defaultWeapon = CreateWeapon(CombatStatusType.Unbalanced);
            WeaponLoadoutState state = new WeaponLoadoutState();

            bool initialized = state.Initialize(null, defaultWeapon);

            Assert.That(initialized, Is.True);
            Assert.That(state.ActiveSlotIndex, Is.Zero);
            Assert.That(state.ActiveWeapon, Is.SameAs(defaultWeapon));
            Object.DestroyImmediate(defaultWeapon);
        }

        [Test]
        public void Initialize_CopiesAtMostThreeWeaponsAndAllowsDuplicates()
        {
            WeaponDefinition repeatedWeapon = CreateWeapon(CombatStatusType.Airborne);
            WeaponDefinition ignoredWeapon = CreateWeapon(CombatStatusType.Stunned);
            WeaponLoadoutState state = new WeaponLoadoutState();

            state.Initialize(
                new[] { repeatedWeapon, repeatedWeapon, repeatedWeapon, ignoredWeapon },
                ignoredWeapon);

            Assert.That(state.GetWeapon(0), Is.SameAs(repeatedWeapon));
            Assert.That(state.GetWeapon(1), Is.SameAs(repeatedWeapon));
            Assert.That(state.GetWeapon(2), Is.SameAs(repeatedWeapon));
            Assert.That(state.GetWeapon(3), Is.Null);
            Object.DestroyImmediate(repeatedWeapon);
            Object.DestroyImmediate(ignoredWeapon);
        }

        [Test]
        public void BuildQteCandidateMask_ExcludesActiveWeaponAndIncludesEveryMatch()
        {
            WeaponDefinition sword = CreateWeapon(CombatStatusType.Unbalanced);
            WeaponLoadoutState state = new WeaponLoadoutState();
            state.Initialize(new[] { sword, sword, sword }, sword);

            int candidateMask = state.BuildQteCandidateMask(CombatStatusType.Unbalanced);

            Assert.That(candidateMask, Is.EqualTo(0b110));
            Object.DestroyImmediate(sword);
        }

        [Test]
        public void TrySelectSlot_ChangesActiveWeaponAndResetRestoresInitialSlot()
        {
            WeaponDefinition sword = CreateWeapon(CombatStatusType.Unbalanced);
            WeaponDefinition daggers = CreateWeapon(CombatStatusType.Airborne);
            WeaponLoadoutState state = new WeaponLoadoutState();
            state.Initialize(new[] { sword, daggers }, sword);

            bool selected = state.TrySelectSlot(1);
            bool reset = state.ResetActiveSlot();

            Assert.That(selected, Is.True);
            Assert.That(reset, Is.True);
            Assert.That(state.ActiveSlotIndex, Is.Zero);
            Assert.That(state.ActiveWeapon, Is.SameAs(sword));
            Object.DestroyImmediate(sword);
            Object.DestroyImmediate(daggers);
        }

        private static WeaponDefinition CreateWeapon(CombatStatusType qteTriggerStatus)
        {
            WeaponDefinition weapon = ScriptableObject.CreateInstance<WeaponDefinition>();
            FieldInfo field = typeof(WeaponDefinition).GetField(
                "qteTriggerStatus",
                BindingFlags.Instance | BindingFlags.NonPublic);
            field?.SetValue(weapon, qteTriggerStatus);
            return weapon;
        }
    }
}
