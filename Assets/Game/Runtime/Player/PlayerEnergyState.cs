using System;

namespace JustTest.Game.Player
{
    internal sealed class PlayerEnergyState
    {
        private readonly float startingEnergy;

        internal PlayerEnergyState(float maximumEnergy, float startingEnergy)
        {
            if (!IsFinitePositive(maximumEnergy))
            {
                throw new ArgumentOutOfRangeException(nameof(maximumEnergy));
            }

            if (!IsFiniteNonNegative(startingEnergy) || startingEnergy > maximumEnergy)
            {
                throw new ArgumentOutOfRangeException(nameof(startingEnergy));
            }

            MaximumEnergy = maximumEnergy;
            this.startingEnergy = startingEnergy;
            CurrentEnergy = startingEnergy;
        }

        internal float CurrentEnergy { get; private set; }
        internal float MaximumEnergy { get; }

        internal bool CanSpend(float amount)
        {
            return IsFinitePositive(amount) && CurrentEnergy >= amount;
        }

        internal bool TrySpend(float amount)
        {
            if (!CanSpend(amount))
            {
                return false;
            }

            CurrentEnergy -= amount;
            return true;
        }

        internal float Restore(float amount)
        {
            if (!IsFinitePositive(amount) || CurrentEnergy >= MaximumEnergy)
            {
                return 0f;
            }

            float previousEnergy = CurrentEnergy;
            CurrentEnergy = Math.Min(MaximumEnergy, CurrentEnergy + amount);
            return CurrentEnergy - previousEnergy;
        }

        internal bool Reset()
        {
            if (CurrentEnergy == startingEnergy)
            {
                return false;
            }

            CurrentEnergy = startingEnergy;
            return true;
        }

        private static bool IsFinitePositive(float value)
        {
            return value > 0f && !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private static bool IsFiniteNonNegative(float value)
        {
            return value >= 0f && !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
