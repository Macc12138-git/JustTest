using UnityEngine;

namespace JustTest.Game.Combat
{
    [DefaultExecutionOrder(200)]
    public sealed class CombatCameraShake2D : MonoBehaviour
    {
        private Vector3 appliedOffset;
        private float remainingDuration;
        private float totalDuration;
        private float amplitude;
        private float frequency;

        private void Update()
        {
            RemoveAppliedOffset();
            if (remainingDuration <= 0f)
            {
                return;
            }

            remainingDuration = Mathf.Max(0f, remainingDuration - Time.unscaledDeltaTime);
            if (remainingDuration <= 0f)
            {
                ClearState();
            }
        }

        private void LateUpdate()
        {
            if (remainingDuration <= 0f || amplitude <= 0f)
            {
                return;
            }

            float envelope = totalDuration > 0f
                ? Mathf.Clamp01(remainingDuration / totalDuration)
                : 0f;
            float sampleTime = Time.unscaledTime * frequency;
            float x = Mathf.PerlinNoise(sampleTime, 0.173f) * 2f - 1f;
            float y = Mathf.PerlinNoise(0.719f, sampleTime) * 2f - 1f;
            appliedOffset = new Vector3(x, y, 0f) * (amplitude * envelope);
            transform.position += appliedOffset;
        }

        private void OnDisable()
        {
            ResetShake();
        }

        internal void RequestShake(float duration, float requestedAmplitude, float requestedFrequency)
        {
            if (!IsFinitePositive(duration) || !IsFinitePositive(requestedAmplitude))
            {
                return;
            }

            remainingDuration = Mathf.Max(remainingDuration, duration);
            totalDuration = remainingDuration;
            if (requestedAmplitude >= amplitude)
            {
                amplitude = requestedAmplitude;
                frequency = Mathf.Max(0f, requestedFrequency);
            }
        }

        internal void ResetShake()
        {
            RemoveAppliedOffset();
            ClearState();
        }

        private void RemoveAppliedOffset()
        {
            if (appliedOffset == Vector3.zero)
            {
                return;
            }

            transform.position -= appliedOffset;
            appliedOffset = Vector3.zero;
        }

        private void ClearState()
        {
            remainingDuration = 0f;
            totalDuration = 0f;
            amplitude = 0f;
            frequency = 0f;
        }

        private static bool IsFinitePositive(float value)
        {
            return value > 0f && !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
