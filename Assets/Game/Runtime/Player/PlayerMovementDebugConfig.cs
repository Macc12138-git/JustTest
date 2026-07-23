using UnityEngine;

namespace JustTest.Game.Player
{
    [CreateAssetMenu(fileName = "PlayerMovementDebugConfig", menuName = "JustTest/Player/Movement Debug Config")]
    public sealed class PlayerMovementDebugConfig : ScriptableObject
    {
        [Header("Scene Debug")]
        [SerializeField] private bool drawGroundProbe = true;
        [SerializeField] private bool drawVelocity = true;
        [SerializeField, Min(0f)] private float velocityRayScale = 0.2f;
        [SerializeField] private Color groundedColor = new Color(0.25f, 0.9f, 0.45f, 1f);
        [SerializeField] private Color airborneColor = new Color(1f, 0.65f, 0.2f, 1f);
        [SerializeField] private Color velocityColor = new Color(0.25f, 0.75f, 1f, 1f);

        [Header("Overlay")]
        [SerializeField] private bool showOverlay = true;
        [SerializeField] private Vector2 overlayPosition = new Vector2(16f, 16f);
        [SerializeField] private Vector2 overlaySize = new Vector2(300f, 72f);
        [SerializeField] private bool logStateChanges;

        [Header("Sandbox Reset")]
        [SerializeField] private bool allowManualReset = true;
        [SerializeField] private float respawnBelowY = -8f;
        [SerializeField, Min(0f)] private float respawnDelay;

        internal bool DrawGroundProbe => drawGroundProbe;
        internal bool DrawVelocity => drawVelocity;
        internal float VelocityRayScale => velocityRayScale;
        internal Color GroundedColor => groundedColor;
        internal Color AirborneColor => airborneColor;
        internal Color VelocityColor => velocityColor;
        internal bool ShowOverlay => showOverlay;
        internal Vector2 OverlayPosition => overlayPosition;
        internal Vector2 OverlaySize => overlaySize;
        internal bool LogStateChanges => logStateChanges;
        internal bool AllowManualReset => allowManualReset;
        internal float RespawnBelowY => respawnBelowY;
        internal float RespawnDelay => respawnDelay;
    }
}
