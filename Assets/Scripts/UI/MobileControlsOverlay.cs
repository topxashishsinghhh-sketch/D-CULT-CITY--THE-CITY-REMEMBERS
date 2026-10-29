using DCultCity.Player;
using UnityEngine;

namespace DCultCity.UI
{
    /// <summary>
    /// Renders on-screen touch controls (Virtual Joystick, Jump, Sprint, Customize)
    /// optimized for mobile devices and playable via mouse drag in the Editor.
    /// </summary>
    public class MobileControlsOverlay : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private bool showInEditor = true;
        [SerializeField] private float joystickRadius = 70f;

        private PlayerInputReader inputReader;
        private bool isDraggingJoystick = false;
        private Vector2 joystickCenter;
        private Vector2 joystickCurrentPos;
        private bool isSprinting = false;

        private GUIStyle buttonStyle;
        private GUIStyle labelStyle;
        private bool stylesInit = false;

        private void Start()
        {
            inputReader = PlayerInputReader.Instance;
            if (inputReader == null) inputReader = FindObjectOfType<PlayerInputReader>();
        }

        private void InitStyles()
        {
            if (stylesInit) return;
            float scale = Mathf.Max(1.0f, Screen.height / 720f);

            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = Mathf.RoundToInt(14 * scale),
                fontStyle = FontStyle.Bold
            };

            labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(12 * scale),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            labelStyle.normal.textColor = Color.white;

            stylesInit = true;
        }

        private void OnGUI()
        {
            if (!Application.isMobilePlatform && !showInEditor) return;
            InitStyles();

            float screenW = Screen.width;
            float screenH = Screen.height;
            float scale = Mathf.Max(1.0f, screenH / 720f);

            // 1. Virtual Joystick Area (Bottom-Left)
            DrawVirtualJoystick(screenH, scale);

            // 2. Action Buttons (Bottom-Right)
            DrawActionButtons(screenW, screenH, scale);

            // 3. Top-Right Customize / Change Look Button
            DrawTopUtilityButtons(screenW, scale);
        }

        private void DrawVirtualJoystick(float screenH, float scale)
        {
            float baseRadius = joystickRadius * scale;
            Vector2 defaultCenter = new Vector2(baseRadius + 35f * scale, screenH - baseRadius - 40f * scale);

            if (!isDraggingJoystick)
            {
                joystickCenter = defaultCenter;
                joystickCurrentPos = defaultCenter;
            }

            // Outer Base Ring
            Rect baseRect = new Rect(joystickCenter.x - baseRadius, joystickCenter.y - baseRadius, baseRadius * 2f, baseRadius * 2f);
            GUI.color = new Color(1f, 1f, 1f, 0.25f);
            GUI.Box(baseRect, GUIContent.none);

            // Handle
            float handleRadius = baseRadius * 0.45f;
            Rect handleRect = new Rect(joystickCurrentPos.x - handleRadius, joystickCurrentPos.y - handleRadius, handleRadius * 2f, handleRadius * 2f);
            GUI.color = new Color(0.2f, 0.8f, 1f, 0.7f);
            GUI.Box(handleRect, GUIContent.none);
            GUI.color = Color.white;

            // Touch / Mouse Tracking for Joystick
            Vector2 pointerPos = GetPointerPosition();
            bool pointerDown = GetPointerDown();

            if (pointerDown)
            {
                // If touch began in left half bottom area
                if (!isDraggingJoystick && Vector2.Distance(pointerPos, joystickCenter) < baseRadius * 1.5f)
                {
                    isDraggingJoystick = true;
                }

                if (isDraggingJoystick)
                {
                    Vector2 diff = pointerPos - joystickCenter;
                    if (diff.magnitude > baseRadius)
                    {
                        diff = diff.normalized * baseRadius;
                    }
                    joystickCurrentPos = joystickCenter + diff;

                    // Invert Y for standard Unity input (Up is +1)
                    Vector2 normalizedInput = new Vector2(diff.x / baseRadius, -diff.y / baseRadius);
                    inputReader?.SetMobileMove(normalizedInput);
                }
            }
            else if (isDraggingJoystick)
            {
                isDraggingJoystick = false;
                joystickCurrentPos = joystickCenter;
                inputReader?.SetMobileMove(Vector2.zero);
            }
        }

        private void DrawActionButtons(float screenW, float screenH, float scale)
        {
            float btnSize = 65f * scale;

            // Jump Button
            Rect jumpRect = new Rect(screenW - btnSize - 30f * scale, screenH - btnSize - 35f * scale, btnSize, btnSize);
            GUI.backgroundColor = new Color(0.25f, 0.75f, 1f);
            if (GUI.Button(jumpRect, "JUMP", buttonStyle))
            {
                inputReader?.TriggerMobileJump();
            }

            // Sprint Toggle Button
            Rect sprintRect = new Rect(screenW - (btnSize * 2f) - 45f * scale, screenH - btnSize - 35f * scale, btnSize, btnSize);
            GUI.backgroundColor = isSprinting ? new Color(1f, 0.4f, 0.2f) : new Color(0.4f, 0.4f, 0.4f);
            if (GUI.Button(sprintRect, isSprinting ? "RUN" : "WALK", buttonStyle))
            {
                isSprinting = !isSprinting;
                inputReader?.SetMobileSprint(isSprinting);
            }

            // Contextual Mobile Action Buttons (Above Jump/Run)
            // [E] Talk / Interact Button
            Rect interactRect = new Rect(screenW - btnSize - 30f * scale, screenH - (btnSize * 2f) - 50f * scale, btnSize, btnSize * 0.85f);
            GUI.backgroundColor = new Color(0.2f, 0.9f, 0.45f);
            if (GUI.Button(interactRect, "[E] TALK", buttonStyle))
            {
                // Trigger simulated 'E' keypress
                SimulateActionKey(KeyCode.E);
            }

            // [F] Drive / Vehicle Button
            Rect vehicleRect = new Rect(screenW - (btnSize * 2f) - 45f * scale, screenH - (btnSize * 2f) - 50f * scale, btnSize, btnSize * 0.85f);
            GUI.backgroundColor = new Color(1f, 0.75f, 0.15f);
            if (GUI.Button(vehicleRect, "[F] CAR", buttonStyle))
            {
                SimulateActionKey(KeyCode.F);
            }

            GUI.backgroundColor = Color.white;
        }

        private void SimulateActionKey(KeyCode key)
        {
            if (key == KeyCode.E)
            {
                // Check nearby StoryNPC
                var npcs = FindObjectsOfType<DCultCity.NPC.StoryNPC>();
                foreach (var npc in npcs)
                {
                    float d = Vector3.Distance(transform.position, npc.transform.position);
                    if (d < 3.5f)
                    {
                        var method = typeof(DCultCity.NPC.StoryNPC).GetMethod("OpenDialogue", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        method?.Invoke(npc, null);
                        break;
                    }
                }
            }
            else if (key == KeyCode.F)
            {
                var vehicle = FindObjectOfType<DCultCity.Vehicles.VehicleController>();
                if (vehicle != null)
                {
                    if (vehicle.IsOccupied)
                    {
                        var method = typeof(DCultCity.Vehicles.VehicleController).GetMethod("ExitVehicle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        method?.Invoke(vehicle, null);
                    }
                    else if (vehicle.IsPlayerNearby)
                    {
                        var method = typeof(DCultCity.Vehicles.VehicleController).GetMethod("EnterVehicle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        method?.Invoke(vehicle, null);
                    }
                }
            }
        }

        private void DrawTopUtilityButtons(float screenW, float scale)
        {
            float btnW = 120f * scale;
            float btnH = 38f * scale;

            Rect custRect = new Rect(screenW - btnW - 20f * scale, 20f * scale, btnW, btnH);
            GUI.backgroundColor = new Color(0.85f, 0.35f, 0.95f);
            if (GUI.Button(custRect, "CUSTOMIZE", buttonStyle))
            {
                var creatorUI = FindObjectOfType<CharacterCreatorUI>();
                if (creatorUI != null)
                {
                    creatorUI.ToggleCreatorView();
                }
            }

            GUI.backgroundColor = Color.white;
        }

        private Vector2 GetPointerPosition()
        {
            if (Input.touchCount > 0)
            {
                Touch t = Input.GetTouch(0);
                return new Vector2(t.position.x, Screen.height - t.position.y);
            }
            return new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
        }

        private bool GetPointerDown()
        {
            if (Input.touchCount > 0) return true;
            return Input.GetMouseButton(0);
        }
    }
}
