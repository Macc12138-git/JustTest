using UnityEngine;

namespace JustTest.Game.Player
{
    public sealed class PlayerGroundProbe2D : MonoBehaviour
    {
        [SerializeField] private PlayerMovementConfig config;
        [SerializeField] private Collider2D bodyCollider;

        private readonly RaycastHit2D[] castResults = new RaycastHit2D[8];
        private ContactFilter2D contactFilter;

        internal bool IsGrounded { get; private set; }
        internal Collider2D GroundCollider { get; private set; }
        internal Vector2 GroundNormal { get; private set; } = Vector2.up;
        internal bool IsStandingOnOneWayPlatform =>
            GroundCollider != null && IsLayerInMask(GroundCollider.gameObject.layer, config.OneWayPlatformLayerMask);

        private void Awake()
        {
            if (config == null || bodyCollider == null)
            {
                Debug.LogError($"{nameof(PlayerGroundProbe2D)} is missing an Inspector reference.", this);
                enabled = false;
                return;
            }

            RefreshContactFilter();
        }

        internal void Refresh()
        {
            IsGrounded = false;
            GroundCollider = null;
            GroundNormal = Vector2.up;

            if (!enabled || bodyCollider == null)
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

            for (int i = 0; i < count; i++)
            {
                RaycastHit2D hit = castResults[i];
                if (hit.collider == null || hit.normal.y < minimumNormalY || hit.distance >= closestDistance)
                {
                    continue;
                }

                IsGrounded = true;
                GroundCollider = hit.collider;
                GroundNormal = hit.normal;
                closestDistance = hit.distance;
            }
        }

        internal void GetDebugProbeBounds(out Vector3 center, out Vector3 size)
        {
            Bounds bounds = bodyCollider != null ? bodyCollider.bounds : new Bounds(transform.position, Vector3.one);
            float inset = Mathf.Min(config.GroundProbeHorizontalInset, bounds.extents.x);
            center = bounds.center + Vector3.down * config.GroundProbeDistance;
            size = new Vector3(
                Mathf.Max(0.01f, bounds.size.x - inset * 2f),
                Mathf.Max(0.01f, bounds.size.y),
                0.01f);
        }

        private void RefreshContactFilter()
        {
            int layerMask = config.GroundLayerMask.value | config.OneWayPlatformLayerMask.value;
            contactFilter.useLayerMask = true;
            contactFilter.SetLayerMask(layerMask);
            contactFilter.useTriggers = false;
        }

        private static bool IsLayerInMask(int layer, LayerMask layerMask)
        {
            return (layerMask.value & (1 << layer)) != 0;
        }
    }
}
