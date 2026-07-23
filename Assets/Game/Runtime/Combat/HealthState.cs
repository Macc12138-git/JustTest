using System;

namespace JustTest.Game.Combat
{
    internal sealed class HealthState
    {
        internal HealthState(float maximumHealth)
        {
            if (maximumHealth <= 0f || float.IsNaN(maximumHealth) || float.IsInfinity(maximumHealth))
            {
                throw new ArgumentOutOfRangeException(nameof(maximumHealth));
            }

            MaximumHealth = maximumHealth;
            CurrentHealth = maximumHealth;
        }

        internal float MaximumHealth { get; }

        internal float CurrentHealth { get; private set; }

        internal bool IsDead => CurrentHealth <= 0f;

        internal float ApplyDamage(float damage)
        {
            if (damage <= 0f || IsDead)
            {
                return 0f;
            }

            float previousHealth = CurrentHealth;
            CurrentHealth = Math.Max(0f, CurrentHealth - damage);
            return previousHealth - CurrentHealth;
        }

        internal float Restore(float amount)
        {
            if (amount <= 0f || IsDead)
            {
                return 0f;
            }

            float previousHealth = CurrentHealth;
            CurrentHealth = Math.Min(MaximumHealth, CurrentHealth + amount);
            return CurrentHealth - previousHealth;
        }

        internal void RestoreToFull()
        {
            CurrentHealth = MaximumHealth;
        }
    }
}
