using System.Collections.Generic;
using UnityEngine;

namespace DCultCity.World
{
    /// <summary>
    /// Builds a compact, stylized 3D test city block for Milestone 1.
    /// Creates streets, sidewalks, landmark buildings (Casino, Police, Shops, Apartments),
    /// a central park area, and streetlights using lightweight procedural primitives.
    /// Can be generated at runtime or in the Editor.
    /// </summary>
    [ExecuteAlways]
    public class TestCityBuilder : MonoBehaviour
    {
        [Header("City Configuration")]
        [SerializeField] private bool buildOnAwake = true;
        [SerializeField] private float blockSize = 60f;
        [SerializeField] private float roadWidth = 10f;

        private GameObject cityRoot;

        private void Awake()
        {
            if (buildOnAwake && Application.isPlaying)
            {
                RebuildCity();
            }
        }

        [ContextMenu("Rebuild Test City")]
        public void RebuildCity()
        {
            // Clear existing city if any
            if (cityRoot != null)
            {
                if (Application.isPlaying) Destroy(cityRoot);
                else DestroyImmediate(cityRoot);
            }

            Transform existing = transform.Find("Procedural_TestCity");
            if (existing != null)
            {
                if (Application.isPlaying) Destroy(existing.gameObject);
                else DestroyImmediate(existing.gameObject);
            }

            cityRoot = new GameObject("Procedural_TestCity");
            cityRoot.transform.SetParent(transform, false);

            CreateGroundAndRoads(cityRoot.transform);
            CreateCentralPark(cityRoot.transform);
            CreateBuildings(cityRoot.transform);
            CreateStreetlights(cityRoot.transform);

            Debug.Log("<color=#00FFCC>[TestCityBuilder]</color> Test city block generated successfully.");
        }

        private void CreateGroundAndRoads(Transform parent)
        {
            // Main ground (Asphalt / Road Base)
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground_RoadGrid";
            ground.transform.SetParent(parent, false);
            ground.transform.localScale = new Vector3(blockSize / 10f, 1f, blockSize / 10f);
            ground.transform.position = Vector3.zero;

            SetColor(ground, new Color(0.18f, 0.18f, 0.20f)); // Dark Asphalt

            // Sidewalk blocks
            float halfBlock = blockSize * 0.45f;
            float sidewalkOffset = roadWidth * 0.5f + 4f;

            CreateSidewalkBlock(parent, new Vector3(halfBlock * 0.5f, 0.08f, halfBlock * 0.5f), new Vector3(halfBlock - 2f, 0.15f, halfBlock - 2f));
            CreateSidewalkBlock(parent, new Vector3(-halfBlock * 0.5f, 0.08f, halfBlock * 0.5f), new Vector3(halfBlock - 2f, 0.15f, halfBlock - 2f));
            CreateSidewalkBlock(parent, new Vector3(-halfBlock * 0.5f, 0.08f, -halfBlock * 0.5f), new Vector3(halfBlock - 2f, 0.15f, halfBlock - 2f));
            CreateSidewalkBlock(parent, new Vector3(halfBlock * 0.5f, 0.08f, -halfBlock * 0.5f), new Vector3(halfBlock - 2f, 0.15f, halfBlock - 2f));
        }

        private void CreateSidewalkBlock(Transform parent, Vector3 pos, Vector3 size)
        {
            GameObject curb = GameObject.CreatePrimitive(PrimitiveType.Cube);
            curb.name = "Sidewalk";
            curb.transform.SetParent(parent, false);
            curb.transform.position = pos;
            curb.transform.localScale = size;
            SetColor(curb, new Color(0.72f, 0.72f, 0.70f)); // Light concrete
        }

        private void CreateCentralPark(Transform parent)
        {
            // Green Park grass block in south-west quadrant
            GameObject park = GameObject.CreatePrimitive(PrimitiveType.Cube);
            park.name = "Park_Lawn";
            park.transform.SetParent(parent, false);
            park.transform.position = new Vector3(-13f, 0.18f, -13f);
            park.transform.localScale = new Vector3(20f, 0.2f, 20f);
            SetColor(park, new Color(0.22f, 0.58f, 0.28f)); // Grass Green

            // Park Fountain center
            GameObject fountain = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            fountain.name = "Park_Fountain";
            fountain.transform.SetParent(park.transform, false);
            fountain.transform.localPosition = new Vector3(0f, 0.8f, 0f);
            fountain.transform.localScale = new Vector3(0.25f, 0.6f, 0.25f);
            SetColor(fountain, new Color(0.3f, 0.65f, 0.85f)); // Water Blue

            // Stylized park trees (Cylinder trunk + Sphere foliage)
            Vector3[] treePositions = {
                new Vector3(-6f, 0.2f, -6f),
                new Vector3(6f, 0.2f, -6f),
                new Vector3(-6f, 0.2f, 6f),
                new Vector3(6f, 0.2f, 6f)
            };

            foreach (var pos in treePositions)
            {
                CreateParkTree(park.transform, pos);
            }
        }

        private void CreateParkTree(Transform parent, Vector3 localPos)
        {
            GameObject tree = new GameObject("ParkTree");
            tree.transform.SetParent(parent, false);
            tree.transform.localPosition = localPos;

            // Trunk
            GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "Trunk";
            trunk.transform.SetParent(tree.transform, false);
            trunk.transform.localPosition = new Vector3(0f, 1.2f, 0f);
            trunk.transform.localScale = new Vector3(0.04f, 1.2f, 0.04f);
            SetColor(trunk, new Color(0.35f, 0.22f, 0.12f));

            // Foliage
            GameObject foliage = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            foliage.name = "Foliage";
            foliage.transform.SetParent(tree.transform, false);
            foliage.transform.localPosition = new Vector3(0f, 2.6f, 0f);
            foliage.transform.localScale = new Vector3(0.2f, 1.5f, 0.2f);
            SetColor(foliage, new Color(0.18f, 0.48f, 0.22f));
        }

        private void CreateBuildings(Transform parent)
        {
            // 1. Casino (North-East quadrant) - Golden / Purple neon style
            CreateBuilding(parent, "Building_Casino_GrandLux",
                position: new Vector3(15f, 6f, 15f),
                scale: new Vector3(18f, 12f, 18f),
                color: new Color(0.85f, 0.65f, 0.15f),
                signName: "CASINO & LOUNGE");

            // 2. Police Precinct (North-West quadrant) - Blue & White
            CreateBuilding(parent, "Building_Police_Precinct4",
                position: new Vector3(-15f, 5f, 15f),
                scale: new Vector3(18f, 10f, 16f),
                color: new Color(0.20f, 0.35f, 0.65f),
                signName: "POLICE DEPARTMENT");

            // 3. Commercial Shopping Plaza (South-East quadrant) - Clean Modern Glass / Red
            CreateBuilding(parent, "Building_Downtown_Shops",
                position: new Vector3(15f, 4f, -15f),
                scale: new Vector3(16f, 8f, 16f),
                color: new Color(0.82f, 0.28f, 0.28f),
                signName: "METRO SHOPS & CAFE");

            // 4. Luxury Apartments (Tall Skyscraper in the background)
            CreateBuilding(parent, "Building_Residential_Tower",
                position: new Vector3(0f, 12f, 26f),
                scale: new Vector3(12f, 24f, 8f),
                color: new Color(0.35f, 0.40f, 0.45f),
                signName: "SKYLINE SUITES");
        }

        private void CreateBuilding(Transform parent, string name, Vector3 position, Vector3 scale, Color color, string signName)
        {
            GameObject bldg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bldg.name = name;
            bldg.transform.SetParent(parent, false);
            bldg.transform.position = position;
            bldg.transform.localScale = scale;
            SetColor(bldg, color);

            // Add simple roof accent
            GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof.name = "Roof_Trim";
            roof.transform.SetParent(bldg.transform, false);
            roof.transform.localPosition = new Vector3(0f, 0.52f, 0f);
            roof.transform.localScale = new Vector3(1.05f, 0.05f, 1.05f);
            SetColor(roof, Color.white * 0.9f);
        }

        private void CreateStreetlights(Transform parent)
        {
            Vector3[] lightPositions = {
                new Vector3(6f, 0f, 6f),
                new Vector3(-6f, 0f, 6f),
                new Vector3(6f, 0f, -6f),
                new Vector3(-6f, 0f, -6f),
                new Vector3(18f, 0f, 6f),
                new Vector3(-18f, 0f, 6f)
            };

            for (int i = 0; i < lightPositions.Length; i++)
            {
                CreateSingleStreetlight(parent, lightPositions[i], $"Streetlight_{i + 1}");
            }
        }

        private void CreateSingleStreetlight(Transform parent, Vector3 basePos, string name)
        {
            GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pole.name = name;
            pole.transform.SetParent(parent, false);
            pole.transform.position = basePos + new Vector3(0f, 2.5f, 0f);
            pole.transform.localScale = new Vector3(0.18f, 2.5f, 0.18f);
            SetColor(pole, new Color(0.2f, 0.2f, 0.2f));

            // Light fixture head
            GameObject lampHead = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            lampHead.name = "LampBulb";
            lampHead.transform.SetParent(pole.transform, false);
            lampHead.transform.localPosition = new Vector3(0f, 1.05f, 0.5f);
            lampHead.transform.localScale = new Vector3(1.2f, 0.4f, 1.2f);
            SetColor(lampHead, Color.yellow);

            // Point light for night/day ambiance
            GameObject lightObj = new GameObject("LightSource");
            lightObj.transform.SetParent(lampHead.transform, false);
            lightObj.transform.localPosition = Vector3.down * 0.2f;
            Light ptLight = lightObj.AddComponent<Light>();
            ptLight.type = LightType.Point;
            ptLight.color = new Color(1f, 0.92f, 0.75f);
            ptLight.range = 14f;
            ptLight.intensity = 1.2f;
        }

        private void SetColor(GameObject go, Color color)
        {
            var renderer = go.GetComponent<Renderer>();
            if (renderer == null) return;

            // Mark geometry as Navigation Static so the NavMesh baker can walk on it
            go.isStatic = true;

            // Create temporary runtime material so it renders with colors properly
            Shader standardShader = Shader.Find("Universal Render Pipeline/Lit");
            if (standardShader == null) standardShader = Shader.Find("Standard");
            if (standardShader == null) standardShader = Shader.Find("Diffuse");

            Material mat = new Material(standardShader);
            mat.color = color;
            renderer.sharedMaterial = mat;
        }
    }
}
