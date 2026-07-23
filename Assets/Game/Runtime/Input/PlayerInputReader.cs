using UnityEngine;

namespace JustTest.Game.Input
{
    [DefaultExecutionOrder(-200)]
    public sealed class PlayerInputReader : MonoBehaviour
    {
        [SerializeField] private PlayerInputConfig config;

        private readonly BufferedButton jumpButton = new BufferedButton();
        private readonly BufferedButton rollButton = new BufferedButton();

        internal float Horizontal { get; private set; }
        internal bool DownHeld { get; private set; }
        internal bool JumpHeld { get; private set; }
        internal bool ResetPressedThisFrame { get; private set; }

        private void Awake()
        {
            if (config != null)
            {
                return;
            }

            Debug.LogError($"{nameof(PlayerInputReader)} requires an input config.", this);
            enabled = false;
        }

        private void Update()
        {
            bool leftHeld = UnityEngine.Input.GetKey(config.MoveLeftKey);
            bool rightHeld = UnityEngine.Input.GetKey(config.MoveRightKey);

            Horizontal = (rightHeld ? 1f : 0f) - (leftHeld ? 1f : 0f);
            DownHeld = UnityEngine.Input.GetKey(config.MoveDownKey);
            JumpHeld = UnityEngine.Input.GetKey(config.JumpKey);
            ResetPressedThisFrame = UnityEngine.Input.GetKeyDown(config.ResetKey);

            if (UnityEngine.Input.GetKeyDown(config.JumpKey))
            {
                jumpButton.Press(Time.time);
            }

            if (UnityEngine.Input.GetKeyDown(config.RollKey))
            {
                rollButton.Press(Time.time);
            }
        }

        private void OnDisable()
        {
            Horizontal = 0f;
            DownHeld = false;
            JumpHeld = false;
            ResetPressedThisFrame = false;
            ClearBufferedActions();
        }

        internal bool HasBufferedJump(float timestamp, float bufferDuration)
        {
            return jumpButton.IsAvailable(timestamp, bufferDuration);
        }

        internal bool HasBufferedRoll(float timestamp, float bufferDuration)
        {
            return rollButton.IsAvailable(timestamp, bufferDuration);
        }

        internal void ConsumeJump()
        {
            jumpButton.Consume();
        }

        internal void ConsumeRoll()
        {
            rollButton.Consume();
        }

        internal void ClearBufferedActions()
        {
            jumpButton.Clear();
            rollButton.Clear();
        }
    }
}
