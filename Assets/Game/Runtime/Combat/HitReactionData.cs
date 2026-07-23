using UnityEngine;

namespace JustTest.Game.Combat
{
    public readonly struct HitReactionData
    {
        public HitReactionData(float hitStunDuration, Vector2 knockbackVelocity)
        {
            HitStunDuration = hitStunDuration;
            KnockbackVelocity = knockbackVelocity;
        }

        public float HitStunDuration { get; }

        public Vector2 KnockbackVelocity { get; }

        public bool CausesReaction => HitStunDuration > 0f || KnockbackVelocity.sqrMagnitude > 0f;

        internal bool IsValid =>
            HitStunDuration >= 0f &&
            IsFinite(HitStunDuration) &&
            IsFinite(KnockbackVelocity.x) &&
            IsFinite(KnockbackVelocity.y);

        internal HitReactionData ToWorld(int attackDirection)
        {
            return new HitReactionData(
                HitStunDuration,
                new Vector2(
                    Mathf.Abs(KnockbackVelocity.x) * attackDirection,
                    KnockbackVelocity.y));
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
