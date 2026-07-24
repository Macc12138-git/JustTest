using UnityEngine;

namespace JustTest.Game.Presentation
{
    [CreateAssetMenu(fileName = "LocomotionMotionConfig", menuName = "JustTest/Presentation/Locomotion Motion Config")]
    public sealed class LocomotionMotionConfig : ScriptableObject
    {
        [Header("Blending")]
        [SerializeField, Min(0.1f)] private float blendSpeed = 18f;
        [SerializeField, Min(0f)] private float movementSpeedThreshold = 0.1f;

        [Header("Idle")]
        [SerializeField, Min(0f)] private float idleBobAmplitude = 0.035f;
        [SerializeField, Min(0f)] private float idleBobFrequency = 2.2f;
        [SerializeField, Min(0f)] private float idleBreathingScale = 0.025f;

        [Header("Run")]
        [SerializeField, Min(0f)] private float runBobAmplitude = 0.08f;
        [SerializeField, Min(0f)] private float runBobFrequency = 10f;
        [SerializeField] private float runLeanAngle = -7f;
        [SerializeField, Min(0f)] private float runStretch = 0.05f;

        [Header("Airborne")]
        [SerializeField] private Vector2 jumpScale = new Vector2(0.88f, 1.12f);
        [SerializeField] private Vector2 fallScale = new Vector2(1.08f, 0.92f);
        [SerializeField] private float jumpBodyOffset = 0.06f;
        [SerializeField] private float fallBodyOffset = -0.04f;

        [Header("Landing")]
        [SerializeField, Min(0.01f)] private float landingDuration = 0.12f;
        [SerializeField] private Vector2 landingScale = new Vector2(1.16f, 0.82f);
        [SerializeField] private float landingBodyOffset = -0.08f;

        [Header("Roll")]
        [SerializeField] private float rollRotation = -360f;
        [SerializeField] private Vector2 rollScale = new Vector2(1.05f, 0.92f);

        [Header("Reaction")]
        [SerializeField] private float hitReactionLeanAngle = 16f;
        [SerializeField] private Vector2 hitReactionScale = new Vector2(1.08f, 0.92f);
        [SerializeField] private float controlledTiltAngle = 10f;
        [SerializeField] private Vector2 controlledScale = new Vector2(1.1f, 0.88f);
        [SerializeField, Min(0f)] private float controlledWobbleFrequency = 18f;

        [Header("Death")]
        [SerializeField] private float deathRotation = 90f;
        [SerializeField] private Vector2 deathScale = new Vector2(1.15f, 0.75f);
        [SerializeField] private float deathBodyOffset = -0.45f;

        internal float BlendSpeed => blendSpeed;
        internal float MovementSpeedThreshold => movementSpeedThreshold;
        internal float IdleBobAmplitude => idleBobAmplitude;
        internal float IdleBobFrequency => idleBobFrequency;
        internal float IdleBreathingScale => idleBreathingScale;
        internal float RunBobAmplitude => runBobAmplitude;
        internal float RunBobFrequency => runBobFrequency;
        internal float RunLeanAngle => runLeanAngle;
        internal float RunStretch => runStretch;
        internal Vector2 JumpScale => jumpScale;
        internal Vector2 FallScale => fallScale;
        internal float JumpBodyOffset => jumpBodyOffset;
        internal float FallBodyOffset => fallBodyOffset;
        internal float LandingDuration => landingDuration;
        internal Vector2 LandingScale => landingScale;
        internal float LandingBodyOffset => landingBodyOffset;
        internal float RollRotation => rollRotation;
        internal Vector2 RollScale => rollScale;
        internal float HitReactionLeanAngle => hitReactionLeanAngle;
        internal Vector2 HitReactionScale => hitReactionScale;
        internal float ControlledTiltAngle => controlledTiltAngle;
        internal Vector2 ControlledScale => controlledScale;
        internal float ControlledWobbleFrequency => controlledWobbleFrequency;
        internal float DeathRotation => deathRotation;
        internal Vector2 DeathScale => deathScale;
        internal float DeathBodyOffset => deathBodyOffset;

        private void OnValidate()
        {
            blendSpeed = Mathf.Max(0.1f, blendSpeed);
            movementSpeedThreshold = Mathf.Max(0f, movementSpeedThreshold);
            landingDuration = Mathf.Max(0.01f, landingDuration);
            controlledWobbleFrequency = Mathf.Max(0f, controlledWobbleFrequency);
        }
    }
}
