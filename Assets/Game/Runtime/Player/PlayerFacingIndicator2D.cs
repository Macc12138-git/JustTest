using UnityEngine;

namespace JustTest.Game.Player
{
    public sealed class PlayerFacingIndicator2D : MonoBehaviour
    {
        [SerializeField] private PlayerMovementController movementController;
        [SerializeField] private Transform indicatorRoot;
        [SerializeField] private SpriteRenderer[] indicatorRenderers;
        [SerializeField] private PlayerFacingIndicatorConfig config;

        private int displayedDirection;
        private bool ready;

        private void Awake()
        {
            ready =
                movementController != null &&
                indicatorRoot != null &&
                indicatorRenderers != null &&
                indicatorRenderers.Length > 0 &&
                config != null;
            if (!ready)
            {
                Debug.LogError($"{nameof(PlayerFacingIndicator2D)} is missing an Inspector reference.", this);
                enabled = false;
                return;
            }

            foreach (SpriteRenderer indicatorRenderer in indicatorRenderers)
            {
                if (indicatorRenderer == null)
                {
                    Debug.LogError(
                        $"{nameof(PlayerFacingIndicator2D)} has an empty renderer reference.",
                        this);
                    enabled = false;
                    return;
                }

                indicatorRenderer.color = config.Color;
                indicatorRenderer.sortingOrder = config.SortingOrder;
            }

            indicatorRoot.localPosition = new Vector3(
                config.LocalOffset.x,
                config.LocalOffset.y,
                indicatorRoot.localPosition.z);
            indicatorRoot.gameObject.SetActive(config.Visible);
            UpdateDirection(true);
        }

        private void LateUpdate()
        {
            UpdateDirection(false);
        }

        private void UpdateDirection(bool force)
        {
            if (!ready || !config.Visible)
            {
                return;
            }

            int direction = movementController.FacingDirection < 0 ? -1 : 1;
            if (!force && direction == displayedDirection)
            {
                return;
            }

            displayedDirection = direction;
            indicatorRoot.localScale = new Vector3(
                config.Scale * direction,
                config.Scale,
                1f);
        }
    }
}
