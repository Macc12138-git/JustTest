using UnityEngine;

namespace JustTest.Game.Combat
{
    [CreateAssetMenu(fileName = "CombatDebugConfig", menuName = "JustTest/Combat/Debug Config")]
    public sealed class CombatDebugConfig : ScriptableObject
    {
        [SerializeField] private bool showOverlay = true;
        [SerializeField] private bool logHitResults;
        [SerializeField] private bool allowManualReset = true;
        [SerializeField] private KeyCode normalEnemyAttackKey = KeyCode.U;
        [SerializeField] private KeyCode heavyEnemyAttackKey = KeyCode.I;
        [SerializeField] private KeyCode applyUnbalancedKey = KeyCode.F1;
        [SerializeField] private KeyCode applyAirborneKey = KeyCode.F2;
        [SerializeField] private KeyCode applyStunnedKey = KeyCode.F3;
        [SerializeField] private bool bypassPostHitInvulnerabilityForStatusTests = true;
        [SerializeField, Range(10, 32)] private int overlayFontSize = 18;
        [SerializeField] private Vector2 overlayPosition = new Vector2(16f, 164f);
        [SerializeField] private Vector2 overlaySize = new Vector2(760f, 430f);
        [SerializeField] private Vector2 weaponSlotsPosition = new Vector2(16f, 92f);
        [SerializeField] private Vector2 weaponSlotSize = new Vector2(210f, 56f);
        [SerializeField, Min(0f)] private float weaponSlotGap = 8f;
        [SerializeField] private Color activeWeaponColor = new Color(0.25f, 0.85f, 0.45f, 1f);
        [SerializeField] private Color qteCandidateColor = new Color(1f, 0.75f, 0.15f, 1f);
        [SerializeField] private Color inactiveWeaponColor = new Color(0.45f, 0.45f, 0.45f, 1f);

        internal bool ShowOverlay => showOverlay;

        internal bool LogHitResults => logHitResults;

        internal bool AllowManualReset => allowManualReset;

        internal KeyCode NormalEnemyAttackKey => normalEnemyAttackKey;

        internal KeyCode HeavyEnemyAttackKey => heavyEnemyAttackKey;

        internal KeyCode ApplyUnbalancedKey => applyUnbalancedKey;

        internal KeyCode ApplyAirborneKey => applyAirborneKey;

        internal KeyCode ApplyStunnedKey => applyStunnedKey;

        internal bool BypassPostHitInvulnerabilityForStatusTests =>
            bypassPostHitInvulnerabilityForStatusTests;

        internal int OverlayFontSize => overlayFontSize;

        internal Vector2 OverlayPosition => overlayPosition;

        internal Vector2 OverlaySize => overlaySize;
        internal Vector2 WeaponSlotsPosition => weaponSlotsPosition;
        internal Vector2 WeaponSlotSize => weaponSlotSize;
        internal float WeaponSlotGap => weaponSlotGap;
        internal Color ActiveWeaponColor => activeWeaponColor;
        internal Color QteCandidateColor => qteCandidateColor;
        internal Color InactiveWeaponColor => inactiveWeaponColor;

        private void OnValidate()
        {
            overlayFontSize = Mathf.Clamp(overlayFontSize, 10, 32);
            overlaySize.x = Mathf.Max(1f, overlaySize.x);
            overlaySize.y = Mathf.Max(1f, overlaySize.y);
            weaponSlotSize.x = Mathf.Max(1f, weaponSlotSize.x);
            weaponSlotSize.y = Mathf.Max(1f, weaponSlotSize.y);
            weaponSlotGap = Mathf.Max(0f, weaponSlotGap);
        }
    }
}
