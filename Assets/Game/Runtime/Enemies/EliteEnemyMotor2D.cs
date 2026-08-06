using UnityEngine;

namespace JustTest.Game.Enemies
{
    [DefaultExecutionOrder(-40)]
    public sealed class EliteEnemyMotor2D : MonoBehaviour
    {
        [SerializeField] private EliteEnemyConfig config;
        [SerializeField] private EliteEnemyGroundProbe2D groundProbe;
        [SerializeField] private Rigidbody2D body;

        private float desiredHorizontalDirection;
        private float dashStopX;
        private int dashDirection;
        private bool controlEnabled = true;
        private bool ready;

        internal bool IsGrounded => groundProbe != null && groundProbe.IsGrounded;
        internal bool IsDashing { get; private set; }
        internal int FacingDirection { get; private set; } = -1;
        internal Vector2 Velocity => body != null ? body.velocity : Vector2.zero;

        private void Awake()
        {
            ready = config != null && groundProbe != null && body != null;
            if (ready)
            {
                return;
            }

            Debug.LogError($"{nameof(EliteEnemyMotor2D)} is missing an Inspector reference.", this);
            enabled = false;
        }

        private void FixedUpdate()
        {
            groundProbe.Refresh();
            if (!controlEnabled)
            {
                return;
            }

            Vector2 velocity = body.velocity;
            if (IsDashing)
            {
                float remainingDistance = (dashStopX - body.position.x) * dashDirection;
                if (remainingDistance <= 0.001f)
                {
                    StopDash();
                    return;
                }

                float maximumStepSpeed = remainingDistance / Time.fixedDeltaTime;
                velocity.x = Mathf.Min(config.DashSpeed, maximumStepSpeed) * dashDirection;
                body.velocity = velocity;
                return;
            }

            bool hasDirection = Mathf.Abs(desiredHorizontalDirection) > 0.01f;
            float acceleration = hasDirection
                ? config.GroundAcceleration
                : config.GroundDeceleration;
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

        internal void SetHorizontalDirection(float direction)
        {
            if (!IsDashing)
            {
                desiredHorizontalDirection = Mathf.Clamp(direction, -1f, 1f);
            }
        }

        internal void Face(int direction)
        {
            if (direction == -1 || direction == 1)
            {
                FacingDirection = direction;
            }
        }

        internal void Stop()
        {
            desiredHorizontalDirection = 0f;
        }

        internal void BeginDash(int direction, float stopX)
        {
            if (!controlEnabled ||
                (direction != -1 && direction != 1) ||
                (stopX - body.position.x) * direction <= 0f)
            {
                return;
            }

            dashDirection = direction;
            dashStopX = stopX;
            FacingDirection = direction;
            desiredHorizontalDirection = 0f;
            IsDashing = true;
        }

        internal void StopDash()
        {
            IsDashing = false;
            dashDirection = 0;
            dashStopX = 0f;
            if (body != null)
            {
                Vector2 velocity = body.velocity;
                velocity.x = 0f;
                body.velocity = velocity;
            }
        }

        internal void SetControlEnabled(bool enabledState)
        {
            controlEnabled = enabledState;
            if (!controlEnabled)
            {
                desiredHorizontalDirection = 0f;
                StopDash();
            }
        }

        internal void ResetMotion()
        {
            desiredHorizontalDirection = 0f;
            dashStopX = 0f;
            dashDirection = 0;
            IsDashing = false;
            controlEnabled = true;
            body.velocity = Vector2.zero;
            body.angularVelocity = 0f;
        }
    }
}
