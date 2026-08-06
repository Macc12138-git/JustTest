using JustTest.Game.Combat;
using JustTest.Game.Enemies;
using UnityEngine;

namespace JustTest.Game.Presentation
{
    public sealed class EliteEnemyWhiteboxMotionPresenter2D : MonoBehaviour
    {
        [SerializeField] private EliteEnemyMotor2D motor;
        [SerializeField] private EnemyAttackRunner attackRunner;
        [SerializeField] private CombatReactionReceiver reactionReceiver;
        [SerializeField] private CombatStatusController statusController;
        [SerializeField] private HealthComponent health;
        [SerializeField] private CharacterVisualRig2D visualRig;
        [SerializeField] private LocomotionMotionConfig locomotionConfig;
        [SerializeField] private AttackMotionProfileBinding[] attackProfiles;

        private float landingStartedAt = float.NegativeInfinity;
        private bool wasGrounded;
        private bool ready;

        public string CurrentMotionState { get; private set; } = "Not Ready";

        private void Awake()
        {
            ready =
                motor != null &&
                attackRunner != null &&
                reactionReceiver != null &&
                statusController != null &&
                health != null &&
                visualRig != null &&
                locomotionConfig != null &&
                attackProfiles != null &&
                attackProfiles.Length > 0;
            if (!ready)
            {
                Debug.LogError($"{nameof(EliteEnemyWhiteboxMotionPresenter2D)} is missing an Inspector reference.", this);
                enabled = false;
                return;
            }

            wasGrounded = motor.IsGrounded;
        }

        private void LateUpdate()
        {
            if (!ready)
            {
                return;
            }

            bool grounded = motor.IsGrounded;
            if (grounded && !wasGrounded)
            {
                landingStartedAt = Time.time;
            }
            wasGrounded = grounded;

            visualRig.SetFacing(motor.FacingDirection);
            CombatMotionProfile activeProfile = ResolveMotionProfile();
            visualRig.ApplyWeaponStyle(activeProfile);
            visualRig.ApplyPose(
                ResolvePose(activeProfile, grounded),
                locomotionConfig.BlendSpeed,
                Time.deltaTime);
        }

        private EvaluatedMotionPose2D ResolvePose(
            CombatMotionProfile activeProfile,
            bool grounded)
        {
            if (health.IsDead)
            {
                CurrentMotionState = "Dead";
                return BuildBodyPose(
                    Vector2.up * locomotionConfig.DeathBodyOffset,
                    locomotionConfig.DeathRotation,
                    locomotionConfig.DeathScale);
            }

            if (reactionReceiver.IsReacting)
            {
                CurrentMotionState = "Hit Reaction";
                float reactionDirection = Mathf.Sign(
                    reactionReceiver.CurrentReaction.KnockbackVelocity.x);
                if (Mathf.Approximately(reactionDirection, 0f))
                {
                    reactionDirection = motor.FacingDirection;
                }

                return BuildBodyPose(
                    Vector2.zero,
                    -locomotionConfig.HitReactionLeanAngle * reactionDirection,
                    locomotionConfig.HitReactionScale);
            }

            if (statusController.ActiveStatusCount > 0)
            {
                CurrentMotionState = "Controlled";
                float wave = Mathf.Sin(
                    Time.time * locomotionConfig.ControlledWobbleFrequency);
                return BuildBodyPose(
                    Vector2.zero,
                    locomotionConfig.ControlledTiltAngle * wave,
                    locomotionConfig.ControlledScale);
            }

            if (attackRunner.IsAttacking && activeProfile != null)
            {
                CurrentMotionState = $"Attack {attackRunner.Phase}";
                return activeProfile.Evaluate(attackRunner.Phase, attackRunner.PhaseProgress);
            }

            float landingProgress =
                (Time.time - landingStartedAt) / locomotionConfig.LandingDuration;
            if (grounded && landingProgress >= 0f && landingProgress < 1f)
            {
                CurrentMotionState = "Landing";
                float strength = 1f - landingProgress;
                return BuildBodyPose(
                    Vector2.up * (locomotionConfig.LandingBodyOffset * strength),
                    0f,
                    Vector2.Lerp(Vector2.one, locomotionConfig.LandingScale, strength));
            }

            Vector2 velocity = motor.Velocity;
            if (!grounded)
            {
                bool jumping = velocity.y >= 0f;
                CurrentMotionState = jumping ? "Jump" : "Fall";
                return BuildBodyPose(
                    Vector2.up * (jumping
                        ? locomotionConfig.JumpBodyOffset
                        : locomotionConfig.FallBodyOffset),
                    0f,
                    jumping ? locomotionConfig.JumpScale : locomotionConfig.FallScale);
            }

            if (Mathf.Abs(velocity.x) > locomotionConfig.MovementSpeedThreshold)
            {
                CurrentMotionState = "Run";
                float wave = Mathf.Sin(Time.time * locomotionConfig.RunBobFrequency);
                float stretch = 1f + Mathf.Abs(wave) * locomotionConfig.RunStretch;
                return BuildBodyPose(
                    Vector2.up * (Mathf.Abs(wave) * locomotionConfig.RunBobAmplitude),
                    locomotionConfig.RunLeanAngle,
                    new Vector2(1f / stretch, stretch));
            }

            CurrentMotionState = "Idle";
            float idleWave = Mathf.Sin(Time.time * locomotionConfig.IdleBobFrequency);
            return BuildBodyPose(
                Vector2.up * (idleWave * locomotionConfig.IdleBobAmplitude),
                0f,
                new Vector2(
                    1f - idleWave * locomotionConfig.IdleBreathingScale,
                    1f + idleWave * locomotionConfig.IdleBreathingScale));
        }

        private CombatMotionProfile ResolveMotionProfile()
        {
            AttackDefinition activeAttack = attackRunner.CurrentDefinition;
            CombatMotionProfile fallbackProfile = null;
            for (int index = 0; index < attackProfiles.Length; index++)
            {
                AttackMotionProfileBinding binding = attackProfiles[index];
                if (binding == null || binding.MotionProfile == null)
                {
                    continue;
                }

                fallbackProfile ??= binding.MotionProfile;
                if (binding.Attack == activeAttack)
                {
                    return binding.MotionProfile;
                }
            }

            return fallbackProfile;
        }

        private static EvaluatedMotionPose2D BuildBodyPose(
            Vector2 offset,
            float rotation,
            Vector2 scale)
        {
            return new EvaluatedMotionPose2D(
                offset,
                rotation,
                scale,
                Vector2.zero,
                0f,
                Vector2.one,
                Vector2.zero,
                0f,
                Vector2.one);
        }
    }
}
