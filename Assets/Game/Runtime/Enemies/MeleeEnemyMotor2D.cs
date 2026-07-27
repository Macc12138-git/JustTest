using UnityEngine;

namespace JustTest.Game.Enemies
{
    [DefaultExecutionOrder(-40)]
    public sealed class MeleeEnemyMotor2D : MonoBehaviour
    {
        [SerializeField] private MeleeEnemyConfig config;
        [SerializeField] private EnemyGroundProbe2D groundProbe;
        [SerializeField] private Rigidbody2D body;

        private float desiredHorizontalDirection;
        private bool controlEnabled = true;
        private bool ready;

        internal bool IsGrounded => groundProbe != null && groundProbe.IsGrounded;
        internal int FacingDirection { get; private set; } = -1;
        internal Vector2 Velocity => body != null ? body.velocity : Vector2.zero;

        private void Awake()
        {
            ready = config != null && groundProbe != null && body != null;
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

            Vector2 velocity = body.velocity;
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
            desiredHorizontalDirection = Mathf.Clamp(direction, -1f, 1f);
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

        internal void SetControlEnabled(bool enabledState)
        {
            controlEnabled = enabledState;
            if (!controlEnabled)
            {
                desiredHorizontalDirection = 0f;
            }
        }

        internal void ResetMotion()
        {
            desiredHorizontalDirection = 0f;
            controlEnabled = true;
            body.velocity = Vector2.zero;
            body.angularVelocity = 0f;
        }

    }
}
