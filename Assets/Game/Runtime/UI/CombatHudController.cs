using JustTest.Game.Combat;
using JustTest.Game.Player;
using JustTest.Game.Weapons;
using UnityEngine;
using UnityEngine.UI;

namespace JustTest.Game.UI
{
    [DefaultExecutionOrder(100)]
    public sealed class CombatHudController : MonoBehaviour
    {
        [SerializeField] private CombatHudConfig config;
        [SerializeField] private HealthComponent playerHealth;
        [SerializeField] private PlayerEnergyController playerEnergy;
        [SerializeField] private PlayerWeaponLoadout weaponLoadout;
        [SerializeField] private PlayerWeaponQteController qteController;
        [SerializeField] private PlayerWeaponQteExecutor qteExecutor;
        [SerializeField] private CombatResourceBarView healthBar;
        [SerializeField] private CombatResourceBarView energyBar;
        [SerializeField] private CombatWeaponSlotView[] weaponSlots;
        [SerializeField] private Text skillNameText;
        [SerializeField] private Text skillCostText;

        private readonly CombatWeaponSlotStateResolver slotStateResolver =
            new CombatWeaponSlotStateResolver();
        private bool ready;

        private void Awake()
        {
            ready =
                config != null &&
                config.IsValid &&
                playerHealth != null &&
                playerEnergy != null &&
                weaponLoadout != null &&
                qteController != null &&
                qteExecutor != null &&
                healthBar != null &&
                energyBar != null &&
                weaponSlots != null &&
                weaponSlots.Length == WeaponLoadoutState.Capacity &&
                skillNameText != null &&
                skillCostText != null;
            if (ready)
            {
                for (int index = 0; index < weaponSlots.Length; index++)
                {
                    if (weaponSlots[index] == null)
                    {
                        ready = false;
                        break;
                    }
                }
            }

            if (ready)
            {
                return;
            }

            Debug.LogError($"{nameof(CombatHudController)} is missing an Inspector reference.", this);
            enabled = false;
        }

        private void OnEnable()
        {
            if (!ready)
            {
                return;
            }

            playerHealth.HealthChanged += OnHealthChanged;
            playerEnergy.EnergyChanged += OnEnergyChanged;
            weaponLoadout.ActiveWeaponChanged += OnActiveWeaponChanged;
            qteController.OpportunityChanged += OnQteOpportunityChanged;
            qteExecutor.ExecutionStateChanged += OnQteExecutionStateChanged;
            RefreshAll();
        }

        private void Update()
        {
            if (ready && qteController.HasOpportunity)
            {
                RefreshWeaponSlots();
            }
        }

        private void OnDisable()
        {
            if (playerHealth != null)
            {
                playerHealth.HealthChanged -= OnHealthChanged;
            }

            if (playerEnergy != null)
            {
                playerEnergy.EnergyChanged -= OnEnergyChanged;
            }

            if (weaponLoadout != null)
            {
                weaponLoadout.ActiveWeaponChanged -= OnActiveWeaponChanged;
            }

            if (qteController != null)
            {
                qteController.OpportunityChanged -= OnQteOpportunityChanged;
            }

            if (qteExecutor != null)
            {
                qteExecutor.ExecutionStateChanged -= OnQteExecutionStateChanged;
            }
        }

        private void RefreshAll()
        {
            RefreshHealth();
            RefreshEnergy();
            RefreshWeaponSlots();
            RefreshSkill();
        }

        private void RefreshHealth()
        {
            healthBar.SetValue(
                playerHealth.CurrentHealth,
                playerHealth.MaximumHealth,
                config.BarBackgroundColor,
                config.HealthColor,
                config.LowHealthColor,
                config.LowHealthThreshold);
        }

        private void RefreshEnergy()
        {
            energyBar.SetValue(
                playerEnergy.CurrentEnergy,
                playerEnergy.MaximumEnergy,
                config.BarBackgroundColor,
                config.EnergyColor,
                config.EnergyColor,
                0f);
        }

        private void RefreshWeaponSlots()
        {
            float qteNormalizedTime = qteController.OpportunityNormalizedTime;
            float pulse = Mathf.Sin(
                Time.unscaledTime * Mathf.PI * 2f * config.QtePulseCyclesPerSecond);
            float pulseAlpha = Mathf.Lerp(
                config.QtePulseMinimumAlpha,
                1f,
                (pulse + 1f) * 0.5f);

            for (int slotIndex = 0; slotIndex < weaponSlots.Length; slotIndex++)
            {
                WeaponDefinition weapon = weaponLoadout.GetWeapon(slotIndex);
                CombatWeaponSlotVisualState visualState = slotStateResolver.Resolve(
                    weapon != null,
                    weaponLoadout.ActiveSlotIndex == slotIndex,
                    qteController.IsCandidate(slotIndex),
                    qteExecutor.IsExecuting && qteExecutor.PendingSlotIndex == slotIndex);
                weaponSlots[slotIndex].Render(
                    slotIndex,
                    weapon,
                    visualState,
                    qteNormalizedTime,
                    pulseAlpha,
                    config);
            }
        }

        private void RefreshSkill()
        {
            WeaponSkillDefinition skill = weaponLoadout.ActiveWeapon?.Skill;
            if (skill == null)
            {
                skillNameText.text = "No Skill";
                skillCostText.text = string.Empty;
                return;
            }

            bool canAfford = playerEnergy.CurrentEnergy >= skill.EnergyCost;
            skillNameText.text = skill.DisplayName;
            skillCostText.text = $"{skill.EnergyCost:0} EN";
            skillCostText.color = canAfford
                ? config.AvailableSkillColor
                : config.UnavailableSkillColor;
        }

        private void OnHealthChanged(float _, float __)
        {
            RefreshHealth();
        }

        private void OnEnergyChanged(float _, float __)
        {
            RefreshEnergy();
            RefreshSkill();
        }

        private void OnActiveWeaponChanged(int _, WeaponDefinition __)
        {
            RefreshWeaponSlots();
            RefreshSkill();
        }

        private void OnQteOpportunityChanged()
        {
            RefreshWeaponSlots();
        }

        private void OnQteExecutionStateChanged()
        {
            RefreshWeaponSlots();
        }
    }
}
