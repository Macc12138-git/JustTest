using JustTest.Game.Combat;
using UnityEngine;

namespace JustTest.Game.Enemies
{
    public sealed class EliteEnemyAttackTelegraph2D : MonoBehaviour
    {
        [SerializeField] private EnemyAttackRunner attackRunner;
        [SerializeField] private EliteEnemyController2D enemyController;
        [SerializeField] private EliteEnemyConfig config;
        [SerializeField] private Transform telegraphRoot;
        [SerializeField] private SpriteRenderer telegraphRenderer;

        private Vector3 baseScale;
        private float flashEndsAt;
        private bool flashTriggered;
        private bool ready;

        private void Awake()
        {
            ready =
                attackRunner != null &&
                enemyController != null &&
                config != null &&
                telegraphRoot != null &&
                telegraphRenderer != null;
            if (!ready)
            {
                Debug.LogError($"{nameof(EliteEnemyAttackTelegraph2D)} is missing an Inspector reference.", this);
                enabled = false;
                return;
            }

            baseScale = telegraphRoot.localScale;
            ResetTelegraph();
        }

        private void OnEnable()
        {
            if (ready)
            {
                attackRunner.PhaseChanged += OnAttackPhaseChanged;
            }
        }

        private void Update()
        {
            if (!ready || attackRunner.Phase != AttackPhase.Windup)
            {
                return;
            }

            EliteEnemyAttackType attackType = enemyController.CurrentAttackType;
            float flashThreshold = attackType switch
            {
                EliteEnemyAttackType.HeavySmash => config.HeavyFlashNormalizedTime,
                EliteEnemyAttackType.DashCleave => config.DashFlashNormalizedTime,
                _ => float.PositiveInfinity
            };
            if (!flashTriggered && attackRunner.PhaseProgress >= flashThreshold)
            {
                flashTriggered = true;
                flashEndsAt = Time.time + config.FlashDuration;
            }

            bool flashing = flashTriggered && Time.time < flashEndsAt;
            telegraphRenderer.color = flashing
                ? config.FlashColor
                : ResolveWindupColor(attackType);
            ApplyScale(flashing ? config.FlashScale : 1f);
        }

        private void OnDisable()
        {
            if (attackRunner != null)
            {
                attackRunner.PhaseChanged -= OnAttackPhaseChanged;
            }

            ResetTelegraph();
        }

        private void OnAttackPhaseChanged(AttackPhase phase)
        {
            flashTriggered = false;
            flashEndsAt = 0f;
            ApplyScale(1f);

            switch (phase)
            {
                case AttackPhase.Windup:
                    telegraphRenderer.color = ResolveWindupColor(
                        enemyController.CurrentAttackType);
                    telegraphRenderer.enabled = true;
                    break;
                case AttackPhase.Active:
                    telegraphRenderer.color = config.ActiveTelegraphColor;
                    telegraphRenderer.enabled = true;
                    break;
                default:
                    telegraphRenderer.enabled = false;
                    break;
            }
        }

        private Color ResolveWindupColor(EliteEnemyAttackType attackType)
        {
            return attackType switch
            {
                EliteEnemyAttackType.HeavySmash => config.HeavyTelegraphColor,
                EliteEnemyAttackType.DashCleave => config.DashTelegraphColor,
                _ => config.QuickTelegraphColor
            };
        }

        private void ApplyScale(float flashMultiplier)
        {
            float facing = attackRunner != null ? attackRunner.FacingDirection : 1f;
            float lengthMultiplier =
                enemyController != null &&
                enemyController.CurrentAttackType == EliteEnemyAttackType.DashCleave
                    ? config.DashTelegraphLengthScale
                    : 1f;
            telegraphRoot.localScale = new Vector3(
                Mathf.Abs(baseScale.x) * facing * lengthMultiplier * flashMultiplier,
                baseScale.y * flashMultiplier,
                baseScale.z);
        }

        private void ResetTelegraph()
        {
            flashTriggered = false;
            flashEndsAt = 0f;
            if (telegraphRoot != null)
            {
                telegraphRoot.localScale = baseScale;
            }

            if (telegraphRenderer != null)
            {
                telegraphRenderer.enabled = false;
            }
        }
    }
}
