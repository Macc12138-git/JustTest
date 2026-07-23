using UnityEngine;

namespace JustTest.Game.Combat
{
    public sealed class RigidbodyCombatReactionMotor2D : MonoBehaviour
    {
        [SerializeField] private CombatReactionReceiver reactionReceiver;
        [SerializeField] private DamageReceiver damageReceiver;
        [SerializeField] private Rigidbody2D body;

        private bool ready;

        private void Awake()
        {
            ready = reactionReceiver != null && damageReceiver != null && body != null;
            if (ready)
            {
                return;
            }

            Debug.LogError($"{nameof(RigidbodyCombatReactionMotor2D)} is missing an Inspector reference.", this);
            enabled = false;
        }

        private void OnEnable()
        {
            if (!ready)
            {
                return;
            }

            reactionReceiver.ReactionStarted += OnReactionStarted;
            damageReceiver.CombatStateReset += ResetMotion;
        }

        private void OnDisable()
        {
            if (reactionReceiver != null)
            {
                reactionReceiver.ReactionStarted -= OnReactionStarted;
            }

            if (damageReceiver != null)
            {
                damageReceiver.CombatStateReset -= ResetMotion;
            }
        }

        private void OnReactionStarted(HitReactionData reaction)
        {
            body.velocity = reaction.KnockbackVelocity;
        }

        private void ResetMotion()
        {
            body.velocity = Vector2.zero;
            body.angularVelocity = 0f;
        }
    }
}
