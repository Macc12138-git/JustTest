using UnityEngine;

namespace JustTest.Game.Presentation
{
    public sealed class CombatAttackRecoil2D : MonoBehaviour
    {
        [SerializeField] private CharacterVisualRig2D visualRig;
        [SerializeField] private CharacterModelView2D modelView;

        private AnimationCurve recoveryCurve;
        private float bodyDistance;
        private float weaponRotation;
        private float duration;
        private float elapsed;
        private int attackDirection = 1;
        private bool active;
        private bool ready;

        private void Awake()
        {
            ready = visualRig != null || modelView != null;
            if (ready)
            {
                return;
            }

            Debug.LogError($"{nameof(CombatAttackRecoil2D)} is missing an Inspector reference.", this);
            enabled = false;
        }

        private void Update()
        {
            if (!ready || !active || Time.deltaTime <= 0f)
            {
                return;
            }

            elapsed = Mathf.Min(duration, elapsed + Time.deltaTime);
            float progress = duration > 0f ? Mathf.Clamp01(elapsed / duration) : 1f;
            float strength = recoveryCurve != null
                ? Mathf.Clamp01(recoveryCurve.Evaluate(progress))
                : 1f - progress;
            ApplyPose(strength);
            if (elapsed >= duration)
            {
                ResetRecoil();
            }
        }

        private void OnDisable()
        {
            ResetRecoil();
        }

        internal void RequestRecoil(
            int direction,
            float requestedBodyDistance,
            float requestedWeaponRotation,
            float requestedDuration,
            AnimationCurve requestedRecoveryCurve)
        {
            if (!ready ||
                (direction != -1 && direction != 1) ||
                !IsFinitePositive(requestedDuration))
            {
                return;
            }

            attackDirection = direction;
            bodyDistance = SanitizeNonNegative(requestedBodyDistance);
            weaponRotation = SanitizeNonNegative(requestedWeaponRotation);
            duration = requestedDuration;
            recoveryCurve = requestedRecoveryCurve;
            elapsed = 0f;
            active = true;
            ApplyPose(1f);
        }

        internal void ResetRecoil()
        {
            active = false;
            elapsed = 0f;
            duration = 0f;
            bodyDistance = 0f;
            weaponRotation = 0f;
            recoveryCurve = null;
            if (visualRig != null)
            {
                visualRig.ClearFeedbackPose();
            }

            if (modelView != null)
            {
                modelView.ClearFeedbackPose();
            }
        }

        private void ApplyPose(float strength)
        {
            float signedWeaponRotation = -weaponRotation * strength;
            if (visualRig != null)
            {
                float signedBodyOffset =
                    -attackDirection * visualRig.FacingDirection * bodyDistance * strength;
                EvaluatedMotionPose2D pose = new EvaluatedMotionPose2D(
                    new Vector2(signedBodyOffset, 0f),
                    0f,
                    Vector2.one,
                    Vector2.zero,
                    signedWeaponRotation,
                    Vector2.one,
                    Vector2.zero,
                    signedWeaponRotation,
                    Vector2.one);
                visualRig.SetFeedbackPose(pose);
            }

            if (modelView != null)
            {
                float signedModelOffset =
                    -attackDirection * modelView.FacingDirection * bodyDistance * strength;
                modelView.SetFeedbackPose(signedModelOffset, signedWeaponRotation);
            }
        }

        private static float SanitizeNonNegative(float value)
        {
            return value >= 0f && !float.IsNaN(value) && !float.IsInfinity(value)
                ? value
                : 0f;
        }

        private static bool IsFinitePositive(float value)
        {
            return value > 0f && !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
