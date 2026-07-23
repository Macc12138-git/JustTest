using UnityEngine;

namespace JustTest.Game.Combat
{
    public sealed class CombatSandboxDebugView : MonoBehaviour
    {
        [SerializeField] private PlayerAttackRunner attackRunner;
        [SerializeField] private HealthComponent targetHealth;
        [SerializeField] private CombatDebugConfig config;

        private HitOutcome lastOutcome;
        private GUIStyle overlayStyle;
        private bool hasLastOutcome;
        private bool ready;

        private void Awake()
        {
            ready = attackRunner != null && targetHealth != null && config != null;
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
            }
        }

        private void OnDisable()
        {
            if (attackRunner != null)
            {
                attackRunner.HitResolved -= OnHitResolved;
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
            string text =
                $"Attack: {attackRunner.Phase}\n" +
                $"Attack ID: {attackRunner.CurrentAttackInstanceId}\n" +
                $"Last Hit: {outcomeText}\n" +
                $"Target HP: {targetHealth.CurrentHealth:0}/{targetHealth.MaximumHealth:0}";
            GUI.Label(
                new Rect(config.OverlayPosition, config.OverlaySize),
                text,
                overlayStyle);
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
    }
}
