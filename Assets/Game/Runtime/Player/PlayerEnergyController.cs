using System;
using JustTest.Game.Combat;
using JustTest.Game.Weapons;
using UnityEngine;

namespace JustTest.Game.Player
{
    [DefaultExecutionOrder(-50)]
    public sealed class PlayerEnergyController : MonoBehaviour
    {
        [SerializeField] private PlayerEnergyConfig config;
        [SerializeField] private HealthComponent playerHealth;
        [SerializeField] private DamageReceiver playerDamageReceiver;
        [SerializeField] private PlayerAttackRunner attackRunner;
        [SerializeField] private PlayerWeaponSkillRunner skillRunner;
        [SerializeField] private PlayerWeaponQteExecutor qteExecutor;

        private PlayerEnergyState state;
        private bool ready;

        public event Action<float, float> EnergyChanged;

        public float CurrentEnergy => state?.CurrentEnergy ?? 0f;
        public float MaximumEnergy => state?.MaximumEnergy ?? 0f;
        public float NormalizedEnergy => MaximumEnergy > 0f ? CurrentEnergy / MaximumEnergy : 0f;

        private void Awake()
        {
            ready =
                config != null &&
                config.IsValid &&
                playerHealth != null &&
                playerDamageReceiver != null &&
                attackRunner != null &&
                skillRunner != null &&
                qteExecutor != null;
            if (!ready)
            {
                Debug.LogError($"{nameof(PlayerEnergyController)} is missing an Inspector reference.", this);
                enabled = false;
                return;
            }

            state = new PlayerEnergyState(config.MaximumEnergy, config.StartingEnergy);
        }

        private void OnEnable()
        {
            if (!ready)
            {
                return;
            }

            attackRunner.HitResolved += OnPlayerHitResolved;
            skillRunner.HitResolved += OnPlayerHitResolved;
            qteExecutor.HitResolved += OnPlayerHitResolved;
            playerDamageReceiver.CombatStateReset += OnCombatStateReset;
        }

        private void Update()
        {
            if (!ready || playerHealth.IsDead || config.AutomaticRecoveryPerSecond <= 0f)
            {
                return;
            }

            RestoreEnergy(config.AutomaticRecoveryPerSecond * Time.deltaTime);
        }

        private void OnDisable()
        {
            if (attackRunner != null)
            {
                attackRunner.HitResolved -= OnPlayerHitResolved;
            }

            if (skillRunner != null)
            {
                skillRunner.HitResolved -= OnPlayerHitResolved;
            }

            if (qteExecutor != null)
            {
                qteExecutor.HitResolved -= OnPlayerHitResolved;
            }

            if (playerDamageReceiver != null)
            {
                playerDamageReceiver.CombatStateReset -= OnCombatStateReset;
            }
        }

        internal bool TrySpend(float amount)
        {
            if (!ready || !state.TrySpend(amount))
            {
                return false;
            }

            NotifyEnergyChanged();
            return true;
        }

        private void OnPlayerHitResolved(HitResult result)
        {
            if (result.WasApplied && result.AppliedDamage > 0f)
            {
                RestoreEnergy(config.RecoveryPerHit);
            }
        }

        private void RestoreEnergy(float amount)
        {
            if (state.Restore(amount) > 0f)
            {
                NotifyEnergyChanged();
            }
        }

        private void OnCombatStateReset()
        {
            if (state.Reset())
            {
                NotifyEnergyChanged();
            }
        }

        private void NotifyEnergyChanged()
        {
            EnergyChanged?.Invoke(state.CurrentEnergy, state.MaximumEnergy);
        }
    }
}
