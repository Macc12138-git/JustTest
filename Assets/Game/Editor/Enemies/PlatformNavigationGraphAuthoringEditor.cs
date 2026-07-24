using System.Collections.Generic;
using JustTest.Game.Enemies;
using UnityEditor;
using UnityEngine;

namespace JustTest.Game.Editor.Enemies
{
    [CustomEditor(typeof(PlatformNavigationGraphAuthoring))]
    public sealed class PlatformNavigationGraphAuthoringEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();

            using (new EditorGUI.DisabledScope(!CanBake((PlatformNavigationGraphAuthoring)target)))
            {
                if (GUILayout.Button("Bake Navigation Graph"))
                {
                    Bake((PlatformNavigationGraphAuthoring)target);
                }
            }
        }

        private static bool CanBake(PlatformNavigationGraphAuthoring authoring)
        {
            return authoring != null &&
                   authoring.Graph != null &&
                   authoring.Surfaces != null &&
                   authoring.Surfaces.Length > 0;
        }

        private static void Bake(PlatformNavigationGraphAuthoring authoring)
        {
            Collider2D[] surfaces = authoring.Surfaces;
            List<PlatformNavigationNode> nodes = new List<PlatformNavigationNode>(surfaces.Length);
            for (int index = 0; index < surfaces.Length; index++)
            {
                Collider2D surface = surfaces[index];
                if (surface == null)
                {
                    Debug.LogError($"Navigation surface index {index} is empty.", authoring);
                    return;
                }

                Bounds bounds = surface.bounds;
                nodes.Add(new PlatformNavigationNode(
                    index,
                    surface.name,
                    bounds.min.x,
                    bounds.max.x,
                    bounds.max.y,
                    surface.usedByEffector));
            }

            PlatformNavigationLinkAuthoring[] authoredLinks = authoring.Links;
            List<PlatformNavigationEdge> edges = new List<PlatformNavigationEdge>(authoredLinks?.Length ?? 0);
            if (authoredLinks != null)
            {
                for (int index = 0; index < authoredLinks.Length; index++)
                {
                    PlatformNavigationLinkAuthoring link = authoredLinks[index];
                    if (link == null ||
                        link.FromSurfaceIndex < 0 || link.FromSurfaceIndex >= nodes.Count ||
                        link.ToSurfaceIndex < 0 || link.ToSurfaceIndex >= nodes.Count)
                    {
                        Debug.LogError($"Navigation link index {index} is invalid.", authoring);
                        return;
                    }

                    Vector2 takeoffPoint = new Vector2(
                        link.TakeoffX,
                        nodes[link.FromSurfaceIndex].SurfaceY);
                    Vector2 landingPoint = new Vector2(
                        link.LandingX,
                        nodes[link.ToSurfaceIndex].SurfaceY);
                    float cost = Mathf.Max(
                        0.01f,
                        Vector2.Distance(takeoffPoint, landingPoint) * link.CostMultiplier);
                    edges.Add(new PlatformNavigationEdge(
                        link.FromSurfaceIndex,
                        link.ToSurfaceIndex,
                        link.Action,
                        takeoffPoint,
                        landingPoint,
                        cost));
                }
            }

            Undo.RecordObject(authoring.Graph, "Bake Platform Navigation Graph");
            authoring.Graph.ReplaceData(nodes, edges);
            EditorUtility.SetDirty(authoring.Graph);
            AssetDatabase.SaveAssetIfDirty(authoring.Graph);
            Debug.Log($"Baked {nodes.Count} navigation nodes and {edges.Count} links.", authoring);
        }
    }
}
