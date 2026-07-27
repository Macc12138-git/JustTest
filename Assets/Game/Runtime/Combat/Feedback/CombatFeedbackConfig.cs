using UnityEngine;

namespace JustTest.Game.Combat
{
    [CreateAssetMenu(
        fileName = "CombatFeedbackConfig",
        menuName = "JustTest/Combat/Combat Feedback Config")]
    public sealed class CombatFeedbackConfig : ScriptableObject
    {
        [SerializeField] private CombatFeedbackProfile light = new CombatFeedbackProfile(
            0.03f,
            0.06f,
            0.05f,
            28f,
            0.06f,
            1.2f,
            0.04f,
            3f,
            0.08f,
            new Vector2(0.3f, 0.1f),
            0.8f,
            0.15f);
        [SerializeField] private CombatFeedbackProfile medium = new CombatFeedbackProfile(
            0.05f,
            0.1f,
            0.1f,
            24f,
            0.09f,
            1.25f,
            0.07f,
            6f,
            0.12f,
            new Vector2(0.35f, 0.12f),
            1f,
            0.2f);
        [SerializeField] private CombatFeedbackProfile heavy = new CombatFeedbackProfile(
            0.09f,
            0.16f,
            0.18f,
            20f,
            0.13f,
            1.35f,
            0.12f,
            10f,
            0.18f,
            new Vector2(0.4f, 0.15f),
            1.25f,
            0.3f);

        internal CombatFeedbackProfile GetProfile(CombatFeedbackTier tier)
        {
            return tier switch
            {
                CombatFeedbackTier.Light => light,
                CombatFeedbackTier.Medium => medium,
                CombatFeedbackTier.Heavy => heavy,
                _ => null
            };
        }

        private void OnValidate()
        {
            light?.Sanitize();
            medium?.Sanitize();
            heavy?.Sanitize();
        }
    }
}
