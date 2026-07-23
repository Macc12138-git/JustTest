using JustTest.Game.Input;
using UnityEngine;

namespace JustTest.Game.Combat
{
    public sealed class CombatSandboxReset : MonoBehaviour
    {
        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private DamageReceiver[] targets;
        [SerializeField] private CombatDebugConfig config;

        private bool ready;

        private void Awake()
        {
            ready = inputReader != null && targets != null && config != null;
            if (ready)
            {
                return;
            }

            Debug.LogError($"{nameof(CombatSandboxReset)} is missing an Inspector reference.", this);
            enabled = false;
        }

        private void Update()
        {
            if (!ready || !config.AllowManualReset || !inputReader.ResetPressedThisFrame)
            {
                return;
            }

            foreach (DamageReceiver target in targets)
            {
                target?.ResetCombatState();
            }
        }
    }
}
