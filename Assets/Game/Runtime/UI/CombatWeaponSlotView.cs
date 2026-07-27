using JustTest.Game.Weapons;
using UnityEngine;
using UnityEngine.UI;

namespace JustTest.Game.UI
{
    public sealed class CombatWeaponSlotView : MonoBehaviour
    {
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image stateBorderImage;
        [SerializeField] private Image qteTimerFillImage;
        [SerializeField] private Text shortcutText;
        [SerializeField] private Text weaponNameText;
        [SerializeField] private Text stateText;

        private bool ready;

        private void Awake()
        {
            ready =
                backgroundImage != null &&
                stateBorderImage != null &&
                qteTimerFillImage != null &&
                shortcutText != null &&
                weaponNameText != null &&
                stateText != null;
            if (ready)
            {
                return;
            }

            Debug.LogError($"{nameof(CombatWeaponSlotView)} is missing an Inspector reference.", this);
            enabled = false;
        }

        internal void Render(
            int slotIndex,
            WeaponDefinition weapon,
            CombatWeaponSlotVisualState visualState,
            float qteNormalizedTime,
            float qtePulseAlpha,
            CombatHudConfig config)
        {
            if (!ready || config == null)
            {
                return;
            }

            shortcutText.text = (slotIndex + 1).ToString();
            weaponNameText.text = weapon != null ? weapon.DisplayName : "Empty";
            ApplyWeaponTint(weapon, visualState, config);
            ApplyState(visualState, qteNormalizedTime, qtePulseAlpha, config);
        }

        private void ApplyWeaponTint(
            WeaponDefinition weapon,
            CombatWeaponSlotVisualState visualState,
            CombatHudConfig config)
        {
            Color backgroundColor = visualState == CombatWeaponSlotVisualState.Empty
                ? config.EmptySlotColor
                : config.InactiveSlotColor;
            if (weapon != null)
            {
                Color weaponColor = weapon.DebugColor;
                weaponColor.a = backgroundColor.a;
                backgroundColor = Color.Lerp(backgroundColor, weaponColor, 0.28f);
            }

            backgroundImage.color = backgroundColor;
        }

        private void ApplyState(
            CombatWeaponSlotVisualState visualState,
            float qteNormalizedTime,
            float qtePulseAlpha,
            CombatHudConfig config)
        {
            bool showTimer = visualState == CombatWeaponSlotVisualState.QteCandidate;
            qteTimerFillImage.gameObject.SetActive(showTimer);
            qteTimerFillImage.fillAmount = showTimer ? Mathf.Clamp01(qteNormalizedTime) : 0f;

            Color borderColor;
            switch (visualState)
            {
                case CombatWeaponSlotVisualState.Active:
                    borderColor = config.ActiveSlotColor;
                    stateText.text = "ACTIVE";
                    break;
                case CombatWeaponSlotVisualState.QteCandidate:
                    borderColor = config.QteCandidateColor;
                    borderColor.a = qtePulseAlpha;
                    stateText.text = "QTE";
                    break;
                case CombatWeaponSlotVisualState.QteExecuting:
                    borderColor = config.QteExecutingColor;
                    stateText.text = "EXECUTING";
                    break;
                default:
                    borderColor = config.InactiveSlotColor;
                    stateText.text = string.Empty;
                    break;
            }

            stateBorderImage.color = borderColor;
            qteTimerFillImage.color = config.QteCandidateColor;
        }
    }
}
