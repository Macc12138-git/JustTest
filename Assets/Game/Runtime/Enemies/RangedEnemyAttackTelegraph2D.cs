using JustTest.Game.Combat;
using UnityEngine;

namespace JustTest.Game.Enemies
{
    public sealed class RangedEnemyAttackTelegraph2D : MonoBehaviour
    {
        [SerializeField] private RangedEnemyAttackRunner attackRunner;
        [SerializeField] private RangedEnemyConfig config;
        [SerializeField] private LineRenderer laserGlow;
        [SerializeField] private LineRenderer laserCore;

        private float flashEndsAt;
        private bool flashTriggered;
        private bool ready;

        private void Awake()
        {
            ready =
                attackRunner != null &&
                config != null &&
                laserGlow != null &&
                laserCore != null;
            if (!ready)
            {
                Debug.LogError($"{nameof(RangedEnemyAttackTelegraph2D)} is missing an Inspector reference.", this);
                enabled = false;
                return;
            }

            ConfigureRenderer(laserGlow, config.TelegraphGlowWidth);
            ConfigureRenderer(laserCore, config.TelegraphCoreWidth);
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

            if (!flashTriggered &&
                attackRunner.PhaseProgress >= config.TelegraphFlashNormalizedTime)
            {
                flashTriggered = true;
                flashEndsAt = Time.time + config.TelegraphFlashDuration;
            }

            bool flashing = flashTriggered && Time.time < flashEndsAt;
            float pulse = 1f +
                Mathf.Sin(Time.time * config.TelegraphPulseFrequency) *
                config.TelegraphPulseAmplitude;
            float coreLength = Mathf.Lerp(
                config.TelegraphMinimumCoreLength,
                1f,
                Mathf.SmoothStep(0f, 1f, attackRunner.PhaseProgress));
            UpdateLaser(
                coreLength,
                flashing ? config.TelegraphFlashScale : pulse,
                flashing ? config.TelegraphFlashColor : config.TelegraphWindupColor,
                flashing ? config.TelegraphFlashColor : config.TelegraphGlowColor);
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

            switch (phase)
            {
                case AttackPhase.Windup:
                    SetLaserVisible(true);
                    UpdateLaser(
                        config.TelegraphMinimumCoreLength,
                        1f,
                        config.TelegraphWindupColor,
                        config.TelegraphGlowColor);
                    break;
                case AttackPhase.Active:
                    SetLaserVisible(true);
                    UpdateLaser(
                        1f,
                        config.TelegraphFlashScale,
                        config.TelegraphFlashColor,
                        config.TelegraphActiveColor);
                    break;
                default:
                    SetLaserVisible(false);
                    break;
            }
        }

        private void UpdateLaser(
            float coreLengthNormalized,
            float widthMultiplier,
            Color coreColor,
            Color glowColor)
        {
            float direction = attackRunner.FacingDirection;
            float fullLength = config.TelegraphLaserLength * direction;
            laserGlow.SetPosition(0, Vector3.zero);
            laserGlow.SetPosition(1, Vector3.right * fullLength);
            laserCore.SetPosition(0, Vector3.zero);
            laserCore.SetPosition(
                1,
                Vector3.right * (fullLength * Mathf.Clamp01(coreLengthNormalized)));
            laserGlow.startWidth = config.TelegraphGlowWidth * widthMultiplier;
            laserGlow.endWidth = config.TelegraphGlowWidth * widthMultiplier;
            laserCore.startWidth = config.TelegraphCoreWidth * widthMultiplier;
            laserCore.endWidth = config.TelegraphCoreWidth * widthMultiplier;
            laserGlow.startColor = glowColor;
            laserGlow.endColor = glowColor;
            laserCore.startColor = coreColor;
            laserCore.endColor = coreColor;
        }

        private static void ConfigureRenderer(LineRenderer renderer, float width)
        {
            renderer.useWorldSpace = false;
            renderer.loop = false;
            renderer.positionCount = 2;
            renderer.numCapVertices = 4;
            renderer.startWidth = width;
            renderer.endWidth = width;
        }

        private void SetLaserVisible(bool visible)
        {
            laserGlow.enabled = visible;
            laserCore.enabled = visible;
        }

        private void ResetTelegraph()
        {
            flashTriggered = false;
            flashEndsAt = 0f;
            laserGlow?.SetPosition(0, Vector3.zero);
            laserGlow?.SetPosition(1, Vector3.zero);
            laserCore?.SetPosition(0, Vector3.zero);
            laserCore?.SetPosition(1, Vector3.zero);
            if (laserGlow != null)
            {
                laserGlow.enabled = false;
            }

            if (laserCore != null)
            {
                laserCore.enabled = false;
            }
        }
    }
}
