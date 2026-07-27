using UnityEngine;

namespace JustTest.Game.Combat
{
    public sealed class CombatHitFlash2D : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer[] renderers;

        private Color[] originalColors;
        private float remainingDuration;
        private bool ready;

        private void Awake()
        {
            ready = renderers != null && renderers.Length > 0;
            if (ready)
            {
                originalColors = new Color[renderers.Length];
                for (int index = 0; index < renderers.Length; index++)
                {
                    if (renderers[index] == null)
                    {
                        ready = false;
                        break;
                    }

                    originalColors[index] = renderers[index].color;
                }
            }

            if (ready)
            {
                return;
            }

            Debug.LogError($"{nameof(CombatHitFlash2D)} is missing an Inspector reference.", this);
            enabled = false;
        }

        private void Update()
        {
            if (!ready || remainingDuration <= 0f)
            {
                return;
            }

            remainingDuration = Mathf.Max(0f, remainingDuration - Time.unscaledDeltaTime);
            if (remainingDuration <= 0f)
            {
                RestoreColors();
            }
        }

        private void OnDisable()
        {
            ResetFlash();
        }

        internal void RequestFlash(Color flashColor, float duration)
        {
            if (!ready || duration <= 0f || float.IsNaN(duration) || float.IsInfinity(duration))
            {
                return;
            }

            remainingDuration = Mathf.Max(remainingDuration, duration);
            for (int index = 0; index < renderers.Length; index++)
            {
                Color displayedColor = flashColor;
                displayedColor.a = originalColors[index].a;
                renderers[index].color = displayedColor;
            }
        }

        internal void ResetFlash()
        {
            remainingDuration = 0f;
            if (ready)
            {
                RestoreColors();
            }
        }

        private void RestoreColors()
        {
            for (int index = 0; index < renderers.Length; index++)
            {
                renderers[index].color = originalColors[index];
            }
        }
    }
}
