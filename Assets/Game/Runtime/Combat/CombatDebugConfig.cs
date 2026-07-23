using UnityEngine;

namespace JustTest.Game.Combat
{
    [CreateAssetMenu(fileName = "CombatDebugConfig", menuName = "JustTest/Combat/Debug Config")]
    public sealed class CombatDebugConfig : ScriptableObject
    {
        [SerializeField] private bool showOverlay = true;
        [SerializeField] private bool logHitResults;
        [SerializeField] private bool allowManualReset = true;
        [SerializeField, Range(10, 32)] private int overlayFontSize = 18;
        [SerializeField] private Vector2 overlayPosition = new Vector2(16f, 128f);
        [SerializeField] private Vector2 overlaySize = new Vector2(340f, 140f);

        internal bool ShowOverlay => showOverlay;

        internal bool LogHitResults => logHitResults;

        internal bool AllowManualReset => allowManualReset;

        internal int OverlayFontSize => overlayFontSize;

        internal Vector2 OverlayPosition => overlayPosition;

        internal Vector2 OverlaySize => overlaySize;

        private void OnValidate()
        {
            overlayFontSize = Mathf.Clamp(overlayFontSize, 10, 32);
            overlaySize.x = Mathf.Max(1f, overlaySize.x);
            overlaySize.y = Mathf.Max(1f, overlaySize.y);
        }
    }
}
