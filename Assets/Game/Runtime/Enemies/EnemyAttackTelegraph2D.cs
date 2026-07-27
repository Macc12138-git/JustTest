using JustTest.Game.Combat;
using UnityEngine;

namespace JustTest.Game.Enemies
{
    public sealed class EnemyAttackTelegraph2D : MonoBehaviour
    {
        [SerializeField] private EnemyAttackRunner attackRunner;
        [SerializeField] private MeleeEnemyController2D enemyController;
        [SerializeField] private MeleeEnemyConfig config;
        [SerializeField] private Transform telegraphRoot;
        [SerializeField] private SpriteRenderer telegraphRenderer;

        private Vector3 baseScale;
        private float heavyFlashEndsAt;
        private bool heavyFlashTriggered;
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
                Debug.LogError($"{nameof(EnemyAttackTelegraph2D)} is missing an Inspector reference.", this);
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
            if (!ready ||
                attackRunner.Phase != AttackPhase.Windup ||
                !enemyController.IsCurrentAttackHeavy)
            {
                return;
            }

            if (!heavyFlashTriggered && attackRunner.PhaseProgress >= config.HeavyFlashNormalizedTime)
            {
                heavyFlashTriggered = true;
                heavyFlashEndsAt = Time.time + config.HeavyFlashDuration;
            }

            bool flashing = heavyFlashTriggered && Time.time < heavyFlashEndsAt;
            telegraphRenderer.color = flashing
                ? config.HeavyFlashColor
                : config.HeavyTelegraphColor;
            ApplyScale(flashing ? config.HeavyFlashScale : 1f);
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
            heavyFlashTriggered = false;
            heavyFlashEndsAt = 0f;
            ApplyScale(1f);

            switch (phase)
            {
                case AttackPhase.Windup:
                    telegraphRenderer.color = enemyController.IsCurrentAttackHeavy
                        ? config.HeavyTelegraphColor
                        : config.TelegraphWindupColor;
                    telegraphRenderer.enabled = true;
                    break;
                case AttackPhase.Active:
                    telegraphRenderer.color = config.TelegraphActiveColor;
                    telegraphRenderer.enabled = true;
                    break;
                default:
                    telegraphRenderer.enabled = false;
                    break;
            }
        }

        private void ApplyScale(float multiplier)
        {
            float facing = attackRunner != null ? attackRunner.FacingDirection : 1f;
            telegraphRoot.localScale = new Vector3(
                Mathf.Abs(baseScale.x) * facing * multiplier,
                baseScale.y * multiplier,
                baseScale.z);
        }

        private void ResetTelegraph()
        {
            heavyFlashTriggered = false;
            heavyFlashEndsAt = 0f;
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
