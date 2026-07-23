using System;
using UnityEngine;

namespace JustTest.Game.Combat
{
    public sealed class HealthComponent : MonoBehaviour
    {
        private HealthState state;

        public event Action<float, float> HealthChanged;

        public event Action Died;

        public float CurrentHealth => state?.CurrentHealth ?? 0f;

        public float MaximumHealth => state?.MaximumHealth ?? 0f;

        public bool IsDead => state == null || state.IsDead;

        internal HealthState State => state;

        internal void Initialize(float maximumHealth)
        {
            state = new HealthState(maximumHealth);
            HealthChanged?.Invoke(state.CurrentHealth, state.MaximumHealth);
        }

        internal void NotifyDamageApplied(float appliedDamage)
        {
            if (state == null || appliedDamage <= 0f)
            {
                return;
            }

            HealthChanged?.Invoke(state.CurrentHealth, state.MaximumHealth);
            if (state.IsDead)
            {
                Died?.Invoke();
            }
        }

        public float Restore(float amount)
        {
            if (state == null)
            {
                return 0f;
            }

            float restoredHealth = state.Restore(amount);
            if (restoredHealth > 0f)
            {
                HealthChanged?.Invoke(state.CurrentHealth, state.MaximumHealth);
            }

            return restoredHealth;
        }

        public void RestoreToFull()
        {
            if (state == null)
            {
                return;
            }

            state.RestoreToFull();
            HealthChanged?.Invoke(state.CurrentHealth, state.MaximumHealth);
        }
    }
}
