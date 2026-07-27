using UnityEngine;

namespace JustTest.Game.Combat
{
    public sealed class CombatImpactEffect2D : MonoBehaviour
    {
        [SerializeField] private ParticleSystem particles;

        private float remainingDuration;
        private bool playing;
        private bool ready;

        private void Awake()
        {
            ready = particles != null;
            if (ready)
            {
                return;
            }

            Debug.LogError($"{nameof(CombatImpactEffect2D)} is missing an Inspector reference.", this);
            enabled = false;
        }

        private void Update()
        {
            if (!playing)
            {
                return;
            }

            remainingDuration = Mathf.Max(0f, remainingDuration - Time.unscaledDeltaTime);
            if (remainingDuration <= 0f)
            {
                Destroy(gameObject);
            }
        }

        private void OnDisable()
        {
            playing = false;
            remainingDuration = 0f;
            if (particles != null)
            {
                particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        internal void Play(float duration)
        {
            if (!ready || duration <= 0f || float.IsNaN(duration) || float.IsInfinity(duration))
            {
                return;
            }

            remainingDuration = duration;
            playing = true;
            particles.Play(true);
        }
    }
}
