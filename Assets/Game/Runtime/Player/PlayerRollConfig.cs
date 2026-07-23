using UnityEngine;

namespace JustTest.Game.Player
{
    [CreateAssetMenu(fileName = "PlayerRollConfig", menuName = "JustTest/Player/Roll Config")]
    public sealed class PlayerRollConfig : ScriptableObject
    {
        [SerializeField, Min(0f)] private float speed = 12f;
        [SerializeField, Min(0.01f)] private float duration = 0.35f;
        [SerializeField, Min(0f)] private float invulnerabilityStartTime;
        [SerializeField, Min(0f)] private float invulnerabilityDuration = 0.2f;
        [SerializeField, Min(0f)] private float minimumStartInterval = 0.45f;
        [SerializeField, Min(0f)] private float inputBufferTime = 0.1f;
        [SerializeField, Min(0f)] private float gravityMultiplier = 1f;
        [SerializeField, Range(0f, 1f)] private float exitVelocityRetention = 0.35f;
        [SerializeField] private bool allowRollingOffPlatforms = true;

        internal float Speed => speed;
        internal float Duration => duration;
        internal float InvulnerabilityStartTime => invulnerabilityStartTime;
        internal float InvulnerabilityDuration => invulnerabilityDuration;
        internal float MinimumStartInterval => minimumStartInterval;
        internal float InputBufferTime => inputBufferTime;
        internal float GravityMultiplier => gravityMultiplier;
        internal float ExitVelocityRetention => exitVelocityRetention;
        internal bool AllowRollingOffPlatforms => allowRollingOffPlatforms;

        private void OnValidate()
        {
            duration = Mathf.Max(0.01f, duration);
            invulnerabilityStartTime = Mathf.Clamp(invulnerabilityStartTime, 0f, duration);
            invulnerabilityDuration = Mathf.Clamp(invulnerabilityDuration, 0f, duration - invulnerabilityStartTime);
            minimumStartInterval = Mathf.Max(duration, minimumStartInterval);
        }
    }
}
