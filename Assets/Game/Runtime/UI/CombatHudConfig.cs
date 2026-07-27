using UnityEngine;

namespace JustTest.Game.UI
{
    [CreateAssetMenu(fileName = "CombatHudConfig", menuName = "JustTest/UI/Combat HUD Config")]
    public sealed class CombatHudConfig : ScriptableObject
    {
        [SerializeField] private Color barBackgroundColor = new Color(0.04f, 0.05f, 0.06f, 0.9f);
        [SerializeField] private Color healthColor = new Color(0.82f, 0.18f, 0.2f, 1f);
        [SerializeField] private Color lowHealthColor = new Color(1f, 0.58f, 0.12f, 1f);
        [SerializeField] private Color energyColor = new Color(0.18f, 0.72f, 0.92f, 1f);
        [SerializeField] private Color inactiveSlotColor = new Color(0.22f, 0.24f, 0.27f, 0.92f);
        [SerializeField] private Color activeSlotColor = new Color(0.88f, 0.9f, 0.94f, 1f);
        [SerializeField] private Color qteCandidateColor = new Color(1f, 0.78f, 0.12f, 1f);
        [SerializeField] private Color qteExecutingColor = new Color(1f, 0.42f, 0.12f, 1f);
        [SerializeField] private Color emptySlotColor = new Color(0.13f, 0.14f, 0.16f, 0.82f);
        [SerializeField] private Color availableSkillColor = Color.white;
        [SerializeField] private Color unavailableSkillColor = new Color(1f, 0.3f, 0.28f, 1f);
        [SerializeField] private Color defeatOverlayColor = new Color(0.02f, 0.02f, 0.025f, 0.72f);
        [SerializeField] private Color defeatTitleColor = new Color(0.95f, 0.2f, 0.18f, 1f);
        [SerializeField] private Color restartingTitleColor = Color.white;
        [SerializeField, Range(0.01f, 1f)] private float lowHealthThreshold = 0.25f;
        [SerializeField, Min(0.01f)] private float qtePulseCyclesPerSecond = 2.5f;
        [SerializeField, Range(0f, 1f)] private float qtePulseMinimumAlpha = 0.55f;

        internal Color BarBackgroundColor => barBackgroundColor;
        internal Color HealthColor => healthColor;
        internal Color LowHealthColor => lowHealthColor;
        internal Color EnergyColor => energyColor;
        internal Color InactiveSlotColor => inactiveSlotColor;
        internal Color ActiveSlotColor => activeSlotColor;
        internal Color QteCandidateColor => qteCandidateColor;
        internal Color QteExecutingColor => qteExecutingColor;
        internal Color EmptySlotColor => emptySlotColor;
        internal Color AvailableSkillColor => availableSkillColor;
        internal Color UnavailableSkillColor => unavailableSkillColor;
        internal Color DefeatOverlayColor => defeatOverlayColor;
        internal Color DefeatTitleColor => defeatTitleColor;
        internal Color RestartingTitleColor => restartingTitleColor;
        internal float LowHealthThreshold => lowHealthThreshold;
        internal float QtePulseCyclesPerSecond => qtePulseCyclesPerSecond;
        internal float QtePulseMinimumAlpha => qtePulseMinimumAlpha;

        internal bool IsValid =>
            IsFinitePositive(lowHealthThreshold) &&
            lowHealthThreshold <= 1f &&
            IsFinitePositive(qtePulseCyclesPerSecond) &&
            IsFiniteNonNegative(qtePulseMinimumAlpha) &&
            qtePulseMinimumAlpha <= 1f;

        private void OnValidate()
        {
            lowHealthThreshold = Mathf.Clamp(SanitizeFinite(lowHealthThreshold), 0.01f, 1f);
            qtePulseCyclesPerSecond = Mathf.Max(0.01f, SanitizeFinite(qtePulseCyclesPerSecond));
            qtePulseMinimumAlpha = Mathf.Clamp01(SanitizeFinite(qtePulseMinimumAlpha));
        }

        private static bool IsFinitePositive(float value)
        {
            return value > 0f && !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private static bool IsFiniteNonNegative(float value)
        {
            return value >= 0f && !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private static float SanitizeFinite(float value)
        {
            return float.IsNaN(value) || float.IsInfinity(value) ? 0f : value;
        }
    }
}
