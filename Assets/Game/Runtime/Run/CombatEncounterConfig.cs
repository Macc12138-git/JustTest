using UnityEngine;

namespace JustTest.Game.Run
{
    [CreateAssetMenu(fileName = "CombatEncounterConfig", menuName = "JustTest/Run/Combat Encounter Config")]
    public sealed class CombatEncounterConfig : ScriptableObject
    {
        [Header("Waves")]
        [SerializeField] private CombatWaveDefinition[] waves;
        [SerializeField, Min(1)] private int maximumConcurrentEnemies = 3;
        [SerializeField, Min(0f)] private float spawnInterval = 0.35f;
        [SerializeField, Min(0f)] private float interWaveDelay = 1.1f;

        [Header("Enemy Lifecycle")]
        [SerializeField, Min(0f)] private float enemyAppearanceDelay = 0.2f;
        [SerializeField, Min(0f)] private float corpseLifetime = 0.75f;
        [SerializeField, Min(0f)] private float spawnRetryInterval = 0.1f;

        [Header("Spawn Safety")]
        [SerializeField, Min(0f)] private float minimumDistanceFromPlayer = 1.25f;
        [SerializeField, Min(0f)] private float minimumEnemySeparation = 0.75f;

        internal int WaveCount => waves?.Length ?? 0;
        internal int MaximumConcurrentEnemies => maximumConcurrentEnemies;
        internal float SpawnInterval => spawnInterval;
        internal float InterWaveDelay => interWaveDelay;
        internal float EnemyAppearanceDelay => enemyAppearanceDelay;
        internal float CorpseLifetime => corpseLifetime;
        internal float SpawnRetryInterval => spawnRetryInterval;
        internal float MinimumDistanceFromPlayer => minimumDistanceFromPlayer;
        internal float MinimumEnemySeparation => minimumEnemySeparation;

        internal bool IsValid
        {
            get
            {
                if (waves == null ||
                    waves.Length == 0 ||
                    maximumConcurrentEnemies <= 0 ||
                    !IsFiniteNonNegative(spawnInterval) ||
                    !IsFiniteNonNegative(interWaveDelay) ||
                    !IsFiniteNonNegative(enemyAppearanceDelay) ||
                    !IsFiniteNonNegative(corpseLifetime) ||
                    !IsFiniteNonNegative(spawnRetryInterval) ||
                    !IsFiniteNonNegative(minimumDistanceFromPlayer) ||
                    !IsFiniteNonNegative(minimumEnemySeparation))
                {
                    return false;
                }

                for (int index = 0; index < waves.Length; index++)
                {
                    if (waves[index] == null || !waves[index].IsValid)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        private void OnValidate()
        {
            maximumConcurrentEnemies = Mathf.Max(1, maximumConcurrentEnemies);
            spawnInterval = SanitizeNonNegative(spawnInterval);
            interWaveDelay = SanitizeNonNegative(interWaveDelay);
            enemyAppearanceDelay = SanitizeNonNegative(enemyAppearanceDelay);
            corpseLifetime = SanitizeNonNegative(corpseLifetime);
            spawnRetryInterval = SanitizeNonNegative(spawnRetryInterval);
            minimumDistanceFromPlayer = SanitizeNonNegative(minimumDistanceFromPlayer);
            minimumEnemySeparation = SanitizeNonNegative(minimumEnemySeparation);
            if (waves == null)
            {
                return;
            }

            for (int index = 0; index < waves.Length; index++)
            {
                waves[index]?.Sanitize();
            }
        }

        internal CombatWaveDefinition GetWave(int waveIndex)
        {
            return waveIndex >= 0 && waveIndex < WaveCount ? waves[waveIndex] : null;
        }

        internal int[] BuildWaveEnemyCounts()
        {
            int[] counts = new int[WaveCount];
            for (int index = 0; index < counts.Length; index++)
            {
                counts[index] = waves[index]?.TotalEnemyCount ?? 0;
            }

            return counts;
        }

        private static float SanitizeNonNegative(float value)
        {
            return IsFiniteNonNegative(value) ? value : 0f;
        }

        private static bool IsFiniteNonNegative(float value)
        {
            return value >= 0f && !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
