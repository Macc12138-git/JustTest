using System;
using JustTest.Game.Combat;
using UnityEngine;

namespace JustTest.Game.Weapons
{
    [Flags]
    public enum WeaponBasicComboMovementPhase
    {
        None = 0,
        Windup = 1 << 0,
        Active = 1 << 1,
        Recovery = 1 << 2
    }

    [Serializable]
    public sealed class WeaponBasicComboStep
    {
        private const WeaponBasicComboMovementPhase AllMovementPhases =
            WeaponBasicComboMovementPhase.Windup |
            WeaponBasicComboMovementPhase.Active |
            WeaponBasicComboMovementPhase.Recovery;

        [SerializeField] private AttackDefinition attack;
        [SerializeField] private Vector2 hitboxOffset = new Vector2(0.9f, 0f);
        [SerializeField] private Vector2 hitboxSize = new Vector2(1.2f, 1.2f);
        [SerializeField] private float forwardSpeed;
        [SerializeField] private WeaponBasicComboMovementPhase movementPhases =
            WeaponBasicComboMovementPhase.Active;
        [SerializeField, Min(0f)] private float inputBufferDuration = 0.18f;
        [SerializeField, Range(0f, 1f)] private float chainStartProgress = 0.35f;

        internal AttackDefinition Attack => attack;
        internal Vector2 HitboxOffset => hitboxOffset;
        internal Vector2 HitboxSize => hitboxSize;
        internal float ForwardSpeed => forwardSpeed;
        internal float InputBufferDuration => inputBufferDuration;
        internal float ChainStartProgress => chainStartProgress;

        internal bool IsValid =>
            attack != null &&
            IsFinite(hitboxOffset) &&
            IsFinitePositive(hitboxSize.x) &&
            IsFinitePositive(hitboxSize.y) &&
            IsFinite(forwardSpeed) &&
            (movementPhases & ~AllMovementPhases) == 0 &&
            IsFiniteNonNegative(inputBufferDuration) &&
            IsFinite(chainStartProgress) &&
            chainStartProgress >= 0f &&
            chainStartProgress <= 1f;

        internal bool UsesMovement(AttackPhase phase)
        {
            WeaponBasicComboMovementPhase requiredPhase = phase switch
            {
                AttackPhase.Windup => WeaponBasicComboMovementPhase.Windup,
                AttackPhase.Active => WeaponBasicComboMovementPhase.Active,
                AttackPhase.Recovery => WeaponBasicComboMovementPhase.Recovery,
                _ => WeaponBasicComboMovementPhase.None
            };
            return requiredPhase != WeaponBasicComboMovementPhase.None &&
                   (movementPhases & requiredPhase) != 0;
        }

        internal void Sanitize()
        {
            hitboxOffset = SanitizeFinite(hitboxOffset);
            hitboxSize = new Vector2(
                Mathf.Max(0.01f, SanitizeFinite(hitboxSize.x)),
                Mathf.Max(0.01f, SanitizeFinite(hitboxSize.y)));
            forwardSpeed = SanitizeFinite(forwardSpeed);
            movementPhases &= AllMovementPhases;
            inputBufferDuration = Mathf.Max(0f, SanitizeFinite(inputBufferDuration));
            chainStartProgress = Mathf.Clamp01(SanitizeFinite(chainStartProgress));
        }

        private static bool IsFinitePositive(float value)
        {
            return value > 0f && IsFinite(value);
        }

        private static bool IsFiniteNonNegative(float value)
        {
            return value >= 0f && IsFinite(value);
        }

        private static bool IsFinite(Vector2 value)
        {
            return IsFinite(value.x) && IsFinite(value.y);
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private static Vector2 SanitizeFinite(Vector2 value)
        {
            return new Vector2(SanitizeFinite(value.x), SanitizeFinite(value.y));
        }

        private static float SanitizeFinite(float value)
        {
            return IsFinite(value) ? value : 0f;
        }
    }

    [CreateAssetMenu(
        fileName = "WeaponBasicComboDefinition",
        menuName = "JustTest/Weapons/Basic Combo Definition")]
    public sealed class WeaponBasicComboDefinition : ScriptableObject
    {
        [SerializeField] private WeaponBasicComboStep[] steps =
            Array.Empty<WeaponBasicComboStep>();
        [SerializeField, Min(0f)] private float comboResetDelay = 0.35f;
        [SerializeField] private bool loopAfterFinalStep = true;

        internal int StepCount => steps?.Length ?? 0;
        internal float ComboResetDelay => comboResetDelay;
        internal bool LoopAfterFinalStep => loopAfterFinalStep;

        internal bool IsValid
        {
            get
            {
                if (steps == null || steps.Length == 0 || !IsFiniteNonNegative(comboResetDelay))
                {
                    return false;
                }

                for (int index = 0; index < steps.Length; index++)
                {
                    if (steps[index] == null || !steps[index].IsValid)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        internal WeaponBasicComboStep GetStep(int index)
        {
            return index >= 0 && index < StepCount ? steps[index] : null;
        }

        internal bool TryGetNextStepIndex(int currentStepIndex, out int nextStepIndex)
        {
            nextStepIndex = currentStepIndex + 1;
            if (nextStepIndex < StepCount)
            {
                return true;
            }

            if (loopAfterFinalStep && StepCount > 0)
            {
                nextStepIndex = 0;
                return true;
            }

            nextStepIndex = -1;
            return false;
        }

        private void OnValidate()
        {
            comboResetDelay = Mathf.Max(0f, SanitizeFinite(comboResetDelay));
            if (steps == null)
            {
                steps = Array.Empty<WeaponBasicComboStep>();
                return;
            }

            for (int index = 0; index < steps.Length; index++)
            {
                steps[index]?.Sanitize();
            }
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
