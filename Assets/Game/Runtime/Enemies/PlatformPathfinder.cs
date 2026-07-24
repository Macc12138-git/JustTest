using System.Collections.Generic;
using UnityEngine;

namespace JustTest.Game.Enemies
{
    internal sealed class PlatformPathfinder
    {
        private readonly List<int> openNodes = new();
        private readonly HashSet<int> closedNodes = new();
        private readonly Dictionary<int, float> costs = new();
        private readonly Dictionary<int, float> estimates = new();
        private readonly Dictionary<int, PlatformNavigationEdge> arrivalEdges = new();
        private readonly List<PlatformNavigationEdge> outgoingEdges = new();

        internal bool TryFindPath(
            PlatformNavigationGraph graph,
            int startNodeId,
            int destinationNodeId,
            List<PlatformNavigationEdge> results)
        {
            results.Clear();
            ResetSearch();

            if (graph == null ||
                !graph.TryGetNode(startNodeId, out PlatformNavigationNode startNode) ||
                !graph.TryGetNode(destinationNodeId, out PlatformNavigationNode destinationNode))
            {
                return false;
            }

            if (startNodeId == destinationNodeId)
            {
                return true;
            }

            costs[startNodeId] = 0f;
            estimates[startNodeId] = Estimate(startNode, destinationNode);
            openNodes.Add(startNodeId);

            while (openNodes.Count > 0)
            {
                int currentNodeId = TakeLowestEstimateNode();
                if (currentNodeId == destinationNodeId)
                {
                    BuildPath(startNodeId, destinationNodeId, results);
                    return results.Count > 0;
                }

                closedNodes.Add(currentNodeId);
                graph.GetOutgoingEdges(currentNodeId, outgoingEdges);
                for (int index = 0; index < outgoingEdges.Count; index++)
                {
                    PlatformNavigationEdge edge = outgoingEdges[index];
                    if (closedNodes.Contains(edge.ToNodeId) ||
                        !graph.TryGetNode(edge.ToNodeId, out PlatformNavigationNode nextNode))
                    {
                        continue;
                    }

                    float candidateCost = costs[currentNodeId] + edge.Cost;
                    if (costs.TryGetValue(edge.ToNodeId, out float existingCost) &&
                        candidateCost >= existingCost)
                    {
                        continue;
                    }

                    costs[edge.ToNodeId] = candidateCost;
                    estimates[edge.ToNodeId] = candidateCost + Estimate(nextNode, destinationNode);
                    arrivalEdges[edge.ToNodeId] = edge;
                    if (!openNodes.Contains(edge.ToNodeId))
                    {
                        openNodes.Add(edge.ToNodeId);
                    }
                }
            }

            return false;
        }

        private void ResetSearch()
        {
            openNodes.Clear();
            closedNodes.Clear();
            costs.Clear();
            estimates.Clear();
            arrivalEdges.Clear();
            outgoingEdges.Clear();
        }

        private int TakeLowestEstimateNode()
        {
            int bestListIndex = 0;
            float bestEstimate = estimates[openNodes[0]];
            for (int index = 1; index < openNodes.Count; index++)
            {
                float estimate = estimates[openNodes[index]];
                if (estimate < bestEstimate)
                {
                    bestEstimate = estimate;
                    bestListIndex = index;
                }
            }

            int nodeId = openNodes[bestListIndex];
            openNodes.RemoveAt(bestListIndex);
            return nodeId;
        }

        private void BuildPath(int startNodeId, int destinationNodeId, List<PlatformNavigationEdge> results)
        {
            int currentNodeId = destinationNodeId;
            while (currentNodeId != startNodeId && arrivalEdges.TryGetValue(currentNodeId, out PlatformNavigationEdge edge))
            {
                results.Add(edge);
                currentNodeId = edge.FromNodeId;
            }

            results.Reverse();
        }

        private static float Estimate(PlatformNavigationNode from, PlatformNavigationNode to)
        {
            Vector2 fromCenter = new Vector2((from.MinimumX + from.MaximumX) * 0.5f, from.SurfaceY);
            Vector2 toCenter = new Vector2((to.MinimumX + to.MaximumX) * 0.5f, to.SurfaceY);
            return Vector2.Distance(fromCenter, toCenter);
        }
    }
}
