using UnityEngine;

namespace JustTest.Game.Enemies
{
    public sealed class EnemyGroundProbe2D : MonoBehaviour
    {
        [SerializeField] private MeleeEnemyConfig config;
        [SerializeField] private Collider2D bodyCollider;

        private readonly RaycastHit2D[] castResults = new RaycastHit2D[8];
        private ContactFilter2D contactFilter;
        private bool ready;

        internal bool IsGrounded { get; private set; }

        private void Awake()
        {
            ready = config != null && bodyCollider != null;
            if (!ready)
            {
                Debug.LogError($"{nameof(EnemyGroundProbe2D)} is missing an Inspector reference.", this);
                enabled = false;
                return;
            }

            RefreshContactFilter();
        }

        internal void Refresh()
        {
            IsGrounded = false;
            if (!ready)
            {
                return;
            }

            RefreshContactFilter();
            int count = bodyCollider.Cast(
                Vector2.down,
                contactFilter,
                castResults,
                config.GroundProbeDistance);
            float minimumNormalY = Mathf.Cos(config.MaximumGroundAngle * Mathf.Deg2Rad);
            float closestDistance = float.PositiveInfinity;

            for (int index = 0; index < count; index++)
            {
                RaycastHit2D hit = castResults[index];
                if (hit.collider == null || hit.normal.y < minimumNormalY || hit.distance >= closestDistance)
                {
                    continue;
                }

                IsGrounded = true;
                closestDistance = hit.distance;
            }
        }

        private void RefreshContactFilter()
        {
            contactFilter.useLayerMask = true;
            contactFilter.SetLayerMask(config.GroundLayers);
            contactFilter.useTriggers = false;
        }
    }
}
