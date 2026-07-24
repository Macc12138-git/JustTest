using JustTest.Game.Combat;
using UnityEngine;

namespace JustTest.Game.Enemies
{
    public sealed class EnemyAttackTelegraph2D : MonoBehaviour
    {
        [SerializeField] private EnemyAttackRunner attackRunner;
        [SerializeField] private MeleeEnemyConfig config;
        [SerializeField] private Transform telegraphRoot;
        [SerializeField] private SpriteRenderer telegraphRenderer;

        private Vector3 baseScale;
        private bool ready;

        private void Awake()
        {
            ready =
                attackRunner != null &&
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
            telegraphRenderer.enabled = false;
        }

        private void OnEnable()
        {
            if (ready)
            {
                attackRunner.PhaseChanged += OnAttackPhaseChanged;
            }
        }

        private void OnDisable()
        {
            if (attackRunner != null)
            {
                attackRunner.PhaseChanged -= OnAttackPhaseChanged;
            }

            if (telegraphRenderer != null)
            {
                telegraphRenderer.enabled = false;
            }
        }

        private void OnAttackPhaseChanged(AttackPhase phase)
        {
            telegraphRoot.localScale = new Vector3(
                Mathf.Abs(baseScale.x) * attackRunner.FacingDirection,
                baseScale.y,
                baseScale.z);

            switch (phase)
            {
                case AttackPhase.Windup:
                    telegraphRenderer.color = config.TelegraphWindupColor;
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
    }
}
