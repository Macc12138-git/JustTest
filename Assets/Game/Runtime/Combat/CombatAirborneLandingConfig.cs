using UnityEngine;

namespace JustTest.Game.Combat
{
    [CreateAssetMenu(
        fileName = "CombatAirborneLandingConfig",
        menuName = "JustTest/Combat/Airborne Landing Config")]
    public sealed class CombatAirborneLandingConfig : ScriptableObject
    {
        [SerializeField] private LayerMask groundLayers;
        [SerializeField, Range(0f, 1f)] private float minimumGroundNormalY = 0.65f;
        [SerializeField, Min(0f)] private float minimumAirborneDuration = 0.05f;

        internal LayerMask GroundLayers => groundLayers;

        internal float MinimumGroundNormalY => minimumGroundNormalY;

        internal float MinimumAirborneDuration => minimumAirborneDuration;

        private void OnValidate()
        {
            minimumGroundNormalY = Mathf.Clamp01(minimumGroundNormalY);
            minimumAirborneDuration = Mathf.Max(0f, minimumAirborneDuration);
        }
    }
}
