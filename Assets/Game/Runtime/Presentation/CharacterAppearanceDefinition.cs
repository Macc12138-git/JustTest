using System;
using JustTest.Game.Combat;
using JustTest.Game.Weapons;
using UnityEngine;

namespace JustTest.Game.Presentation
{
    [CreateAssetMenu(
        fileName = "CharacterAppearanceDefinition",
        menuName = "JustTest/Presentation/Character Appearance")]
    public sealed class CharacterAppearanceDefinition : ScriptableObject
    {
        [Header("View")]
        [SerializeField] private bool useModelByDefault = true;
        [SerializeField, Min(0f)] private float locomotionBlendDuration = 0.06f;
        [SerializeField, Min(0.01f)] private float movementSpeedThreshold = 0.1f;
        [SerializeField, Min(0.01f)] private float landingDuration = 0.12f;
        [SerializeField, Min(0f)] private float controlledHitStunThreshold = 0.2f;

        [Header("States")]
        [SerializeField] private string idleState = "Idle";
        [SerializeField] private string runState = "Run";
        [SerializeField] private string jumpState = "Jump";
        [SerializeField] private string fallState = "Fall";
        [SerializeField] private string landState = "Land";
        [SerializeField] private string rollState = "Roll";
        [SerializeField] private string hurtState = "Hurt";
        [SerializeField] private string controlledState = "Controlled";
        [SerializeField] private string deadState = "Dead";
        [SerializeField] private string qteApproachState = "QteApproach";
        [SerializeField] private string fallbackAttackState = "AttackFallback";

        [Header("Bindings")]
        [SerializeField] private AttackAnimationBinding[] attackAnimations =
            Array.Empty<AttackAnimationBinding>();
        [SerializeField] private WeaponPresentationDefinition[] weaponPresentations =
            Array.Empty<WeaponPresentationDefinition>();

        internal bool UseModelByDefault => useModelByDefault;
        internal float LocomotionBlendDuration => locomotionBlendDuration;
        internal float MovementSpeedThreshold => movementSpeedThreshold;
        internal float LandingDuration => landingDuration;
        internal float ControlledHitStunThreshold => controlledHitStunThreshold;
        internal string IdleState => idleState;
        internal string RunState => runState;
        internal string JumpState => jumpState;
        internal string FallState => fallState;
        internal string LandState => landState;
        internal string RollState => rollState;
        internal string HurtState => hurtState;
        internal string ControlledState => controlledState;
        internal string DeadState => deadState;
        internal string QteApproachState => qteApproachState;
        internal string FallbackAttackState => fallbackAttackState;

        internal bool IsValid =>
            locomotionBlendDuration >= 0f &&
            movementSpeedThreshold > 0f &&
            landingDuration > 0f &&
            controlledHitStunThreshold >= 0f &&
            HasStateNames() &&
            HasValidAttackBindings() &&
            HasValidWeaponPresentations();

        internal AttackAnimationBinding ResolveAttack(AttackDefinition attack)
        {
            if (attack == null || attackAnimations == null)
            {
                return null;
            }

            for (int index = 0; index < attackAnimations.Length; index++)
            {
                AttackAnimationBinding binding = attackAnimations[index];
                if (binding != null && binding.Attack == attack)
                {
                    return binding;
                }
            }

            return null;
        }

        internal WeaponPresentationDefinition ResolveWeapon(WeaponDefinition weapon)
        {
            if (weapon == null || weaponPresentations == null)
            {
                return null;
            }

            for (int index = 0; index < weaponPresentations.Length; index++)
            {
                WeaponPresentationDefinition presentation = weaponPresentations[index];
                if (presentation != null && presentation.Weapon == weapon)
                {
                    return presentation;
                }
            }

            return null;
        }

        private bool HasStateNames()
        {
            return
                !string.IsNullOrWhiteSpace(idleState) &&
                !string.IsNullOrWhiteSpace(runState) &&
                !string.IsNullOrWhiteSpace(jumpState) &&
                !string.IsNullOrWhiteSpace(fallState) &&
                !string.IsNullOrWhiteSpace(landState) &&
                !string.IsNullOrWhiteSpace(rollState) &&
                !string.IsNullOrWhiteSpace(hurtState) &&
                !string.IsNullOrWhiteSpace(controlledState) &&
                !string.IsNullOrWhiteSpace(deadState) &&
                !string.IsNullOrWhiteSpace(qteApproachState) &&
                !string.IsNullOrWhiteSpace(fallbackAttackState);
        }

        private bool HasValidAttackBindings()
        {
            if (attackAnimations == null)
            {
                return false;
            }

            for (int index = 0; index < attackAnimations.Length; index++)
            {
                if (attackAnimations[index] == null || !attackAnimations[index].IsValid)
                {
                    return false;
                }
            }

            return true;
        }

        private bool HasValidWeaponPresentations()
        {
            if (weaponPresentations == null || weaponPresentations.Length == 0)
            {
                return false;
            }

            for (int index = 0; index < weaponPresentations.Length; index++)
            {
                if (weaponPresentations[index] == null || !weaponPresentations[index].IsValid)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
