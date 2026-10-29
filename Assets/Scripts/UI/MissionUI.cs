using DCultCity.Reputation;
using UnityEngine;

namespace DCultCity.UI
{
    /// <summary>
    /// Always-on mission HUD: shows active mission title, current objective,
    /// timer (for timed missions), and a compact mission log of completed missions.
    /// Also shows available missions and lets the player start them.
    /// </summary>
    public class MissionUI : MonoBehaviour
    {
        private bool       missionLogOpen  = false;
        private GUIStyle   titleStyle;
        private GUIStyle   objStyle;
        private GUIStyle   buttonStyle;
        private GUIStyle   factionStyle;
        private GUIStyle   headerStyle;
        private GUIStyle   wantedStyle;
        private bool       stylesInit = false;
        private Vector2    logScrollPos;

        private Missions.MissionManager mm;

        private void Start() => mm = Missions.MissionManager.Instance;

        private void InitStyles()
        {
            if (stylesInit) return;
            float sc = Mathf.Max(1f, Screen.height / 720f);

            titleStyle = new GUIStyle(GUI.skin.box)
            {
                fontSize  = Mathf.RoundToInt(14 * sc), fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft, richText = true,
                padding   = new RectOffset(10, 10, 6, 6)
            };
            titleStyle.normal.textColor = new Color(1f, 0.9f, 0.3f);

            objStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize  = Mathf.RoundToInt(12 * sc), wordWrap = true, richText = true
            };
            objStyle.normal.textColor = Color.white;

            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize  = Mathf.RoundToInt(12 * sc), fontStyle = FontStyle.Bold
            };

            factionStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize  = Mathf.RoundToInt(11 * sc), richText = true
            };
            factionStyle.normal.textColor = new Color(0.75f, 0.75f, 0.80f);

            headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize  = Mathf.RoundToInt(14 * sc), fontStyle = FontStyle.Bold, richText = true
            };
            headerStyle.normal.textColor = new Color(0.2f, 0.95f, 1f);

            wantedStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize  = Mathf.RoundToInt(13 * sc), fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleRight, richText = true
            };
            wantedStyle.normal.textColor = new Color(1f, 0.4f, 0.2f);

            stylesInit = true;
        }

        private void OnGUI()
        {
            if (mm == null) { mm = Missions.MissionManager.Instance; return; }
            InitStyles();

            float sc      = Mathf.Max(1f, Screen.height / 720f);
            float screenW = Screen.width;
            float screenH = Screen.height;

            // ---- Active Mission Tracker (top-right) ----
            DrawActiveMission(sc, screenW);

            // ---- Wanted Stars (top-right below mission) ----
            DrawWantedLevel(sc, screenW);

            // ---- Mission Log toggle button ----
            float btnW = 130f * sc;
            float btnH = 32f  * sc;
            GUI.backgroundColor = new Color(0.15f, 0.18f, 0.22f, 0.92f);
            if (GUI.Button(new Rect(screenW - btnW - 12f * sc, screenH - btnH - 12f * sc, btnW, btnH),
                           missionLogOpen ? "◀ Close Log" : "Missions ▶", buttonStyle))
                missionLogOpen = !missionLogOpen;

            if (missionLogOpen)
                DrawMissionLog(sc, screenW, screenH);

            GUI.backgroundColor = Color.white;
        }

        // ------------------------------------------------------------------ //
        private void DrawActiveMission(float sc, float screenW)
        {
            var active = mm?.ActiveMission;
            if (active == null) return;

            float panelW = Mathf.Min(320f * sc, screenW * 0.38f);
            float panelX = screenW - panelW - 12f * sc;
            float panelY = 52f * sc;

            GUI.backgroundColor = new Color(0.08f, 0.10f, 0.14f, 0.92f);
            GUILayout.BeginArea(new Rect(panelX, panelY, panelW, 200f * sc), titleStyle);

            string factionColor = active.Faction switch
            {
                Missions.MissionFaction.Police   => "#4488FF",
                Missions.MissionFaction.Gangster => "#FF4444",
                Missions.MissionFaction.Casino   => "#FFDD44",
                Missions.MissionFaction.Wildlife => "#44FF88",
                Missions.MissionFaction.Racing   => "#FF8822",
                _                               => "#FFFFFF"
            };
            GUILayout.Label($"<color={factionColor}>▶ {active.Title}</color>", titleStyle);

            // Current objective
            if (active is Missions.MissionBase mb && mb.CurrentObjectiveIndex < mb.Objectives.Count)
            {
                var obj = mb.Objectives[mb.CurrentObjectiveIndex];
                GUILayout.Label($"  → {obj.description}", objStyle);
            }

            // Timer display for timed missions
            if (active is Missions.SideQuest_StreetRace race)
                GUILayout.Label($"  ⏱ {race.GetTimerDisplay()}", objStyle);
            if (active is Missions.SideQuest_Delivery delivery && !string.IsNullOrEmpty(delivery.TimerDisplay))
                GUILayout.Label($"  ⏱ {delivery.TimerDisplay}", objStyle);

            GUI.backgroundColor = Color.white;
            GUILayout.EndArea();
        }

        private void DrawWantedLevel(float sc, float screenW)
        {
            if (FactionReputation.Instance == null) return;
            var wanted = FactionReputation.Instance.GetWantedLevel();
            if (wanted == WantedLevel.None) return;

            int stars = (int)wanted;
            string starStr = "<color=#FF4422>" + new string('★', stars) + "</color>" + new string('☆', 5 - stars);
            float pw = 160f * sc;
            GUI.Label(new Rect(screenW - pw - 14f * sc, 14f * sc, pw, 28f * sc),
                      $"WANTED {starStr}", wantedStyle);
        }

        private void DrawMissionLog(float sc, float screenW, float screenH)
        {
            float logW = Mathf.Min(420f * sc, screenW * 0.9f);
            float logH = Mathf.Min(500f * sc, screenH * 0.85f);
            float logX = screenW - logW - 12f * sc;
            float logY = screenH - logH - 55f * sc;

            GUI.backgroundColor = new Color(0.07f, 0.09f, 0.12f, 0.97f);
            GUILayout.BeginArea(new Rect(logX, logY, logW, logH), titleStyle);
            GUILayout.Label("MISSION JOURNAL", headerStyle);
            GUILayout.Space(6f * sc);

            logScrollPos = GUILayout.BeginScrollView(logScrollPos);

            foreach (Missions.MissionFaction faction in System.Enum.GetValues(typeof(Missions.MissionFaction)))
            {
                var list = mm.GetByFaction(faction);
                if (list.Count == 0) continue;

                GUILayout.Label($"── {faction} ──", factionStyle);

                foreach (var m in list)
                {
                    string statusColor = m.Status switch
                    {
                        Missions.MissionStatus.Completed  => "#00FF88",
                        Missions.MissionStatus.Failed     => "#FF4444",
                        Missions.MissionStatus.Active     => "#FFDD44",
                        Missions.MissionStatus.Available  => "#AAAAFF",
                        _                                 => "#555555"
                    };
                    string statusLabel = m.Status switch
                    {
                        Missions.MissionStatus.Completed => "✓ DONE",
                        Missions.MissionStatus.Failed    => "✗ FAIL",
                        Missions.MissionStatus.Active    => "● ACTIVE",
                        Missions.MissionStatus.Available => "○ Available",
                        _                               => "🔒 Locked"
                    };

                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"<color={statusColor}>{statusLabel}</color>  <b>{m.Title}</b>", objStyle);

                    if (m.Status == Missions.MissionStatus.Available)
                    {
                        GUI.backgroundColor = new Color(0.2f, 0.55f, 0.3f);
                        if (GUILayout.Button("START", buttonStyle, GUILayout.Width(60f * sc), GUILayout.Height(26f * sc)))
                        {
                            mm.StartMission(m.MissionId);
                            missionLogOpen = false;
                        }
                        GUI.backgroundColor = Color.white;
                    }
                    GUILayout.EndHorizontal();

                    if (m.Status != Missions.MissionStatus.Locked)
                        GUILayout.Label($"  {m.Description}", factionStyle);

                    GUILayout.Space(3f * sc);
                }
                GUILayout.Space(6f * sc);
            }

            GUILayout.EndScrollView();
            GUI.backgroundColor = Color.white;
            GUILayout.EndArea();
        }
    }
}
