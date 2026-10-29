using DCultCity.SaveLoad;
using UnityEngine;

namespace DCultCity.UI
{
    /// <summary>
    /// In-game Save / Load UI with 3 slot support.
    /// • Opens via [Escape] on desktop or the MENU button on mobile.
    /// • Shows slot metadata (save date, in-game time, NPC memory count).
    /// • One-tap save, load, and delete per slot.
    /// • Displays auto-save countdown and last-save feedback toast.
    /// Rendered via legacy OnGUI — no Canvas or prefab required.
    /// </summary>
    public class SaveLoadUI : MonoBehaviour
    {
        [Header("Keybindings")]
        [SerializeField] private KeyCode toggleKey = KeyCode.Escape;

        private bool isOpen = false;

        // Toast notification state
        private string toastMessage = "";
        private float  toastTimer   = 0f;
        private Color  toastColor   = Color.green;

        private SaveLoadManager slm;

        // GUI styles
        private GUIStyle windowStyle;
        private GUIStyle titleStyle;
        private GUIStyle slotBoxStyle;
        private GUIStyle buttonStyle;
        private GUIStyle dangerButtonStyle;
        private GUIStyle labelStyle;
        private GUIStyle metaStyle;
        private GUIStyle toastStyle;
        private bool     stylesInit = false;

        // ------------------------------------------------------------------ //
        //  Lifecycle
        // ------------------------------------------------------------------ //
        private void Start()
        {
            slm = SaveLoadManager.Instance;
            if (slm != null)
            {
                slm.OnSaveCompleted += OnSaved;
                slm.OnLoadCompleted += OnLoaded;
            }
        }

        private void OnDestroy()
        {
            if (slm != null)
            {
                slm.OnSaveCompleted -= OnSaved;
                slm.OnLoadCompleted -= OnLoaded;
            }
        }

        private void OnSaved(int slot)  => ShowToast($"✓ Game saved to slot {slot + 1}!", Color.green);
        private void OnLoaded(GameSaveData d) => ShowToast($"✓ Loaded — {d.saveDateTime}", Color.cyan);

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey)) isOpen = !isOpen;
            if (toastTimer > 0f) toastTimer -= Time.deltaTime;
        }

        public void ToggleMenu() => isOpen = !isOpen;

        // ------------------------------------------------------------------ //
        //  Style initialisation
        // ------------------------------------------------------------------ //
        private void InitStyles()
        {
            if (stylesInit) return;
            float sc = Mathf.Max(1f, Screen.height / 720f);

            windowStyle = new GUIStyle(GUI.skin.box) { padding = new RectOffset(16, 16, 14, 14) };

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(20 * sc), fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter, richText = true
            };
            titleStyle.normal.textColor = new Color(0.2f, 0.95f, 1f);

            slotBoxStyle = new GUIStyle(GUI.skin.box) { padding = new RectOffset(10, 10, 8, 8) };

            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = Mathf.RoundToInt(13 * sc), fontStyle = FontStyle.Bold
            };

            dangerButtonStyle = new GUIStyle(buttonStyle);

            labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(13 * sc), richText = true
            };
            labelStyle.normal.textColor = Color.white;

            metaStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(11 * sc), fontStyle = FontStyle.Italic, richText = true
            };
            metaStyle.normal.textColor = new Color(0.7f, 0.7f, 0.75f);

            toastStyle = new GUIStyle(GUI.skin.box)
            {
                fontSize = Mathf.RoundToInt(14 * sc), fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter, richText = true
            };
            toastStyle.normal.textColor = Color.white;

            stylesInit = true;
        }

        // ------------------------------------------------------------------ //
        //  OnGUI
        // ------------------------------------------------------------------ //
        private void OnGUI()
        {
            InitStyles();
            float sc      = Mathf.Max(1f, Screen.height / 720f);
            float screenW = Screen.width;
            float screenH = Screen.height;

            // ---- Toast notification ----
            if (toastTimer > 0f)
            {
                float tw = 360f * sc;
                float th = 40f  * sc;
                GUI.backgroundColor = new Color(toastColor.r * 0.3f, toastColor.g * 0.3f, toastColor.b * 0.3f, 0.92f);
                toastStyle.normal.textColor = toastColor;
                GUI.Box(new Rect((screenW - tw) * 0.5f, screenH * 0.88f, tw, th), toastMessage, toastStyle);
                GUI.backgroundColor = Color.white;
            }

            // ---- Mobile MENU button (always visible, top-left) ----
            float mBtnW = 80f * sc;
            float mBtnH = 34f * sc;
            GUI.backgroundColor = new Color(0.2f, 0.2f, 0.25f, 0.9f);
            if (GUI.Button(new Rect(screenW * 0.5f - mBtnW * 0.5f, 8f * sc, mBtnW, mBtnH), "MENU", buttonStyle))
                isOpen = !isOpen;
            GUI.backgroundColor = Color.white;

            if (!isOpen) return;

            // ---- Main Panel ----
            float panelW = Mathf.Min(520f * sc, screenW * 0.94f);
            float panelH = Mathf.Min(460f * sc, screenH * 0.92f);
            float panelX = (screenW - panelW) * 0.5f;
            float panelY = (screenH - panelH) * 0.5f;

            GUI.backgroundColor = new Color(0.08f, 0.10f, 0.13f, 0.97f);
            GUILayout.BeginArea(new Rect(panelX, panelY, panelW, panelH), windowStyle);

            GUILayout.Label("D-CULT CITY  —  SAVE / LOAD", titleStyle);
            GUILayout.Space(8f * sc);

            // Show save folder path for debugging
            if (slm != null)
            {
                GUILayout.Label($"<color=#555566>Save folder: {slm.SaveFolder}</color>", metaStyle);
                GUILayout.Space(6f * sc);
            }

            // ---- Slot rows ----
            if (slm != null)
            {
                for (int i = 0; i < slm.TotalSlots; i++)
                {
                    DrawSlotRow(i, sc);
                    GUILayout.Space(6f * sc);
                }
            }

            GUILayout.FlexibleSpace();

            // ---- Resume / Close ----
            GUI.backgroundColor = new Color(0.2f, 0.5f, 0.25f, 1f);
            if (GUILayout.Button("RESUME GAME", buttonStyle, GUILayout.Height(42f * sc)))
                isOpen = false;

            GUI.backgroundColor = Color.white;
            GUILayout.EndArea();
        }

        // ------------------------------------------------------------------ //
        //  Slot row
        // ------------------------------------------------------------------ //
        private void DrawSlotRow(int slot, float sc)
        {
            bool   exists = slm != null && slm.SlotExists(slot);
            bool   active = slm != null && slm.ActiveSlot == slot;
            string header = $"<b>SLOT {slot + 1}</b>{(active ? " <color=#FFDD44>◄ ACTIVE</color>" : "")}";

            GUI.backgroundColor = active
                ? new Color(0.14f, 0.18f, 0.26f)
                : new Color(0.10f, 0.12f, 0.16f);

            GUILayout.BeginVertical(slotBoxStyle);
            GUILayout.Label(header, labelStyle);

            // Slot metadata
            if (exists && slm != null)
            {
                var peek = slm.PeekSlot(slot);
                if (peek != null)
                {
                    int memCount = 0;
                    foreach (var n in peek.npcs) memCount += n.memories.Count;
                    string clockStr = $"{Mathf.FloorToInt(peek.inGameHour):00}:{Mathf.FloorToInt((peek.inGameHour % 1f) * 60f):00}";
                    GUILayout.Label($"Saved: {peek.saveDateTime}  •  In-game: {clockStr}  •  NPC memories: {memCount}", metaStyle);
                    GUILayout.Label($"Character: <color=#AADDFF>{peek.player.characterName}</color>  " +
                                    $"Pos: ({peek.player.posX:F0}, {peek.player.posZ:F0})  " +
                                    $"NPCs captured: {peek.npcs.Count}", metaStyle);
                }
            }
            else
            {
                GUILayout.Label("<color=#444455><i>— Empty Slot —</i></color>", metaStyle);
            }

            GUILayout.Space(4f * sc);
            GUILayout.BeginHorizontal();

            // SAVE button
            GUI.backgroundColor = new Color(0.20f, 0.65f, 0.30f);
            if (GUILayout.Button("SAVE", buttonStyle, GUILayout.Height(34f * sc)))
            {
                slm?.Save(slot);
            }

            // LOAD button (greyed if slot empty)
            GUI.backgroundColor = exists ? new Color(0.20f, 0.45f, 0.75f) : new Color(0.25f, 0.25f, 0.30f);
            GUI.enabled = exists;
            if (GUILayout.Button("LOAD", buttonStyle, GUILayout.Height(34f * sc)))
            {
                slm?.Load(slot);
                isOpen = false;
            }
            GUI.enabled = true;

            // DELETE button
            GUI.backgroundColor = exists ? new Color(0.65f, 0.15f, 0.15f) : new Color(0.25f, 0.20f, 0.20f);
            GUI.enabled = exists;
            if (GUILayout.Button("DELETE", dangerButtonStyle, GUILayout.Height(34f * sc)))
            {
                slm?.DeleteSlot(slot);
                ShowToast($"Slot {slot + 1} deleted.", Color.yellow);
            }
            GUI.enabled = true;
            GUI.backgroundColor = Color.white;

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        // ------------------------------------------------------------------ //
        //  Toast helper
        // ------------------------------------------------------------------ //
        private void ShowToast(string message, Color color)
        {
            toastMessage = message;
            toastColor   = color;
            toastTimer   = 3.5f;
        }
    }
}
