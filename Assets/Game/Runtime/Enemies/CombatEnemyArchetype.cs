using UnityEngine;

namespace JustTest.Game.Enemies
{
    [CreateAssetMenu(fileName = "CombatEnemyArchetype", menuName = "JustTest/Enemies/Combat Enemy Archetype")]
    public sealed class CombatEnemyArchetype : ScriptableObject
    {
        [SerializeField] private CombatEnemyRuntime2D prefab;
        [SerializeField, Min(0)] private int initialCapacity = 1;
        [SerializeField, Min(1)] private int maximumCapacity = 3;

        internal CombatEnemyRuntime2D Prefab => prefab;
        internal int InitialCapacity => initialCapacity;
        internal int MaximumCapacity => maximumCapacity;
        internal bool IsValid =>
            prefab != null &&
            initialCapacity >= 0 &&
            maximumCapacity > 0 &&
            initialCapacity <= maximumCapacity;

        private void OnValidate()
        {
            initialCapacity = Mathf.Max(0, initialCapacity);
            maximumCapacity = Mathf.Max(1, maximumCapacity);
            initialCapacity = Mathf.Min(initialCapacity, maximumCapacity);
        }
    }
}
