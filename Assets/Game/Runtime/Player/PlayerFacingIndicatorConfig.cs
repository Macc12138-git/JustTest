using UnityEngine;

namespace JustTest.Game.Player
{
    [CreateAssetMenu(
        fileName = "PlayerFacingIndicatorConfig",
        menuName = "JustTest/Player/Facing Indicator Config")]
    public sealed class PlayerFacingIndicatorConfig : ScriptableObject
    {
        [SerializeField] private bool visible = true;
        [SerializeField] private Vector2 localOffset = new Vector2(0f, 1.25f);
        [SerializeField, Min(0.1f)] private float scale = 1f;
        [SerializeField] private Color color = new Color(1f, 0.82f, 0.18f, 1f);
        [SerializeField] private int sortingOrder = 30;

        internal bool Visible => visible;

        internal Vector2 LocalOffset => localOffset;

        internal float Scale => scale;

        internal Color Color => color;

        internal int SortingOrder => sortingOrder;

        private void OnValidate()
        {
            scale = Mathf.Max(0.1f, scale);
        }
    }
}
