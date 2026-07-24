using System;
using JustTest.Game.Combat;
using UnityEngine;

namespace JustTest.Game.Weapons
{
    [Serializable]
    internal struct WeaponQteStrikeDefinition
    {
        [SerializeField] private AttackDefinition attack;
        [SerializeField] private Vector2 hitboxOffset;
        [SerializeField] private Vector2 hitboxSize;
        [SerializeField] private Vector2 movementVelocity;

        internal AttackDefinition Attack => attack;

        internal Vector2 HitboxOffset => hitboxOffset;

        internal Vector2 HitboxSize => hitboxSize;

        internal Vector2 MovementVelocity => movementVelocity;

        internal bool IsValid =>
            attack != null &&
            hitboxSize.x > 0f &&
            hitboxSize.y > 0f &&
            IsFinite(hitboxOffset.x) &&
            IsFinite(hitboxOffset.y) &&
            IsFinite(hitboxSize.x) &&
            IsFinite(hitboxSize.y) &&
            IsFinite(movementVelocity.x) &&
            IsFinite(movementVelocity.y);

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
