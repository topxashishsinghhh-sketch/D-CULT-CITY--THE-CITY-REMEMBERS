using UnityEngine;

namespace DCultCity.Vehicles
{
    /// <summary>
    /// Constructs a complete, stylized low-poly vehicle from primitives.
    /// Builds body, roof, hood, windows, wheel pivots, headlights, taillights
    /// and brake-light meshes. All parts are referenced for runtime recoloring
    /// and wheel spin animation.
    /// </summary>
    public class VehicleProceduralVisual : MonoBehaviour
    {
        [Header("Definition")]
        [SerializeField] public VehicleDefinition def = new VehicleDefinition();

        // Part references exposed for runtime tinting/animation
        [HideInInspector] public Transform[] wheelPivots  = new Transform[4]; // FL, FR, RL, RR
        [HideInInspector] public Renderer bodyRenderer;
        [HideInInspector] public Renderer roofRenderer;
        [HideInInspector] public Renderer[] brakeLightRenderers = new Renderer[2];
        [HideInInspector] public Light[]    headLights          = new Light[2];
        [HideInInspector] public Light[]    taillights          = new Light[2];

        private bool built = false;

        private void Awake()
        {
            if (!built) BuildVehicle();
        }

        // ------------------------------------------------------------------
        // Public rebuild entry point (also callable from Inspector via context menu)
        // ------------------------------------------------------------------
        [ContextMenu("Rebuild Vehicle Visual")]
        public void BuildVehicle()
        {
            // Clear old visuals
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform c = transform.GetChild(i);
                if (c.name.StartsWith("VFX_"))
                {
                    if (Application.isPlaying) Destroy(c.gameObject);
                    else DestroyImmediate(c.gameObject);
                }
            }

            GameObject root = new GameObject("VFX_Body");
            root.transform.SetParent(transform, false);

            BuildBody(root.transform);
            BuildRoof(root.transform);
            BuildWindows(root.transform);
            BuildWheels(root.transform);
            BuildLights(root.transform);

            built = true;
        }

        // ------------------------------------------------------------------
        // Body sections
        // ------------------------------------------------------------------
        private void BuildBody(Transform parent)
        {
            // Main chassis block
            GameObject body = CreateCube("Body_Main", parent,
                new Vector3(0f, 0.42f, 0f),
                new Vector3(1.80f, 0.58f, 4.20f),
                def.bodyColor);
            bodyRenderer = body.GetComponent<Renderer>();

            // Hood (front slope)
            CreateCube("Body_Hood", parent,
                new Vector3(0f, 0.52f, 1.55f),
                new Vector3(1.76f, 0.22f, 0.80f),
                def.bodyColor);

            // Trunk (rear)
            CreateCube("Body_Trunk", parent,
                new Vector3(0f, 0.58f, -1.40f),
                new Vector3(1.76f, 0.18f, 0.80f),
                def.bodyColor);

            // Front bumper
            CreateCube("Body_Bumper_F", parent,
                new Vector3(0f, 0.22f, 2.14f),
                new Vector3(1.80f, 0.22f, 0.18f),
                Color.Lerp(def.bodyColor, Color.black, 0.4f));

            // Rear bumper
            CreateCube("Body_Bumper_R", parent,
                new Vector3(0f, 0.22f, -2.14f),
                new Vector3(1.80f, 0.22f, 0.18f),
                Color.Lerp(def.bodyColor, Color.black, 0.4f));
        }

        private void BuildRoof(Transform parent)
        {
            // Passenger cabin roof
            GameObject roofObj = CreateCube("Roof_Cabin", parent,
                new Vector3(0f, 0.95f, -0.15f),
                new Vector3(1.72f, 0.38f, 2.40f),
                def.roofColor);
            roofRenderer = roofObj.GetComponent<Renderer>();

            // Windscreen pillars (A-pillar hint)
            CreateCube("Pillar_FL", parent, new Vector3(-0.82f, 0.82f, 0.95f), new Vector3(0.08f, 0.30f, 0.10f), def.roofColor);
            CreateCube("Pillar_FR", parent, new Vector3( 0.82f, 0.82f, 0.95f), new Vector3(0.08f, 0.30f, 0.10f), def.roofColor);
        }

        private void BuildWindows(Transform parent)
        {
            // Windshield (front)
            CreateCube("Window_Front", parent,
                new Vector3(0f, 0.86f, 1.06f),
                new Vector3(1.62f, 0.34f, 0.06f),
                def.windowColor);

            // Rear window
            CreateCube("Window_Rear", parent,
                new Vector3(0f, 0.86f, -1.28f),
                new Vector3(1.62f, 0.30f, 0.06f),
                def.windowColor);

            // Side windows
            CreateCube("Window_SideL", parent,
                new Vector3(-0.88f, 0.90f, -0.14f),
                new Vector3(0.04f, 0.28f, 1.55f),
                def.windowColor);
            CreateCube("Window_SideR", parent,
                new Vector3( 0.88f, 0.90f, -0.14f),
                new Vector3(0.04f, 0.28f, 1.55f),
                def.windowColor);
        }

        private void BuildWheels(Transform parent)
        {
            // Wheel positions: FL, FR, RL, RR
            Vector3[] wheelPositions =
            {
                new Vector3(-0.95f, 0.25f,  1.30f),  // Front-Left
                new Vector3( 0.95f, 0.25f,  1.30f),  // Front-Right
                new Vector3(-0.95f, 0.25f, -1.30f),  // Rear-Left
                new Vector3( 0.95f, 0.25f, -1.30f),  // Rear-Right
            };

            for (int i = 0; i < 4; i++)
            {
                // Pivot at wheel center (allows steering + spin)
                GameObject pivot = new GameObject($"WheelPivot_{i}");
                pivot.transform.SetParent(parent, false);
                pivot.transform.localPosition = wheelPositions[i];
                wheelPivots[i] = pivot.transform;

                // Tyre (Cylinder rotated on Z)
                GameObject tyre = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                tyre.name = "Tyre";
                tyre.transform.SetParent(pivot.transform, false);
                tyre.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                tyre.transform.localScale = new Vector3(0.48f, 0.16f, 0.48f);
                SetColor(tyre, def.wheelColor);

                // Hub cap dot
                GameObject hub = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                hub.name = "Hub";
                hub.transform.SetParent(pivot.transform, false);
                hub.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                hub.transform.localScale = new Vector3(0.24f, 0.18f, 0.24f);
                SetColor(hub, new Color(0.65f, 0.65f, 0.65f));
            }
        }

        private void BuildLights(Transform parent)
        {
            // Headlight positions (front)
            Vector3[] headPos = { new Vector3(-0.60f, 0.52f, 2.08f), new Vector3( 0.60f, 0.52f, 2.08f) };
            // Taillight positions (rear)
            Vector3[] tailPos = { new Vector3(-0.60f, 0.52f, -2.08f), new Vector3( 0.60f, 0.52f, -2.08f) };

            for (int i = 0; i < 2; i++)
            {
                // Headlight lens cube
                CreateCube($"Headlight_{i}", parent, headPos[i], new Vector3(0.30f, 0.14f, 0.06f), def.lightColor);

                GameObject hlGO = new GameObject($"HL_Point_{i}");
                hlGO.transform.SetParent(parent, false);
                hlGO.transform.localPosition = headPos[i] + Vector3.forward * 0.15f;
                Light hl = hlGO.AddComponent<Light>();
                hl.type = LightType.Spot;
                hl.color = def.lightColor;
                hl.intensity = 3f;
                hl.range = 22f;
                hl.spotAngle = 45f;
                hl.enabled = false; // enabled when player enters at night (Milestone 6)
                headLights[i] = hl;

                // Brake-light / tail-light lens
                GameObject tlMesh = CreateCube($"Taillight_{i}", parent, tailPos[i], new Vector3(0.30f, 0.14f, 0.06f), new Color(0.65f, 0.08f, 0.08f));
                brakeLightRenderers[i] = tlMesh.GetComponent<Renderer>();

                GameObject tlGO = new GameObject($"TL_Point_{i}");
                tlGO.transform.SetParent(parent, false);
                tlGO.transform.localPosition = tailPos[i] - Vector3.forward * 0.15f;
                Light tl = tlGO.AddComponent<Light>();
                tl.type = LightType.Point;
                tl.color = new Color(1f, 0.15f, 0.10f);
                tl.intensity = 0f;
                tl.range = 5f;
                taillights[i] = tl;
            }
        }

        // ------------------------------------------------------------------
        // Wheel animation called by VehicleController every frame
        // ------------------------------------------------------------------
        public void AnimateWheels(float speedMs, float steerAngle, bool isBraking)
        {
            if (wheelPivots == null || wheelPivots.Length < 4) return;

            float spinDelta = speedMs * Time.deltaTime * 57.3f * 0.65f; // rad→deg normalised

            // Spin all four wheels
            for (int i = 0; i < 4; i++)
            {
                if (wheelPivots[i] == null) continue;
                wheelPivots[i].Rotate(Vector3.right, spinDelta, Space.Self);
            }

            // Steer front wheels
            if (wheelPivots[0] != null)
            {
                Vector3 eFL = wheelPivots[0].localEulerAngles;
                eFL.y = steerAngle;
                wheelPivots[0].localEulerAngles = eFL;
            }
            if (wheelPivots[1] != null)
            {
                Vector3 eFR = wheelPivots[1].localEulerAngles;
                eFR.y = steerAngle;
                wheelPivots[1].localEulerAngles = eFR;
            }

            // Brake-light brightness
            float brakeIntensity = isBraking ? 2.4f : 0f;
            foreach (var tl in taillights)
                if (tl != null) tl.intensity = brakeIntensity;

            Color brakeColor = isBraking ? new Color(1f, 0.12f, 0.10f) : new Color(0.65f, 0.08f, 0.08f);
            foreach (var bl in brakeLightRenderers)
                if (bl != null) bl.material.color = brakeColor;
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------
        private GameObject CreateCube(string name, Transform parent, Vector3 pos, Vector3 scale, Color color)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            SetColor(go, color);
            return go;
        }

        private void SetColor(GameObject go, Color color)
        {
            var r = go.GetComponent<Renderer>();
            if (r == null) return;
            Shader s = Shader.Find("Universal Render Pipeline/Lit")
                    ?? Shader.Find("Standard")
                    ?? Shader.Find("Diffuse");
            Material mat = new Material(s);
            mat.color = color;
            r.sharedMaterial = mat;
        }
    }
}
