using UnityEngine;

namespace JustTest.Game.Run
{
    [CreateAssetMenu(fileName = "CombatPlatformConfig", menuName = "JustTest/Run/Combat Platform Config")]
    public sealed class CombatPlatformConfig : ScriptableObject
    {
        [Header("Encounter Timing")]
        [SerializeField, Min(0f)] private float appearanceDelay = 0.45f;
        [SerializeField, Min(0f)] private float sharedAttackInterval = 0.3f;

        [Header("Surface Validation")]
        [SerializeField] private LayerMask oneWayPlatformLayers;

        [Header("Enemy Positioning")]
        [SerializeField, Min(0f)] private float platformEdgePadding = 0.35f;
        [SerializeField, Min(0f)] private float slotInnerPadding = 0.08f;

        internal float AppearanceDelay => appearanceDelay;
        internal float SharedAttackInterval => sharedAttackInterval;
        internal float PlatformEdgePadding => platformEdgePadding;
        internal float SlotInnerPadding => slotInnerPadding;

        internal bool IsValid =>
            IsFiniteNonNegative(appearanceDelay) &&
            IsFiniteNonNegative(sharedAttackInterval) &&
            IsFiniteNonNegative(platformEdgePadding) &&
            IsFiniteNonNegative(slotInnerPadding) &&
            oneWayPlatformLayers.value != 0;

        internal bool IsValidCombatSurface(Collider2D surface)
        {
            return surface != null &&
                   !surface.isTrigger &&
                   !surface.usedByEffector &&
                   !IsLayerInMask(surface.gameObject.layer, oneWayPlatformLayers);
        }

        private void OnValidate()
        {
            appearanceDelay = Mathf.Max(0f, SanitizeFinite(appearanceDelay));
            sharedAttackInterval = Mathf.Max(0f, SanitizeFinite(sharedAttackInterval));
            platformEdgePadding = Mathf.Max(0f, SanitizeFinite(platformEdgePadding));
            slotInnerPadding = Mathf.Max(0f, SanitizeFinite(slotInnerPadding));
        }

        private static bool IsLayerInMask(int layer, LayerMask layerMask)
        {
            return (layerMask.value & (1 << layer)) != 0;
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
