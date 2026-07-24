using UnityEngine;

namespace JustTest.Game.Enemies
{
    public sealed class MeleeEnemyNavigationProbe2D : MonoBehaviour
    {
        [SerializeField] private MeleeEnemyConfig config;
        [SerializeField] private Collider2D bodyCollider;

        private readonly RaycastHit2D[] castResults = new RaycastHit2D[8];
        private readonly RaycastHit2D[] obstacleCastResults = new RaycastHit2D[8];
        private ContactFilter2D contactFilter;
        private bool ready;

        internal bool IsGrounded { get; private set; }
        internal Collider2D GroundCollider { get; private set; }
        internal bool IsStandingOnOneWayPlatform =>
            GroundCollider != null &&
            IsLayerInMask(GroundCollider.gameObject.layer, config.OneWayPlatformLayers);

        private void Awake()
        {
            ready = config != null && bodyCollider != null;
            if (!ready)
            {
                Debug.LogError($"{nameof(MeleeEnemyNavigationProbe2D)} is missing an Inspector reference.", this);
                enabled = false;
                return;
            }

            RefreshContactFilter();
        }

        internal void Refresh()
        {
            IsGrounded = false;
            GroundCollider = null;
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
                GroundCollider = hit.collider;
                closestDistance = hit.distance;
            }
        }

        internal bool TryGetForwardObstacle(int direction, out Collider2D obstacle)
        {
            obstacle = null;
            if (!ready || (direction != -1 && direction != 1))
            {
                return false;
            }

            RefreshContactFilter();
            int count = bodyCollider.Cast(
                Vector2.right * direction,
                contactFilter,
                obstacleCastResults,
                config.ForwardObstacleProbeDistance);
            float closestDistance = float.PositiveInfinity;

            for (int index = 0; index < count; index++)
            {
                RaycastHit2D hit = obstacleCastResults[index];
                if (hit.collider == null ||
                    hit.collider == GroundCollider ||
                    Mathf.Abs(hit.normal.x) < 0.5f ||
                    hit.distance >= closestDistance)
                {
                    continue;
                }

                obstacle = hit.collider;
                closestDistance = hit.distance;
            }

            return obstacle != null;
        }

        private void RefreshContactFilter()
        {
            contactFilter.useLayerMask = true;
            contactFilter.SetLayerMask(config.GroundLayers.value | config.OneWayPlatformLayers.value);
            contactFilter.useTriggers = false;
        }

        private static bool IsLayerInMask(int layer, LayerMask layerMask)
        {
            return (layerMask.value & (1 << layer)) != 0;
        }
    }
}
