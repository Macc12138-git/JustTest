using System.Collections;
using JustTest.Game.Input;
using UnityEngine;

namespace JustTest.Game.Player
{
    public sealed class SandboxRespawn : MonoBehaviour
    {
        [SerializeField] private PlayerMovementController movementController;
        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private Transform respawnPoint;
        [SerializeField] private PlayerMovementDebugConfig debugConfig;

        private Vector2 fallbackRespawnPosition;
        private Coroutine respawnRoutine;

        private void Awake()
        {
            fallbackRespawnPosition = transform.position;

            if (movementController == null || inputReader == null || debugConfig == null)
            {
                Debug.LogError($"{nameof(SandboxRespawn)} is missing a required reference.", this);
                enabled = false;
            }
        }

        private void Update()
        {
            if (respawnRoutine != null)
            {
                return;
            }

            bool manualReset = debugConfig.AllowManualReset && inputReader.ResetPressedThisFrame;
            bool fellOutOfBounds = transform.position.y < debugConfig.RespawnBelowY;
            if (manualReset || fellOutOfBounds)
            {
                respawnRoutine = StartCoroutine(RespawnAfterDelay());
            }
        }

        private void OnDisable()
        {
            if (respawnRoutine != null)
            {
                StopCoroutine(respawnRoutine);
                respawnRoutine = null;
            }
        }

        private IEnumerator RespawnAfterDelay()
        {
            if (debugConfig.RespawnDelay > 0f)
            {
                yield return new WaitForSeconds(debugConfig.RespawnDelay);
            }

            Vector2 position = respawnPoint != null ? respawnPoint.position : fallbackRespawnPosition;
            movementController.Teleport(position);
            respawnRoutine = null;
        }
    }
}
