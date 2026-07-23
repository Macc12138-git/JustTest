using UnityEngine;

namespace JustTest.Game.Combat
{
    public sealed class CombatStatusIndicator2D : MonoBehaviour
    {
        [SerializeField] private CombatStatusController statusController;
        [SerializeField] private SpriteRenderer unbalancedRenderer;
        [SerializeField] private SpriteRenderer airborneRenderer;
        [SerializeField] private SpriteRenderer stunnedRenderer;

        private bool ready;

        private void Awake()
        {
            ready =
                statusController != null &&
                unbalancedRenderer != null &&
                airborneRenderer != null &&
                stunnedRenderer != null;
            if (ready)
            {
                return;
            }

            Debug.LogError($"{nameof(CombatStatusIndicator2D)} is missing an Inspector reference.", this);
            enabled = false;
        }

        private void OnEnable()
        {
            if (!ready)
            {
                return;
            }

            statusController.StatusApplied += OnStatusChanged;
            statusController.StatusEnded += OnStatusChanged;
            RefreshIndicators();
        }

        private void OnDisable()
        {
            if (statusController != null)
            {
                statusController.StatusApplied -= OnStatusChanged;
                statusController.StatusEnded -= OnStatusChanged;
            }

            SetRenderer(unbalancedRenderer, false);
            SetRenderer(airborneRenderer, false);
            SetRenderer(stunnedRenderer, false);
        }

        private void OnStatusChanged(CombatStatusEvent _)
        {
            RefreshIndicators();
        }

        private void RefreshIndicators()
        {
            SetRenderer(
                unbalancedRenderer,
                statusController.IsActive(CombatStatusType.Unbalanced));
            SetRenderer(
                airborneRenderer,
                statusController.IsActive(CombatStatusType.Airborne));
            SetRenderer(
                stunnedRenderer,
                statusController.IsActive(CombatStatusType.Stunned));
        }

        private static void SetRenderer(SpriteRenderer renderer, bool active)
        {
            if (renderer != null)
            {
                renderer.enabled = active;
            }
        }
    }
}
