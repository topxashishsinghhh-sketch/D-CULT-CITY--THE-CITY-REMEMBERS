using UnityEngine;

namespace DCultCity.Player
{
    /// <summary>
    /// Constructs and renders the stylized low-poly character model.
    /// Dynamically applies skin tone, hair style/color, gender proportions, and apparel colors.
    /// Includes procedural locomotion animation (arm/leg swing, body bobbing) for instant visual feedback.
    /// </summary>
    public class CharacterVisual : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private CharacterCustomizationData data = new CharacterCustomizationData();

        [Header("Hierarchy References")]
        private Transform headBone;
        private Transform hairRoot;
        private Transform torsoBone;
        private Transform leftArmBone;
        private Transform rightArmBone;
        private Transform leftLegBone;
        private Transform rightLegBone;

        private Renderer headRenderer;
        private Renderer hairRenderer;
        private Renderer torsoRenderer;
        private Renderer pantsRendererLeft;
        private Renderer pantsRendererRight;

        // Procedural animation state
        private float walkCycle = 0f;
        private bool isInitialized = false;

        public CharacterCustomizationData Data => data;

        private void Awake()
        {
            if (!isInitialized)
            {
                BuildVisualHierarchy();
            }
        }

        public void ApplyCustomization(CharacterCustomizationData newData)
        {
            this.data = newData.Clone();
            if (!isInitialized)
            {
                BuildVisualHierarchy();
            }
            UpdateVisualAppearance();
        }

        /// <summary>
        /// Builds a clean, stylized low-poly humanoid rig from primitives.
        /// </summary>
        public void BuildVisualHierarchy()
        {
            // Clear existing visuals if re-building
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
                if (child.name.StartsWith("Rig_"))
                {
                    if (Application.isPlaying) Destroy(child.gameObject);
                    else DestroyImmediate(child.gameObject);
                }
            }

            GameObject rigRoot = new GameObject("Rig_Humanoid");
            rigRoot.transform.SetParent(transform, false);

            bool isFemale = data.gender == GenderType.Female;
            float shoulderScale = isFemale ? 0.42f : 0.52f;
            float hipScale = isFemale ? 0.45f : 0.40f;

            // Torso (Jacket/Shirt)
            GameObject torsoObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            torsoObj.name = "Torso";
            torsoObj.transform.SetParent(rigRoot.transform, false);
            torsoObj.transform.localPosition = new Vector3(0f, 1.15f, 0f);
            torsoObj.transform.localScale = new Vector3(shoulderScale, 0.65f, 0.28f);
            torsoBone = torsoObj.transform;
            torsoRenderer = torsoObj.GetComponent<Renderer>();

            // Head (Skin)
            GameObject headObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            headObj.name = "Head";
            headObj.transform.SetParent(torsoBone, false);
            headObj.transform.localPosition = new Vector3(0f, 0.65f, 0f);
            headObj.transform.localScale = new Vector3(0.55f / shoulderScale, 0.42f, 0.85f);
            headBone = headObj.transform;
            headRenderer = headObj.GetComponent<Renderer>();

            // Hair
            GameObject hairObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hairObj.name = "Hair";
            hairObj.transform.SetParent(headBone, false);
            hairObj.transform.localPosition = new Vector3(0f, 0.35f, -0.05f);
            hairObj.transform.localScale = new Vector3(1.1f, 0.5f, 1.1f);
            hairRoot = hairObj.transform;
            hairRenderer = hairObj.GetComponent<Renderer>();

            // Left Arm
            GameObject leftArm = CreateLimb("Arm_L", rigRoot.transform, new Vector3(-shoulderScale * 0.6f, 1.35f, 0f), new Vector3(0.12f, 0.55f, 0.12f));
            leftArmBone = leftArm.transform;

            // Right Arm
            GameObject rightArm = CreateLimb("Arm_R", rigRoot.transform, new Vector3(shoulderScale * 0.6f, 1.35f, 0f), new Vector3(0.12f, 0.55f, 0.12f));
            rightArmBone = rightArm.transform;

            // Left Leg (Pants)
            GameObject leftLeg = CreateLimb("Leg_L", rigRoot.transform, new Vector3(-hipScale * 0.35f, 0.75f, 0f), new Vector3(0.16f, 0.70f, 0.16f));
            leftLegBone = leftLeg.transform;
            pantsRendererLeft = leftLeg.transform.GetChild(0).GetComponent<Renderer>();

            // Right Leg (Pants)
            GameObject rightLeg = CreateLimb("Leg_R", rigRoot.transform, new Vector3(hipScale * 0.35f, 0.75f, 0f), new Vector3(0.16f, 0.70f, 0.16f));
            rightLegBone = rightLeg.transform;
            pantsRendererRight = rightLeg.transform.GetChild(0).GetComponent<Renderer>();

            isInitialized = true;
            UpdateVisualAppearance();
        }

        private GameObject CreateLimb(string name, Transform parent, Vector3 pivotPos, Vector3 scale)
        {
            // Pivot object so rotations happen at the shoulder/hip joint
            GameObject pivot = new GameObject(name);
            pivot.transform.SetParent(parent, false);
            pivot.transform.localPosition = pivotPos;

            GameObject mesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mesh.name = "Mesh";
            mesh.transform.SetParent(pivot.transform, false);
            mesh.transform.localPosition = new Vector3(0f, -scale.y * 0.5f, 0f);
            mesh.transform.localScale = scale;

            return pivot;
        }

        /// <summary>
        /// Recolors materials according to current CustomizationData.
        /// </summary>
        public void UpdateVisualAppearance()
        {
            if (headRenderer != null) SetColor(headRenderer, data.GetCurrentSkinColor());
            if (hairRenderer != null)
            {
                SetColor(hairRenderer, data.GetCurrentHairColor());
                // Hair style alterations
                hairRoot.localScale = data.hairStyleIndex switch
                {
                    1 => new Vector3(0.4f, 0.8f, 1.1f),   // Mohawk
                    2 => new Vector3(1.2f, 0.7f, 1.25f),  // Long / Volume
                    _ => new Vector3(1.1f, 0.45f, 1.1f)   // Standard Short
                };
            }
            if (torsoRenderer != null) SetColor(torsoRenderer, data.GetCurrentClothingColor());

            Color pantsColor = data.GetCurrentPantsColor();
            if (pantsRendererLeft != null) SetColor(pantsRendererLeft, pantsColor);
            if (pantsRendererRight != null) SetColor(pantsRendererRight, pantsColor);
        }

        /// <summary>
        /// Procedural locomotion animation for walking, running, and jumping.
        /// </summary>
        public void AnimateLocomotion(float speed, bool isGrounded)
        {
            if (!isInitialized || torsoBone == null) return;

            if (!isGrounded)
            {
                // In-air / jumping pose
                leftLegBone.localRotation = Quaternion.Euler(-30f, 0f, 0f);
                rightLegBone.localRotation = Quaternion.Euler(20f, 0f, 0f);
                leftArmBone.localRotation = Quaternion.Euler(-70f, 0f, 0f);
                rightArmBone.localRotation = Quaternion.Euler(-70f, 0f, 0f);
                torsoBone.localPosition = new Vector3(0f, 1.18f, 0f);
                return;
            }

            if (speed > 0.1f)
            {
                // Moving - swing limbs based on speed
                float frequency = speed * 2.2f;
                walkCycle += Time.deltaTime * frequency;

                float legAngle = Mathf.Sin(walkCycle) * 35f;
                float armAngle = -Mathf.Sin(walkCycle) * 40f;
                float bob = Mathf.Abs(Mathf.Sin(walkCycle * 2f)) * 0.06f;

                leftLegBone.localRotation = Quaternion.Euler(legAngle, 0f, 0f);
                rightLegBone.localRotation = Quaternion.Euler(-legAngle, 0f, 0f);
                leftArmBone.localRotation = Quaternion.Euler(armAngle, 0f, 0f);
                rightArmBone.localRotation = Quaternion.Euler(-armAngle, 0f, 0f);

                torsoBone.localPosition = new Vector3(0f, 1.15f + bob, 0f);
            }
            else
            {
                // Idle breathing pose
                walkCycle = 0f;
                float breath = Mathf.Sin(Time.time * 2f) * 2f;

                leftLegBone.localRotation = Quaternion.identity;
                rightLegBone.localRotation = Quaternion.identity;
                leftArmBone.localRotation = Quaternion.Euler(breath, 0f, 0f);
                rightArmBone.localRotation = Quaternion.Euler(-breath, 0f, 0f);
                torsoBone.localPosition = new Vector3(0f, 1.15f + Mathf.Sin(Time.time * 2f) * 0.015f, 0f);
            }
        }

        private void SetColor(Renderer rend, Color col)
        {
            if (rend == null) return;
            Shader s = Shader.Find("Universal Render Pipeline/Lit");
            if (s == null) s = Shader.Find("Standard");
            if (s == null) s = Shader.Find("Diffuse");

            Material m = new Material(s);
            m.color = col;
            rend.sharedMaterial = m;
        }
    }
}
