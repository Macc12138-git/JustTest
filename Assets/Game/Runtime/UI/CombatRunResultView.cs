using JustTest.Game.Run;
using UnityEngine;
using UnityEngine.UI;

namespace JustTest.Game.UI
{
    [DefaultExecutionOrder(110)]
    public sealed class CombatRunResultView : MonoBehaviour
    {
        [SerializeField] private CombatHudConfig config;
        [SerializeField] private CombatRunController runController;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Text titleText;

        private bool ready;

        private void Awake()
        {
            ready =
                config != null &&
                config.IsValid &&
                runController != null &&
                canvasGroup != null &&
                backgroundImage != null &&
                titleText != null;
            if (!ready)
            {
                Debug.LogError($"{nameof(CombatRunResultView)} is missing an Inspector reference.", this);
                enabled = false;
                return;
            }

            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        private void OnEnable()
        {
            if (!ready)
            {
                return;
            }

            runController.RunStateChanged += OnRunStateChanged;
            Refresh(runController.State);
        }

        private void OnDisable()
        {
            if (runController != null)
            {
                runController.RunStateChanged -= OnRunStateChanged;
            }
        }

        private void OnRunStateChanged(CombatRunState state)
        {
            Refresh(state);
        }

        private void Refresh(CombatRunState state)
        {
            bool visible = state != CombatRunState.Active;
            canvasGroup.alpha = visible ? 1f : 0f;
            switch (state)
            {
                case CombatRunState.Victory:
                    backgroundImage.color = config.VictoryOverlayColor;
                    titleText.text = "VICTORY";
                    titleText.color = config.VictoryTitleColor;
                    break;
                case CombatRunState.Restarting:
                    backgroundImage.color = config.DefeatOverlayColor;
                    titleText.text = "RESTARTING";
                    titleText.color = config.RestartingTitleColor;
                    break;
                default:
                    backgroundImage.color = config.DefeatOverlayColor;
                    titleText.text = "DEFEAT";
                    titleText.color = config.DefeatTitleColor;
                    break;
            }
        }
    }
}
