using DCultCity.Core;
using UnityEngine;

namespace DCultCity.UI
{
    /// <summary>
    /// Renders the complete menu flow screens:
    /// - Splash: Atmospheric title screen
    /// - Login: Guest or Email sign-in options
    /// - Main Menu: New Game, Continue, Settings, Quit
    /// - Settings: Low / Medium / High graphic presets & volume
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        private GameFlowManager flow;

        // Login inputs
        private string emailInput = "player@dcultcity.com";
        private string passwordInput = "••••••••";
        private float volumeSlider = 0.85f;

        private GUIStyle titleStyle;
        private GUIStyle subtitleStyle;
        private GUIStyle buttonStyle;
        private GUIStyle panelStyle;
        private GUIStyle labelStyle;
        private GUIStyle inputStyle;
        private bool stylesInit = false;

        private void Start()
        {
            flow = GameFlowManager.Instance;
            if (flow == null) flow = FindObjectOfType<GameFlowManager>();
        }

        private void InitStyles()
        {
            if (stylesInit) return;
            float sc = Mathf.Max(1f, Screen.height / 720f);

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(34 * sc),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            titleStyle.normal.textColor = new Color(0.2f, 0.95f, 1f);

            subtitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(15 * sc),
                fontStyle = FontStyle.Italic,
                alignment = TextAnchor.MiddleCenter
            };
            subtitleStyle.normal.textColor = new Color(1f, 0.85f, 0.4f);

            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = Mathf.RoundToInt(15 * sc),
                fontStyle = FontStyle.Bold
            };

            panelStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(20, 20, 20, 20)
            };

            labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(13 * sc),
                alignment = TextAnchor.MiddleLeft
            };
            labelStyle.normal.textColor = Color.white;

            inputStyle = new GUIStyle(GUI.skin.textField)
            {
                fontSize = Mathf.RoundToInt(14 * sc)
            };

            stylesInit = true;
        }

        private void OnGUI()
        {
            if (flow == null) { flow = GameFlowManager.Instance; return; }
            if (flow.CurrentState == GameFlowState.InGame || flow.CurrentState == GameFlowState.CharacterSelect)
            {
                DrawInGameMenuButton();
                return;
            }

            InitStyles();
            float sc = Mathf.Max(1f, Screen.height / 720f);
            float screenW = Screen.width;
            float screenH = Screen.height;

            switch (flow.CurrentState)
            {
                case GameFlowState.Splash:
                    DrawSplashScreen(sc, screenW, screenH);
                    break;
                case GameFlowState.Login:
                    DrawLoginScreen(sc, screenW, screenH);
                    break;
                case GameFlowState.MainMenu:
                    DrawMainMenuScreen(sc, screenW, screenH);
                    break;
                case GameFlowState.Settings:
                    DrawSettingsScreen(sc, screenW, screenH);
                    break;
            }
        }

        private void DrawInGameMenuButton()
        {
            // Small toggle in top bar to return to Main Menu for testing
            float sc = Mathf.Max(1f, Screen.height / 720f);
            float w = 110f * sc;
            float h = 32f * sc;
            float x = Screen.width * 0.5f - w * 0.5f - 100f * sc;

            GUI.backgroundColor = new Color(0.18f, 0.22f, 0.28f, 0.9f);
            if (GUI.Button(new Rect(x, 8f * sc, w, h), "TITLE MENU"))
            {
                flow?.TransitionTo(GameFlowState.Splash);
            }
            GUI.backgroundColor = Color.white;
        }

        private void DrawSplashScreen(float sc, float screenW, float screenH)
        {
            float panelW = Mathf.Min(500f * sc, screenW * 0.9f);
            float panelH = Mathf.Min(320f * sc, screenH * 0.7f);
            float panelX = (screenW - panelW) * 0.5f;
            float panelY = (screenH - panelH) * 0.5f;

            GUI.backgroundColor = new Color(0.06f, 0.08f, 0.12f, 0.96f);
            GUILayout.BeginArea(new Rect(panelX, panelY, panelW, panelH), panelStyle);

            GUILayout.FlexibleSpace();
            GUILayout.Label("D-CULT CITY", titleStyle);
            GUILayout.Label("The City That Remembers", subtitleStyle);
            GUILayout.Space(25f * sc);

            GUI.backgroundColor = new Color(0.2f, 0.85f, 0.45f);
            if (GUILayout.Button("PRESS START / ENTER", buttonStyle, GUILayout.Height(50f * sc)))
            {
                flow?.TransitionTo(GameFlowState.Login);
            }
            GUI.backgroundColor = Color.white;

            GUILayout.FlexibleSpace();
            GUILayout.EndArea();
        }

        private void DrawLoginScreen(float sc, float screenW, float screenH)
        {
            float panelW = Mathf.Min(460f * sc, screenW * 0.92f);
            float panelH = Mathf.Min(400f * sc, screenH * 0.85f);
            float panelX = (screenW - panelW) * 0.5f;
            float panelY = (screenH - panelH) * 0.5f;

            GUI.backgroundColor = new Color(0.08f, 0.10f, 0.14f, 0.96f);
            GUILayout.BeginArea(new Rect(panelX, panelY, panelW, panelH), panelStyle);

            GUILayout.Label("CITIZEN LOGIN", titleStyle);
            GUILayout.Space(12f * sc);

            GUILayout.Label("Email Address:", labelStyle);
            emailInput = GUILayout.TextField(emailInput, 40, inputStyle);
            GUILayout.Space(8f * sc);

            GUILayout.Label("Password:", labelStyle);
            passwordInput = GUILayout.PasswordField(passwordInput, '*', 32, inputStyle);
            GUILayout.Space(16f * sc);

            GUI.backgroundColor = new Color(0.25f, 0.65f, 1f);
            if (GUILayout.Button("LOGIN WITH EMAIL", buttonStyle, GUILayout.Height(42f * sc)))
            {
                if (flow != null)
                {
                    flow.loggedInUser = emailInput;
                    flow.isGuest = false;
                    flow.TransitionTo(GameFlowState.MainMenu);
                }
            }

            GUI.backgroundColor = new Color(0.45f, 0.50f, 0.58f);
            if (GUILayout.Button("PLAY AS GUEST", buttonStyle, GUILayout.Height(38f * sc)))
            {
                if (flow != null)
                {
                    flow.loggedInUser = "Guest_Citizen";
                    flow.isGuest = true;
                    flow.TransitionTo(GameFlowState.MainMenu);
                }
            }
            GUI.backgroundColor = Color.white;

            GUILayout.EndArea();
        }

        private void DrawMainMenuScreen(float sc, float screenW, float screenH)
        {
            float panelW = Mathf.Min(440f * sc, screenW * 0.9f);
            float panelH = Mathf.Min(450f * sc, screenH * 0.88f);
            float panelX = (screenW - panelW) * 0.5f;
            float panelY = (screenH - panelH) * 0.5f;

            GUI.backgroundColor = new Color(0.08f, 0.10f, 0.14f, 0.96f);
            GUILayout.BeginArea(new Rect(panelX, panelY, panelW, panelH), panelStyle);

            GUILayout.Label("D-CULT CITY", titleStyle);
            GUILayout.Label($"Citizen: <color=#00FFAA>{flow?.loggedInUser}</color>", subtitleStyle);
            GUILayout.Space(16f * sc);

            GUI.backgroundColor = new Color(0.25f, 0.85f, 0.4f);
            if (GUILayout.Button("NEW GAME", buttonStyle, GUILayout.Height(45f * sc)))
            {
                flow?.StartNewGame();
            }

            GUI.backgroundColor = new Color(0.2f, 0.6f, 1f);
            if (GUILayout.Button("CONTINUE", buttonStyle, GUILayout.Height(45f * sc)))
            {
                flow?.ContinueGame();
            }

            GUI.backgroundColor = new Color(0.85f, 0.70f, 0.2f);
            if (GUILayout.Button("SETTINGS", buttonStyle, GUILayout.Height(45f * sc)))
            {
                flow?.TransitionTo(GameFlowState.Settings);
            }

            GUI.backgroundColor = new Color(0.85f, 0.25f, 0.25f);
            if (GUILayout.Button("QUIT", buttonStyle, GUILayout.Height(40f * sc)))
            {
                Application.Quit();
            }
            GUI.backgroundColor = Color.white;

            GUILayout.EndArea();
        }

        private void DrawSettingsScreen(float sc, float screenW, float screenH)
        {
            float panelW = Mathf.Min(480f * sc, screenW * 0.92f);
            float panelH = Mathf.Min(450f * sc, screenH * 0.88f);
            float panelX = (screenW - panelW) * 0.5f;
            float panelY = (screenH - panelH) * 0.5f;

            GUI.backgroundColor = new Color(0.08f, 0.10f, 0.14f, 0.96f);
            GUILayout.BeginArea(new Rect(panelX, panelY, panelW, panelH), panelStyle);

            GUILayout.Label("SETTINGS", titleStyle);
            GUILayout.Space(14f * sc);

            // Graphic Quality Preset
            GUILayout.Label("Graphics Quality Preset:", labelStyle);
            GUILayout.BeginHorizontal();

            var qsm = QualitySettingsManager.Instance;
            GameQualityPreset current = qsm != null ? qsm.CurrentPreset : GameQualityPreset.Medium;

            GUI.backgroundColor = current == GameQualityPreset.Low ? new Color(0.2f, 0.8f, 0.4f) : Color.gray;
            if (GUILayout.Button("LOW (30 FPS)", buttonStyle, GUILayout.Height(36f * sc)))
            {
                qsm?.ApplyPreset(GameQualityPreset.Low);
            }

            GUI.backgroundColor = current == GameQualityPreset.Medium ? new Color(0.2f, 0.8f, 0.4f) : Color.gray;
            if (GUILayout.Button("MEDIUM (60 FPS)", buttonStyle, GUILayout.Height(36f * sc)))
            {
                qsm?.ApplyPreset(GameQualityPreset.Medium);
            }

            GUI.backgroundColor = current == GameQualityPreset.High ? new Color(0.2f, 0.8f, 0.4f) : Color.gray;
            if (GUILayout.Button("HIGH (120 FPS)", buttonStyle, GUILayout.Height(36f * sc)))
            {
                qsm?.ApplyPreset(GameQualityPreset.High);
            }
            GUILayout.EndHorizontal();
            GUI.backgroundColor = Color.white;

            GUILayout.Space(15f * sc);

            // Master Volume
            GUILayout.Label($"Master Audio Volume: {Mathf.RoundToInt(volumeSlider * 100)}%", labelStyle);
            volumeSlider = GUILayout.HorizontalSlider(volumeSlider, 0f, 1f);
            AudioListener.volume = volumeSlider;

            GUILayout.FlexibleSpace();

            GUI.backgroundColor = new Color(0.35f, 0.45f, 0.6f);
            if (GUILayout.Button("BACK TO MAIN MENU", buttonStyle, GUILayout.Height(42f * sc)))
            {
                flow?.TransitionTo(GameFlowState.MainMenu);
            }
            GUI.backgroundColor = Color.white;

            GUILayout.EndArea();
        }
    }
}
