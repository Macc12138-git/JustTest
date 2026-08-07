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

        [Header("Target Assist")]
        [SerializeField] private bool targetAssistEnabled = true;
        [SerializeField, Min(0f)] private float targetLockDistance = 2.4f;
        [SerializeField, Min(0f)] private float targetRetainDistance = 3.2f;
        [SerializeField, Min(0f)] private float targetRetentionDuration = 0.45f;
        [SerializeField, Min(0f)] private float maximumTargetVerticalDifference = 0.75f;
        [SerializeField, Min(0f)] private float targetOverlapDepth = 0.12f;
        [SerializeField, Min(0f)] private float maximumAssistTravelDistance = 1.1f;
        [SerializeField, Min(0f)] private float maximumWarpSpeed = 7f;
        [SerializeField, Range(0f, 1f)] private float retargetThreshold = 0.65f;
        [SerializeField, Min(0f)] private float rearTargetPenalty = 0.4f;
        [SerializeField, Range(0f, 1f)] private float windupCorrectionStrength = 1f;
        [SerializeField, Range(0f, 1f)] private float activeCorrectionStrength = 0.35f;
        [SerializeField] private bool allowAutoTurn = true;
        [SerializeField] private bool allowDirectionalRetarget = true;
        [SerializeField] private AnimationCurve movementCurve = CreateDefaultMovementCurve();

        [Header("Combo")]
        [SerializeField, Min(0f)] private float inputBufferDuration = 0.18f;
        [SerializeField, Range(0f, 1f)] private float chainStartProgress = 0.35f;

        internal AttackDefinition Attack => attack;
        internal Vector2 HitboxOffset => hitboxOffset;
        internal Vector2 HitboxSize => hitboxSize;
        internal float ForwardSpeed => forwardSpeed;
        internal bool TargetAssistEnabled => targetAssistEnabled;
        internal float TargetLockDistance => targetLockDistance;
        internal float TargetRetainDistance => targetRetainDistance;
        internal float TargetRetentionDuration => targetRetentionDuration;
        internal float MaximumTargetVerticalDifference => maximumTargetVerticalDifference;
        internal float TargetOverlapDepth => targetOverlapDepth;
        internal float MaximumAssistTravelDistance => maximumAssistTravelDistance;
        internal float MaximumWarpSpeed => maximumWarpSpeed;
        internal float RetargetThreshold => retargetThreshold;
        internal float RearTargetPenalty => rearTargetPenalty;
        internal bool AllowAutoTurn => allowAutoTurn;
        internal bool AllowDirectionalRetarget => allowDirectionalRetarget;
        internal float InputBufferDuration => inputBufferDuration;
        internal float ChainStartProgress => chainStartProgress;

        internal bool IsValid =>
            attack != null &&
            IsFinite(hitboxOffset) &&
            IsFinitePositive(hitboxSize.x) &&
            IsFinitePositive(hitboxSize.y) &&
            IsFinite(forwardSpeed) &&
            (movementPhases & ~AllMovementPhases) == 0 &&
            IsFiniteNonNegative(targetLockDistance) &&
            IsFiniteNonNegative(targetRetainDistance) &&
            targetRetainDistance >= targetLockDistance &&
            IsFiniteNonNegative(targetRetentionDuration) &&
            IsFiniteNonNegative(maximumTargetVerticalDifference) &&
            IsFiniteNonNegative(targetOverlapDepth) &&
            IsFiniteNonNegative(maximumAssistTravelDistance) &&
            IsFiniteNonNegative(maximumWarpSpeed) &&
            IsFinite(retargetThreshold) &&
            retargetThreshold >= 0f &&
            retargetThreshold <= 1f &&
            IsFiniteNonNegative(rearTargetPenalty) &&
            IsFinite(windupCorrectionStrength) &&
            windupCorrectionStrength >= 0f &&
            windupCorrectionStrength <= 1f &&
            IsFinite(activeCorrectionStrength) &&
            activeCorrectionStrength >= 0f &&
            activeCorrectionStrength <= 1f &&
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

        internal float EvaluateMovementCurve(AttackPhase phase, float phaseProgress)
        {
            float normalizedProgress = CalculateMovementProgress(phase, phaseProgress);
            if (movementCurve == null || movementCurve.length == 0)
            {
                return Mathf.SmoothStep(0f, 1f, normalizedProgress);
            }

            return Mathf.Clamp01(movementCurve.Evaluate(normalizedProgress));
        }

        internal float GetTargetCorrectionStrength(AttackPhase phase)
        {
            return phase switch
            {
                AttackPhase.Windup => windupCorrectionStrength,
                AttackPhase.Active => activeCorrectionStrength,
                _ => 0f
            };
        }

        internal void Sanitize()
        {
            hitboxOffset = SanitizeFinite(hitboxOffset);
            hitboxSize = new Vector2(
                Mathf.Max(0.01f, SanitizeFinite(hitboxSize.x)),
                Mathf.Max(0.01f, SanitizeFinite(hitboxSize.y)));
            forwardSpeed = SanitizeFinite(forwardSpeed);
            movementPhases &= AllMovementPhases;
            targetLockDistance = Mathf.Max(0f, SanitizeFinite(targetLockDistance));
            targetRetainDistance = Mathf.Max(
                targetLockDistance,
                SanitizeFinite(targetRetainDistance));
            targetRetentionDuration = Mathf.Max(
                0f,
                SanitizeFinite(targetRetentionDuration));
            maximumTargetVerticalDifference = Mathf.Max(
                0f,
                SanitizeFinite(maximumTargetVerticalDifference));
            targetOverlapDepth = Mathf.Max(0f, SanitizeFinite(targetOverlapDepth));
            maximumAssistTravelDistance = Mathf.Max(
                0f,
                SanitizeFinite(maximumAssistTravelDistance));
            maximumWarpSpeed = Mathf.Max(0f, SanitizeFinite(maximumWarpSpeed));
            retargetThreshold = Mathf.Clamp01(SanitizeFinite(retargetThreshold));
            rearTargetPenalty = Mathf.Max(0f, SanitizeFinite(rearTargetPenalty));
            windupCorrectionStrength = Mathf.Clamp01(
                SanitizeFinite(windupCorrectionStrength));
            activeCorrectionStrength = Mathf.Clamp01(
                SanitizeFinite(activeCorrectionStrength));
            movementCurve ??= CreateDefaultMovementCurve();
            inputBufferDuration = Mathf.Max(0f, SanitizeFinite(inputBufferDuration));
            chainStartProgress = Mathf.Clamp01(SanitizeFinite(chainStartProgress));
        }

        private float CalculateMovementProgress(AttackPhase phase, float phaseProgress)
        {
            if (attack == null || !UsesMovement(phase))
            {
                return 0f;
            }

            float windup = UsesMovement(AttackPhase.Windup) ? attack.WindupDuration : 0f;
            float active = UsesMovement(AttackPhase.Active) ? attack.ActiveDuration : 0f;
            float recovery = UsesMovement(AttackPhase.Recovery) ? attack.RecoveryDuration : 0f;
            float total = windup + active + recovery;
            if (total <= 0f)
            {
                return 0f;
            }

            float elapsed = phase switch
            {
                AttackPhase.Windup => windup * Mathf.Clamp01(phaseProgress),
                AttackPhase.Active => windup + active * Mathf.Clamp01(phaseProgress),
                AttackPhase.Recovery =>
                    windup + active + recovery * Mathf.Clamp01(phaseProgress),
                _ => 0f
            };
            return Mathf.Clamp01(elapsed / total);
        }

        private static AnimationCurve CreateDefaultMovementCurve()
        {
            return new AnimationCurve(
                new Keyframe(0f, 0f, 0f, 0f),
                new Keyframe(0.68f, 1f, 0f, 0f),
                new Keyframe(1f, 1f, 0f, 0f));
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
