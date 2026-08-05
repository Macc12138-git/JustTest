using UnityEngine;

namespace JustTest.Game.Enemies
{
    [CreateAssetMenu(fileName = "RangedEnemyConfig", menuName = "JustTest/Enemies/Ranged Enemy Config")]
    public sealed class RangedEnemyConfig : ScriptableObject
    {
        [Header("Movement")]
        [SerializeField, Min(0f)] private float movementSpeed = 4f;
        [SerializeField, Min(0f)] private float groundAcceleration = 30f;
        [SerializeField, Min(0f)] private float groundDeceleration = 45f;

        [Header("Ground Probe")]
        [SerializeField] private LayerMask groundLayers;
        [SerializeField, Min(0.001f)] private float groundProbeDistance = 0.1f;
        [SerializeField, Range(0f, 89f)] private float maximumGroundAngle = 50f;

        [Header("Combat Decision")]
        [SerializeField, Min(0f)] private float attackVerticalTolerance = 1.15f;
        [SerializeField, Min(0.1f)] private float preferredMinimumDistance = 3.2f;
        [SerializeField, Min(0.1f)] private float preferredMaximumDistance = 5f;
        [SerializeField, Min(0f)] private float attackCooldown = 1.8f;
        [SerializeField, Min(0f)] private float initialObservationDuration = 0.65f;
        [SerializeField, Min(0f)] private float observationDuration = 0.25f;
        [SerializeField, Min(0.01f)] private float decisionRetryInterval = 0.12f;
        [SerializeField, Min(0.01f)] private float attackRequestRetryInterval = 0.15f;
        [SerializeField, Min(0f)] private float postAttackObservationDuration = 0.45f;
        [SerializeField, Min(0f)] private float blockedRetreatGraceDuration = 0.4f;
        [SerializeField, Min(0.01f)] private float repositionDuration = 0.35f;
        [SerializeField, Min(0.01f)] private float positionTargetTolerance = 0.12f;

        [Header("Attack Telegraph")]
        [SerializeField] private Color telegraphWindupColor = new Color(0.2f, 0.8f, 1f, 0.85f);
        [SerializeField] private Color telegraphActiveColor = new Color(1f, 0.35f, 0.12f, 0.95f);
        [SerializeField] private Color telegraphFlashColor = Color.white;
        [SerializeField] private Color telegraphGlowColor = new Color(0.1f, 0.65f, 1f, 0.38f);
        [SerializeField, Min(0.1f)] private float telegraphLaserLength = 6.5f;
        [SerializeField, Min(0.001f)] private float telegraphGlowWidth = 0.18f;
        [SerializeField, Min(0.001f)] private float telegraphCoreWidth = 0.045f;
        [SerializeField, Min(0f)] private float telegraphPulseFrequency = 12f;
        [SerializeField, Range(0f, 1f)] private float telegraphPulseAmplitude = 0.16f;
        [SerializeField, Range(0f, 1f)] private float telegraphMinimumCoreLength = 0.18f;
        [SerializeField, Range(0f, 1f)] private float telegraphFlashNormalizedTime = 0.72f;
        [SerializeField, Min(0.01f)] private float telegraphFlashDuration = 0.1f;
        [SerializeField, Min(1f)] private float telegraphFlashScale = 1.18f;

        internal float MovementSpeed => movementSpeed;
        internal float GroundAcceleration => groundAcceleration;
        internal float GroundDeceleration => groundDeceleration;
        internal LayerMask GroundLayers => groundLayers;
        internal float GroundProbeDistance => groundProbeDistance;
        internal float MaximumGroundAngle => maximumGroundAngle;
        internal float AttackVerticalTolerance => attackVerticalTolerance;
        internal float PreferredMinimumDistance => preferredMinimumDistance;
        internal float PreferredMaximumDistance => preferredMaximumDistance;
        internal float AttackCooldown => attackCooldown;
        internal float InitialObservationDuration => initialObservationDuration;
        internal float ObservationDuration => observationDuration;
        internal float DecisionRetryInterval => decisionRetryInterval;
        internal float AttackRequestRetryInterval => attackRequestRetryInterval;
        internal float PostAttackObservationDuration => postAttackObservationDuration;
        internal float BlockedRetreatGraceDuration => blockedRetreatGraceDuration;
        internal float RepositionDuration => repositionDuration;
        internal float PositionTargetTolerance => positionTargetTolerance;
        internal Color TelegraphWindupColor => telegraphWindupColor;
        internal Color TelegraphActiveColor => telegraphActiveColor;
        internal Color TelegraphFlashColor => telegraphFlashColor;
        internal Color TelegraphGlowColor => telegraphGlowColor;
        internal float TelegraphLaserLength => telegraphLaserLength;
        internal float TelegraphGlowWidth => telegraphGlowWidth;
        internal float TelegraphCoreWidth => telegraphCoreWidth;
        internal float TelegraphPulseFrequency => telegraphPulseFrequency;
        internal float TelegraphPulseAmplitude => telegraphPulseAmplitude;
        internal float TelegraphMinimumCoreLength => telegraphMinimumCoreLength;
        internal float TelegraphFlashNormalizedTime => telegraphFlashNormalizedTime;
        internal float TelegraphFlashDuration => telegraphFlashDuration;
        internal float TelegraphFlashScale => telegraphFlashScale;

        internal bool IsValid =>
            IsFinitePositive(movementSpeed) &&
            IsFinitePositive(groundAcceleration) &&
            IsFinitePositive(groundDeceleration) &&
            groundLayers.value != 0 &&
            IsFinitePositive(groundProbeDistance) &&
            IsFiniteNonNegative(attackVerticalTolerance) &&
            IsFinitePositive(preferredMinimumDistance) &&
            preferredMaximumDistance >= preferredMinimumDistance &&
            IsFiniteNonNegative(attackCooldown) &&
            IsFiniteNonNegative(initialObservationDuration) &&
            IsFiniteNonNegative(observationDuration) &&
            IsFinitePositive(decisionRetryInterval) &&
            IsFinitePositive(attackRequestRetryInterval) &&
            IsFiniteNonNegative(postAttackObservationDuration) &&
            IsFiniteNonNegative(blockedRetreatGraceDuration) &&
            IsFinitePositive(repositionDuration) &&
            IsFinitePositive(positionTargetTolerance) &&
            IsFinitePositive(telegraphLaserLength) &&
            IsFinitePositive(telegraphGlowWidth) &&
            IsFinitePositive(telegraphCoreWidth) &&
            IsFiniteNonNegative(telegraphPulseFrequency) &&
            IsFinitePositive(telegraphFlashDuration) &&
            telegraphFlashScale >= 1f;

        private void OnValidate()
        {
            movementSpeed = Mathf.Max(0.01f, SanitizeFinite(movementSpeed));
            groundAcceleration = Mathf.Max(0.01f, SanitizeFinite(groundAcceleration));
            groundDeceleration = Mathf.Max(0.01f, SanitizeFinite(groundDeceleration));
            groundProbeDistance = Mathf.Max(0.001f, SanitizeFinite(groundProbeDistance));
            attackVerticalTolerance = SanitizeNonNegative(attackVerticalTolerance);
            preferredMinimumDistance = Mathf.Max(0.1f, SanitizeFinite(preferredMinimumDistance));
            preferredMaximumDistance = Mathf.Max(
                preferredMinimumDistance,
                SanitizeFinite(preferredMaximumDistance));
            attackCooldown = SanitizeNonNegative(attackCooldown);
            initialObservationDuration = SanitizeNonNegative(initialObservationDuration);
            observationDuration = SanitizeNonNegative(observationDuration);
            decisionRetryInterval = Mathf.Max(0.01f, SanitizeFinite(decisionRetryInterval));
            attackRequestRetryInterval = Mathf.Max(0.01f, SanitizeFinite(attackRequestRetryInterval));
            postAttackObservationDuration = SanitizeNonNegative(postAttackObservationDuration);
            blockedRetreatGraceDuration = SanitizeNonNegative(blockedRetreatGraceDuration);
            repositionDuration = Mathf.Max(0.01f, SanitizeFinite(repositionDuration));
            positionTargetTolerance = Mathf.Max(0.01f, SanitizeFinite(positionTargetTolerance));
            telegraphLaserLength = Mathf.Max(0.1f, SanitizeFinite(telegraphLaserLength));
            telegraphGlowWidth = Mathf.Max(0.001f, SanitizeFinite(telegraphGlowWidth));
            telegraphCoreWidth = Mathf.Max(0.001f, SanitizeFinite(telegraphCoreWidth));
            telegraphPulseFrequency = SanitizeNonNegative(telegraphPulseFrequency);
            telegraphPulseAmplitude = Mathf.Clamp01(SanitizeFinite(telegraphPulseAmplitude));
            telegraphMinimumCoreLength = Mathf.Clamp01(SanitizeFinite(telegraphMinimumCoreLength));
            telegraphFlashNormalizedTime = Mathf.Clamp01(SanitizeFinite(telegraphFlashNormalizedTime));
            telegraphFlashDuration = Mathf.Max(0.01f, SanitizeFinite(telegraphFlashDuration));
            telegraphFlashScale = Mathf.Max(1f, SanitizeFinite(telegraphFlashScale));
        }

        private static float SanitizeNonNegative(float value)
        {
            return Mathf.Max(0f, SanitizeFinite(value));
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
