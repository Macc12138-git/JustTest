using JustTest.Game.Weapons;
using UnityEngine;

namespace JustTest.Game.Presentation
{
    [CreateAssetMenu(
        fileName = "WeaponPresentationDefinition",
        menuName = "JustTest/Presentation/Weapon Presentation")]
    public sealed class WeaponPresentationDefinition : ScriptableObject
    {
        [SerializeField] private WeaponDefinition weapon;
        [SerializeField] private Sprite sprite;
        [SerializeField] private Color color = Color.white;
        [SerializeField] private Vector2 localPosition;
        [SerializeField] private float localRotation;
        [SerializeField] private Vector2 localScale = Vector2.one;

        internal WeaponDefinition Weapon => weapon;
        internal Sprite Sprite => sprite;
        internal Color Color => color;
        internal Vector2 LocalPosition => localPosition;
        internal float LocalRotation => localRotation;
        internal Vector2 LocalScale => localScale;

        internal bool IsValid =>
            weapon != null &&
            sprite != null &&
            localScale.x > 0f &&
            localScale.y > 0f &&
            IsFinite(localPosition.x) &&
            IsFinite(localPosition.y) &&
            IsFinite(localRotation) &&
            IsFinite(localScale.x) &&
            IsFinite(localScale.y);

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
