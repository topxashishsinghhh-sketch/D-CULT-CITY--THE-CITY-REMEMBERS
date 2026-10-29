using DCultCity.NPC;
using DCultCity.Reputation;
using UnityEngine;

namespace DCultCity.UI
{
    /// <summary>
    /// Adds Milestone 4 diagnostics to the on-screen HUD:
    ///   • In-game clock (top-left corner)
    ///   • Faction reputation bars (Police, Civilians, Gangsters, Casino)
    ///   • Nearest Story NPC name + trust level
    ///   • Current wanted level
    /// Rendered via legacy OnGUI so no Canvas setup is needed.
    /// </summary>
    public class NPCDebugHUD : MonoBehaviour
    {
        private GUIStyle clockStyle;
        private GUIStyle barLabelStyle;
        private GUIStyle wantedStyle;
        private bool     stylesInit = false;

        private void InitStyles()
        {
            if (stylesInit) return;
            float scale = Mathf.Max(1f, Screen.height / 720f);

            clockStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize  = Mathf.RoundToInt(16 * scale),
                fontStyle = FontStyle.Bold,
                richText  = true
            };
            clockStyle.normal.textColor = new Color(0.8f, 0.95f, 1f);

            barLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize  = Mathf.RoundToInt(12 * scale),
                richText  = true
            };
            barLabelStyle.normal.textColor = Color.white;

            wantedStyle = new GUIStyle(GUI.skin.box)
            {
                fontSize  = Mathf.RoundToInt(14 * scale),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                richText  = true
            };
            wantedStyle.normal.textColor = new Color(1f, 0.4f, 0.2f);

            stylesInit = true;
        }

        private void OnGUI()
        {
            InitStyles();
            float scale  = Mathf.Max(1f, Screen.height / 720f);
            float panelW = 220f * scale;
            float x      = 15f * scale;
            float y      = 15f * scale;

            // ---- Clock ----
            if (GameClock.Instance != null)
            {
                GUI.Label(new Rect(x, y, panelW, 28f * scale),
                          $"🕐 <b>{GameClock.Instance.TimeString}</b>  D-CULT CITY", clockStyle);
                y += 32f * scale;
            }

            // ---- Faction Reputation bars ----
            if (FactionReputation.Instance != null)
            {
                DrawRepBar(ref x, ref y, scale, panelW, "Civilians", FactionReputation.Instance.Get("Civilians"), new Color(0.3f, 0.8f, 0.4f));
                DrawRepBar(ref x, ref y, scale, panelW, "Police",    FactionReputation.Instance.Get("Police"),    new Color(0.25f, 0.45f, 0.85f));
                DrawRepBar(ref x, ref y, scale, panelW, "Gangsters", FactionReputation.Instance.Get("Gangsters"), new Color(0.70f, 0.20f, 0.20f));
                DrawRepBar(ref x, ref y, scale, panelW, "Casino",    FactionReputation.Instance.Get("Casino"),    new Color(0.85f, 0.65f, 0.10f));

                // Wanted stars
                WantedLevel wanted = FactionReputation.Instance.GetWantedLevel();
                if (wanted != WantedLevel.None)
                {
                    int stars = (int)wanted;
                    string starStr = new string('★', stars) + new string('☆', 5 - stars);
                    GUI.backgroundColor = new Color(0.5f, 0.1f, 0.1f, 0.85f);
                    GUI.Box(new Rect(x, y + 4f * scale, panelW, 30f * scale),
                            $"WANTED  {starStr}", wantedStyle);
                    GUI.backgroundColor = Color.white;
                    y += 38f * scale;
                }
            }

            // ---- Nearest story NPC trust ----
            var allNPCs = FindObjectsOfType<StoryNPC>();
            var cam     = Camera.main;
            if (cam != null && allNPCs.Length > 0)
            {
                StoryNPC nearest = null;
                float    minDist = float.MaxValue;
                foreach (var n in allNPCs)
                {
                    float d = Vector3.Distance(cam.transform.position, n.transform.position);
                    if (d < minDist) { minDist = d; nearest = n; }
                }

                if (nearest != null && minDist < 25f)
                {
                    y += 8f * scale;
                    string trustColor = nearest.TrustTracker.Level switch
                    {
                        TrustLevel.Trusted    => "#00FF88",
                        TrustLevel.Suspicious => "#FFAA00",
                        TrustLevel.Hostile    => "#FF4444",
                        _                     => "#FFFFFF"
                    };
                    GUI.Label(new Rect(x, y, panelW + 60f * scale, 24f * scale),
                              $"Nearest NPC: <b>{nearest.displayName}</b>  " +
                              $"<color={trustColor}>{nearest.TrustTracker.ToDisplayString()}</color>" +
                              $"  Memories: {nearest.Memory.Entries.Count}",
                              barLabelStyle);
                }
            }
        }

        private void DrawRepBar(ref float x, ref float y, float scale, float panelW,
                                string label, float value, Color barColor)
        {
            float barH   = 16f * scale;
            float labelW = 75f * scale;
            float barW   = panelW - labelW;

            GUI.Label(new Rect(x, y, labelW, barH), label, barLabelStyle);

            // Background
            GUI.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.7f);
            GUI.Box(new Rect(x + labelW, y + 2f, barW, barH - 4f), "");

            // Filled portion — value is -1 to +1, map to 0–barW
            float normalised = (value + 1f) * 0.5f;
            GUI.backgroundColor = barColor;
            float filledW = normalised * barW;
            if (filledW > 1f)
                GUI.Box(new Rect(x + labelW, y + 2f, filledW, barH - 4f), "");

            // Value label
            GUI.backgroundColor = Color.clear;
            GUI.Label(new Rect(x + labelW + barW + 4f, y, 50f * scale, barH),
                      $"{value:+0.00;-0.00;0}", barLabelStyle);
            GUI.backgroundColor = Color.white;

            y += barH + 2f * scale;
        }
    }
}
