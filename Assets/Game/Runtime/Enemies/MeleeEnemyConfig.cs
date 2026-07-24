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
        [SerializeField, Min(0f)] private float airAcceleration = 20f;
        [SerializeField, Min(0.1f)] private float jumpHeight = 3.4f;

        [Header("Ground Probe")]
        [SerializeField] private LayerMask groundLayers;
        [SerializeField] private LayerMask oneWayPlatformLayers;
        [SerializeField, Min(0.001f)] private float groundProbeDistance = 0.1f;
        [SerializeField, Range(0f, 89f)] private float maximumGroundAngle = 50f;

        [Header("Local Obstacle Traversal")]
        [SerializeField, Min(0.01f)] private float forwardObstacleProbeDistance = 0.2f;
        [SerializeField, Min(0f)] private float minimumJumpObstacleHeight = 0.15f;
        [SerializeField, Min(0f)] private float obstacleExitClearance = 0.12f;

        [Header("Platform Navigation")]
        [SerializeField, Min(0.1f)] private float nodeSnapDistance = 2.5f;
        [SerializeField, Min(0.01f)] private float waypointTolerance = 0.18f;
        [SerializeField, Min(0.05f)] private float replanInterval = 0.3f;
        [SerializeField, Min(0.05f)] private float stuckDuration = 0.8f;
        [SerializeField, Min(0.001f)] private float stuckMovementThreshold = 0.08f;
        [SerializeField, Min(0.05f)] private float dropThroughMinimumDuration = 0.18f;
        [SerializeField, Min(0.05f)] private float dropThroughMaximumDuration = 0.6f;
        [SerializeField, Min(0f)] private float dropThroughSpeed = 3.5f;
        [SerializeField, Min(0f)] private float dropThroughClearance = 0.08f;

        [Header("Combat Decision")]
        [SerializeField, Min(0f)] private float detectionRange = 18f;
        [SerializeField, Min(0.1f)] private float attackRange = 1.45f;
        [SerializeField, Min(0f)] private float attackVerticalTolerance = 1.15f;
        [SerializeField, Min(0f)] private float attackCooldown = 0.55f;

        [Header("Attack Telegraph")]
        [SerializeField] private Color telegraphWindupColor = new Color(1f, 0.75f, 0.15f, 0.8f);
        [SerializeField] private Color telegraphActiveColor = new Color(1f, 0.15f, 0.1f, 0.9f);

        internal float MovementSpeed => movementSpeed;
        internal float GroundAcceleration => groundAcceleration;
        internal float GroundDeceleration => groundDeceleration;
        internal float AirAcceleration => airAcceleration;
        internal float JumpHeight => jumpHeight;
        internal LayerMask GroundLayers => groundLayers;
        internal LayerMask OneWayPlatformLayers => oneWayPlatformLayers;
        internal float GroundProbeDistance => groundProbeDistance;
        internal float MaximumGroundAngle => maximumGroundAngle;
        internal float ForwardObstacleProbeDistance => forwardObstacleProbeDistance;
        internal float MinimumJumpObstacleHeight => minimumJumpObstacleHeight;
        internal float ObstacleExitClearance => obstacleExitClearance;
        internal float NodeSnapDistance => nodeSnapDistance;
        internal float WaypointTolerance => waypointTolerance;
        internal float ReplanInterval => replanInterval;
        internal float StuckDuration => stuckDuration;
        internal float StuckMovementThreshold => stuckMovementThreshold;
        internal float DropThroughMinimumDuration => dropThroughMinimumDuration;
        internal float DropThroughMaximumDuration => dropThroughMaximumDuration;
        internal float DropThroughSpeed => dropThroughSpeed;
        internal float DropThroughClearance => dropThroughClearance;
        internal float DetectionRange => detectionRange;
        internal float AttackRange => attackRange;
        internal float AttackVerticalTolerance => attackVerticalTolerance;
        internal float AttackCooldown => attackCooldown;
        internal Color TelegraphWindupColor => telegraphWindupColor;
        internal Color TelegraphActiveColor => telegraphActiveColor;

        private void OnValidate()
        {
            movementSpeed = Mathf.Max(0f, movementSpeed);
            jumpHeight = Mathf.Max(0.1f, jumpHeight);
            forwardObstacleProbeDistance = Mathf.Max(0.01f, forwardObstacleProbeDistance);
            minimumJumpObstacleHeight = Mathf.Clamp(
                minimumJumpObstacleHeight,
                0f,
                jumpHeight);
            obstacleExitClearance = Mathf.Max(0f, obstacleExitClearance);
            dropThroughMaximumDuration = Mathf.Max(
                dropThroughMinimumDuration,
                dropThroughMaximumDuration);
            attackRange = Mathf.Max(0.1f, attackRange);
        }
    }
}
