using UnityEngine;

namespace JustTest.Game.Player
{
    [CreateAssetMenu(fileName = "PlayerMovementConfig", menuName = "JustTest/Player/Movement Config")]
    public sealed class PlayerMovementConfig : ScriptableObject
    {
        [Header("Ground Movement")]
        [SerializeField, Min(0f)] private float maxGroundSpeed = 8f;
        [SerializeField, Min(0f)] private float groundAcceleration = 65f;
        [SerializeField, Min(0f)] private float groundDeceleration = 80f;
        [SerializeField, Min(0f)] private float groundedVerticalSpeed = 2f;
        [SerializeField, Min(0f)] private float groundedMaximumUpwardSpeed = 0.1f;

        [Header("Air Movement")]
        [SerializeField, Min(0f)] private float maxAirSpeed = 8f;
        [SerializeField, Min(0f)] private float airAcceleration = 40f;
        [SerializeField, Min(0f)] private float airDeceleration = 20f;

        [Header("Jump")]
        [SerializeField, Min(0.01f)] private float jumpHeight = 3f;
        [SerializeField, Min(0.01f)] private float timeToJumpApex = 0.32f;
        [SerializeField, Min(0f)] private float coyoteTime = 0.1f;
        [SerializeField, Min(0f)] private float jumpBufferTime = 0.1f;
        [SerializeField, Min(1f)] private float jumpReleaseGravityMultiplier = 2.2f;
        [SerializeField, Min(1f)] private float fallGravityMultiplier = 1.5f;
        [SerializeField, Min(0f)] private float maximumFallSpeed = 22f;

        [Header("Ground Probe")]
        [SerializeField] private LayerMask groundLayerMask;
        [SerializeField] private LayerMask oneWayPlatformLayerMask;
        [SerializeField, Min(0.001f)] private float groundProbeDistance = 0.08f;
        [SerializeField, Min(0f)] private float groundProbeHorizontalInset = 0.05f;
        [SerializeField, Range(0f, 89f)] private float maximumGroundAngle = 50f;

        [Header("One-way Platform")]
        [SerializeField, Min(0f)] private float dropThroughMinimumDuration = 0.15f;
        [SerializeField, Min(0f)] private float dropThroughMaximumDuration = 0.5f;
        [SerializeField, Min(0f)] private float dropThroughSpeed = 3f;
        [SerializeField, Min(0f)] private float dropThroughReenableClearance = 0.05f;

        internal float MaxGroundSpeed => maxGroundSpeed;
        internal float GroundAcceleration => groundAcceleration;
        internal float GroundDeceleration => groundDeceleration;
        internal float GroundedVerticalSpeed => groundedVerticalSpeed;
        internal float GroundedMaximumUpwardSpeed => groundedMaximumUpwardSpeed;
        internal float MaxAirSpeed => maxAirSpeed;
        internal float AirAcceleration => airAcceleration;
        internal float AirDeceleration => airDeceleration;
        internal float JumpHeight => jumpHeight;
        internal float TimeToJumpApex => timeToJumpApex;
        internal float CoyoteTime => coyoteTime;
        internal float JumpBufferTime => jumpBufferTime;
        internal float JumpReleaseGravityMultiplier => jumpReleaseGravityMultiplier;
        internal float FallGravityMultiplier => fallGravityMultiplier;
        internal float MaximumFallSpeed => maximumFallSpeed;
        internal LayerMask GroundLayerMask => groundLayerMask;
        internal LayerMask OneWayPlatformLayerMask => oneWayPlatformLayerMask;
        internal float GroundProbeDistance => groundProbeDistance;
        internal float GroundProbeHorizontalInset => groundProbeHorizontalInset;
        internal float MaximumGroundAngle => maximumGroundAngle;
        internal float DropThroughMinimumDuration => dropThroughMinimumDuration;
        internal float DropThroughMaximumDuration => dropThroughMaximumDuration;
        internal float DropThroughSpeed => dropThroughSpeed;
        internal float DropThroughReenableClearance => dropThroughReenableClearance;

        internal float GravityMagnitude => 2f * jumpHeight / (timeToJumpApex * timeToJumpApex);
        internal float JumpSpeed => GravityMagnitude * timeToJumpApex;

        private void OnValidate()
        {
            timeToJumpApex = Mathf.Max(0.01f, timeToJumpApex);
            dropThroughMaximumDuration = Mathf.Max(dropThroughMinimumDuration, dropThroughMaximumDuration);
        }
    }
}
