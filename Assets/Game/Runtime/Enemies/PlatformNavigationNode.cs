using System;
using UnityEngine;

namespace JustTest.Game.Enemies
{
    [Serializable]
    public sealed class PlatformNavigationNode
    {
        [SerializeField] private int id;
        [SerializeField] private string displayName;
        [SerializeField] private float minimumX;
        [SerializeField] private float maximumX;
        [SerializeField] private float surfaceY;
        [SerializeField] private bool oneWay;

        internal PlatformNavigationNode(
            int id,
            string displayName,
            float minimumX,
            float maximumX,
            float surfaceY,
            bool oneWay)
        {
            this.id = id;
            this.displayName = displayName;
            this.minimumX = Mathf.Min(minimumX, maximumX);
            this.maximumX = Mathf.Max(minimumX, maximumX);
            this.surfaceY = surfaceY;
            this.oneWay = oneWay;
        }

        internal int Id => id;
        internal string DisplayName => displayName;
        internal float MinimumX => minimumX;
        internal float MaximumX => maximumX;
        internal float SurfaceY => surfaceY;
        internal bool OneWay => oneWay;

        internal Vector2 GetClosestPoint(Vector2 point)
        {
            return new Vector2(Mathf.Clamp(point.x, minimumX, maximumX), surfaceY);
        }
    }
}
