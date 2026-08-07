using UnityEngine;

namespace JustTest.Game.Combat
{
    public sealed class Hurtbox2D : MonoBehaviour, ICombatHurtbox
    {
        [SerializeField] private Collider2D hurtboxCollider;
        [SerializeField] private DamageReceiver damageReceiver;

        private bool ready;
        private bool registered;

        internal Collider2D Collider => hurtboxCollider;

        bool ICombatHurtbox.IsAvailable =>
            this != null &&
            isActiveAndEnabled &&
            ready &&
            registered;

        private void Awake()
        {
            ready = hurtboxCollider != null && damageReceiver != null;
            if (ready)
            {
                return;
            }

            Debug.LogError($"{nameof(Hurtbox2D)} is missing an Inspector reference.", this);
            enabled = false;
        }

        private void OnEnable()
        {
            if (!ready)
            {
                return;
            }

            registered = CombatColliderRegistry.Instance.Register(
                hurtboxCollider.GetInstanceID(),
                this);
            if (registered)
            {
                return;
            }

            Debug.LogError(
                $"{nameof(Hurtbox2D)} could not register its Collider2D. " +
                "Each Collider2D can belong to only one Hurtbox2D.",
                this);
            enabled = false;
        }

        private void OnDisable()
        {
            if (!registered || hurtboxCollider == null)
            {
                return;
            }

            CombatColliderRegistry.Instance.Unregister(
                hurtboxCollider.GetInstanceID(),
                this);
            registered = false;
        }

        HitResult ICombatHurtbox.ReceiveHit(AttackInstance attackInstance)
        {
            if (!ready || !registered || attackInstance == null)
            {
                return new HitResult(
                    HitOutcome.InvalidRequest,
                    attackInstance?.InstanceId ?? 0,
                    0,
                    0f,
                    0f,
                    damageReceiver != null && damageReceiver.Health != null
                        ? damageReceiver.Health.CurrentHealth
                        : 0f,
                    false);
            }

            return attackInstance.TryHit(damageReceiver);
        }
    }
}
