using System.Collections;
using UnityEngine;

namespace JustTest.Game.Enemies
{
    [DefaultExecutionOrder(-40)]
    public sealed class MeleeEnemyMotor2D : MonoBehaviour
    {
        [SerializeField] private MeleeEnemyConfig config;
        [SerializeField] private MeleeEnemyNavigationProbe2D groundProbe;
        [SerializeField] private Rigidbody2D body;
        [SerializeField] private Collider2D bodyCollider;

        private Collider2D ignoredPlatform;
        private Coroutine restoreCollisionRoutine;
        private float desiredHorizontalDirection;
        private bool jumpRequested;
        private bool dropThroughRequested;
        private bool controlEnabled = true;
        private bool ready;

        internal bool IsGrounded => ready && groundProbe.IsGrounded;
        internal int FacingDirection { get; private set; } = -1;
        internal Vector2 Velocity => body != null ? body.velocity : Vector2.zero;

        private void Awake()
        {
            ready = config != null && groundProbe != null && body != null && bodyCollider != null;
            if (ready)
            {
                return;
            }

            Debug.LogError($"{nameof(MeleeEnemyMotor2D)} is missing an Inspector reference.", this);
            enabled = false;
        }

        private void FixedUpdate()
        {
            groundProbe.Refresh();
            if (!controlEnabled)
            {
                return;
            }

            if (dropThroughRequested)
            {
                dropThroughRequested = false;
                TryStartDropThrough();
            }

            if (jumpRequested)
            {
                jumpRequested = false;
                TryJump();
            }

            Vector2 velocity = body.velocity;
            bool hasDirection = Mathf.Abs(desiredHorizontalDirection) > 0.01f;
            float acceleration = groundProbe.IsGrounded
                ? (hasDirection ? config.GroundAcceleration : config.GroundDeceleration)
                : config.AirAcceleration;
            velocity.x = Mathf.MoveTowards(
                velocity.x,
                desiredHorizontalDirection * config.MovementSpeed,
                acceleration * Time.fixedDeltaTime);
            body.velocity = velocity;

            if (hasDirection)
            {
                FacingDirection = desiredHorizontalDirection > 0f ? 1 : -1;
            }
        }

        private void OnDisable()
        {
            RestoreIgnoredPlatformCollision();
        }

        internal void SetHorizontalDirection(float direction)
        {
            desiredHorizontalDirection = Mathf.Clamp(direction, -1f, 1f);
        }

        internal void Face(int direction)
        {
            if (direction == -1 || direction == 1)
            {
                FacingDirection = direction;
            }
        }

        internal void RequestJump()
        {
            jumpRequested = true;
        }

        internal void RequestDropThrough()
        {
            dropThroughRequested = true;
        }

        internal bool TryStartObstacleTraversal(int direction, out float exitX)
        {
            exitX = transform.position.x;
            if (!controlEnabled ||
                !groundProbe.IsGrounded ||
                !groundProbe.TryGetForwardObstacle(direction, out Collider2D obstacle))
            {
                return false;
            }

            float obstacleHeight = obstacle.bounds.max.y - bodyCollider.bounds.min.y;
            if (obstacleHeight < config.MinimumJumpObstacleHeight ||
                obstacleHeight > config.JumpHeight)
            {
                return false;
            }

            float exitOffset = bodyCollider.bounds.extents.x + config.ObstacleExitClearance;
            exitX = direction > 0
                ? obstacle.bounds.max.x + exitOffset
                : obstacle.bounds.min.x - exitOffset;
            Face(direction);
            jumpRequested = true;
            return true;
        }

        internal void SetControlEnabled(bool enabledState)
        {
            controlEnabled = enabledState;
            if (!controlEnabled)
            {
                desiredHorizontalDirection = 0f;
                jumpRequested = false;
                dropThroughRequested = false;
            }
        }

        internal void ResetMotion()
        {
            RestoreIgnoredPlatformCollision();
            desiredHorizontalDirection = 0f;
            jumpRequested = false;
            dropThroughRequested = false;
            controlEnabled = true;
            body.velocity = Vector2.zero;
            body.angularVelocity = 0f;
        }

        private void TryJump()
        {
            if (!groundProbe.IsGrounded)
            {
                return;
            }

            float gravityMagnitude = Mathf.Abs(Physics2D.gravity.y * body.gravityScale);
            if (gravityMagnitude <= 0.01f)
            {
                return;
            }

            Vector2 velocity = body.velocity;
            velocity.y = Mathf.Sqrt(2f * gravityMagnitude * config.JumpHeight);
            body.velocity = velocity;
        }

        private void TryStartDropThrough()
        {
            if (!groundProbe.IsStandingOnOneWayPlatform || ignoredPlatform != null)
            {
                return;
            }

            ignoredPlatform = groundProbe.GroundCollider;
            Physics2D.IgnoreCollision(bodyCollider, ignoredPlatform, true);
            Vector2 velocity = body.velocity;
            velocity.y = -config.DropThroughSpeed;
            body.velocity = velocity;
            restoreCollisionRoutine = StartCoroutine(RestoreCollisionWhenClear(ignoredPlatform));
        }

        private IEnumerator RestoreCollisionWhenClear(Collider2D platform)
        {
            float startedAt = Time.time;
            while (platform != null)
            {
                float elapsed = Time.time - startedAt;
                bool minimumDurationPassed = elapsed >= config.DropThroughMinimumDuration;
                bool maximumDurationPassed = elapsed >= config.DropThroughMaximumDuration;
                bool clearedPlatform =
                    bodyCollider.bounds.max.y <= platform.bounds.min.y - config.DropThroughClearance;
                if ((minimumDurationPassed && clearedPlatform) || maximumDurationPassed)
                {
                    break;
                }

                yield return null;
            }

            RestoreIgnoredPlatformCollision();
        }

        private void RestoreIgnoredPlatformCollision()
        {
            if (restoreCollisionRoutine != null)
            {
                StopCoroutine(restoreCollisionRoutine);
                restoreCollisionRoutine = null;
            }

            if (bodyCollider != null && ignoredPlatform != null)
            {
                Physics2D.IgnoreCollision(bodyCollider, ignoredPlatform, false);
            }

            ignoredPlatform = null;
        }
    }
}
