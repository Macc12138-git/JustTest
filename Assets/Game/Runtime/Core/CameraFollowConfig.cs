using UnityEngine;

namespace JustTest.Game.Core
{
    [CreateAssetMenu(fileName = "CameraFollowConfig", menuName = "JustTest/Camera/Follow Config")]
    public sealed class CameraFollowConfig : ScriptableObject
    {
        [SerializeField] private Vector2 baseOffset = new Vector2(0f, 1f);
        [SerializeField, Min(0f)] private float facingLookAheadDistance = 2f;
        [SerializeField, Min(0.001f)] private float lookAheadSmoothTime = 0.12f;
        [SerializeField, Min(0.001f)] private float horizontalSmoothTime = 0.12f;
        [SerializeField, Min(0.001f)] private float verticalSmoothTime = 0.18f;
        [SerializeField, Min(0f)] private float horizontalDeadZone = 0.1f;
        [SerializeField, Min(0f)] private float verticalDeadZone = 0.15f;
        [SerializeField, Min(0f)] private float maximumFollowSpeed = 50f;
        [SerializeField] private bool followVerticalMovement = true;
        [SerializeField] private bool snapOnEnable = true;

        internal Vector2 BaseOffset => baseOffset;
        internal float FacingLookAheadDistance => facingLookAheadDistance;
        internal float LookAheadSmoothTime => lookAheadSmoothTime;
        internal float HorizontalSmoothTime => horizontalSmoothTime;
        internal float VerticalSmoothTime => verticalSmoothTime;
        internal float HorizontalDeadZone => horizontalDeadZone;
        internal float VerticalDeadZone => verticalDeadZone;
        internal float MaximumFollowSpeed => maximumFollowSpeed;
        internal bool FollowVerticalMovement => followVerticalMovement;
        internal bool SnapOnEnable => snapOnEnable;
    }
}
