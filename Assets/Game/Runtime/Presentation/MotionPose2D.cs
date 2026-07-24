using System;
using UnityEngine;

namespace JustTest.Game.Presentation
{
    [Serializable]
    public sealed class MotionPose2D
    {
        [SerializeField] private Vector2 bodyOffset;
        [SerializeField] private float bodyRotation;
        [SerializeField] private Vector2 bodyScale = Vector2.one;
        [SerializeField] private Vector2 mainWeaponOffset;
        [SerializeField] private float mainWeaponRotation;
        [SerializeField] private Vector2 mainWeaponScale = Vector2.one;
        [SerializeField] private Vector2 offhandWeaponOffset;
        [SerializeField] private float offhandWeaponRotation;
        [SerializeField] private Vector2 offhandWeaponScale = Vector2.one;

        internal EvaluatedMotionPose2D Evaluate()
        {
            return new EvaluatedMotionPose2D(
                bodyOffset,
                bodyRotation,
                bodyScale,
                mainWeaponOffset,
                mainWeaponRotation,
                mainWeaponScale,
                offhandWeaponOffset,
                offhandWeaponRotation,
                offhandWeaponScale);
        }
    }

    internal readonly struct EvaluatedMotionPose2D
    {
        internal static readonly EvaluatedMotionPose2D Identity = new EvaluatedMotionPose2D(
            Vector2.zero,
            0f,
            Vector2.one,
            Vector2.zero,
            0f,
            Vector2.one,
            Vector2.zero,
            0f,
            Vector2.one);

        internal EvaluatedMotionPose2D(
            Vector2 bodyOffset,
            float bodyRotation,
            Vector2 bodyScale,
            Vector2 mainWeaponOffset,
            float mainWeaponRotation,
            Vector2 mainWeaponScale,
            Vector2 offhandWeaponOffset,
            float offhandWeaponRotation,
            Vector2 offhandWeaponScale)
        {
            BodyOffset = bodyOffset;
            BodyRotation = bodyRotation;
            BodyScale = bodyScale;
            MainWeaponOffset = mainWeaponOffset;
            MainWeaponRotation = mainWeaponRotation;
            MainWeaponScale = mainWeaponScale;
            OffhandWeaponOffset = offhandWeaponOffset;
            OffhandWeaponRotation = offhandWeaponRotation;
            OffhandWeaponScale = offhandWeaponScale;
        }

        internal Vector2 BodyOffset { get; }
        internal float BodyRotation { get; }
        internal Vector2 BodyScale { get; }
        internal Vector2 MainWeaponOffset { get; }
        internal float MainWeaponRotation { get; }
        internal Vector2 MainWeaponScale { get; }
        internal Vector2 OffhandWeaponOffset { get; }
        internal float OffhandWeaponRotation { get; }
        internal Vector2 OffhandWeaponScale { get; }

        internal static EvaluatedMotionPose2D Lerp(
            in EvaluatedMotionPose2D from,
            in EvaluatedMotionPose2D to,
            float progress)
        {
            float t = Mathf.Clamp01(progress);
            return new EvaluatedMotionPose2D(
                Vector2.LerpUnclamped(from.BodyOffset, to.BodyOffset, t),
                Mathf.LerpAngle(from.BodyRotation, to.BodyRotation, t),
                Vector2.LerpUnclamped(from.BodyScale, to.BodyScale, t),
                Vector2.LerpUnclamped(from.MainWeaponOffset, to.MainWeaponOffset, t),
                Mathf.LerpAngle(from.MainWeaponRotation, to.MainWeaponRotation, t),
                Vector2.LerpUnclamped(from.MainWeaponScale, to.MainWeaponScale, t),
                Vector2.LerpUnclamped(from.OffhandWeaponOffset, to.OffhandWeaponOffset, t),
                Mathf.LerpAngle(from.OffhandWeaponRotation, to.OffhandWeaponRotation, t),
                Vector2.LerpUnclamped(from.OffhandWeaponScale, to.OffhandWeaponScale, t));
        }
    }
}
