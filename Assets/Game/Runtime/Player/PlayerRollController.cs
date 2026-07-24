using System;
using JustTest.Game.Input;
using UnityEngine;

namespace JustTest.Game.Player
{
    public sealed class PlayerRollController : MonoBehaviour
    {
        [SerializeField] private PlayerRollConfig config;

        private PlayerRollTimer timer;
        private bool previousInvulnerability;

        public event Action<bool> InvulnerabilityChanged;

        public bool IsRolling => timer != null && timer.IsRolling;
        public bool IsInvulnerable => timer != null && timer.IsInvulnerable;

        public float NormalizedTime => timer?.NormalizedTime ?? 0f;

        internal int Direction { get; private set; } = 1;
        internal float Speed => config.Speed;
        internal float GravityMultiplier => config.GravityMultiplier;
        internal float ExitVelocityRetention => config.ExitVelocityRetention;
        internal bool AllowRollingOffPlatforms => config.AllowRollingOffPlatforms;

        private void Awake()
        {
            if (config == null)
            {
                Debug.LogError($"{nameof(PlayerRollController)} requires a roll config.", this);
                enabled = false;
                return;
            }

            timer = new PlayerRollTimer(
                config.Duration,
                config.InvulnerabilityStartTime,
                config.InvulnerabilityDuration,
                config.MinimumStartInterval);
        }

        private void OnDisable()
        {
            Cancel();
        }

        internal bool TryStart(
            PlayerInputReader inputReader,
            float timestamp,
            bool canStart,
            int facingDirection)
        {
            if (!enabled || timer == null || !canStart ||
                !inputReader.HasBufferedRoll(timestamp, config.InputBufferTime) ||
                !timer.CanStart(timestamp))
            {
                return false;
            }

            inputReader.ConsumeRoll();
            Direction = facingDirection == 0 ? 1 : facingDirection;
            timer.Start(timestamp);
            UpdateInvulnerability();
            return true;
        }

        internal bool Tick(float deltaTime)
        {
            if (timer == null)
            {
                return false;
            }

            bool completed = timer.Tick(deltaTime);
            UpdateInvulnerability();
            return completed;
        }

        internal void Cancel()
        {
            if (timer == null)
            {
                return;
            }

            timer.Reset();
            UpdateInvulnerability();
        }

        private void UpdateInvulnerability()
        {
            bool current = IsInvulnerable;
            if (current == previousInvulnerability)
            {
                return;
            }

            previousInvulnerability = current;
            InvulnerabilityChanged?.Invoke(current);
        }
    }
}
