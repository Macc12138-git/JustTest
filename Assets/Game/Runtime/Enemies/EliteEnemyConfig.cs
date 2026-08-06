using UnityEngine;

namespace JustTest.Game.Enemies
{
    [CreateAssetMenu(fileName = "EliteEnemyConfig", menuName = "JustTest/Enemies/Elite Enemy Config")]
    public sealed class EliteEnemyConfig : ScriptableObject
    {
        [Header("Movement")]
        [SerializeField, Min(0f)] private float movementSpeed = 4.2f;
        [SerializeField, Min(0f)] private float groundAcceleration = 32f;
        [SerializeField, Min(0f)] private float groundDeceleration = 48f;
        [SerializeField, Min(0f)] private float dashSpeed = 9.5f;

        [Header("Ground Probe")]
        [SerializeField] private LayerMask groundLayers;
        [SerializeField, Min(0.001f)] private float groundProbeDistance = 0.1f;
        [SerializeField, Range(0f, 89f)] private float maximumGroundAngle = 50f;

        [Header("Combat Distance")]
        [SerializeField, Min(0f)] private float attackVerticalTolerance = 1.15f;
        [SerializeField, Min(0.1f)] private float preferredMinimumDistance = 1.4f;
        [SerializeField, Min(0.1f)] private float preferredMaximumDistance = 2.4f;
        [SerializeField, Min(0.1f)] private float quickAttackRange = 1.75f;
        [SerializeField, Min(0.1f)] private float heavyAttackRange = 1.95f;
        [SerializeField, Min(0.1f)] private float dashMinimumDistance = 3.2f;
        [SerializeField, Min(0.1f)] private float dashMaximumDistance = 6f;
        [SerializeField, Min(0.01f)] private float repositionDuration = 0.35f;
        [SerializeField, Min(0.01f)] private float positionTargetTolerance = 0.12f;

        [Header("Combat Cadence")]
        [SerializeField, Min(0f)] private float initialObservationDuration = 0.75f;
        [SerializeField, Min(0f)] private float observationDuration = 0.35f;
        [SerializeField, Min(0.01f)] private float decisionRetryInterval = 0.12f;
        [SerializeField, Min(0.01f)] private float attackRequestRetryInterval = 0.15f;
        [SerializeField, Min(0f)] private float opportunityReactionDelay = 0.16f;
        [SerializeField, Min(0f)] private float rollExitObservationDuration = 0.2f;
        [SerializeField, Min(0.1f)] private float maximumPassiveDuration = 1.6f;
        [SerializeField, Min(0f)] private float postAttackObservationDuration = 0.3f;
        [SerializeField, Min(0f)] private float quickAttackCooldown = 0.9f;
        [SerializeField, Min(0f)] private float heavyAttackCooldown = 3.2f;
        [SerializeField, Min(0f)] private float dashAttackCooldown = 4f;
        [SerializeField, Min(0f)] private float heavyOpportunityDuration = 0.7f;

        [Header("Attack Telegraph")]
        [SerializeField] private Color quickTelegraphColor = new Color(1f, 0.75f, 0.15f, 0.85f);
        [SerializeField] private Color heavyTelegraphColor = new Color(1f, 0.35f, 0.08f, 0.95f);
        [SerializeField] private Color dashTelegraphColor = new Color(0.15f, 0.8f, 1f, 0.9f);
        [SerializeField] private Color activeTelegraphColor = new Color(1f, 0.12f, 0.08f, 0.95f);
        [SerializeField] private Color flashColor = Color.white;
        [SerializeField, Range(0f, 1f)] private float heavyFlashNormalizedTime = 0.65f;
        [SerializeField, Range(0f, 1f)] private float dashFlashNormalizedTime = 0.72f;
        [SerializeField, Min(0.01f)] private float flashDuration = 0.1f;
        [SerializeField, Min(1f)] private float flashScale = 1.22f;
        [SerializeField, Min(1f)] private float dashTelegraphLengthScale = 2.2f;

        internal float MovementSpeed => movementSpeed;
        internal float GroundAcceleration => groundAcceleration;
        internal float GroundDeceleration => groundDeceleration;
        internal float DashSpeed => dashSpeed;
        internal LayerMask GroundLayers => groundLayers;
        internal float GroundProbeDistance => groundProbeDistance;
        internal float MaximumGroundAngle => maximumGroundAngle;
        internal float AttackVerticalTolerance => attackVerticalTolerance;
        internal float PreferredMinimumDistance => preferredMinimumDistance;
        internal float PreferredMaximumDistance => preferredMaximumDistance;
        internal float QuickAttackRange => quickAttackRange;
        internal float HeavyAttackRange => heavyAttackRange;
        internal float DashMinimumDistance => dashMinimumDistance;
        internal float DashMaximumDistance => dashMaximumDistance;
        internal float RepositionDuration => repositionDuration;
        internal float PositionTargetTolerance => positionTargetTolerance;
        internal float InitialObservationDuration => initialObservationDuration;
        internal float ObservationDuration => observationDuration;
        internal float DecisionRetryInterval => decisionRetryInterval;
        internal float AttackRequestRetryInterval => attackRequestRetryInterval;
        internal float OpportunityReactionDelay => opportunityReactionDelay;
        internal float RollExitObservationDuration => rollExitObservationDuration;
        internal float MaximumPassiveDuration => maximumPassiveDuration;
        internal float PostAttackObservationDuration => postAttackObservationDuration;
        internal float QuickAttackCooldown => quickAttackCooldown;
        internal float HeavyAttackCooldown => heavyAttackCooldown;
        internal float DashAttackCooldown => dashAttackCooldown;
        internal float HeavyOpportunityDuration => heavyOpportunityDuration;
        internal Color QuickTelegraphColor => quickTelegraphColor;
        internal Color HeavyTelegraphColor => heavyTelegraphColor;
        internal Color DashTelegraphColor => dashTelegraphColor;
        internal Color ActiveTelegraphColor => activeTelegraphColor;
        internal Color FlashColor => flashColor;
        internal float HeavyFlashNormalizedTime => heavyFlashNormalizedTime;
        internal float DashFlashNormalizedTime => dashFlashNormalizedTime;
        internal float FlashDuration => flashDuration;
        internal float FlashScale => flashScale;
        internal float DashTelegraphLengthScale => dashTelegraphLengthScale;

        internal EliteEnemyDecisionParameters DecisionParameters =>
            new EliteEnemyDecisionParameters(
                attackVerticalTolerance,
                quickAttackRange,
                heavyAttackRange,
                dashMinimumDistance,
                dashMaximumDistance,
                preferredMinimumDistance,
                preferredMaximumDistance,
                heavyOpportunityDuration);

        internal bool IsValid =>
            IsFinitePositive(movementSpeed) &&
            IsFinitePositive(groundAcceleration) &&
            IsFinitePositive(groundDeceleration) &&
            IsFinitePositive(dashSpeed) &&
            groundLayers.value != 0 &&
            IsFinitePositive(groundProbeDistance) &&
            IsFiniteNonNegative(attackVerticalTolerance) &&
            IsFinitePositive(preferredMinimumDistance) &&
            preferredMaximumDistance >= preferredMinimumDistance &&
            IsFinitePositive(quickAttackRange) &&
            IsFinitePositive(heavyAttackRange) &&
            IsFinitePositive(dashMinimumDistance) &&
            dashMaximumDistance >= dashMinimumDistance &&
            IsFinitePositive(repositionDuration) &&
            IsFinitePositive(positionTargetTolerance) &&
            IsFiniteNonNegative(initialObservationDuration) &&
            IsFiniteNonNegative(observationDuration) &&
            IsFinitePositive(decisionRetryInterval) &&
            IsFinitePositive(attackRequestRetryInterval) &&
            IsFiniteNonNegative(opportunityReactionDelay) &&
            IsFiniteNonNegative(rollExitObservationDuration) &&
            IsFinitePositive(maximumPassiveDuration) &&
            IsFiniteNonNegative(postAttackObservationDuration) &&
            IsFiniteNonNegative(quickAttackCooldown) &&
            IsFiniteNonNegative(heavyAttackCooldown) &&
            IsFiniteNonNegative(dashAttackCooldown) &&
            IsFiniteNonNegative(heavyOpportunityDuration) &&
            IsFinitePositive(flashDuration) &&
            flashScale >= 1f &&
            dashTelegraphLengthScale >= 1f;

        private void OnValidate()
        {
            movementSpeed = Mathf.Max(0.01f, SanitizeFinite(movementSpeed));
            groundAcceleration = Mathf.Max(0.01f, SanitizeFinite(groundAcceleration));
            groundDeceleration = Mathf.Max(0.01f, SanitizeFinite(groundDeceleration));
            dashSpeed = Mathf.Max(0.01f, SanitizeFinite(dashSpeed));
            groundProbeDistance = Mathf.Max(0.001f, SanitizeFinite(groundProbeDistance));
            attackVerticalTolerance = SanitizeNonNegative(attackVerticalTolerance);
            preferredMinimumDistance = Mathf.Max(0.1f, SanitizeFinite(preferredMinimumDistance));
            preferredMaximumDistance = Mathf.Max(preferredMinimumDistance, SanitizeFinite(preferredMaximumDistance));
            quickAttackRange = Mathf.Max(0.1f, SanitizeFinite(quickAttackRange));
            heavyAttackRange = Mathf.Max(0.1f, SanitizeFinite(heavyAttackRange));
            dashMinimumDistance = Mathf.Max(0.1f, SanitizeFinite(dashMinimumDistance));
            dashMaximumDistance = Mathf.Max(dashMinimumDistance, SanitizeFinite(dashMaximumDistance));
            repositionDuration = Mathf.Max(0.01f, SanitizeFinite(repositionDuration));
            positionTargetTolerance = Mathf.Max(0.01f, SanitizeFinite(positionTargetTolerance));
            initialObservationDuration = SanitizeNonNegative(initialObservationDuration);
            observationDuration = SanitizeNonNegative(observationDuration);
            decisionRetryInterval = Mathf.Max(0.01f, SanitizeFinite(decisionRetryInterval));
            attackRequestRetryInterval = Mathf.Max(0.01f, SanitizeFinite(attackRequestRetryInterval));
            opportunityReactionDelay = SanitizeNonNegative(opportunityReactionDelay);
            rollExitObservationDuration = SanitizeNonNegative(rollExitObservationDuration);
            maximumPassiveDuration = Mathf.Max(0.1f, SanitizeFinite(maximumPassiveDuration));
            postAttackObservationDuration = SanitizeNonNegative(postAttackObservationDuration);
            quickAttackCooldown = SanitizeNonNegative(quickAttackCooldown);
            heavyAttackCooldown = SanitizeNonNegative(heavyAttackCooldown);
            dashAttackCooldown = SanitizeNonNegative(dashAttackCooldown);
            heavyOpportunityDuration = SanitizeNonNegative(heavyOpportunityDuration);
            heavyFlashNormalizedTime = Mathf.Clamp01(SanitizeFinite(heavyFlashNormalizedTime));
            dashFlashNormalizedTime = Mathf.Clamp01(SanitizeFinite(dashFlashNormalizedTime));
            flashDuration = Mathf.Max(0.01f, SanitizeFinite(flashDuration));
            flashScale = Mathf.Max(1f, SanitizeFinite(flashScale));
            dashTelegraphLengthScale = Mathf.Max(1f, SanitizeFinite(dashTelegraphLengthScale));
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
