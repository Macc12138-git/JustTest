using UnityEngine;

namespace JustTest.Game.Enemies
{
    public sealed class PlatformNavigationGraphAuthoring : MonoBehaviour
    {
        [SerializeField] private PlatformNavigationGraph graph;
        [SerializeField] private Collider2D[] surfaces;
        [SerializeField] private PlatformNavigationLinkAuthoring[] links;

        internal PlatformNavigationGraph Graph => graph;
        internal Collider2D[] Surfaces => surfaces;
        internal PlatformNavigationLinkAuthoring[] Links => links;
    }
}
