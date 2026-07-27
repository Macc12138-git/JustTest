using UnityEngine;

namespace JustTest.Game.Combat
{
    public sealed class CombatHitStopController : MonoBehaviour
    {
        private float remainingDuration;
        private float resumeTimeScale = 1f;
        private bool stopping;

        private void Update()
        {
            if (!stopping)
            {
                return;
            }

            remainingDuration -= Time.unscaledDeltaTime;
            if (remainingDuration <= 0f)
            {
                ResetStop();
            }
        }

        private void OnDisable()
        {
            ResetStop();
        }

        internal void RequestStop(float duration)
        {
            if (duration <= 0f || float.IsNaN(duration) || float.IsInfinity(duration))
            {
                return;
            }

            remainingDuration = Mathf.Max(remainingDuration, duration);
            if (stopping)
            {
                return;
            }

            resumeTimeScale = Time.timeScale > 0f ? Time.timeScale : 1f;
            Time.timeScale = 0f;
            stopping = true;
        }

        internal void ResetStop()
        {
            if (stopping)
            {
                Time.timeScale = resumeTimeScale;
            }

            remainingDuration = 0f;
            resumeTimeScale = 1f;
            stopping = false;
        }
    }
}
