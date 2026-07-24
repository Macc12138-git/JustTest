using JustTest.Game.Combat;
using JustTest.Game.Player;
using JustTest.Game.Weapons;
using UnityEngine;

namespace JustTest.Game.Presentation
{
    public sealed class PlayerWhiteboxMotionPresenter2D : MonoBehaviour
    {
        [SerializeField] private PlayerMovementController movementController;
        [SerializeField] private PlayerRollController rollController;
        [SerializeField] private PlayerAttackRunner attackRunner;
        [SerializeField] private PlayerWeaponLoadout weaponLoadout;
        [SerializeField] private CombatReactionReceiver reactionReceiver;
        [SerializeField] private CharacterVisualRig2D visualRig;
        [SerializeField] private LocomotionMotionConfig locomotionConfig;
        [SerializeField] private WeaponMotionProfileBinding[] weaponProfiles;

        private float landingStartedAt = float.NegativeInfinity;
        private bool wasGrounded;
        private bool ready;

        public string CurrentMotionState { get; private set; } = "Not Ready";

        private void Awake()
        {
            ready =
                movementController != null &&
                rollController != null &&
                attackRunner != null &&
                weaponLoadout != null &&
                reactionReceiver != null &&
                visualRig != null &&
                locomotionConfig != null &&
                weaponProfiles != null &&
                weaponProfiles.Length > 0;
            if (!ready)
            {
                Debug.LogError($"{nameof(PlayerWhiteboxMotionPresenter2D)} is missing an Inspector reference.", this);
                enabled = false;
                return;
            }

            wasGrounded = movementController.IsGrounded;
        }

        private void LateUpdate()
        {
            if (!ready)
            {
                return;
            }

            bool grounded = movementController.IsGrounded;
            if (grounded && !wasGrounded)
            {
                landingStartedAt = Time.time;
            }
            wasGrounded = grounded;

            int facingDirection = attackRunner.IsAttacking
                ? attackRunner.CurrentAttackDirection
                : movementController.FacingDirection;
            visualRig.SetFacing(facingDirection);
            CombatMotionProfile activeProfile = ResolveActiveWeaponProfile();
            visualRig.ApplyWeaponStyle(activeProfile);
            EvaluatedMotionPose2D pose = ResolvePose(activeProfile, grounded);
            visualRig.ApplyPose(pose, locomotionConfig.BlendSpeed, Time.deltaTime);
        }

        private EvaluatedMotionPose2D ResolvePose(
            CombatMotionProfile activeProfile,
            bool grounded)
        {
            if (reactionReceiver.IsReacting)
            {
                CurrentMotionState = "Hit Reaction";
                float direction = Mathf.Sign(reactionReceiver.CurrentReaction.KnockbackVelocity.x);
                return BuildBodyPose(
                    Vector2.zero,
                    -locomotionConfig.HitReactionLeanAngle * direction,
                    locomotionConfig.HitReactionScale);
            }

            if (attackRunner.IsAttacking && activeProfile != null)
            {
                CurrentMotionState = $"Attack {attackRunner.Phase}";
                return activeProfile.Evaluate(attackRunner.Phase, attackRunner.PhaseProgress);
            }

            if (rollController.IsRolling)
            {
                CurrentMotionState = "Roll";
                float rotation = locomotionConfig.RollRotation * rollController.NormalizedTime;
                return new EvaluatedMotionPose2D(
                    Vector2.zero,
                    rotation,
                    locomotionConfig.RollScale,
                    Vector2.zero,
                    rotation,
                    Vector2.one,
                    Vector2.zero,
                    rotation,
                    Vector2.one);
            }

            float landingProgress = (Time.time - landingStartedAt) / locomotionConfig.LandingDuration;
            if (grounded && landingProgress >= 0f && landingProgress < 1f)
            {
                CurrentMotionState = "Landing";
                float strength = 1f - landingProgress;
                return BuildBodyPose(
                    Vector2.up * (locomotionConfig.LandingBodyOffset * strength),
                    0f,
                    Vector2.Lerp(Vector2.one, locomotionConfig.LandingScale, strength));
            }

            Vector2 velocity = movementController.Velocity;
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

        private CombatMotionProfile ResolveActiveWeaponProfile()
        {
            WeaponDefinition activeWeapon = weaponLoadout.ActiveWeapon;
            int stepIndex = attackRunner.IsAttacking
                ? attackRunner.CurrentComboStepIndex
                : 0;
            for (int index = 0; index < weaponProfiles.Length; index++)
            {
                WeaponMotionProfileBinding binding = weaponProfiles[index];
                if (binding != null && binding.Weapon == activeWeapon)
                {
                    return binding.GetBasicComboProfile(stepIndex);
                }
            }

            return null;
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
