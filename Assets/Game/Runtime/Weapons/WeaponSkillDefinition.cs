using JustTest.Game.Combat;
using UnityEngine;

namespace JustTest.Game.Weapons
{
    [CreateAssetMenu(fileName = "WeaponSkillDefinition", menuName = "JustTest/Weapons/Skill Definition")]
    public sealed class WeaponSkillDefinition : ScriptableObject
    {
        [SerializeField] private string displayName = "Skill";
        [SerializeField] private AttackDefinition attack;
        [SerializeField, Min(0.01f)] private float energyCost = 30f;
        [SerializeField, Min(0f)] private float inputBufferDuration = 0.1f;
        [SerializeField] private Vector2 hitboxOffset = new Vector2(1f, 0f);
        [SerializeField] private Vector2 hitboxSize = new Vector2(1.5f, 1f);
        [SerializeField] private Vector2 movementVelocity;
        [SerializeField] private Vector2 completionVelocity;

        internal string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        internal AttackDefinition Attack => attack;
        internal float EnergyCost => energyCost;
        internal float InputBufferDuration => inputBufferDuration;
        internal Vector2 HitboxOffset => hitboxOffset;
        internal Vector2 HitboxSize => hitboxSize;
        internal Vector2 MovementVelocity => movementVelocity;
        internal Vector2 CompletionVelocity => completionVelocity;

        internal bool IsValid =>
            attack != null &&
            IsFinitePositive(energyCost) &&
            IsFiniteNonNegative(inputBufferDuration) &&
            IsFinite(hitboxOffset) &&
            IsFinitePositive(hitboxSize.x) &&
            IsFinitePositive(hitboxSize.y) &&
            IsFinite(movementVelocity) &&
            IsFinite(completionVelocity);

        private void OnValidate()
        {
            energyCost = Mathf.Max(0.01f, SanitizeFinite(energyCost));
            inputBufferDuration = Mathf.Max(0f, SanitizeFinite(inputBufferDuration));
            hitboxOffset = SanitizeFinite(hitboxOffset);
            hitboxSize = new Vector2(
                Mathf.Max(0.01f, SanitizeFinite(hitboxSize.x)),
                Mathf.Max(0.01f, SanitizeFinite(hitboxSize.y)));
            movementVelocity = SanitizeFinite(movementVelocity);
            completionVelocity = SanitizeFinite(completionVelocity);
        }

        private static bool IsFinitePositive(float value)
        {
            return value > 0f && IsFinite(value);
        }

        private static bool IsFiniteNonNegative(float value)
        {
            return value >= 0f && IsFinite(value);
        }

        private static bool IsFinite(Vector2 value)
        {
            return IsFinite(value.x) && IsFinite(value.y);
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private static Vector2 SanitizeFinite(Vector2 value)
        {
            return new Vector2(SanitizeFinite(value.x), SanitizeFinite(value.y));
        }

        private static float SanitizeFinite(float value)
        {
            return IsFinite(value) ? value : 0f;
        }
    }
}
