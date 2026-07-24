using System;
using UnityEngine;

namespace JustTest.Game.Weapons
{
    [CreateAssetMenu(fileName = "WeaponQteDefinition", menuName = "JustTest/Weapons/QTE Definition")]
    public sealed class WeaponQteDefinition : ScriptableObject
    {
        [SerializeField] private WeaponQteMotionMode motionMode;
        [SerializeField, Min(0.01f)] private float maximumTriggerDistance = 5f;
        [SerializeField, Min(0.01f)] private float approachSpeed = 20f;
        [SerializeField, Min(0.01f)] private float approachTimeout = 0.3f;
        [SerializeField, Min(0f)] private float horizontalTargetDistance = 0.8f;
        [SerializeField] private float verticalTargetOffset;
        [SerializeField, Min(0.001f)] private float stoppingDistance = 0.05f;
        [SerializeField] private LayerMask obstacleLayers;
        [SerializeField] private bool grantInvulnerability = true;
        [SerializeField] private WeaponQteStrikeDefinition[] strikes =
            Array.Empty<WeaponQteStrikeDefinition>();
        [SerializeField] private Vector2 completionVelocity;

        internal WeaponQteMotionMode MotionMode => motionMode;

        internal float MaximumTriggerDistance => maximumTriggerDistance;

        internal float ApproachSpeed => approachSpeed;

        internal float ApproachTimeout => approachTimeout;

        internal float HorizontalTargetDistance => horizontalTargetDistance;

        internal float VerticalTargetOffset => verticalTargetOffset;

        internal float StoppingDistance => stoppingDistance;

        internal LayerMask ObstacleLayers => obstacleLayers;

        internal bool GrantInvulnerability => grantInvulnerability;

        internal int StrikeCount => strikes?.Length ?? 0;

        internal Vector2 CompletionVelocity => completionVelocity;

        internal bool IsValid
        {
            get
            {
                if (maximumTriggerDistance <= 0f ||
                    approachSpeed <= 0f ||
                    approachTimeout <= 0f ||
                    horizontalTargetDistance < 0f ||
                    stoppingDistance <= 0f ||
                    !IsFinite(verticalTargetOffset) ||
                    !IsFinite(completionVelocity.x) ||
                    !IsFinite(completionVelocity.y) ||
                    strikes == null ||
                    strikes.Length == 0)
                {
                    return false;
                }

                for (int index = 0; index < strikes.Length; index++)
                {
                    if (!strikes[index].IsValid)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        internal WeaponQteStrikeDefinition GetStrike(int index)
        {
            return index >= 0 && index < StrikeCount ? strikes[index] : default;
        }

        private void OnValidate()
        {
            maximumTriggerDistance = Mathf.Max(0.01f, maximumTriggerDistance);
            approachSpeed = Mathf.Max(0.01f, approachSpeed);
            approachTimeout = Mathf.Max(0.01f, approachTimeout);
            horizontalTargetDistance = Mathf.Max(0f, horizontalTargetDistance);
            stoppingDistance = Mathf.Max(0.001f, stoppingDistance);
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
