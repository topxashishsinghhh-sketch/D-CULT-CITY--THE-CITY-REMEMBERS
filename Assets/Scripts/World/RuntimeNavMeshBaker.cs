using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace DCultCity.World
{
    /// <summary>
    /// Bakes a NavMesh at runtime over the procedurally generated test city
    /// so that NPC NavMeshAgents can navigate its roads and sidewalks.
    ///
    /// REQUIRES: Unity AI Navigation package (com.unity.ai.navigation).
    /// If the package is not installed, this script falls back to printing
    /// clear instructions for manual baking in the Editor.
    ///
    /// HOW IT WORKS:
    ///   1. Finds the NavMeshSurface component on this GameObject.
    ///   2. Calls BuildNavMesh() after the city geometry has been generated.
    ///   3. NPCFactory is delayed one frame so NPCs spawn after the bake completes.
    ///
    /// EDITOR ALTERNATIVE (no package needed):
    ///   • Select the city root GameObject in the Hierarchy.
    ///   • Set all city primitives to Navigation Static in the Inspector.
    ///   • Window → AI → Navigation → Bake tab → click Bake.
    /// </summary>
    public class RuntimeNavMeshBaker : MonoBehaviour
    {
        [Header("Bake Settings")]
        [Tooltip("Delay in seconds after scene load before baking (let city generate first)")]
        [SerializeField] private float bakeDelay = 0.15f;

        [Tooltip("If true, logs the baked NavMesh triangle count as a health check")]
        [SerializeField] private bool logBakeResult = true;

        private bool baked = false;

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(bakeDelay);
            TryBakeNavMesh();
        }

        [ContextMenu("Bake NavMesh Now")]
        public void TryBakeNavMesh()
        {
            if (baked) return;

#if UNITY_AI_NAVIGATION
            // NavMeshSurface path (requires com.unity.ai.navigation package)
            var surface = GetComponent<UnityEngine.AI.NavMeshSurface>();
            if (surface == null)
                surface = gameObject.AddComponent<UnityEngine.AI.NavMeshSurface>();

            surface.collectObjects = UnityEngine.AI.CollectObjects.All;
            surface.useGeometry    = UnityEngine.AI.NavMeshCollectGeometry.PhysicsColliders;
            surface.BuildNavMesh();
            baked = true;

            if (logBakeResult)
                Debug.Log($"<color=#00FFCC>[RuntimeNavMeshBaker]</color> NavMesh baked via NavMeshSurface. Triangles: {NavMesh.CalculateTriangulation().indices.Length / 3}");
#else
            // Fallback — try the legacy static bake API
            NavMeshBuildSettings settings = NavMesh.GetSettingsByIndex(0);

            // Mark all static Renderers as nav-mesh sources
            var sources = new System.Collections.Generic.List<NavMeshBuildSource>();
            var renderers = FindObjectsOfType<MeshRenderer>();
            foreach (var r in renderers)
            {
                if (!r.gameObject.isStatic) continue;
                var filter = r.GetComponent<MeshFilter>();
                if (filter == null || filter.sharedMesh == null) continue;

                var src = new NavMeshBuildSource
                {
                    shape     = NavMeshBuildSourceShape.Mesh,
                    sourceData = filter.sharedMesh,
                    transform  = r.transform.localToWorldMatrix,
                    area       = 0
                };
                sources.Add(src);
            }

            if (sources.Count == 0)
            {
                Debug.LogWarning("<color=#FFAA00>[RuntimeNavMeshBaker]</color> No Static renderers found. " +
                                 "Either install 'com.unity.ai.navigation' package OR manually bake: " +
                                 "Window > AI > Navigation > Bake. NPCs will wander in place until NavMesh is ready.");
                return;
            }

            Bounds worldBounds = new Bounds(Vector3.zero, new Vector3(200f, 50f, 200f));
            var data = NavMeshBuilder.BuildNavMeshData(settings, sources, worldBounds, Vector3.zero, Quaternion.identity);

            if (data != null)
            {
                NavMesh.AddNavMeshData(data);
                baked = true;
                if (logBakeResult)
                    Debug.Log($"<color=#00FFCC>[RuntimeNavMeshBaker]</color> NavMesh baked via legacy API. " +
                              $"Triangles: {NavMesh.CalculateTriangulation().indices.Length / 3}");
            }
            else
            {
                Debug.LogWarning("<color=#FF6600>[RuntimeNavMeshBaker]</color> NavMesh bake returned null. " +
                                 "Please bake manually: Window > AI > Navigation > Bake.");
            }
#endif
        }
    }
}
