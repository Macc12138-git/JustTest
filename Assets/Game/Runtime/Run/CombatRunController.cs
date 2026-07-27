using System;
using System.Collections;
using JustTest.Game.Combat;
using JustTest.Game.Input;
using JustTest.Game.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JustTest.Game.Run
{
    [DefaultExecutionOrder(-10)]
    public sealed class CombatRunController : MonoBehaviour
    {
        [SerializeField] private CombatRunConfig config;
        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private HealthComponent playerHealth;
        [SerializeField] private PlayerMovementController playerMovement;
        [SerializeField] private CombatHitStopController hitStopController;

        private readonly CombatRunStateMachine stateMachine = new CombatRunStateMachine();
        private Coroutine restartRoutine;
        private float restartAvailableAt;
        private bool ready;

        internal event Action<CombatRunState> RunStateChanged;

        internal CombatRunState State => stateMachine.State;

        private void Awake()
        {
            ready =
                config != null &&
                config.IsValid &&
                inputReader != null &&
                playerHealth != null &&
                playerMovement != null &&
                hitStopController != null;
            if (ready)
            {
                return;
            }

            Debug.LogError($"{nameof(CombatRunController)} is missing an Inspector reference.", this);
            enabled = false;
        }

        private void OnEnable()
        {
            if (ready)
            {
                playerHealth.Died += OnPlayerDied;
            }
        }

        private void Update()
        {
            if (!ready || !inputReader.ResetPressedThisFrame || restartRoutine != null)
            {
                return;
            }

            if (stateMachine.State == CombatRunState.PlayerDefeated &&
                Time.unscaledTime < restartAvailableAt)
            {
                return;
            }

            TryBeginRestart();
        }

        private void OnDisable()
        {
            if (playerHealth != null)
            {
                playerHealth.Died -= OnPlayerDied;
            }

            if (restartRoutine != null)
            {
                StopCoroutine(restartRoutine);
                restartRoutine = null;
            }

            if (playerMovement != null)
            {
                playerMovement.SetControlLock(PlayerControlLockSource.External, false);
            }
        }

        private void OnPlayerDied()
        {
            if (!stateMachine.TryMarkPlayerDefeated())
            {
                return;
            }

            restartAvailableAt = Time.unscaledTime + config.RestartInputDelayAfterDefeat;
            RunStateChanged?.Invoke(stateMachine.State);
        }

        private void TryBeginRestart()
        {
            if (!stateMachine.TryBeginRestart(config.AllowRestartWhileActive))
            {
                return;
            }

            playerMovement.SetControlLock(PlayerControlLockSource.External, true);
            playerMovement.CancelRoll();
            playerMovement.ResetMotion();
            inputReader.ClearBufferedActions();
            hitStopController.ResetStop();
            RunStateChanged?.Invoke(stateMachine.State);
            restartRoutine = StartCoroutine(ReloadCurrentScene());
        }

        private IEnumerator ReloadCurrentScene()
        {
            if (config.SceneReloadDelay > 0f)
            {
                yield return new WaitForSecondsRealtime(config.SceneReloadDelay);
            }

            Scene currentScene = gameObject.scene;
            if (!currentScene.IsValid() || currentScene.buildIndex < 0)
            {
                Debug.LogError($"{nameof(CombatRunController)} cannot reload the current scene.", this);
                restartRoutine = null;
                yield break;
            }

            SceneManager.LoadSceneAsync(currentScene.buildIndex, LoadSceneMode.Single);
        }
    }
}
