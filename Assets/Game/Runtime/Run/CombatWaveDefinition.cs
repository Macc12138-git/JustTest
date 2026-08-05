using System;
using JustTest.Game.Enemies;
using UnityEngine;

namespace JustTest.Game.Run
{
    [Serializable]
    public sealed class CombatWaveDefinition
    {
        [SerializeField] private CombatWaveEnemyGroup[] groups;

        internal int TotalEnemyCount
        {
            get
            {
                if (groups == null)
                {
                    return 0;
                }

                int total = 0;
                for (int index = 0; index < groups.Length; index++)
                {
                    if (groups[index] != null)
                    {
                        total += groups[index].Count;
                    }
                }

                return total;
            }
        }

        internal bool IsValid
        {
            get
            {
                if (groups == null || groups.Length == 0)
                {
                    return false;
                }

                for (int index = 0; index < groups.Length; index++)
                {
                    if (groups[index] == null || !groups[index].IsValid)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        internal bool TryGetArchetypeAt(
            int spawnIndex,
            out CombatEnemyArchetype archetype)
        {
            archetype = null;
            if (spawnIndex < 0 || groups == null)
            {
                return false;
            }

            int remainingIndex = spawnIndex;
            for (int index = 0; index < groups.Length; index++)
            {
                CombatWaveEnemyGroup group = groups[index];
                if (group == null)
                {
                    return false;
                }

                if (remainingIndex < group.Count)
                {
                    archetype = group.Archetype;
                    return archetype != null;
                }

                remainingIndex -= group.Count;
            }

            return false;
        }

        internal void Sanitize()
        {
            if (groups == null)
            {
                return;
            }

            for (int index = 0; index < groups.Length; index++)
            {
                groups[index]?.Sanitize();
            }
        }
    }
}
