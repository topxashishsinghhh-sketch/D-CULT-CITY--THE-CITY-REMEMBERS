using DCultCity.Core;
using UnityEngine;

namespace DCultCity.UI
{
    /// <summary>
    /// Interactive on-screen test harness for Milestone 1.
    /// Provides large, responsive touch buttons for mobile testing and full desktop support.
    /// Displays real-time EventBus logs directly on screen so testers can verify functionality
    /// without relying solely on USB logcat / device console.
    /// </summary>
    public class Milestone1HUD : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private EventBusTestPublisher publisher;
        [SerializeField] private EventBusTestListener listener;

        private Vector2 scrollPos;
        private GUIStyle titleStyle;
        private GUIStyle buttonStyle;
        private GUIStyle logStyle;
        private GUIStyle boxStyle;
        private GUIStyle hintStyle;
        private bool stylesInitialized = false;

        private void Awake()
        {
            if (publisher == null) publisher = FindObjectOfType<EventBusTestPublisher>();
            if (listener == null) listener = FindObjectOfType<EventBusTestListener>();
        }

        private void InitStyles()
        {
            if (stylesInitialized) return;

            // Responsive scaling factor based on screen height (phone vs desktop)
            float scale = Mathf.Max(1.0f, Screen.height / 720f);

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(18 * scale),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
            titleStyle.normal.textColor = new Color(0f, 0.95f, 1f);

            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = Mathf.RoundToInt(14 * scale),
                fontStyle = FontStyle.Bold
            };

            logStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(12 * scale),
                wordWrap = true,
                richText = true
            };
            logStyle.normal.textColor = Color.white;

            boxStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(10, 10, 10, 10)
            };

            hintStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(11 * scale),
                fontStyle = FontStyle.Italic
            };
            hintStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f);

            stylesInitialized = true;
        }

        private void OnGUI()
        {
            InitStyles();

            float screenW = Screen.width;
            float screenH = Screen.height;
            float scale = Mathf.Max(1.0f, screenH / 720f);

            // Left Panel: Action Buttons
            float panelW = Mathf.Min(340f * scale, screenW * 0.42f);
            float btnH = 44f * scale;
            float padding = 8f * scale;

            GUILayout.BeginArea(new Rect(15f, 15f, panelW, screenH - 30f), boxStyle);

            GUILayout.Label("D-CULT CITY [M1]", titleStyle);
            GUILayout.Label("The City That Remembers", hintStyle);
            GUILayout.Space(10f * scale);

            GUILayout.Label($"<b>EventBus Active:</b> {(listener != null ? "Connected" : "Standby")}", logStyle);
            GUILayout.Label($"<b>Events Logged:</b> {(listener != null ? listener.TotalEventsReceived : 0)}", logStyle);
            GUILayout.Space(12f * scale);

            GUILayout.Label("<b>TEST ACTIONS (Publish):</b>", logStyle);

            GUI.backgroundColor = new Color(0.3f, 0.85f, 0.4f);
            if (GUILayout.Button("[1] Helped NPC (+Trust)", buttonStyle, GUILayout.Height(btnH)))
            {
                publisher?.TriggerHelpedNPC();
            }

            GUI.backgroundColor = new Color(0.9f, 0.5f, 0.2f);
            if (GUILayout.Button("[2] Stole Car (-Rep)", buttonStyle, GUILayout.Height(btnH)))
            {
                publisher?.TriggerStoleCar();
            }

            GUI.backgroundColor = new Color(1f, 0.85f, 0.2f);
            if (GUILayout.Button("[3] Won Casino (+Cash)", buttonStyle, GUILayout.Height(btnH)))
            {
                publisher?.TriggerWonCasino();
            }

            GUI.backgroundColor = new Color(0.95f, 0.25f, 0.25f);
            if (GUILayout.Button("[4] Hit Pedestrian (-Trust)", buttonStyle, GUILayout.Height(btnH)))
            {
                publisher?.TriggerHitPedestrian();
            }

            GUI.backgroundColor = Color.white;
            GUILayout.Space(10f * scale);

            if (GUILayout.Button("Clear Event History", buttonStyle, GUILayout.Height(32f * scale)))
            {
                listener?.ClearHistory();
            }

            GUILayout.FlexibleSpace();
            GUILayout.Label("<b>Controls:</b>\n• Desktop: WASD + Right Mouse Look, [1-4] Keys\n• Mobile: Tap buttons, Drag to rotate city, Pinch zoom", hintStyle);

            GUILayout.EndArea();

            // Right Panel: Live EventBus Log Viewer
            float logPanelW = Mathf.Min(480f * scale, screenW - panelW - 40f);
            float logPanelX = screenW - logPanelW - 15f;
            float logPanelH = Mathf.Min(320f * scale, screenH * 0.48f);

            GUILayout.BeginArea(new Rect(logPanelX, 15f, logPanelW, logPanelH), boxStyle);
            GUILayout.Label("<b>LIVE EVENTBUS STREAM:</b>", titleStyle);
            GUILayout.Space(5f);

            scrollPos = GUILayout.BeginScrollView(scrollPos);

            if (listener != null && listener.EventLogHistory.Count > 0)
            {
                foreach (string entry in listener.EventLogHistory)
                {
                    GUILayout.Label(entry, logStyle);
                    GUILayout.Space(2f);
                }
            }
            else
            {
                GUILayout.Label("<color=#888888><i>Waiting for events... Press an action button on the left or keys 1-4.</i></color>", logStyle);
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
    }
}
