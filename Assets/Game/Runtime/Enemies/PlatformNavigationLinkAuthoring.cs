using System;
using UnityEngine;

namespace JustTest.Game.Enemies
{
    [Serializable]
    public sealed class PlatformNavigationLinkAuthoring
    {
        [SerializeField, Min(0)] private int fromSurfaceIndex;
        [SerializeField, Min(0)] private int toSurfaceIndex;
        [SerializeField] private PlatformNavigationAction action;
        [SerializeField] private float takeoffX;
        [SerializeField] private float landingX;
        [SerializeField, Min(0.01f)] private float costMultiplier = 1f;

        internal int FromSurfaceIndex => fromSurfaceIndex;
        internal int ToSurfaceIndex => toSurfaceIndex;
        internal PlatformNavigationAction Action => action;
        internal float TakeoffX => takeoffX;
        internal float LandingX => landingX;
        internal float CostMultiplier => costMultiplier;
    }
}
