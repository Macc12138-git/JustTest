using UnityEngine;

namespace JustTest.Game.Combat
{
    [CreateAssetMenu(fileName = "AttackDefinition", menuName = "JustTest/Combat/Attack Definition")]
    public sealed class AttackDefinition : ScriptableObject
    {
        [SerializeField, Min(0.01f)] private float damage = 20f;
        [SerializeField, Min(0.01f)] private float windupDuration = 0.08f;
        [SerializeField, Min(0.01f)] private float activeDuration = 0.12f;
        [SerializeField, Min(0.01f)] private float recoveryDuration = 0.2f;
        [SerializeField, Min(0f)] private float inputBufferDuration = 0.1f;
        [SerializeField, Min(0f)] private float hitStunDuration = 0.2f;
        [SerializeField] private Vector2 knockbackVelocity = new Vector2(4f, 1.5f);
        [SerializeField] private bool allowFriendlyFire;

        internal float Damage => damage;

        internal float WindupDuration => windupDuration;

        internal float ActiveDuration => activeDuration;

        internal float RecoveryDuration => recoveryDuration;

        internal float InputBufferDuration => inputBufferDuration;

        internal HitReactionData HitReaction => new HitReactionData(
            hitStunDuration,
            knockbackVelocity);

        internal bool AllowFriendlyFire => allowFriendlyFire;

        private void OnValidate()
        {
            damage = Mathf.Max(0.01f, damage);
            windupDuration = Mathf.Max(0.01f, windupDuration);
            activeDuration = Mathf.Max(0.01f, activeDuration);
            recoveryDuration = Mathf.Max(0.01f, recoveryDuration);
            inputBufferDuration = Mathf.Max(0f, inputBufferDuration);
            hitStunDuration = SanitizeNonNegative(hitStunDuration);
            knockbackVelocity = new Vector2(
                SanitizeFinite(knockbackVelocity.x),
                SanitizeFinite(knockbackVelocity.y));
        }

        private static float SanitizeNonNegative(float value)
        {
            return Mathf.Max(0f, SanitizeFinite(value));
        }

        private static float SanitizeFinite(float value)
        {
            return float.IsNaN(value) || float.IsInfinity(value) ? 0f : value;
        }
    }
}
