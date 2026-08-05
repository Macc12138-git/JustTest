using System;
using JustTest.Game.Combat;
using UnityEngine;

namespace JustTest.Game.Enemies
{
    [DefaultExecutionOrder(-10)]
    public sealed class EnemyProjectile2D : MonoBehaviour
    {
        [SerializeField] private EnemyProjectileConfig config;
        [SerializeField] private Rigidbody2D body;
        [SerializeField] private Collider2D projectileCollider;
        [SerializeField] private Hitbox2D hitbox;

        private EnemyProjectileLifetimeState lifetimeState;
        private bool ready;

        internal event Action<EnemyProjectile2D> Completed;
        internal event Action<EnemyProjectile2D> RecycleRequested;

        internal bool IsActive => lifetimeState != null && lifetimeState.IsActive;

        private void Awake()
        {
            ready =
                config != null &&
                config.IsValid &&
                body != null &&
                projectileCollider != null &&
                projectileCollider.isTrigger &&
                hitbox != null;
            if (!ready)
            {
                Debug.LogError($"{nameof(EnemyProjectile2D)} is missing an Inspector reference.", this);
                enabled = false;
                return;
            }

            hitbox.HitResolved += OnHitResolved;
            lifetimeState = new EnemyProjectileLifetimeState(config.MaximumLifetime);
        }

        private void Update()
        {
            if (!IsActive)
            {
                return;
            }

            if (lifetimeState.Tick(Time.deltaTime))
            {
                Complete();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (IsActive && other != null && config.IsObstacleLayer(other.gameObject.layer))
            {
                Complete();
            }
        }

        private void OnDestroy()
        {
            if (hitbox != null)
            {
                hitbox.HitResolved -= OnHitResolved;
            }
        }

        internal bool Launch(
            AttackInstance attack,
            Vector3 position,
            int direction)
        {
            if (attack == null || IsActive || (direction != -1 && direction != 1))
            {
                return false;
            }

            transform.SetPositionAndRotation(position, Quaternion.identity);
            gameObject.SetActive(true);
            if (!ready || !hitbox.BeginAttack(attack))
            {
                gameObject.SetActive(false);
                return false;
            }

            lifetimeState.Start();
            body.position = position;
            body.rotation = 0f;
            body.velocity = new Vector2(config.Speed * direction, 0f);
            body.angularVelocity = 0f;
            return true;
        }

        internal void PrepareForPool()
        {
            lifetimeState?.Reset();
            hitbox?.EndAttack();
            if (body != null)
            {
                body.velocity = Vector2.zero;
                body.angularVelocity = 0f;
            }

            gameObject.SetActive(false);
        }

        private void OnHitResolved(HitResult result)
        {
            if (IsActive)
            {
                Complete();
            }
        }

        private void Complete()
        {
            if (lifetimeState == null || !lifetimeState.TryComplete())
            {
                return;
            }

            hitbox.EndAttack();
            body.velocity = Vector2.zero;
            Completed?.Invoke(this);
            RecycleRequested?.Invoke(this);
        }
    }
}
