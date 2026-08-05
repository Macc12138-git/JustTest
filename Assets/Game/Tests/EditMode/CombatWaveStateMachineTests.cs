using System.Reflection;
using JustTest.Game.Enemies;
using JustTest.Game.Run;
using NUnit.Framework;
using UnityEngine;

namespace JustTest.Game.Tests.EditMode
{
    public sealed class CombatWaveStateMachineTests
    {
        [Test]
        public void EncounterAdvancesAcrossConfiguredWaves()
        {
            CombatWaveStateMachine stateMachine = new CombatWaveStateMachine(
                new[] { 2, 3 },
                3);

            Assert.That(stateMachine.TryBegin(), Is.True);
            Assert.That(stateMachine.TryRecordSpawn(10), Is.True);
            Assert.That(stateMachine.TryRecordSpawn(20), Is.True);
            Assert.That(stateMachine.State, Is.EqualTo(CombatWaveState.WaitingForDefeat));
            Assert.That(stateMachine.TryRecordDefeat(10), Is.True);
            Assert.That(stateMachine.TryRecordDefeat(20), Is.True);
            Assert.That(stateMachine.State, Is.EqualTo(CombatWaveState.InterWaveDelay));

            Assert.That(stateMachine.TryBeginNextWave(), Is.True);
            Assert.That(stateMachine.CurrentWaveIndex, Is.EqualTo(1));
            Assert.That(stateMachine.CurrentWaveEnemyCount, Is.EqualTo(3));
        }

        [Test]
        public void MaximumConcurrentCountPausesSpawningUntilDefeat()
        {
            CombatWaveStateMachine stateMachine = new CombatWaveStateMachine(
                new[] { 4 },
                3);
            stateMachine.TryBegin();

            Assert.That(stateMachine.TryRecordSpawn(10), Is.True);
            Assert.That(stateMachine.TryRecordSpawn(20), Is.True);
            Assert.That(stateMachine.TryRecordSpawn(30), Is.True);
            Assert.That(stateMachine.CanSpawn, Is.False);

            Assert.That(stateMachine.TryRecordDefeat(20), Is.True);
            Assert.That(stateMachine.CanSpawn, Is.True);
            Assert.That(stateMachine.TryRecordSpawn(40), Is.True);
            Assert.That(stateMachine.SpawnedCount, Is.EqualTo(4));
        }

        [Test]
        public void DuplicateDefeatNotificationIsIgnored()
        {
            CombatWaveStateMachine stateMachine = new CombatWaveStateMachine(
                new[] { 2 },
                2);
            stateMachine.TryBegin();
            stateMachine.TryRecordSpawn(10);
            stateMachine.TryRecordSpawn(20);

            Assert.That(stateMachine.TryRecordDefeat(10), Is.True);
            Assert.That(stateMachine.TryRecordDefeat(10), Is.False);
            Assert.That(stateMachine.DefeatedCount, Is.EqualTo(1));
            Assert.That(stateMachine.State, Is.EqualTo(CombatWaveState.WaitingForDefeat));
        }

        [Test]
        public void InterruptClearsActiveParticipantsAndStopsProgression()
        {
            CombatWaveStateMachine stateMachine = new CombatWaveStateMachine(
                new[] { 3 },
                3);
            stateMachine.TryBegin();
            stateMachine.TryRecordSpawn(10);

            Assert.That(stateMachine.TryInterrupt(), Is.True);
            Assert.That(stateMachine.ActiveCount, Is.Zero);
            Assert.That(stateMachine.TryRecordSpawn(20), Is.False);
            Assert.That(stateMachine.TryRecordDefeat(10), Is.False);
        }

        [Test]
        public void MixedWavePreservesGroupOrderAndBuildsTotalCount()
        {
            CombatEnemyArchetype melee = ScriptableObject.CreateInstance<CombatEnemyArchetype>();
            CombatEnemyArchetype ranged = ScriptableObject.CreateInstance<CombatEnemyArchetype>();
            CombatEncounterConfig encounter = ScriptableObject.CreateInstance<CombatEncounterConfig>();
            try
            {
                CombatWaveDefinition firstWave = CreateWave(
                    CreateGroup(melee, 2));
                CombatWaveDefinition mixedWave = CreateWave(
                    CreateGroup(melee, 1),
                    CreateGroup(ranged, 1),
                    CreateGroup(melee, 1));
                SetPrivateField(encounter, "waves", new[] { firstWave, mixedWave });

                Assert.That(encounter.BuildWaveEnemyCounts(), Is.EqualTo(new[] { 2, 3 }));
                Assert.That(mixedWave.TryGetArchetypeAt(0, out CombatEnemyArchetype first), Is.True);
                Assert.That(mixedWave.TryGetArchetypeAt(1, out CombatEnemyArchetype second), Is.True);
                Assert.That(mixedWave.TryGetArchetypeAt(2, out CombatEnemyArchetype third), Is.True);
                Assert.That(first, Is.SameAs(melee));
                Assert.That(second, Is.SameAs(ranged));
                Assert.That(third, Is.SameAs(melee));
                Assert.That(mixedWave.TryGetArchetypeAt(3, out _), Is.False);
            }
            finally
            {
                Object.DestroyImmediate(encounter);
                Object.DestroyImmediate(ranged);
                Object.DestroyImmediate(melee);
            }
        }

        private static CombatWaveEnemyGroup CreateGroup(
            CombatEnemyArchetype archetype,
            int count)
        {
            CombatWaveEnemyGroup group = new CombatWaveEnemyGroup();
            SetPrivateField(group, "archetype", archetype);
            SetPrivateField(group, "count", count);
            return group;
        }

        private static CombatWaveDefinition CreateWave(
            params CombatWaveEnemyGroup[] groups)
        {
            CombatWaveDefinition wave = new CombatWaveDefinition();
            SetPrivateField(wave, "groups", groups);
            return wave;
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
