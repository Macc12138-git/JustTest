namespace JustTest.Game.Player
{
    internal sealed class PlayerRollTimer
    {
        private const float TimeComparisonTolerance = 0.000001f;

        private readonly float duration;
        private readonly float invulnerabilityStartTime;
        private readonly float invulnerabilityDuration;
        private readonly float minimumStartInterval;

        private float elapsed;
        private float lastStartTime = float.NegativeInfinity;

        internal PlayerRollTimer(
            float duration,
            float invulnerabilityStartTime,
            float invulnerabilityDuration,
            float minimumStartInterval)
        {
            this.duration = duration;
            this.invulnerabilityStartTime = invulnerabilityStartTime;
            this.invulnerabilityDuration = invulnerabilityDuration;
            this.minimumStartInterval = minimumStartInterval;
        }

        internal bool IsRolling { get; private set; }

        internal bool IsInvulnerable =>
            IsRolling &&
            elapsed + TimeComparisonTolerance >= invulnerabilityStartTime &&
            elapsed + TimeComparisonTolerance < invulnerabilityStartTime + invulnerabilityDuration;

        internal float NormalizedTime => IsRolling && duration > 0f
            ? System.Math.Min(1f, System.Math.Max(0f, elapsed / duration))
            : 0f;

        internal bool CanStart(float timestamp)
        {
            return !IsRolling && timestamp - lastStartTime >= minimumStartInterval;
        }

        internal void Start(float timestamp)
        {
            elapsed = 0f;
            lastStartTime = timestamp;
            IsRolling = true;
        }

        internal bool Tick(float deltaTime)
        {
            if (!IsRolling)
            {
                return false;
            }

            elapsed += deltaTime;
            if (elapsed < duration)
            {
                return false;
            }

            IsRolling = false;
            return true;
        }

        internal void Reset()
        {
            elapsed = 0f;
            lastStartTime = float.NegativeInfinity;
            IsRolling = false;
        }
    }
}
