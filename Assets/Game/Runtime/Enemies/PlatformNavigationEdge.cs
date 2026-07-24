using System;
using UnityEngine;

namespace JustTest.Game.Enemies
{
    [Serializable]
    public sealed class PlatformNavigationEdge
    {
        [SerializeField] private int fromNodeId;
        [SerializeField] private int toNodeId;
        [SerializeField] private PlatformNavigationAction action;
        [SerializeField] private Vector2 takeoffPoint;
        [SerializeField] private Vector2 landingPoint;
        [SerializeField, Min(0.01f)] private float cost = 1f;

        internal PlatformNavigationEdge(
            int fromNodeId,
            int toNodeId,
            PlatformNavigationAction action,
            Vector2 takeoffPoint,
            Vector2 landingPoint,
            float cost)
        {
            this.fromNodeId = fromNodeId;
            this.toNodeId = toNodeId;
            this.action = action;
            this.takeoffPoint = takeoffPoint;
            this.landingPoint = landingPoint;
            this.cost = Mathf.Max(0.01f, cost);
        }

        internal int FromNodeId => fromNodeId;
        internal int ToNodeId => toNodeId;
        internal PlatformNavigationAction Action => action;
        internal Vector2 TakeoffPoint => takeoffPoint;
        internal Vector2 LandingPoint => landingPoint;
        internal float Cost => cost;
    }
}
