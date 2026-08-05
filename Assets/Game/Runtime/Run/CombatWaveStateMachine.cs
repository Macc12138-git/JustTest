using System;
using System.Collections.Generic;

namespace JustTest.Game.Run
{
    internal sealed class CombatWaveStateMachine
    {
        private readonly int[] waveEnemyCounts;
        private readonly int maximumConcurrentEnemies;
        private readonly HashSet<int> activeParticipantIds = new HashSet<int>();

        internal CombatWaveStateMachine(
            IReadOnlyList<int> waveEnemyCounts,
            int maximumConcurrentEnemies)
        {
            if (waveEnemyCounts == null ||
                waveEnemyCounts.Count == 0 ||
                maximumConcurrentEnemies <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(waveEnemyCounts));
            }

            this.waveEnemyCounts = new int[waveEnemyCounts.Count];
            for (int index = 0; index < waveEnemyCounts.Count; index++)
            {
                if (waveEnemyCounts[index] <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(waveEnemyCounts));
                }

                this.waveEnemyCounts[index] = waveEnemyCounts[index];
            }

            this.maximumConcurrentEnemies = maximumConcurrentEnemies;
        }

        internal CombatWaveState State { get; private set; } = CombatWaveState.Idle;
        internal int CurrentWaveIndex { get; private set; } = -1;
        internal int SpawnedCount { get; private set; }
        internal int DefeatedCount { get; private set; }
        internal int ActiveCount => activeParticipantIds.Count;
        internal int WaveCount => waveEnemyCounts.Length;
        internal int CurrentWaveEnemyCount =>
            CurrentWaveIndex >= 0 && CurrentWaveIndex < waveEnemyCounts.Length
                ? waveEnemyCounts[CurrentWaveIndex]
                : 0;
        internal bool CanSpawn =>
            State == CombatWaveState.Spawning &&
            SpawnedCount < CurrentWaveEnemyCount &&
            ActiveCount < maximumConcurrentEnemies;

        internal bool TryBegin()
        {
            if (State != CombatWaveState.Idle)
            {
                return false;
            }

            CurrentWaveIndex = 0;
            ResetWaveCounts();
            State = CombatWaveState.Spawning;
            return true;
        }

        internal bool TryRecordSpawn(int participantId)
        {
            if (!CanSpawn || participantId == 0 || !activeParticipantIds.Add(participantId))
            {
                return false;
            }

            SpawnedCount++;
            if (SpawnedCount >= CurrentWaveEnemyCount)
            {
                State = CombatWaveState.WaitingForDefeat;
            }

            return true;
        }

        internal bool TryRecordDefeat(int participantId)
        {
            if ((State != CombatWaveState.Spawning && State != CombatWaveState.WaitingForDefeat) ||
                participantId == 0 ||
                !activeParticipantIds.Remove(participantId))
            {
                return false;
            }

            DefeatedCount++;
            if (DefeatedCount >= CurrentWaveEnemyCount)
            {
                State = CurrentWaveIndex >= waveEnemyCounts.Length - 1
                    ? CombatWaveState.Completed
                    : CombatWaveState.InterWaveDelay;
            }
            else if (SpawnedCount < CurrentWaveEnemyCount)
            {
                State = CombatWaveState.Spawning;
            }

            return true;
        }

        internal bool TryBeginNextWave()
        {
            if (State != CombatWaveState.InterWaveDelay || ActiveCount != 0)
            {
                return false;
            }

            CurrentWaveIndex++;
            ResetWaveCounts();
            State = CombatWaveState.Spawning;
            return true;
        }

        internal bool TryInterrupt()
        {
            if (State == CombatWaveState.Completed || State == CombatWaveState.Interrupted)
            {
                return false;
            }

            activeParticipantIds.Clear();
            State = CombatWaveState.Interrupted;
            return true;
        }

        private void ResetWaveCounts()
        {
            activeParticipantIds.Clear();
            SpawnedCount = 0;
            DefeatedCount = 0;
        }
    }
}
