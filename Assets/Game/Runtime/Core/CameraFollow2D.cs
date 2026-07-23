using JustTest.Game.Player;
using UnityEngine;

namespace JustTest.Game.Core
{
    [DefaultExecutionOrder(100)]
    public sealed class CameraFollow2D : MonoBehaviour
    {
        [SerializeField] private CameraFollowConfig config;
        [SerializeField] private Transform target;
        [SerializeField] private PlayerMovementController movementController;

        private float horizontalVelocity;
        private float verticalVelocity;
        private float lookAheadVelocity;
        private float currentLookAhead;

        private void OnEnable()
        {
            if (config == null || target == null)
            {
                return;
            }

            currentLookAhead = movementController != null
                ? movementController.FacingDirection * config.FacingLookAheadDistance
                : 0f;

            if (config.SnapOnEnable)
            {
                SnapToTarget();
            }
        }

        private void LateUpdate()
        {
            if (config == null || target == null)
            {
                return;
            }

            float desiredLookAhead = movementController != null
                ? movementController.FacingDirection * config.FacingLookAheadDistance
                : 0f;
            currentLookAhead = Mathf.SmoothDamp(
                currentLookAhead,
                desiredLookAhead,
                ref lookAheadVelocity,
                config.LookAheadSmoothTime,
                config.MaximumFollowSpeed);

            Vector3 position = transform.position;
            float desiredX = target.position.x + config.BaseOffset.x + currentLookAhead;
            float desiredY = config.FollowVerticalMovement
                ? target.position.y + config.BaseOffset.y
                : position.y;

            if (Mathf.Abs(desiredX - position.x) > config.HorizontalDeadZone)
            {
                position.x = Mathf.SmoothDamp(
                    position.x,
                    desiredX,
                    ref horizontalVelocity,
                    config.HorizontalSmoothTime,
                    config.MaximumFollowSpeed);
            }

            if (Mathf.Abs(desiredY - position.y) > config.VerticalDeadZone)
            {
                position.y = Mathf.SmoothDamp(
                    position.y,
                    desiredY,
                    ref verticalVelocity,
                    config.VerticalSmoothTime,
                    config.MaximumFollowSpeed);
            }

            transform.position = position;
        }

        public void SetTarget(Transform newTarget, PlayerMovementController newMovementController)
        {
            target = newTarget;
            movementController = newMovementController;
            SnapToTarget();
        }

        private void SnapToTarget()
        {
            if (config == null || target == null)
            {
                return;
            }

            Vector3 position = transform.position;
            position.x = target.position.x + config.BaseOffset.x + currentLookAhead;
            if (config.FollowVerticalMovement)
            {
                position.y = target.position.y + config.BaseOffset.y;
            }

            transform.position = position;
            horizontalVelocity = 0f;
            verticalVelocity = 0f;
            lookAheadVelocity = 0f;
        }
    }
}
