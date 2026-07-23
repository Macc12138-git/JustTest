using UnityEngine;

namespace JustTest.Game.Player
{
    public sealed class PlayerMovementDebugView : MonoBehaviour
    {
        [SerializeField] private PlayerMovementController movementController;
        [SerializeField] private PlayerGroundProbe2D groundProbe;
        [SerializeField] private PlayerMovementDebugConfig config;

        private string previousState;

        private void Awake()
        {
            movementController = movementController != null
                ? movementController
                : GetComponent<PlayerMovementController>();
            groundProbe = groundProbe != null ? groundProbe : GetComponent<PlayerGroundProbe2D>();
        }

        private void Update()
        {
            if (movementController == null || config == null)
            {
                return;
            }

            if (config.DrawVelocity)
            {
                Debug.DrawRay(
                    transform.position,
                    movementController.Velocity * config.VelocityRayScale,
                    config.VelocityColor);
            }

            if (config.LogStateChanges && previousState != movementController.DebugState)
            {
                previousState = movementController.DebugState;
                Debug.Log($"Player movement state: {previousState}", this);
            }
        }

        private void OnGUI()
        {
            if (movementController == null || config == null || !config.ShowOverlay)
            {
                return;
            }

            Vector2 position = config.OverlayPosition;
            Vector2 size = config.OverlaySize;
            string text =
                $"State: {movementController.DebugState}\n" +
                $"Velocity: {movementController.Velocity.x:0.00}, {movementController.Velocity.y:0.00}\n" +
                $"Facing: {movementController.FacingDirection}";

            GUI.Label(new Rect(position.x, position.y, size.x, size.y), text);
        }

        private void OnDrawGizmosSelected()
        {
            if (groundProbe == null)
            {
                groundProbe = GetComponent<PlayerGroundProbe2D>();
            }

            if (groundProbe == null || config == null || !config.DrawGroundProbe)
            {
                return;
            }

            groundProbe.GetDebugProbeBounds(out Vector3 center, out Vector3 size);
            Gizmos.color = groundProbe.IsGrounded ? config.GroundedColor : config.AirborneColor;
            Gizmos.DrawWireCube(center, size);
        }
    }
}
