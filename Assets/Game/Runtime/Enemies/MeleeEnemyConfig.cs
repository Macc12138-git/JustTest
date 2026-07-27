using UnityEngine;

namespace JustTest.Game.Enemies
{
    [CreateAssetMenu(fileName = "MeleeEnemyConfig", menuName = "JustTest/Enemies/Melee Enemy Config")]
    public sealed class MeleeEnemyConfig : ScriptableObject
    {
        [Header("Movement")]
        [SerializeField, Min(0f)] private float movementSpeed = 4.5f;
        [SerializeField, Min(0f)] private float groundAcceleration = 35f;
        [SerializeField, Min(0f)] private float groundDeceleration = 45f;

        [Header("Ground Probe")]
        [SerializeField] private LayerMask groundLayers;
        [SerializeField, Min(0.001f)] private float groundProbeDistance = 0.1f;
        [SerializeField, Range(0f, 89f)] private float maximumGroundAngle = 50f;

        [Header("Combat Decision")]
        [SerializeField, Min(0f)] private float attackVerticalTolerance = 1.15f;
        [SerializeField, Min(0f)] private float attackCooldown = 0.55f;

        [Header("Combat Cadence")]
        [SerializeField, Min(0f)] private float initialObservationDuration = 0.55f;
        [SerializeField, Min(0f)] private float observationDuration = 0.35f;
        [SerializeField, Min(0.01f)] private float decisionRetryInterval = 0.12f;
        [SerializeField, Min(0.01f)] private float attackRequestRetryInterval = 0.15f;
        [SerializeField, Min(0f)] private float rollExitObservationDuration = 0.2f;
        [SerializeField, Min(0f)] private float opportunityReactionDelay = 0.16f;
        [SerializeField, Min(0.1f)] private float maximumPassiveDuration = 1.4f;
        [SerializeField, Min(0f)] private float postAttackObservationDuration = 0.4f;
        [SerializeField, Min(0f)] private float initialHeavyAttackDelay = 1.5f;
        [SerializeField, Min(0f)] private float heavyOpportunityDuration = 0.8f;
        [SerializeField, Min(0f)] private float heavyAttackCooldown = 3f;

        [Header("Platform Positioning")]
        [SerializeField, Min(0f)] private float preferredMinimumDistance = 1.15f;
        [SerializeField, Min(0.1f)] private float preferredMaximumDistance = 2.2f;
        [SerializeField, Min(0.01f)] private float repositionDuration = 0.3f;
        [SerializeField, Min(0f)] private float maximumRoamDistance = 2.5f;
        [SerializeField, Min(0.1f)] private float normalAttackRange = 1.55f;
        [SerializeField, Min(0.1f)] private float heavyAttackRange = 1.7f;

        [Header("Attack Telegraph")]
        [SerializeField] private Color telegraphWindupColor = new Color(1f, 0.75f, 0.15f, 0.8f);
        [SerializeField] private Color telegraphActiveColor = new Color(1f, 0.15f, 0.1f, 0.9f);
        [SerializeField] private Color heavyTelegraphColor = new Color(1f, 0.42f, 0.08f, 0.95f);
        [SerializeField] private Color heavyFlashColor = Color.white;
        [SerializeField, Range(0f, 1f)] private float heavyFlashNormalizedTime = 0.58f;
        [SerializeField, Min(0.01f)] private float heavyFlashDuration = 0.1f;
        [SerializeField, Min(1f)] private float heavyFlashScale = 1.22f;

        internal float MovementSpeed => movementSpeed;
        internal float GroundAcceleration => groundAcceleration;
        internal float GroundDeceleration => groundDeceleration;
        internal LayerMask GroundLayers => groundLayers;
        internal float GroundProbeDistance => groundProbeDistance;
        internal float MaximumGroundAngle => maximumGroundAngle;
        internal float AttackVerticalTolerance => attackVerticalTolerance;
        internal float AttackCooldown => attackCooldown;
        internal float InitialObservationDuration => initialObservationDuration;
        internal float ObservationDuration => observationDuration;
        internal float DecisionRetryInterval => decisionRetryInterval;
        internal float AttackRequestRetryInterval => attackRequestRetryInterval;
        internal float RollExitObservationDuration => rollExitObservationDuration;
        internal float OpportunityReactionDelay => opportunityReactionDelay;
        internal float MaximumPassiveDuration => maximumPassiveDuration;
        internal float PostAttackObservationDuration => postAttackObservationDuration;
        internal float InitialHeavyAttackDelay => initialHeavyAttackDelay;
        internal float HeavyOpportunityDuration => heavyOpportunityDuration;
        internal float HeavyAttackCooldown => heavyAttackCooldown;
        internal float PreferredMinimumDistance => preferredMinimumDistance;
        internal float PreferredMaximumDistance => preferredMaximumDistance;
        internal float RepositionDuration => repositionDuration;
        internal float MaximumRoamDistance => maximumRoamDistance;
        internal float NormalAttackRange => normalAttackRange;
        internal float HeavyAttackRange => heavyAttackRange;
        internal Color TelegraphWindupColor => telegraphWindupColor;
        internal Color TelegraphActiveColor => telegraphActiveColor;
        internal Color HeavyTelegraphColor => heavyTelegraphColor;
        internal Color HeavyFlashColor => heavyFlashColor;
        internal float HeavyFlashNormalizedTime => heavyFlashNormalizedTime;
        internal float HeavyFlashDuration => heavyFlashDuration;
        internal float HeavyFlashScale => heavyFlashScale;

        internal bool IsValid =>
            IsFinitePositive(movementSpeed) &&
            IsFinitePositive(groundAcceleration) &&
            IsFinitePositive(groundDeceleration) &&
            groundLayers.value != 0 &&
            IsFinitePositive(groundProbeDistance) &&
            IsFinitePositive(normalAttackRange) &&
            IsFinitePositive(heavyAttackRange) &&
            IsFiniteNonNegative(attackVerticalTolerance) &&
            IsFiniteNonNegative(attackCooldown) &&
            IsFiniteNonNegative(initialObservationDuration) &&
            IsFiniteNonNegative(observationDuration) &&
            IsFinitePositive(decisionRetryInterval) &&
            IsFinitePositive(attackRequestRetryInterval) &&
            IsFinitePositive(maximumPassiveDuration) &&
            IsFinitePositive(repositionDuration) &&
            IsFiniteNonNegative(maximumRoamDistance) &&
            preferredMaximumDistance >= preferredMinimumDistance &&
            IsFinitePositive(heavyFlashDuration) &&
            heavyFlashScale >= 1f;

        private void OnValidate()
        {
            movementSpeed = Mathf.Max(0f, movementSpeed);
            initialObservationDuration = SanitizeNonNegative(initialObservationDuration);
            observationDuration = SanitizeNonNegative(observationDuration);
            decisionRetryInterval = Mathf.Max(0.01f, SanitizeFinite(decisionRetryInterval));
            attackRequestRetryInterval = Mathf.Max(0.01f, SanitizeFinite(attackRequestRetryInterval));
            rollExitObservationDuration = SanitizeNonNegative(rollExitObservationDuration);
            opportunityReactionDelay = SanitizeNonNegative(opportunityReactionDelay);
            maximumPassiveDuration = Mathf.Max(0.1f, SanitizeFinite(maximumPassiveDuration));
            postAttackObservationDuration = SanitizeNonNegative(postAttackObservationDuration);
            initialHeavyAttackDelay = SanitizeNonNegative(initialHeavyAttackDelay);
            heavyOpportunityDuration = SanitizeNonNegative(heavyOpportunityDuration);
            heavyAttackCooldown = SanitizeNonNegative(heavyAttackCooldown);
            preferredMinimumDistance = SanitizeNonNegative(preferredMinimumDistance);
            preferredMaximumDistance = Mathf.Max(
                Mathf.Max(0.1f, preferredMinimumDistance),
                SanitizeFinite(preferredMaximumDistance));
            repositionDuration = Mathf.Max(0.01f, SanitizeFinite(repositionDuration));
            maximumRoamDistance = SanitizeNonNegative(maximumRoamDistance);
            normalAttackRange = Mathf.Max(0.1f, SanitizeFinite(normalAttackRange));
            heavyAttackRange = Mathf.Max(0.1f, SanitizeFinite(heavyAttackRange));
            heavyFlashNormalizedTime = Mathf.Clamp01(SanitizeFinite(heavyFlashNormalizedTime));
            heavyFlashDuration = Mathf.Max(0.01f, SanitizeFinite(heavyFlashDuration));
            heavyFlashScale = Mathf.Max(1f, SanitizeFinite(heavyFlashScale));
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
