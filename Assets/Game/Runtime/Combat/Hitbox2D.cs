using System;
using UnityEngine;

namespace JustTest.Game.Combat
{
    public sealed class Hitbox2D : MonoBehaviour
    {
        [SerializeField] private Collider2D hitboxCollider;
        [SerializeField] private LayerMask targetLayers = ~0;

        private AttackInstance activeAttack;
        private bool ready;

        public event Action<HitResult> HitResolved;

        public bool IsActive => activeAttack != null;

        private void Awake()
        {
            ready = hitboxCollider != null && hitboxCollider.isTrigger;
            if (!ready)
            {
                Debug.LogError(
                    $"{nameof(Hitbox2D)} requires an Inspector-bound trigger Collider2D.",
                    this);
                enabled = false;
                return;
            }

            hitboxCollider.enabled = false;
        }

        private void OnDisable()
        {
            EndAttack();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TryProcessContact(other, out _);
        }

        internal bool BeginAttack(AttackInstance attackInstance)
        {
            if (!ready || attackInstance == null)
            {
                return false;
            }

            activeAttack = attackInstance;
            hitboxCollider.enabled = true;
            return true;
        }

        internal void EndAttack()
        {
            activeAttack = null;
            if (hitboxCollider != null)
            {
                hitboxCollider.enabled = false;
            }
        }

        internal bool TryProcessContact(Collider2D other, out HitResult result)
        {
            result = default;
            if (!ready || activeAttack == null || other == null || !IsTargetLayer(other.gameObject.layer))
            {
                return false;
            }

            if (!CombatColliderRegistry.Instance.TryResolve(
                    other.GetInstanceID(),
                    out ICombatHurtbox hurtbox))
            {
                return false;
            }

            result = hurtbox.ReceiveHit(activeAttack);
            HitResolved?.Invoke(result);
            return true;
        }

        private bool IsTargetLayer(int layer)
        {
            return (targetLayers.value & (1 << layer)) != 0;
        }
    }
}
