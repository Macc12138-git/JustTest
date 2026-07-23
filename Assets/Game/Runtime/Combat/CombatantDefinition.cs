using UnityEngine;

namespace JustTest.Game.Combat
{
    [CreateAssetMenu(fileName = "CombatantDefinition", menuName = "JustTest/Combat/Combatant Definition")]
    public sealed class CombatantDefinition : ScriptableObject
    {
        [SerializeField] private CombatFaction faction = CombatFaction.Enemy;
        [SerializeField, Min(1f)] private float maximumHealth = 100f;
        [SerializeField, Min(0f)] private float postHitInvulnerabilityDuration = 0.6f;

        public CombatFaction Faction => faction;

        public float MaximumHealth => maximumHealth;

        public float PostHitInvulnerabilityDuration => postHitInvulnerabilityDuration;

        private void OnValidate()
        {
            maximumHealth = Mathf.Max(1f, maximumHealth);
            postHitInvulnerabilityDuration = Mathf.Max(0f, postHitInvulnerabilityDuration);
        }
    }
}
