using System;
using System.Collections;
using JustTest.Game.Combat;
using JustTest.Game.Enemies;
using JustTest.Game.Player;
using UnityEngine;

namespace JustTest.Game.Run
{
    [DefaultExecutionOrder(-100)]
    public sealed class CombatPlatformController2D : MonoBehaviour
    {
        [SerializeField] private CombatPlatformConfig config;
        [SerializeField] private Collider2D entryTrigger;
        [SerializeField] private Collider2D combatSurface;
        [SerializeField] private Collider2D playerCollider;
        [SerializeField] private PlayerGroundProbe2D playerGroundProbe;
        [SerializeField] private HealthComponent playerHealth;
        [SerializeField] private Collider2D[] boundaries;
        [SerializeField] private SpriteRenderer[] boundaryVisuals;
        [SerializeField] private MeleeEnemyController2D[] enemies;

        private readonly CombatPlatformStateMachine stateMachine = new CombatPlatformStateMachine();
        private Coroutine appearanceRoutine;
        private MeleeEnemyController2D activeAttacker;
        private float nextAttackAllowedAt;
        private int livingEnemyCount;
        private bool ready;

        internal event Action Completed;

        internal CombatPlatformState State => stateMachine.State;
        internal bool IsCombatActive => stateMachine.State == CombatPlatformState.Active;

        private void Awake()
        {
            ready = ValidateReferences();
            if (!ready)
            {
                Debug.LogError($"{nameof(CombatPlatformController2D)} has an invalid Inspector binding or combat surface.", this);
                enabled = false;
                return;
            }

            SetBoundariesClosed(false);
            for (int index = 0; index < enemies.Length; index++)
            {
                MeleeEnemyController2D enemy = enemies[index];
                enemy.PrepareForEncounter();
                enemy.gameObject.SetActive(false);
            }
        }

        private void OnEnable()
        {
            if (!ready)
            {
                return;
            }

            playerHealth.Died += OnPlayerDied;
            for (int index = 0; index < enemies.Length; index++)
            {
                enemies[index].Defeated += OnEnemyDefeated;
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!ready ||
                stateMachine.State != CombatPlatformState.Dormant ||
                other != playerCollider ||
                playerHealth.IsDead ||
                !playerGroundProbe.IsGrounded ||
                playerGroundProbe.GroundCollider != combatSurface)
            {
                return;
            }

            BeginEncounter();
        }

        private void OnDisable()
        {
            if (playerHealth != null)
            {
                playerHealth.Died -= OnPlayerDied;
            }

            if (enemies != null)
            {
                for (int index = 0; index < enemies.Length; index++)
                {
                    if (enemies[index] != null)
                    {
                        enemies[index].Defeated -= OnEnemyDefeated;
                        enemies[index].InterruptEncounter();
                    }
                }
            }

            if (appearanceRoutine != null)
            {
                StopCoroutine(appearanceRoutine);
                appearanceRoutine = null;
            }

            activeAttacker = null;
            SetBoundariesClosed(false);
        }

        internal bool TryAcquireAttack(MeleeEnemyController2D requester)
        {
            if (!ready ||
                !IsCombatActive ||
                requester == null ||
                activeAttacker != null ||
                Time.time < nextAttackAllowedAt)
            {
                return false;
            }

            activeAttacker = requester;
            return true;
        }

        internal void ReleaseAttack(MeleeEnemyController2D requester)
        {
            if (activeAttacker != requester)
            {
                return;
            }

            activeAttacker = null;
            nextAttackAllowedAt = Time.time + config.SharedAttackInterval;
        }

        private bool ValidateReferences()
        {
            if (config == null ||
                !config.IsValid ||
                entryTrigger == null ||
                !entryTrigger.isTrigger ||
                !config.IsValidCombatSurface(combatSurface) ||
                playerCollider == null ||
                playerGroundProbe == null ||
                playerHealth == null ||
                boundaries == null ||
                boundaries.Length == 0 ||
                enemies == null ||
                enemies.Length == 0)
            {
                return false;
            }

            for (int index = 0; index < boundaries.Length; index++)
            {
                Collider2D boundary = boundaries[index];
                if (boundary == null || boundary.isTrigger || boundary == combatSurface)
                {
                    return false;
                }
            }

            for (int index = 0; index < enemies.Length; index++)
            {
                if (enemies[index] == null)
                {
                    return false;
                }
            }

            return true;
        }

        private void BeginEncounter()
        {
            if (!stateMachine.TryBeginAppearance())
            {
                return;
            }

            SetBoundariesClosed(true);
            appearanceRoutine = StartCoroutine(ActivateEnemiesAfterDelay());
        }

        private IEnumerator ActivateEnemiesAfterDelay()
        {
            if (config.AppearanceDelay > 0f)
            {
                yield return new WaitForSeconds(config.AppearanceDelay);
            }

            if (!stateMachine.TryActivate())
            {
                appearanceRoutine = null;
                yield break;
            }

            livingEnemyCount = enemies.Length;
            for (int index = 0; index < enemies.Length; index++)
            {
                MeleeEnemyController2D enemy = enemies[index];
                enemy.gameObject.SetActive(true);
                enemy.ActivateEncounter();
            }

            appearanceRoutine = null;
        }

        private void OnEnemyDefeated(MeleeEnemyController2D enemy)
        {
            ReleaseAttack(enemy);
            livingEnemyCount = Mathf.Max(0, livingEnemyCount - 1);
            if (livingEnemyCount > 0 || !stateMachine.TryComplete())
            {
                return;
            }

            SetBoundariesClosed(false);
            Completed?.Invoke();
        }

        private void OnPlayerDied()
        {
            bool encounterStarted = stateMachine.State != CombatPlatformState.Dormant;
            if (!stateMachine.TryInterrupt())
            {
                return;
            }

            if (appearanceRoutine != null)
            {
                StopCoroutine(appearanceRoutine);
                appearanceRoutine = null;
            }

            activeAttacker = null;
            for (int index = 0; index < enemies.Length; index++)
            {
                if (enemies[index].gameObject.activeSelf)
                {
                    enemies[index].InterruptEncounter();
                }
            }

            SetBoundariesClosed(encounterStarted);
        }

        private void SetBoundariesClosed(bool closed)
        {
            if (boundaries != null)
            {
                for (int index = 0; index < boundaries.Length; index++)
                {
                    if (boundaries[index] != null)
                    {
                        boundaries[index].enabled = closed;
                    }
                }
            }

            if (boundaryVisuals == null)
            {
                return;
            }

            for (int index = 0; index < boundaryVisuals.Length; index++)
            {
                if (boundaryVisuals[index] != null)
                {
                    boundaryVisuals[index].enabled = closed;
                }
            }
        }
    }
}
