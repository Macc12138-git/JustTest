using System.Collections.Generic;
using UnityEngine;

namespace JustTest.Game.Enemies
{
    [CreateAssetMenu(fileName = "PlatformNavigationGraph", menuName = "JustTest/Enemies/Platform Navigation Graph")]
    public sealed class PlatformNavigationGraph : ScriptableObject
    {
        [SerializeField] private List<PlatformNavigationNode> nodes = new();
        [SerializeField] private List<PlatformNavigationEdge> edges = new();

        internal IReadOnlyList<PlatformNavigationNode> Nodes => nodes;
        internal IReadOnlyList<PlatformNavigationEdge> Edges => edges;

        internal bool TryGetNode(int nodeId, out PlatformNavigationNode node)
        {
            for (int index = 0; index < nodes.Count; index++)
            {
                PlatformNavigationNode candidate = nodes[index];
                if (candidate != null && candidate.Id == nodeId)
                {
                    node = candidate;
                    return true;
                }
            }

            node = null;
            return false;
        }

        internal bool TryFindClosestNode(Vector2 footPosition, float maximumDistance, out int nodeId)
        {
            nodeId = -1;
            float bestDistanceSquared = maximumDistance * maximumDistance;

            for (int index = 0; index < nodes.Count; index++)
            {
                PlatformNavigationNode node = nodes[index];
                if (node == null)
                {
                    continue;
                }

                float distanceSquared = (node.GetClosestPoint(footPosition) - footPosition).sqrMagnitude;
                if (distanceSquared > bestDistanceSquared)
                {
                    continue;
                }

                bestDistanceSquared = distanceSquared;
                nodeId = node.Id;
            }

            return nodeId >= 0;
        }

        internal void GetOutgoingEdges(int nodeId, List<PlatformNavigationEdge> results)
        {
            results.Clear();
            for (int index = 0; index < edges.Count; index++)
            {
                PlatformNavigationEdge edge = edges[index];
                if (edge != null && edge.FromNodeId == nodeId)
                {
                    results.Add(edge);
                }
            }
        }

        internal void ReplaceData(
            List<PlatformNavigationNode> replacementNodes,
            List<PlatformNavigationEdge> replacementEdges)
        {
            nodes = replacementNodes ?? new List<PlatformNavigationNode>();
            edges = replacementEdges ?? new List<PlatformNavigationEdge>();
        }
    }
}
