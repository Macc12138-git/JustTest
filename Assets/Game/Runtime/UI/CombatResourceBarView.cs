using UnityEngine;
using UnityEngine.UI;

namespace JustTest.Game.UI
{
    public sealed class CombatResourceBarView : MonoBehaviour
    {
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image fillImage;
        [SerializeField] private Text valueText;

        private bool ready;

        private void Awake()
        {
            ready = backgroundImage != null && fillImage != null && valueText != null;
            if (ready)
            {
                return;
            }

            Debug.LogError($"{nameof(CombatResourceBarView)} is missing an Inspector reference.", this);
            enabled = false;
        }

        internal void SetValue(
            float currentValue,
            float maximumValue,
            Color backgroundColor,
            Color normalColor,
            Color warningColor,
            float warningThreshold)
        {
            if (!ready)
            {
                return;
            }

            float normalized = maximumValue > 0f
                ? Mathf.Clamp01(currentValue / maximumValue)
                : 0f;
            backgroundImage.color = backgroundColor;
            fillImage.fillAmount = normalized;
            fillImage.color = normalized <= warningThreshold ? warningColor : normalColor;
            valueText.text = $"{currentValue:0}/{maximumValue:0}";
        }
    }
}
