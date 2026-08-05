using System;
using JustTest.Game.Enemies;
using UnityEngine;

namespace JustTest.Game.Run
{
    [Serializable]
    public sealed class CombatWaveEnemyGroup
    {
        [SerializeField] private CombatEnemyArchetype archetype;
        [SerializeField, Min(1)] private int count = 1;

        internal CombatEnemyArchetype Archetype => archetype;
        internal int Count => count;
        internal bool IsValid => archetype != null && archetype.IsValid && count > 0;

        internal void Sanitize()
        {
            count = Mathf.Max(1, count);
        }
    }
}
