using System.Collections.Generic;
using JustTest.Game.Combat;
using JustTest.Game.Player;
using JustTest.Game.Weapons;
using UnityEngine;

namespace JustTest.Game.Presentation
{
    [DefaultExecutionOrder(20)]
    public sealed class CharacterAnimationPresenter2D : MonoBehaviour
    {
        [SerializeField] private PlayerMovementController movementController;
        [SerializeField] private PlayerRollController rollController;
        [SerializeField] private PlayerAttackRunner attackRunner;
        [SerializeField] private PlayerWeaponLoadout weaponLoadout;
        [SerializeField] private PlayerWeaponSkillRunner skillRunner;
        [SerializeField] private PlayerWeaponQteExecutor qteExecutor;
        [SerializeField] private CombatReactionReceiver reactionReceiver;
        [SerializeField] private HealthComponent health;
        [SerializeField] private CharacterModelView2D modelView;
        [SerializeField] private CharacterAppearanceDefinition appearance;

        private readonly HashSet<int> missingStateWarnings = new HashSet<int>();

        private Animator animator;
        private float landingStartedAt = float.NegativeInfinity;
        private int currentStateHash;
        private bool wasGrounded;
        private bool ready;

        public string CurrentAnimationState { get; private set; } = "Not Ready";

        private void Awake()
        {
            ready =
                movementController != null &&
                rollController != null &&
                attackRunner != null &&
                weaponLoadout != null &&
                skillRunner != null &&
                qteExecutor != null &&
                reactionReceiver != null &&
                health != null &&
                modelView != null &&
                appearance != null &&
                appearance.IsValid;
            animator = modelView != null ? modelView.Animator : null;
            ready = ready && animator != null && animator.runtimeAnimatorController != null;
            if (!ready)
            {
                Debug.LogError($"{nameof(CharacterAnimationPresenter2D)} is missing an Inspector reference.", this);
                enabled = false;
                return;
            }

            wasGrounded = movementController.IsGrounded;
            modelView.SetModelVisible(appearance.UseModelByDefault);
            ApplyWeaponPresentation();
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

            modelView.SetFacing(ResolveFacingDirection());
            ApplyWeaponPresentation();
            if (!modelView.IsModelVisible)
            {
                return;
            }

            if (health.IsDead)
            {
                PlayContinuous(appearance.DeadState);
                return;
            }

            if (reactionReceiver.IsReacting)
            {
                string reactionState =
                    reactionReceiver.CurrentReaction.HitStunDuration >=
                    appearance.ControlledHitStunThreshold
                        ? appearance.ControlledState
                        : appearance.HurtState;
                PlayContinuous(reactionState);
                return;
            }

            if (qteExecutor.IsExecuting)
            {
                if (qteExecutor.Phase == WeaponQteExecutionPhase.Approach)
                {
                    PlayContinuous(appearance.QteApproachState);
                }
                else
                {
                    PlayAttack(
                        qteExecutor.CurrentAttackDefinition,
                        qteExecutor.StrikePhase,
                        qteExecutor.StrikePhaseProgress);
                }

                return;
            }

            if (skillRunner.IsExecuting)
            {
                PlayAttack(
                    skillRunner.CurrentAttackDefinition,
                    skillRunner.Phase,
                    skillRunner.PhaseProgress);
                return;
            }

            if (attackRunner.IsAttacking)
            {
                PlayAttack(
                    attackRunner.CurrentDefinition,
                    attackRunner.Phase,
                    attackRunner.PhaseProgress);
                return;
            }

            if (rollController.IsRolling)
            {
                PlaySampled(appearance.RollState, rollController.NormalizedTime);
                return;
            }

            float landingProgress =
                (Time.time - landingStartedAt) / appearance.LandingDuration;
            if (grounded && landingProgress >= 0f && landingProgress < 1f)
            {
                PlaySampled(appearance.LandState, landingProgress);
                return;
            }

            Vector2 velocity = movementController.Velocity;
            if (!grounded)
            {
                PlayContinuous(velocity.y >= 0f ? appearance.JumpState : appearance.FallState);
                return;
            }

            PlayContinuous(
                Mathf.Abs(velocity.x) > appearance.MovementSpeedThreshold
                    ? appearance.RunState
                    : appearance.IdleState);
        }

        private void OnDisable()
        {
            if (animator != null)
            {
                animator.speed = 1f;
            }
        }

        private int ResolveFacingDirection()
        {
            if (qteExecutor.IsExecuting)
            {
                return qteExecutor.CurrentAttackDirection;
            }

            if (skillRunner.IsExecuting)
            {
                return skillRunner.CurrentAttackDirection;
            }

            return attackRunner.IsAttacking
                ? attackRunner.CurrentAttackDirection
                : movementController.FacingDirection;
        }

        private void ApplyWeaponPresentation()
        {
            WeaponDefinition weapon = qteExecutor.IsExecuting
                ? qteExecutor.PendingWeapon
                : weaponLoadout.ActiveWeapon;
            modelView.WeaponVisual.Apply(appearance.ResolveWeapon(weapon));
        }

        private void PlayAttack(
            AttackDefinition attack,
            AttackPhase phase,
            float phaseProgress)
        {
            AttackAnimationBinding binding = appearance.ResolveAttack(attack);
            string stateName = binding != null
                ? binding.StateName
                : appearance.FallbackAttackState;
            float normalizedTime = binding != null
                ? binding.EvaluateNormalizedTime(phase, phaseProgress)
                : EvaluateFallbackAttackTime(phase, phaseProgress);
            PlaySampled(stateName, normalizedTime);
        }

        private void PlayContinuous(string stateName)
        {
            int stateHash = Animator.StringToHash(stateName);
            if (!CanPlay(stateHash, stateName))
            {
                return;
            }

            animator.speed = 1f;
            if (currentStateHash == stateHash)
            {
                return;
            }

            currentStateHash = stateHash;
            CurrentAnimationState = stateName;
            animator.CrossFadeInFixedTime(
                stateHash,
                appearance.LocomotionBlendDuration,
                0,
                0f);
        }

        private void PlaySampled(string stateName, float normalizedTime)
        {
            int stateHash = Animator.StringToHash(stateName);
            if (!CanPlay(stateHash, stateName))
            {
                return;
            }

            animator.speed = 0f;
            currentStateHash = stateHash;
            CurrentAnimationState = stateName;
            animator.Play(stateHash, 0, Mathf.Clamp01(normalizedTime));
        }

        private bool CanPlay(int stateHash, string stateName)
        {
            if (animator.HasState(0, stateHash))
            {
                return true;
            }

            if (missingStateWarnings.Add(stateHash))
            {
                Debug.LogWarning(
                    $"{nameof(CharacterAnimationPresenter2D)} could not find Animator state '{stateName}'.",
                    this);
            }

            return false;
        }

        private static float EvaluateFallbackAttackTime(
            AttackPhase phase,
            float phaseProgress)
        {
            float progress = Mathf.Clamp01(phaseProgress);
            switch (phase)
            {
                case AttackPhase.Windup:
                    return Mathf.Lerp(0f, 0.3f, progress);
                case AttackPhase.Active:
                    return Mathf.Lerp(0.3f, 0.65f, progress);
                case AttackPhase.Recovery:
                    return Mathf.Lerp(0.65f, 1f, progress);
                default:
                    return 0f;
            }
        }
    }
}
