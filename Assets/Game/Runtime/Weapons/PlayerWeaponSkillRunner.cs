using System;
using JustTest.Game.Combat;
using JustTest.Game.Input;
using JustTest.Game.Player;
using UnityEngine;

namespace JustTest.Game.Weapons
{
    [DefaultExecutionOrder(-15)]
    public sealed class PlayerWeaponSkillRunner : MonoBehaviour
    {
        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private PlayerMovementController movementController;
        [SerializeField] private PlayerAttackRunner attackRunner;
        [SerializeField] private PlayerWeaponLoadout weaponLoadout;
        [SerializeField] private PlayerEnergyController energyController;
        [SerializeField] private DamageReceiver playerDamageReceiver;
        [SerializeField] private HealthComponent playerHealth;
        [SerializeField] private CombatReactionReceiver playerReaction;
        [SerializeField] private Transform skillAttackAnchor;
        [SerializeField] private Hitbox2D skillHitbox;
        [SerializeField] private BoxCollider2D skillHitboxCollider;

        private AttackInstanceFactory attackFactory;
        private AttackTimeline timeline;
        private AttackInstance activeAttack;
        private WeaponSkillDefinition activeDefinition;
        private Vector3 attackAnchorBaseLocalPosition;
        private Vector2 hitboxBaseSize;
        private int attackDirection = 1;
        private bool executing;
        private bool ready;

        internal event Action<HitResult> HitResolved;

        internal AttackPhase Phase => timeline?.Phase ?? AttackPhase.Idle;
        internal bool IsExecuting => executing;
        internal WeaponSkillDefinition ActiveDefinition => activeDefinition;
        internal WeaponSkillCancelReason LastCancelReason { get; private set; }

        private void Awake()
        {
            ready =
                inputReader != null &&
                movementController != null &&
                attackRunner != null &&
                weaponLoadout != null &&
                energyController != null &&
                playerDamageReceiver != null &&
                playerHealth != null &&
                playerReaction != null &&
                skillAttackAnchor != null &&
                skillHitbox != null &&
                skillHitboxCollider != null &&
                skillHitboxCollider.isTrigger;
            if (!ready)
            {
                Debug.LogError($"{nameof(PlayerWeaponSkillRunner)} is missing an Inspector reference.", this);
                enabled = false;
                return;
            }

            attackFactory = new AttackInstanceFactory();
            attackAnchorBaseLocalPosition = skillAttackAnchor.localPosition;
            hitboxBaseSize = skillHitboxCollider.size;
            skillHitbox.EndAttack();
        }

        private void OnEnable()
        {
            if (!ready)
            {
                return;
            }

            playerReaction.ReactionStarted += OnPlayerReactionStarted;
            playerHealth.Died += OnPlayerDied;
            playerDamageReceiver.CombatStateReset += OnCombatStateReset;
        }

        private void Update()
        {
            if (!ready)
            {
                return;
            }

            timeline?.Tick(Time.deltaTime);
            TryStartBufferedSkill(Time.time);
        }

        private void FixedUpdate()
        {
            if (!executing || activeDefinition == null)
            {
                return;
            }

            Vector2 velocity = activeDefinition.MovementVelocity;
            velocity.x = Mathf.Abs(velocity.x) * attackDirection;
            movementController.ApplyExternalVelocity(velocity);
        }

        private void OnDisable()
        {
            if (playerReaction != null)
            {
                playerReaction.ReactionStarted -= OnPlayerReactionStarted;
            }

            if (playerHealth != null)
            {
                playerHealth.Died -= OnPlayerDied;
            }

            if (playerDamageReceiver != null)
            {
                playerDamageReceiver.CombatStateReset -= OnCombatStateReset;
            }

            CancelSkill(WeaponSkillCancelReason.Disabled);
        }

        internal void CancelSkill(WeaponSkillCancelReason reason)
        {
            if (!executing)
            {
                return;
            }

            LastCancelReason = reason;
            CleanupSkill();
            movementController.ResetMotion();
        }

        private void TryStartBufferedSkill(float timestamp)
        {
            WeaponSkillDefinition definition = weaponLoadout.ActiveWeapon?.Skill;
            if (definition == null ||
                executing ||
                attackRunner.IsAttacking ||
                playerHealth.IsDead ||
                !movementController.CanStartAction ||
                !inputReader.HasBufferedWeaponSkill(timestamp, definition.InputBufferDuration))
            {
                return;
            }

            inputReader.ConsumeWeaponSkill();
            if (!definition.IsValid)
            {
                LastCancelReason = WeaponSkillCancelReason.InvalidDefinition;
                return;
            }

            if (!energyController.TrySpend(definition.EnergyCost))
            {
                LastCancelReason = WeaponSkillCancelReason.InsufficientEnergy;
                return;
            }

            StartSkill(definition);
        }

        private void StartSkill(WeaponSkillDefinition definition)
        {
            activeDefinition = definition;
            attackDirection = movementController.FacingDirection == -1 ? -1 : 1;
            LastCancelReason = WeaponSkillCancelReason.None;

            Vector3 localPosition = attackAnchorBaseLocalPosition;
            localPosition.x = Mathf.Abs(definition.HitboxOffset.x) * attackDirection;
            localPosition.y = definition.HitboxOffset.y;
            skillAttackAnchor.localPosition = localPosition;
            skillHitboxCollider.size = definition.HitboxSize;

            AttackDefinition attackDefinition = definition.Attack;
            activeAttack = attackFactory.Create(
                GetInstanceID(),
                CombatFaction.Player,
                attackDefinition.Damage,
                attackDirection,
                attackDefinition.HitReaction,
                attackDefinition.StatusApplication,
                attackDefinition.AllowFriendlyFire,
                attackDefinition.IgnorePostHitInvulnerability);
            activeAttack.HitResolved += OnAttackHitResolved;

            timeline = new AttackTimeline(
                attackDefinition.WindupDuration,
                attackDefinition.ActiveDuration,
                attackDefinition.RecoveryDuration);
            timeline.PhaseChanged += OnPhaseChanged;

            executing = true;
            movementController.CancelRoll();
            movementController.SetControlLock(PlayerControlLockSource.WeaponSkill, true);
            movementController.ResetMotion();
            timeline.Start();
        }

        private void OnPhaseChanged(AttackPhase previousPhase, AttackPhase nextPhase)
        {
            if (previousPhase == AttackPhase.Active)
            {
                skillHitbox.EndAttack();
            }

            if (nextPhase == AttackPhase.Active && !skillHitbox.BeginAttack(activeAttack))
            {
                CancelSkill(WeaponSkillCancelReason.HitboxActivationFailed);
                return;
            }

            if (nextPhase == AttackPhase.Idle)
            {
                CompleteSkill();
            }
        }

        private void OnAttackHitResolved(HitResult result)
        {
            HitResolved?.Invoke(result);
        }

        private void CompleteSkill()
        {
            if (!executing)
            {
                return;
            }

            Vector2 completionVelocity = activeDefinition.CompletionVelocity;
            completionVelocity.x = Mathf.Abs(completionVelocity.x) * attackDirection;
            CleanupSkill();
            movementController.ApplyExternalVelocity(completionVelocity);
        }

        private void CleanupSkill()
        {
            skillHitbox.EndAttack();
            if (timeline != null)
            {
                timeline.PhaseChanged -= OnPhaseChanged;
                timeline = null;
            }

            if (activeAttack != null)
            {
                activeAttack.HitResolved -= OnAttackHitResolved;
                activeAttack = null;
            }

            movementController.SetControlLock(PlayerControlLockSource.WeaponSkill, false);
            skillAttackAnchor.localPosition = attackAnchorBaseLocalPosition;
            skillHitboxCollider.size = hitboxBaseSize;
            activeDefinition = null;
            attackDirection = 1;
            executing = false;
        }

        private void OnPlayerReactionStarted(HitReactionData _)
        {
            CancelSkill(WeaponSkillCancelReason.PlayerHit);
        }

        private void OnPlayerDied()
        {
            CancelSkill(WeaponSkillCancelReason.PlayerDied);
        }

        private void OnCombatStateReset()
        {
            CancelSkill(WeaponSkillCancelReason.CombatReset);
        }
    }
}
