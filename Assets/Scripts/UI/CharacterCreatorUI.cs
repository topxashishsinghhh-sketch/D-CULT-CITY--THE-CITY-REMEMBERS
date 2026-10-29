using DCultCity.Core;
using DCultCity.Player;
using UnityEngine;

namespace DCultCity.UI
{
    /// <summary>
    /// Interactive Character Creator UI for Milestone 2.
    /// Allows customizing Gender, Skin Tone, Hair Style/Color, and Apparel.
    /// Operates both as standalone Character Select screen and in-game modal.
    /// </summary>
    public class CharacterCreatorUI : MonoBehaviour
    {
        [Header("State")]
        [SerializeField] private bool isOpen = false;

        [Header("Target Character")]
        [SerializeField] private ThirdPersonController playerController;

        private CharacterCustomizationData currentData = new CharacterCustomizationData();

        private GUIStyle windowStyle;
        private GUIStyle titleStyle;
        private GUIStyle sectionHeaderStyle;
        private GUIStyle buttonStyle;
        private GUIStyle labelStyle;
        private GUIStyle textFieldStyle;
        private bool stylesInit = false;

        public bool IsOpen => isOpen;

        private void Start()
        {
            if (playerController == null)
            {
                playerController = FindObjectOfType<ThirdPersonController>();
            }

            if (playerController != null && playerController.Visual != null)
            {
                currentData = playerController.Visual.Data.Clone();
            }
        }

        private void Update()
        {
            if (PlayerInputReader.Instance != null && PlayerInputReader.Instance.ConsumeCustomizeTrigger())
            {
                ToggleCreatorView();
            }
        }

        public void ToggleCreatorView()
        {
            isOpen = !isOpen;
            if (playerController != null)
            {
                // Disable locomotion while customizing
                playerController.enabled = !isOpen;
            }
        }

        private void InitStyles()
        {
            if (stylesInit) return;
            float scale = Mathf.Max(1.0f, Screen.height / 720f);

            windowStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(16, 16, 16, 16)
            };

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(20 * scale),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            titleStyle.normal.textColor = new Color(0.2f, 0.95f, 1f);

            sectionHeaderStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(13 * scale),
                fontStyle = FontStyle.Bold
            };
            sectionHeaderStyle.normal.textColor = new Color(1f, 0.85f, 0.4f);

            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = Mathf.RoundToInt(13 * scale),
                fontStyle = FontStyle.Bold
            };

            labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(13 * scale),
                alignment = TextAnchor.MiddleLeft
            };
            labelStyle.normal.textColor = Color.white;

            textFieldStyle = new GUIStyle(GUI.skin.textField)
            {
                fontSize = Mathf.RoundToInt(14 * scale)
            };

            stylesInit = true;
        }

        private void OnGUI()
        {
            if (!isOpen) return;
            InitStyles();

            float screenW = Screen.width;
            float screenH = Screen.height;
            float scale = Mathf.Max(1.0f, screenH / 720f);

            float panelW = Mathf.Min(400f * scale, screenW * 0.92f);
            float panelH = Mathf.Min(560f * scale, screenH * 0.94f);
            float panelX = (screenW - panelW) * 0.5f;
            float panelY = (screenH - panelH) * 0.5f;

            GUI.backgroundColor = new Color(0.12f, 0.14f, 0.18f, 0.96f);
            GUILayout.BeginArea(new Rect(panelX, panelY, panelW, panelH), windowStyle);

            GUILayout.Label("CHARACTER CREATOR", titleStyle);
            GUILayout.Label("<color=#AAAAAA>D-CULT CITY: Define Your Identity</color>", labelStyle);
            GUILayout.Space(10f * scale);

            // 1. Name Input
            GUILayout.Label("Citizen Name:", sectionHeaderStyle);
            currentData.characterName = GUILayout.TextField(currentData.characterName, 24, textFieldStyle);
            GUILayout.Space(8f * scale);

            // 2. Gender Selection
            GUILayout.Label("Body Type:", sectionHeaderStyle);
            GUILayout.BeginHorizontal();
            GUI.backgroundColor = currentData.gender == GenderType.Male ? new Color(0.2f, 0.6f, 1f) : Color.gray;
            if (GUILayout.Button("MALE", buttonStyle, GUILayout.Height(36f * scale)))
            {
                currentData.gender = GenderType.Male;
                ApplyPreview();
            }
            GUI.backgroundColor = currentData.gender == GenderType.Female ? new Color(1f, 0.4f, 0.7f) : Color.gray;
            if (GUILayout.Button("FEMALE", buttonStyle, GUILayout.Height(36f * scale)))
            {
                currentData.gender = GenderType.Female;
                ApplyPreview();
            }
            GUILayout.EndHorizontal();
            GUI.backgroundColor = Color.white;
            GUILayout.Space(8f * scale);

            // 3. Skin Tone Cycle
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Skin Tone ({currentData.skinToneIndex + 1}/{CharacterCustomizationData.SkinTones.Length}):", labelStyle);
            if (GUILayout.Button("<", buttonStyle, GUILayout.Width(45f * scale), GUILayout.Height(30f * scale)))
            {
                currentData.skinToneIndex = (currentData.skinToneIndex - 1 + CharacterCustomizationData.SkinTones.Length) % CharacterCustomizationData.SkinTones.Length;
                ApplyPreview();
            }
            if (GUILayout.Button(">", buttonStyle, GUILayout.Width(45f * scale), GUILayout.Height(30f * scale)))
            {
                currentData.skinToneIndex = (currentData.skinToneIndex + 1) % CharacterCustomizationData.SkinTones.Length;
                ApplyPreview();
            }
            GUILayout.EndHorizontal();

            // 4. Hair Style & Color
            string[] hairNames = { "Short Crop", "Mohawk", "Long/Volume" };
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Hair Style ({hairNames[currentData.hairStyleIndex]}):", labelStyle);
            if (GUILayout.Button("<", buttonStyle, GUILayout.Width(45f * scale), GUILayout.Height(30f * scale)))
            {
                currentData.hairStyleIndex = (currentData.hairStyleIndex - 1 + hairNames.Length) % hairNames.Length;
                ApplyPreview();
            }
            if (GUILayout.Button(">", buttonStyle, GUILayout.Width(45f * scale), GUILayout.Height(30f * scale)))
            {
                currentData.hairStyleIndex = (currentData.hairStyleIndex + 1) % hairNames.Length;
                ApplyPreview();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label($"Hair Color ({currentData.hairColorIndex + 1}/{CharacterCustomizationData.HairColors.Length}):", labelStyle);
            if (GUILayout.Button("<", buttonStyle, GUILayout.Width(45f * scale), GUILayout.Height(30f * scale)))
            {
                currentData.hairColorIndex = (currentData.hairColorIndex - 1 + CharacterCustomizationData.HairColors.Length) % CharacterCustomizationData.HairColors.Length;
                ApplyPreview();
            }
            if (GUILayout.Button(">", buttonStyle, GUILayout.Width(45f * scale), GUILayout.Height(30f * scale)))
            {
                currentData.hairColorIndex = (currentData.hairColorIndex + 1) % CharacterCustomizationData.HairColors.Length;
                ApplyPreview();
            }
            GUILayout.EndHorizontal();

            // 5. Outfit Colors
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Jacket/Top ({currentData.clothingColorIndex + 1}/{CharacterCustomizationData.ClothingColors.Length}):", labelStyle);
            if (GUILayout.Button("<", buttonStyle, GUILayout.Width(45f * scale), GUILayout.Height(30f * scale)))
            {
                currentData.clothingColorIndex = (currentData.clothingColorIndex - 1 + CharacterCustomizationData.ClothingColors.Length) % CharacterCustomizationData.ClothingColors.Length;
                ApplyPreview();
            }
            if (GUILayout.Button(">", buttonStyle, GUILayout.Width(45f * scale), GUILayout.Height(30f * scale)))
            {
                currentData.clothingColorIndex = (currentData.clothingColorIndex + 1) % CharacterCustomizationData.ClothingColors.Length;
                ApplyPreview();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label($"Pants ({currentData.pantsColorIndex + 1}/{CharacterCustomizationData.PantsColors.Length}):", labelStyle);
            if (GUILayout.Button("<", buttonStyle, GUILayout.Width(45f * scale), GUILayout.Height(30f * scale)))
            {
                currentData.pantsColorIndex = (currentData.pantsColorIndex - 1 + CharacterCustomizationData.PantsColors.Length) % CharacterCustomizationData.PantsColors.Length;
                ApplyPreview();
            }
            if (GUILayout.Button(">", buttonStyle, GUILayout.Width(45f * scale), GUILayout.Height(30f * scale)))
            {
                currentData.pantsColorIndex = (currentData.pantsColorIndex + 1) % CharacterCustomizationData.PantsColors.Length;
                ApplyPreview();
            }
            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();

            // Confirm Button
            GUI.backgroundColor = new Color(0.2f, 0.9f, 0.4f);
            if (GUILayout.Button("CONFIRM & ENTER CITY", buttonStyle, GUILayout.Height(46f * scale)))
            {
                ConfirmCustomization();
            }
            GUI.backgroundColor = Color.white;

            GUILayout.EndArea();
        }

        private void ApplyPreview()
        {
            if (playerController != null && playerController.Visual != null)
            {
                playerController.Visual.ApplyCustomization(currentData);
            }
        }

        private void ConfirmCustomization()
        {
            ApplyPreview();
            isOpen = false;

            if (playerController != null)
            {
                playerController.enabled = true;
            }

            // Transition game flow to InGame
            GameFlowManager.Instance?.TransitionTo(GameFlowState.InGame);

            // Publish to EventBus
            var evt = new CharacterCustomizedEvent(currentData);
            EventBus.Publish(evt);

            Debug.Log($"<color=#00FFCC>[CharacterCreatorUI]</color> Character confirmed: {evt}");
        }
    }
}
