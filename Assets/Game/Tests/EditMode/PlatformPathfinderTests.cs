using System.Collections.Generic;
using JustTest.Game.Enemies;
using NUnit.Framework;
using UnityEngine;

namespace JustTest.Game.Tests.EditMode
{
    public sealed class PlatformPathfinderTests
    {
        [Test]
        public void TryFindPath_SelectsLowestCostRoute()
        {
            PlatformNavigationGraph graph = ScriptableObject.CreateInstance<PlatformNavigationGraph>();
            graph.ReplaceData(
                new List<PlatformNavigationNode>
                {
                    new PlatformNavigationNode(0, "Start", 0f, 1f, 0f, false),
                    new PlatformNavigationNode(1, "Middle", 2f, 3f, 1f, false),
                    new PlatformNavigationNode(2, "Goal", 4f, 5f, 0f, false)
                },
                new List<PlatformNavigationEdge>
                {
                    new PlatformNavigationEdge(0, 2, PlatformNavigationAction.Jump, Vector2.zero, Vector2.right * 4f, 10f),
                    new PlatformNavigationEdge(0, 1, PlatformNavigationAction.Jump, Vector2.zero, new Vector2(2f, 1f), 2f),
                    new PlatformNavigationEdge(1, 2, PlatformNavigationAction.Drop, new Vector2(3f, 1f), Vector2.right * 4f, 2f)
                });

            List<PlatformNavigationEdge> path = new List<PlatformNavigationEdge>();
            bool found = new PlatformPathfinder().TryFindPath(graph, 0, 2, path);

            Assert.That(found, Is.True);
            Assert.That(path, Has.Count.EqualTo(2));
            Assert.That(path[0].ToNodeId, Is.EqualTo(1));
            Assert.That(path[1].ToNodeId, Is.EqualTo(2));
            Object.DestroyImmediate(graph);
        }

        [Test]
        public void TryFindPath_ReturnsFalseWhenDestinationIsDisconnected()
        {
            PlatformNavigationGraph graph = ScriptableObject.CreateInstance<PlatformNavigationGraph>();
            graph.ReplaceData(
                new List<PlatformNavigationNode>
                {
                    new PlatformNavigationNode(0, "Start", 0f, 1f, 0f, false),
                    new PlatformNavigationNode(1, "Goal", 3f, 4f, 0f, false)
                },
                new List<PlatformNavigationEdge>());

            List<PlatformNavigationEdge> path = new List<PlatformNavigationEdge>();
            bool found = new PlatformPathfinder().TryFindPath(graph, 0, 1, path);

            Assert.That(found, Is.False);
            Assert.That(path, Is.Empty);
            Object.DestroyImmediate(graph);
        }
    }
}
