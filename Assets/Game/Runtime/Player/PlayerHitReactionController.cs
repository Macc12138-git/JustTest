using JustTest.Game.Combat;
using UnityEngine;

namespace JustTest.Game.Player
{
    public sealed class PlayerHitReactionController : MonoBehaviour
    {
        [SerializeField] private CombatReactionReceiver reactionReceiver;
        [SerializeField] private DamageReceiver damageReceiver;
        [SerializeField] private HealthComponent health;
        [SerializeField] private PlayerMovementController movementController;
        [SerializeField] private PlayerAttackRunner attackRunner;

        private bool ready;

        private void Awake()
        {
            ready =
                reactionReceiver != null &&
                damageReceiver != null &&
                health != null &&
                movementController != null &&
                attackRunner != null;
            if (ready)
            {
                return;
            }

            Debug.LogError($"{nameof(PlayerHitReactionController)} is missing an Inspector reference.", this);
            enabled = false;
        }

        private void OnEnable()
        {
            if (!ready)
            {
                return;
            }

            reactionReceiver.ReactionStarted += OnReactionStarted;
            reactionReceiver.ReactionEnded += OnReactionEnded;
            health.Died += OnDied;
            damageReceiver.CombatStateReset += OnCombatStateReset;
        }

        private void OnDisable()
        {
            if (reactionReceiver != null)
            {
                reactionReceiver.ReactionStarted -= OnReactionStarted;
                reactionReceiver.ReactionEnded -= OnReactionEnded;
            }

            if (health != null)
            {
                health.Died -= OnDied;
            }

            if (damageReceiver != null)
            {
                damageReceiver.CombatStateReset -= OnCombatStateReset;
            }

            ReleaseOwnedLocks();
        }

        private void OnReactionStarted(HitReactionData reaction)
        {
            attackRunner.CancelAttack();
            movementController.SetControlLock(PlayerControlLockSource.HitReaction, true);
            movementController.CancelRoll();
            movementController.ApplyExternalVelocity(reaction.KnockbackVelocity);
        }

        private void OnReactionEnded()
        {
            movementController.SetControlLock(PlayerControlLockSource.HitReaction, false);
        }

        private void OnDied()
        {
            attackRunner.CancelAttack();
            reactionReceiver.ResetReaction();
            movementController.SetControlLock(PlayerControlLockSource.Death, true);
            movementController.CancelRoll();
            movementController.ApplyExternalVelocity(Vector2.zero);
        }

        private void OnCombatStateReset()
        {
            attackRunner.CancelAttack();
            ReleaseOwnedLocks();
        }

        private void ReleaseOwnedLocks()
        {
            if (movementController == null)
            {
                return;
            }

            movementController.SetControlLock(PlayerControlLockSource.HitReaction, false);
            movementController.SetControlLock(PlayerControlLockSource.Death, false);
        }
    }
}
