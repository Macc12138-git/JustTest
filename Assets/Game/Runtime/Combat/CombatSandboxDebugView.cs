using JustTest.Game.Player;
using JustTest.Game.Weapons;
using UnityEngine;

namespace JustTest.Game.Combat
{
    public sealed class CombatSandboxDebugView : MonoBehaviour
    {
        [SerializeField] private PlayerAttackRunner attackRunner;
        [SerializeField] private PlayerWeaponLoadout weaponLoadout;
        [SerializeField] private PlayerWeaponQteController weaponQteController;
        [SerializeField] private PlayerWeaponQteExecutor weaponQteExecutor;
        [SerializeField] private PlayerWeaponSkillRunner weaponSkillRunner;
        [SerializeField] private PlayerEnergyController playerEnergy;
        [SerializeField] private HealthComponent targetHealth;
        [SerializeField] private CombatReactionReceiver targetReaction;
        [SerializeField] private CombatStatusController targetStatuses;
        [SerializeField] private HealthComponent playerHealth;
        [SerializeField] private InvulnerabilityController playerInvulnerability;
        [SerializeField] private CombatReactionReceiver playerReaction;
        [SerializeField] private CombatDebugConfig config;

        private HitOutcome lastOutcome;
        private CombatStatusType lastAppliedStatus;
        private int lastStatusApplicationId;
        private GUIStyle overlayStyle;
        private GUIStyle weaponSlotStyle;
        private bool hasLastOutcome;
        private bool ready;

        private void Awake()
        {
            ready =
                attackRunner != null &&
                weaponLoadout != null &&
                weaponQteController != null &&
                weaponQteExecutor != null &&
                weaponSkillRunner != null &&
                playerEnergy != null &&
                targetHealth != null &&
                targetReaction != null &&
                targetStatuses != null &&
                playerHealth != null &&
                playerInvulnerability != null &&
                playerReaction != null &&
                config != null;
            if (ready)
            {
                return;
            }

            Debug.LogError($"{nameof(CombatSandboxDebugView)} is missing an Inspector reference.", this);
            enabled = false;
        }

        private void OnEnable()
        {
            if (ready)
            {
                attackRunner.HitResolved += OnHitResolved;
                weaponSkillRunner.HitResolved += OnHitResolved;
                weaponQteExecutor.HitResolved += OnHitResolved;
                targetStatuses.StatusApplied += OnStatusApplied;
            }
        }

        private void OnDisable()
        {
            if (attackRunner != null)
            {
                attackRunner.HitResolved -= OnHitResolved;
            }

            if (weaponSkillRunner != null)
            {
                weaponSkillRunner.HitResolved -= OnHitResolved;
            }

            if (weaponQteExecutor != null)
            {
                weaponQteExecutor.HitResolved -= OnHitResolved;
            }

            if (targetStatuses != null)
            {
                targetStatuses.StatusApplied -= OnStatusApplied;
            }
        }

        private void OnGUI()
        {
            if (!ready || !config.ShowOverlay)
            {
                return;
            }

            string outcomeText = hasLastOutcome ? lastOutcome.ToString() : "None";
            if (overlayStyle == null)
            {
                overlayStyle = new GUIStyle(GUI.skin.label);
            }

            overlayStyle.fontSize = config.OverlayFontSize;
            if (weaponSlotStyle == null)
            {
                weaponSlotStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    wordWrap = true
                };
            }

            weaponSlotStyle.fontSize = config.OverlayFontSize;
            DrawWeaponSlots();
            WeaponSkillDefinition currentSkill = weaponLoadout.ActiveWeapon?.Skill;
            string text =
                $"Current Weapon: {GetWeaponName(weaponLoadout.ActiveWeapon)}\n" +
                $"Energy: {playerEnergy.CurrentEnergy:0.0}/{playerEnergy.MaximumEnergy:0.0}\n" +
                $"Skill: {GetSkillName(currentSkill)}, Cost: {(currentSkill != null ? currentSkill.EnergyCost : 0f):0.0}\n" +
                $"Skill Action: {weaponSkillRunner.Phase}, Cancel: {weaponSkillRunner.LastCancelReason}\n" +
                $"QTE: {(weaponQteController.HasOpportunity ? $"{weaponQteController.OpportunityStatus} #{weaponQteController.OpportunityApplicationId}" : "None")}\n" +
                $"QTE Action: {weaponQteExecutor.Phase}, Strike: {weaponQteExecutor.CurrentStrikeIndex + 1}\n" +
                $"QTE Pending: {GetWeaponName(weaponQteExecutor.PendingWeapon)}, Cancel: {weaponQteExecutor.LastCancelReason}\n" +
                $"Attack: {attackRunner.Phase}\n" +
                $"Attack ID: {attackRunner.CurrentAttackInstanceId}\n" +
                $"Last Hit: {outcomeText}\n" +
                $"Target HP: {targetHealth.CurrentHealth:0}/{targetHealth.MaximumHealth:0}\n" +
                $"Target Reaction: {(targetReaction.IsReacting ? $"{targetReaction.RemainingDuration:0.00}s" : "None")}\n" +
                $"Statuses: {BuildActiveStatusText()}\n" +
                $"Unbalanced: {targetStatuses.GetRemainingDuration(CombatStatusType.Unbalanced):0.00}s\n" +
                $"Airborne: {targetStatuses.GetRemainingDuration(CombatStatusType.Airborne):0.00}s\n" +
                $"Stunned: {targetStatuses.GetRemainingDuration(CombatStatusType.Stunned):0.00}s\n" +
                $"Last Status: {lastAppliedStatus} #{lastStatusApplicationId}\n" +
                $"Player HP: {playerHealth.CurrentHealth:0}/{playerHealth.MaximumHealth:0}\n" +
                $"Reaction: {(playerReaction.IsReacting ? $"{playerReaction.RemainingDuration:0.00}s" : "None")}\n" +
                $"Invulnerable: {playerInvulnerability.IsInvulnerable}\n" +
                $"Enemy Attack: {config.NormalEnemyAttackKey}=Normal, {config.HeavyEnemyAttackKey}=Heavy\n" +
                $"Status Test: {config.ApplyUnbalancedKey}=Unbalanced, " +
                $"{config.ApplyAirborneKey}=Airborne, {config.ApplyStunnedKey}=Stunned";
            GUI.Label(
                new Rect(config.OverlayPosition, config.OverlaySize),
                text,
                overlayStyle);
        }

        private void DrawWeaponSlots()
        {
            Color previousColor = GUI.color;
            Color previousContentColor = GUI.contentColor;
            Vector2 position = config.WeaponSlotsPosition;
            Vector2 size = config.WeaponSlotSize;

            for (int slotIndex = 0; slotIndex < WeaponLoadoutState.Capacity; slotIndex++)
            {
                WeaponDefinition weapon = weaponLoadout.GetWeapon(slotIndex);
                bool isCandidate = weaponQteController.IsCandidate(slotIndex);
                bool isActive = weaponLoadout.ActiveSlotIndex == slotIndex;
                bool isExecuting =
                    weaponQteExecutor.IsExecuting &&
                    weaponQteExecutor.PendingSlotIndex == slotIndex;
                Color fillColor = isCandidate
                    ? config.QteCandidateColor
                    : isExecuting
                        ? config.QteCandidateColor
                    : isActive
                        ? config.ActiveWeaponColor
                        : config.InactiveWeaponColor;
                fillColor.a = isCandidate || isExecuting ? 0.65f : isActive ? 0.45f : 0.22f;
                GUI.contentColor = weapon != null ? weapon.DebugColor : Color.white;

                string stateText = isExecuting
                    ? "QTE EXECUTING"
                    : isCandidate
                        ? "QTE"
                        : isActive
                            ? "ACTIVE"
                            : string.Empty;
                string label = $"{slotIndex + 1}  {GetWeaponName(weapon)}";
                if (!string.IsNullOrEmpty(stateText))
                {
                    label += $"\n{stateText}";
                }

                float x = position.x + slotIndex * (size.x + config.WeaponSlotGap);
                Rect slotRect = new Rect(x, position.y, size.x, size.y);
                GUI.Box(slotRect, GUIContent.none);
                GUI.color = fillColor;
                GUI.DrawTexture(
                    new Rect(slotRect.x + 1f, slotRect.y + 1f, slotRect.width - 2f, slotRect.height - 2f),
                    Texture2D.whiteTexture);
                GUI.color = previousColor;
                GUI.Label(slotRect, label, weaponSlotStyle);
            }

            GUI.color = previousColor;
            GUI.contentColor = previousContentColor;
        }

        private void OnHitResolved(HitResult result)
        {
            lastOutcome = result.Outcome;
            hasLastOutcome = true;
            if (config.LogHitResults)
            {
                Debug.Log(
                    $"Attack {result.AttackInstanceId}: {result.Outcome}, " +
                    $"damage={result.AppliedDamage:0.##}, remaining={result.RemainingHealth:0.##}",
                    this);
            }
        }

        private void OnStatusApplied(CombatStatusEvent statusEvent)
        {
            lastAppliedStatus = statusEvent.StatusType;
            lastStatusApplicationId = statusEvent.ApplicationId;
        }

        private string BuildActiveStatusText()
        {
            string text = string.Empty;
            AppendStatus(ref text, CombatStatusType.Unbalanced);
            AppendStatus(ref text, CombatStatusType.Airborne);
            AppendStatus(ref text, CombatStatusType.Stunned);
            return string.IsNullOrEmpty(text) ? "None" : text;
        }

        private void AppendStatus(ref string text, CombatStatusType statusType)
        {
            if (!targetStatuses.IsActive(statusType))
            {
                return;
            }

            text = string.IsNullOrEmpty(text) ? statusType.ToString() : $"{text}, {statusType}";
        }

        private static string GetWeaponName(WeaponDefinition weapon)
        {
            return weapon != null ? weapon.DisplayName : "Empty";
        }

        private static string GetSkillName(WeaponSkillDefinition skill)
        {
            return skill != null ? skill.DisplayName : "None";
        }
    }
}
