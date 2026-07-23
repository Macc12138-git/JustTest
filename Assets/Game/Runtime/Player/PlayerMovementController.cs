using System.Collections;
using JustTest.Game.Input;
using UnityEngine;

namespace JustTest.Game.Player
{
    [DefaultExecutionOrder(-50)]
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public sealed class PlayerMovementController : MonoBehaviour
    {
        [SerializeField] private PlayerMovementConfig config;
        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private PlayerGroundProbe2D groundProbe;
        [SerializeField] private PlayerRollController rollController;

        private Rigidbody2D body;
        private Collider2D bodyCollider;
        private Collider2D ignoredPlatform;
        private Coroutine restorePlatformCollisionRoutine;
        private float lastGroundedTime = float.NegativeInfinity;
        private bool controlLocked;
        private bool droppingThroughPlatform;
        private bool ready;

        public bool IsGrounded => ready && groundProbe.IsGrounded && !droppingThroughPlatform;
        public bool IsRolling => ready && rollController.IsRolling;
        public bool IsInvulnerable => ready && rollController.IsInvulnerable;
        public bool CanStartAction => ready && !controlLocked && !IsRolling;
        public int FacingDirection { get; private set; } = 1;
        public Vector2 Velocity => body != null ? body.velocity : Vector2.zero;

        internal string DebugState
        {
            get
            {
                if (!ready) return "Not Ready";
                if (controlLocked) return "Control Locked";
                if (IsRolling) return IsInvulnerable ? "Rolling (Invulnerable)" : "Rolling";
                if (droppingThroughPlatform) return "Dropping Through";
                return IsGrounded ? "Grounded" : "Airborne";
            }
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            bodyCollider = GetComponent<Collider2D>();
            inputReader = inputReader != null ? inputReader : GetComponent<PlayerInputReader>();
            groundProbe = groundProbe != null ? groundProbe : GetComponent<PlayerGroundProbe2D>();
            rollController = rollController != null ? rollController : GetComponent<PlayerRollController>();

            ready = config != null && inputReader != null && groundProbe != null && rollController != null;
            if (ready)
            {
                return;
            }

            Debug.LogError($"{nameof(PlayerMovementController)} is missing a required config or component.", this);
            enabled = false;
        }

        private void FixedUpdate()
        {
            groundProbe.Refresh();

            float timestamp = Time.time;
            float deltaTime = Time.fixedDeltaTime;

            if (groundProbe.IsGrounded &&
                body.velocity.y <= config.GroundedMaximumUpwardSpeed &&
                !droppingThroughPlatform)
            {
                lastGroundedTime = timestamp;
            }

            if (rollController.IsRolling)
            {
                SimulateRoll(deltaTime);
                return;
            }

            bool canStartRoll = !controlLocked && IsGrounded;
            if (rollController.TryStart(inputReader, timestamp, canStartRoll, FacingDirection))
            {
                SimulateRoll(deltaTime);
                return;
            }

            if (!controlLocked)
            {
                TryStartDropThrough(timestamp);
                TryStartJump(timestamp);
            }

            SimulateHorizontalMovement(deltaTime);
            SimulateGravity(deltaTime);
        }

        private void OnDisable()
        {
            RestoreIgnoredPlatformCollision();
        }

        public void SetControlLocked(bool locked)
        {
            controlLocked = locked;
            if (locked && inputReader != null)
            {
                inputReader.ClearBufferedActions();
            }
        }

        public void ApplyExternalVelocity(Vector2 velocity)
        {
            if (!ready)
            {
                return;
            }

            rollController.Cancel();
            body.velocity = velocity;
            if (velocity.y > 0f)
            {
                lastGroundedTime = float.NegativeInfinity;
            }
        }

        public void Teleport(Vector2 position)
        {
            if (!ready)
            {
                return;
            }

            RestoreIgnoredPlatformCollision();
            rollController.Cancel();
            inputReader.ClearBufferedActions();
            body.position = position;
            body.velocity = Vector2.zero;
            body.angularVelocity = 0f;
            lastGroundedTime = float.NegativeInfinity;
            Physics2D.SyncTransforms();
        }

        public void ResetMotion()
        {
            if (!ready)
            {
                return;
            }

            RestoreIgnoredPlatformCollision();
            rollController.Cancel();
            inputReader.ClearBufferedActions();
            body.velocity = Vector2.zero;
            body.angularVelocity = 0f;
        }

        private void TryStartJump(float timestamp)
        {
            if (droppingThroughPlatform ||
                timestamp - lastGroundedTime > config.CoyoteTime ||
                !inputReader.HasBufferedJump(timestamp, config.JumpBufferTime))
            {
                return;
            }

            inputReader.ConsumeJump();
            Vector2 velocity = body.velocity;
            velocity.y = config.JumpSpeed;
            body.velocity = velocity;
            lastGroundedTime = float.NegativeInfinity;
        }

        private void TryStartDropThrough(float timestamp)
        {
            if (droppingThroughPlatform ||
                !inputReader.DownHeld ||
                !groundProbe.IsStandingOnOneWayPlatform ||
                !inputReader.HasBufferedJump(timestamp, config.JumpBufferTime))
            {
                return;
            }

            Collider2D platform = groundProbe.GroundCollider;
            if (platform == null)
            {
                return;
            }

            inputReader.ConsumeJump();
            droppingThroughPlatform = true;
            ignoredPlatform = platform;
            lastGroundedTime = float.NegativeInfinity;
            Physics2D.IgnoreCollision(bodyCollider, ignoredPlatform, true);

            Vector2 velocity = body.velocity;
            velocity.y = -config.DropThroughSpeed;
            body.velocity = velocity;

            restorePlatformCollisionRoutine = StartCoroutine(RestorePlatformCollisionWhenClear(platform));
        }

        private IEnumerator RestorePlatformCollisionWhenClear(Collider2D platform)
        {
            float startedAt = Time.time;

            while (platform != null)
            {
                float elapsed = Time.time - startedAt;
                bool minimumDurationPassed = elapsed >= config.DropThroughMinimumDuration;
                bool maximumDurationPassed = elapsed >= config.DropThroughMaximumDuration;
                bool playerClearedPlatform =
                    bodyCollider.bounds.max.y <= platform.bounds.min.y - config.DropThroughReenableClearance;

                if ((minimumDurationPassed && playerClearedPlatform) || maximumDurationPassed)
                {
                    break;
                }

                yield return null;
            }

            RestoreIgnoredPlatformCollision();
        }

        private void RestoreIgnoredPlatformCollision()
        {
            if (restorePlatformCollisionRoutine != null)
            {
                StopCoroutine(restorePlatformCollisionRoutine);
                restorePlatformCollisionRoutine = null;
            }

            if (bodyCollider != null && ignoredPlatform != null)
            {
                Physics2D.IgnoreCollision(bodyCollider, ignoredPlatform, false);
            }

            ignoredPlatform = null;
            droppingThroughPlatform = false;
        }

        private void SimulateHorizontalMovement(float deltaTime)
        {
            float horizontalInput = controlLocked ? 0f : inputReader.Horizontal;
            bool hasInput = Mathf.Abs(horizontalInput) > 0.01f;
            bool grounded = IsGrounded;
            float maximumSpeed = grounded ? config.MaxGroundSpeed : config.MaxAirSpeed;
            float acceleration = grounded
                ? (hasInput ? config.GroundAcceleration : config.GroundDeceleration)
                : (hasInput ? config.AirAcceleration : config.AirDeceleration);

            Vector2 velocity = body.velocity;
            velocity.x = Mathf.MoveTowards(
                velocity.x,
                horizontalInput * maximumSpeed,
                acceleration * deltaTime);
            body.velocity = velocity;

            if (hasInput)
            {
                FacingDirection = horizontalInput > 0f ? 1 : -1;
            }
        }

        private void SimulateGravity(float deltaTime)
        {
            Vector2 velocity = body.velocity;
            if (IsGrounded && velocity.y <= 0f)
            {
                velocity.y = -config.GroundedVerticalSpeed;
                body.velocity = velocity;
                return;
            }

            float gravityMultiplier = 1f;
            if (velocity.y < 0f)
            {
                gravityMultiplier = config.FallGravityMultiplier;
            }
            else if (velocity.y > 0f && !inputReader.JumpHeld)
            {
                gravityMultiplier = config.JumpReleaseGravityMultiplier;
            }

            velocity.y -= config.GravityMagnitude * gravityMultiplier * deltaTime;
            velocity.y = Mathf.Max(velocity.y, -config.MaximumFallSpeed);
            body.velocity = velocity;
        }

        private void SimulateRoll(float deltaTime)
        {
            if (!rollController.AllowRollingOffPlatforms && !groundProbe.IsGrounded)
            {
                rollController.Cancel();
                return;
            }

            Vector2 velocity = body.velocity;
            velocity.x = rollController.Direction * rollController.Speed;

            if (groundProbe.IsGrounded && velocity.y <= 0f)
            {
                velocity.y = -config.GroundedVerticalSpeed;
            }
            else
            {
                velocity.y -= config.GravityMagnitude * rollController.GravityMultiplier * deltaTime;
                velocity.y = Mathf.Max(velocity.y, -config.MaximumFallSpeed);
            }

            bool completed = rollController.Tick(deltaTime);
            if (completed)
            {
                velocity.x *= rollController.ExitVelocityRetention;
            }

            body.velocity = velocity;
        }
    }
}
