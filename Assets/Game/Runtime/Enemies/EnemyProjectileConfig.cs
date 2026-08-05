using UnityEngine;

namespace JustTest.Game.Enemies
{
    [CreateAssetMenu(fileName = "EnemyProjectileConfig", menuName = "JustTest/Enemies/Enemy Projectile Config")]
    public sealed class EnemyProjectileConfig : ScriptableObject
    {
        [SerializeField, Min(0.01f)] private float speed = 8.5f;
        [SerializeField, Min(0.01f)] private float maximumLifetime = 1.6f;
        [SerializeField] private LayerMask obstacleLayers;

        internal float Speed => speed;
        internal float MaximumLifetime => maximumLifetime;
        internal bool IsValid =>
            IsFinitePositive(speed) &&
            IsFinitePositive(maximumLifetime) &&
            obstacleLayers.value != 0;

        internal bool IsObstacleLayer(int layer)
        {
            return (obstacleLayers.value & (1 << layer)) != 0;
        }

        private void OnValidate()
        {
            speed = Mathf.Max(0.01f, SanitizeFinite(speed));
            maximumLifetime = Mathf.Max(0.01f, SanitizeFinite(maximumLifetime));
        }

        private static bool IsFinitePositive(float value)
        {
            return value > 0f && !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private static float SanitizeFinite(float value)
        {
            return float.IsNaN(value) || float.IsInfinity(value) ? 0f : value;
        }
    }
}
