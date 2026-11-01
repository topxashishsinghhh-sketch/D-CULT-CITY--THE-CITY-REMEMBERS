using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace DCultCity.World
{
    /// <summary>
    /// Bakes a NavMesh at runtime over the procedurally generated test city
    /// so that NPC NavMeshAgents can navigate its roads and sidewalks.
    ///
    /// Uses the legacy NavMeshBuilder API (no extra packages required).
    /// Collects ALL MeshRenderers in the scene (not just static ones) so
    /// procedurally spawned city geometry is included in the bake.
    ///
    /// HOW IT WORKS:
    ///   1. Waits <bakeDelay> seconds so SceneBootstrap can finish generating the city.
    ///   2. Gathers every MeshRenderer with a valid MeshFilter as a NavMesh source.
    ///   3. Calls NavMeshBuilder.BuildNavMeshData and registers the result.
    ///   4. Sets IsBaked = true so other systems (e.g. PolicePursuitManager) can
    ///      wait before attaching NavMeshAgents.
    ///
    /// EDITOR ALTERNATIVE (zero code):
    ///   Window → AI → Navigation → Bake tab → click Bake.
    /// </summary>
    public class RuntimeNavMeshBaker : MonoBehaviour
    {
        [Header("Bake Settings")]
        [Tooltip("Delay in seconds after scene load before baking (let city generate first)")]
        [SerializeField] private float bakeDelay = 0.5f;

        [Tooltip("If true, logs the baked NavMesh triangle count as a health check")]
        [SerializeField] private bool logBakeResult = true;

        /// <summary>True once a valid NavMesh has been baked or was already present.</summary>
        public static bool IsBaked { get; private set; } = false;

        private void Awake()
        {
            // If a pre-baked NavMesh exists from the editor bake, mark as done immediately.
            if (NavMesh.CalculateTriangulation().indices.Length > 0)
            {
                IsBaked = true;
                if (logBakeResult)
                    Debug.Log($"<color=#00FFCC>[RuntimeNavMeshBaker]</color> Pre-baked NavMesh detected. Triangles: {NavMesh.CalculateTriangulation().indices.Length / 3}");
            }
        }

        private IEnumerator Start()
        {
            if (IsBaked) yield break;                  // already baked in editor
            yield return new WaitForSeconds(bakeDelay); // wait for city geometry
            TryBakeNavMesh();
        }

        [ContextMenu("Bake NavMesh Now")]
        public void TryBakeNavMesh()
        {
            if (IsBaked) return;

            NavMeshBuildSettings settings = NavMesh.GetSettingsByIndex(0);

            // Collect ALL MeshRenderers — procedural city objects are NOT isStatic,
            // so we intentionally skip the isStatic filter here.
            var sources = new System.Collections.Generic.List<NavMeshBuildSource>();
            var renderers = FindObjectsOfType<MeshRenderer>();
            foreach (var r in renderers)
            {
                var filter = r.GetComponent<MeshFilter>();
                if (filter == null || filter.sharedMesh == null) continue;

                var src = new NavMeshBuildSource
                {
                    shape        = NavMeshBuildSourceShape.Mesh,
                    sourceObject = filter.sharedMesh,
                    transform    = r.transform.localToWorldMatrix,
                    area         = 0
                };
                sources.Add(src);
            }

            if (sources.Count == 0)
            {
                Debug.LogWarning("<color=#FFAA00>[RuntimeNavMeshBaker]</color> No MeshRenderers found in scene. " +
                                 "NavMesh bake skipped. NPCs will stand still until geometry exists.");
                return;
            }

            Bounds worldBounds = new Bounds(Vector3.zero, new Vector3(400f, 50f, 400f));
            var data = NavMeshBuilder.BuildNavMeshData(settings, sources, worldBounds, Vector3.zero, Quaternion.identity);

            if (data != null)
            {
                NavMesh.AddNavMeshData(data);
                IsBaked = true;
                if (logBakeResult)
                    Debug.Log($"<color=#00FFCC>[RuntimeNavMeshBaker]</color> NavMesh baked. " +
                              $"Sources: {sources.Count}  Triangles: {NavMesh.CalculateTriangulation().indices.Length / 3}");
            }
            else
            {
                Debug.LogWarning("<color=#FF6600>[RuntimeNavMeshBaker]</color> NavMesh bake returned null — " +
                                 "try Window > AI > Navigation > Bake for a manual bake.");
            }
        }
    }
}
