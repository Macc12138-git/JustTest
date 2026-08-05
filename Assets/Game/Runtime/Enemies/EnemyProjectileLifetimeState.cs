using System;

namespace JustTest.Game.Enemies
{
    internal sealed class EnemyProjectileLifetimeState
    {
        private readonly float maximumLifetime;

        internal EnemyProjectileLifetimeState(float maximumLifetime)
        {
            if (maximumLifetime <= 0f ||
                float.IsNaN(maximumLifetime) ||
                float.IsInfinity(maximumLifetime))
            {
                throw new ArgumentOutOfRangeException(nameof(maximumLifetime));
            }

            this.maximumLifetime = maximumLifetime;
        }

        internal bool IsActive { get; private set; }
        internal float RemainingLifetime { get; private set; }

        internal bool Start()
        {
            if (IsActive)
            {
                return false;
            }

            IsActive = true;
            RemainingLifetime = maximumLifetime;
            return true;
        }

        internal bool Tick(float deltaTime)
        {
            if (!IsActive || deltaTime <= 0f)
            {
                return false;
            }

            RemainingLifetime = Math.Max(0f, RemainingLifetime - deltaTime);
            return RemainingLifetime <= 0f;
        }

        internal bool TryComplete()
        {
            if (!IsActive)
            {
                return false;
            }

            IsActive = false;
            RemainingLifetime = 0f;
            return true;
        }

        internal void Reset()
        {
            IsActive = false;
            RemainingLifetime = 0f;
        }
    }
}
