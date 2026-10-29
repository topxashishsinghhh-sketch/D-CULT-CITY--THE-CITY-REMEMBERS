using UnityEngine;

namespace DCultCity.Vehicles
{
    /// <summary>
    /// Mobile on-screen driving HUD.
    /// Provides a left-thumb steer/throttle split axis and right-side brake button.
    /// Connects to the active VehicleController via SetMobileThrottle/Steer/Brake.
    /// Only renders while a vehicle is occupied.
    /// </summary>
    public class VehicleMobileHUD : MonoBehaviour
    {
        private VehicleController activeVehicle;

        private GUIStyle buttonStyle;
        private GUIStyle speedStyle;
        private GUIStyle labelStyle;
        private bool     stylesInit = false;

        // Joystick state
        private bool    steerDragging  = false;
        private Vector2 steerCenter;
        private float   steerValue     = 0f;
        private float   throttleValue  = 0f;
        private bool    brakeHeld      = false;

        private void Update()
        {
            // Auto-find the currently occupied vehicle
            if (activeVehicle == null || !activeVehicle.IsOccupied)
            {
                activeVehicle = null;
                var all = FindObjectsOfType<VehicleController>();
                foreach (var v in all)
                    if (v.IsOccupied) { activeVehicle = v; break; }
            }
        }

        private void InitStyles()
        {
            if (stylesInit) return;
            float scale = Mathf.Max(1f, Screen.height / 720f);

            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize  = Mathf.RoundToInt(15 * scale),
                fontStyle = FontStyle.Bold
            };

            speedStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize  = Mathf.RoundToInt(22 * scale),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            speedStyle.normal.textColor = new Color(0.2f, 0.95f, 1f);

            labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize  = Mathf.RoundToInt(12 * scale),
                fontStyle = FontStyle.Italic,
                alignment = TextAnchor.MiddleCenter
            };
            labelStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f);

            stylesInit = true;
        }

        private void OnGUI()
        {
            if (activeVehicle == null || !activeVehicle.IsOccupied) return;
            InitStyles();

            float screenW = Screen.width;
            float screenH = Screen.height;
            float scale   = Mathf.Max(1f, screenH / 720f);

            // ---------- Speedometer (top-centre) ----------
            float spdW = 160f * scale;
            float spdH = 50f  * scale;
            GUI.Box(new Rect((screenW - spdW) * 0.5f, 12f * scale, spdW, spdH), GUIContent.none);
            GUI.Label(new Rect((screenW - spdW) * 0.5f, 14f * scale, spdW, spdH * 0.55f),
                      $"{activeVehicle.SpeedKmh:F0} km/h", speedStyle);
            GUI.Label(new Rect((screenW - spdW) * 0.5f, 14f * scale + spdH * 0.52f, spdW, spdH * 0.45f),
                      "[F] Exit Vehicle", labelStyle);

            // ---------- Left side: Steer axis + Auto-throttle ----------
            float joyR = 70f * scale;
            Vector2 joyDefault = new Vector2(joyR + 30f * scale, screenH - joyR - 30f * scale);
            if (!steerDragging) steerCenter = joyDefault;

            // Outer ring
            GUI.color = new Color(1f, 1f, 1f, 0.22f);
            GUI.Box(new Rect(steerCenter.x - joyR, steerCenter.y - joyR, joyR * 2f, joyR * 2f), "");

            // Handle
            float handleR = joyR * 0.42f;
            float handleX = steerCenter.x + steerValue * joyR - handleR;
            float handleY = steerCenter.y - handleR;
            GUI.color = new Color(1f, 0.85f, 0.2f, 0.85f);
            GUI.Box(new Rect(handleX, handleY, handleR * 2f, handleR * 2f), "");
            GUI.color = Color.white;

            // Throttle: upper-left virtual pad
            float tpadW = 90f * scale;
            float tpadH = 55f * scale;

            GUI.backgroundColor = new Color(0.3f, 0.9f, 0.4f, 0.85f);
            if (GUI.RepeatButton(new Rect(30f * scale, screenH - tpadH * 2f - joyR * 0.6f, tpadW, tpadH), "GAS▲", buttonStyle))
                throttleValue = 1f;
            else
                throttleValue = Mathf.Lerp(throttleValue, 0f, Time.deltaTime * 8f);

            GUI.backgroundColor = new Color(0.9f, 0.55f, 0.2f, 0.85f);
            if (GUI.RepeatButton(new Rect(30f * scale, screenH - tpadH - joyR * 0.3f, tpadW, tpadH), "REV▼", buttonStyle))
                throttleValue = -0.6f;

            // ---------- Right side: Brake / Handbrake ----------
            float btnSize = 75f * scale;
            GUI.backgroundColor = new Color(0.92f, 0.20f, 0.20f, 0.90f);
            brakeHeld = GUI.RepeatButton(
                new Rect(screenW - btnSize - 20f * scale, screenH - btnSize - 20f * scale, btnSize, btnSize),
                "BRAKE", buttonStyle);
            GUI.backgroundColor = Color.white;

            // ---------- Push values to vehicle ----------
            HandleSteerTouch(joyR);
            activeVehicle.SetMobileThrottle(throttleValue);
            activeVehicle.SetMobileSteer(steerValue);
            activeVehicle.SetMobileBrake(brakeHeld);
        }

        private void HandleSteerTouch(float joyR)
        {
            Vector2 ptr   = GetPointerGUIPos();
            bool    down  = GetPointerDown();

            if (down)
            {
                if (!steerDragging && Vector2.Distance(ptr, steerCenter) < joyR * 1.4f)
                    steerDragging = true;

                if (steerDragging)
                {
                    float rawDelta = ptr.x - steerCenter.x;
                    steerValue = Mathf.Clamp(rawDelta / joyR, -1f, 1f);
                }
            }
            else if (steerDragging)
            {
                steerDragging = false;
                steerValue    = Mathf.Lerp(steerValue, 0f, Time.deltaTime * 12f);
            }
        }

        private Vector2 GetPointerGUIPos()
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
