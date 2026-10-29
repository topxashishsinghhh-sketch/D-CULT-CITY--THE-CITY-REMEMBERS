using UnityEngine;

namespace DCultCity.World
{
    /// <summary>
    /// Generates the expanded open world for D-CULT CITY:
    /// - Downtown City (Casino, Police, Shops, Apartments, Parks)
    /// - Coastal Beach (Golden sand, ocean water plane, beach pier)
    /// - Deep Forest (Pine clusters, clearings for wildlife)
    /// - Mountain Range (Imposing perimeter rock peaks)
    /// - Connected road circuit linking all biomes for vehicles.
    /// </summary>
    [ExecuteAlways]
    public class FullWorldBuilder : MonoBehaviour
    {
        [Header("World Dimensions")]
        [SerializeField] private float worldExtent = 240f;
        [SerializeField] private bool buildOnAwake = true;

        private GameObject worldRoot;

        private void Awake()
        {
            if (buildOnAwake && Application.isPlaying)
            {
                BuildFullWorld();
            }
        }

        [ContextMenu("Build Full World")]
        public void BuildFullWorld()
        {
            // Remove previous instances if any
            Transform old = transform.Find("FullWorld_Root");
            if (old != null)
            {
                if (Application.isPlaying) Destroy(old.gameObject);
                else DestroyImmediate(old.gameObject);
            }

            worldRoot = new GameObject("FullWorld_Root");
            worldRoot.transform.SetParent(transform, false);

            BuildTerrainFoundation(worldRoot.transform);
            BuildCoastalBeach(worldRoot.transform);
            BuildDeepForest(worldRoot.transform);
            BuildMountainRange(worldRoot.transform);
            BuildConnectingHighways(worldRoot.transform);

            Debug.Log("<color=#00FFAA>[FullWorldBuilder]</color> Full open world biomes generated successfully!");
        }

        private void BuildTerrainFoundation(Transform parent)
        {
            // Base city and surrounding continental shelf
            GameObject terrain = GameObject.CreatePrimitive(PrimitiveType.Plane);
            terrain.name = "Terrain_BaseContinental";
            terrain.transform.SetParent(parent, false);
            terrain.transform.position = new Vector3(0f, -0.05f, 0f);
            terrain.transform.localScale = new Vector3(worldExtent / 10f, 1f, worldExtent / 10f);
            SetMaterial(terrain, new Color(0.24f, 0.45f, 0.22f)); // Natural landscape green
        }

        private void BuildCoastalBeach(Transform parent)
        {
            GameObject beachZone = new GameObject("Zone_CoastalBeach");
            beachZone.transform.SetParent(parent, false);

            // Sandy Shore (South-West quadrant, X: -80 to -30, Z: -100 to -40)
            GameObject sand = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sand.name = "Beach_SandPlaza";
            sand.transform.SetParent(beachZone.transform, false);
            sand.transform.position = new Vector3(-65f, 0.05f, -65f);
            sand.transform.localScale = new Vector3(80f, 0.3f, 70f);
            SetMaterial(sand, new Color(0.92f, 0.84f, 0.62f)); // Golden Sand

            // Ocean Water Surface
            GameObject ocean = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ocean.name = "Ocean_WaterPlane";
            ocean.transform.SetParent(beachZone.transform, false);
            ocean.transform.position = new Vector3(-95f, -0.02f, -85f);
            ocean.transform.localScale = new Vector3(8f, 1f, 8f);
            SetMaterial(ocean, new Color(0.12f, 0.45f, 0.72f, 0.85f));

            // Wooden Pier extending into the water
            GameObject pier = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pier.name = "Pier_Boardwalk";
            pier.transform.SetParent(beachZone.transform, false);
            pier.transform.position = new Vector3(-60f, 0.6f, -95f);
            pier.transform.localScale = new Vector3(6f, 0.5f, 40f);
            SetMaterial(pier, new Color(0.48f, 0.32f, 0.18f)); // Dark Wood
        }

        private void BuildDeepForest(Transform parent)
        {
            GameObject forestZone = new GameObject("Zone_DeepForest");
            forestZone.transform.SetParent(parent, false);

            // Forest floor plateau (North-West quadrant, X: -75f, Z: 65f)
            GameObject forestLawn = GameObject.CreatePrimitive(PrimitiveType.Cube);
            forestLawn.name = "Forest_Floor";
            forestLawn.transform.SetParent(forestZone.transform, false);
            forestLawn.transform.position = new Vector3(-65f, 0.2f, 65f);
            forestLawn.transform.localScale = new Vector3(80f, 0.4f, 80f);
            SetMaterial(forestLawn, new Color(0.16f, 0.38f, 0.16f)); // Deep forest moss

            // Procedural Pine Tree Clusters
            Vector3[] pineClusters = {
                new Vector3(-45f, 0f, 45f), new Vector3(-55f, 0f, 55f),
                new Vector3(-70f, 0f, 50f), new Vector3(-80f, 0f, 75f),
                new Vector3(-50f, 0f, 80f), new Vector3(-65f, 0f, 90f),
                new Vector3(-85f, 0f, 60f), new Vector3(-40f, 0f, 65f)
            };

            for (int i = 0; i < pineClusters.Length; i++)
            {
                CreatePineTree(forestZone.transform, pineClusters[i], $"PineTree_{i + 1}");
            }
        }

        private void CreatePineTree(Transform parent, Vector3 rootPos, string name)
        {
            GameObject tree = new GameObject(name);
            tree.transform.SetParent(parent, false);
            tree.transform.position = rootPos;

            // Trunk
            GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "Trunk";
            trunk.transform.SetParent(tree.transform, false);
            trunk.transform.localPosition = new Vector3(0f, 2f, 0f);
            trunk.transform.localScale = new Vector3(0.5f, 2f, 0.5f);
            SetMaterial(trunk, new Color(0.32f, 0.20f, 0.12f));

            // Pine Cones / Foliage levels
            float[] tierHeights = { 3.5f, 5.0f, 6.2f };
            float[] tierRadii = { 2.6f, 2.0f, 1.4f };

            for (int t = 0; t < 3; t++)
            {
                GameObject cone = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                cone.name = $"FoliageTier_{t}";
                cone.transform.SetParent(tree.transform, false);
                cone.transform.localPosition = new Vector3(0f, tierHeights[t], 0f);
                cone.transform.localScale = new Vector3(tierRadii[t], 0.7f, tierRadii[t]);
                SetMaterial(cone, new Color(0.12f, 0.32f, 0.15f));
            }
        }

        private void BuildMountainRange(Transform parent)
        {
            GameObject mountainZone = new GameObject("Zone_MountainRange");
            mountainZone.transform.SetParent(parent, false);

            // Perimeter mountain peaks (North and East borders)
            Vector3[] mountainPeaks = {
                new Vector3( 20f, 18f,  90f), new Vector3( 50f, 26f,  95f),
                new Vector3( 80f, 32f,  85f), new Vector3( 95f, 28f,  55f),
                new Vector3(100f, 34f,  20f), new Vector3( 95f, 22f, -15f),
                new Vector3( 90f, 18f, -50f)
            };

            for (int i = 0; i < mountainPeaks.Length; i++)
            {
                GameObject peak = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                peak.name = $"MountainPeak_{i + 1}";
                peak.transform.SetParent(mountainZone.transform, false);
                peak.transform.position = mountainPeaks[i];
                peak.transform.localScale = new Vector3(32f, mountainPeaks[i].y, 32f);
                SetMaterial(peak, new Color(0.40f, 0.42f, 0.44f)); // Slate Mountain Rock

                // Snow cap
                GameObject snow = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                snow.name = "SnowCap";
                snow.transform.SetParent(peak.transform, false);
                snow.transform.localPosition = new Vector3(0f, 0.95f, 0f);
                snow.transform.localScale = new Vector3(0.75f, 0.35f, 0.75f);
                SetMaterial(snow, Color.white);
            }
        }

        private void BuildConnectingHighways(Transform parent)
        {
            GameObject roadNetwork = new GameObject("Highways_ConnectingRoads");
            roadNetwork.transform.SetParent(parent, false);

            // 1. Coastal Highway (Downtown to Beach)
            CreateRoadSegment(roadNetwork.transform, "Highway_To_Beach",
                new Vector3(-32f, 0.12f, -32f), new Vector3(10f, 0.1f, 65f), -45f);

            // 2. Forest Route (Downtown to Pine Forest)
            CreateRoadSegment(roadNetwork.transform, "Highway_To_Forest",
                new Vector3(-32f, 0.12f, 32f), new Vector3(10f, 0.1f, 65f), 45f);

            // 3. Mountain Scenic Pass (Downtown to Mountain foothills)
            CreateRoadSegment(roadNetwork.transform, "Highway_To_Mountain",
                new Vector3(45f, 0.12f, 20f), new Vector3(65f, 0.1f, 10f), 0f);
        }

        private void CreateRoadSegment(Transform parent, string name, Vector3 pos, Vector3 scale, float angleY)
        {
            GameObject road = GameObject.CreatePrimitive(PrimitiveType.Cube);
            road.name = name;
            road.transform.SetParent(parent, false);
            road.transform.position = pos;
            road.transform.rotation = Quaternion.Euler(0f, angleY, 0f);
            road.transform.localScale = scale;
            road.isStatic = true;
            SetMaterial(road, new Color(0.18f, 0.18f, 0.20f)); // Highway Asphalt
        }

        private void SetMaterial(GameObject go, Color color)
        {
            var r = go.GetComponent<Renderer>();
            if (r == null) return;
            go.isStatic = true;

            Shader s = Shader.Find("Universal Render Pipeline/Lit");
            if (s == null) s = Shader.Find("Standard");
            if (s == null) s = Shader.Find("Diffuse");

            Material mat = new Material(s);
            mat.color = color;
            r.sharedMaterial = mat;
        }
    }
}
