using System;
using UnityEngine;

namespace JustTest.Game.Combat
{
    [Serializable]
    public sealed class CombatFeedbackProfile
    {
        [SerializeField, Min(0f)] private float hitStopDuration;
        [SerializeField, Min(0f)] private float cameraShakeDuration;
        [SerializeField, Min(0f)] private float cameraShakeAmplitude;
        [SerializeField, Min(0f)] private float cameraShakeFrequency = 20f;
        [SerializeField] private Color flashColor = Color.white;
        [SerializeField, Min(0f)] private float flashDuration;
        [SerializeField, Min(1f)] private float killMultiplier = 1f;
        [SerializeField, Min(0f)] private float recoilBodyDistance;
        [SerializeField, Min(0f)] private float recoilWeaponRotation;
        [SerializeField, Min(0f)] private float recoilDuration;
        [SerializeField] private AnimationCurve recoilRecoveryCurve =
            AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
        [SerializeField] private CombatImpactEffect2D impactPrefab;
        [SerializeField] private Vector2 impactOffset;
        [SerializeField, Min(0f)] private float impactScale = 1f;
        [SerializeField, Min(0f)] private float impactLifetime = 0.15f;

        internal CombatFeedbackProfile(
            float hitStopDuration,
            float cameraShakeDuration,
            float cameraShakeAmplitude,
            float cameraShakeFrequency,
            float flashDuration,
            float killMultiplier,
            float recoilBodyDistance,
            float recoilWeaponRotation,
            float recoilDuration,
            Vector2 impactOffset,
            float impactScale,
            float impactLifetime)
        {
            this.hitStopDuration = hitStopDuration;
            this.cameraShakeDuration = cameraShakeDuration;
            this.cameraShakeAmplitude = cameraShakeAmplitude;
            this.cameraShakeFrequency = cameraShakeFrequency;
            this.flashDuration = flashDuration;
            this.killMultiplier = killMultiplier;
            this.recoilBodyDistance = recoilBodyDistance;
            this.recoilWeaponRotation = recoilWeaponRotation;
            this.recoilDuration = recoilDuration;
            this.impactOffset = impactOffset;
            this.impactScale = impactScale;
            this.impactLifetime = impactLifetime;
        }

        internal float HitStopDuration => hitStopDuration;

        internal float CameraShakeDuration => cameraShakeDuration;

        internal float CameraShakeAmplitude => cameraShakeAmplitude;

        internal float CameraShakeFrequency => cameraShakeFrequency;

        internal Color FlashColor => flashColor;

        internal float FlashDuration => flashDuration;

        internal float KillMultiplier => killMultiplier;

        internal float RecoilBodyDistance => recoilBodyDistance;

        internal float RecoilWeaponRotation => recoilWeaponRotation;

        internal float RecoilDuration => recoilDuration;

        internal AnimationCurve RecoilRecoveryCurve => recoilRecoveryCurve;

        internal CombatImpactEffect2D ImpactPrefab => impactPrefab;

        internal Vector2 ImpactOffset => impactOffset;

        internal float ImpactScale => impactScale;

        internal float ImpactLifetime => impactLifetime;

        internal void Sanitize()
        {
            hitStopDuration = SanitizeNonNegative(hitStopDuration);
            cameraShakeDuration = SanitizeNonNegative(cameraShakeDuration);
            cameraShakeAmplitude = SanitizeNonNegative(cameraShakeAmplitude);
            cameraShakeFrequency = SanitizeNonNegative(cameraShakeFrequency);
            flashDuration = SanitizeNonNegative(flashDuration);
            killMultiplier = Mathf.Max(1f, SanitizeFinite(killMultiplier));
            recoilBodyDistance = SanitizeNonNegative(recoilBodyDistance);
            recoilWeaponRotation = SanitizeNonNegative(recoilWeaponRotation);
            recoilDuration = SanitizeNonNegative(recoilDuration);
            recoilRecoveryCurve ??= AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
            impactOffset = new Vector2(
                SanitizeFinite(impactOffset.x),
                SanitizeFinite(impactOffset.y));
            impactScale = SanitizeNonNegative(impactScale);
            impactLifetime = SanitizeNonNegative(impactLifetime);
        }

        private static float SanitizeNonNegative(float value)
        {
            return Mathf.Max(0f, SanitizeFinite(value));
        }

        private static float SanitizeFinite(float value)
        {
            return float.IsNaN(value) || float.IsInfinity(value) ? 0f : value;
        }
    }
}
