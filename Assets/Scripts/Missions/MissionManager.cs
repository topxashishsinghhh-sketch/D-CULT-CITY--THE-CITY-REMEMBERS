using System.Collections.Generic;
using DCultCity.Reputation;
using UnityEngine;

namespace DCultCity.Missions
{
    /// <summary>
    /// Central mission orchestrator. Owns the full mission roster and drives
    /// the active mission's Tick() every frame. Tracks completion/failure state
    /// and exposes the current mission to the MissionUI.
    /// </summary>
    public class MissionManager : MonoBehaviour
    {
        public static MissionManager Instance { get; private set; }

        // ------------------------------------------------------------------ //
        //  Mission registry
        // ------------------------------------------------------------------ //
        private readonly List<IMission> allMissions = new List<IMission>();
        private IMission activeMission = null;

        public IMission ActiveMission => activeMission;
        public IReadOnlyList<IMission> AllMissions => allMissions;

        // ------------------------------------------------------------------ //
        //  Lifecycle
        // ------------------------------------------------------------------ //
        private void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }

            RegisterAllMissions();
            UnlockStartingMissions();
        }

        private void Update()
        {
            activeMission?.Tick(Time.deltaTime);
        }

        // ------------------------------------------------------------------ //
        //  Registration
        // ------------------------------------------------------------------ //
        private void RegisterAllMissions()
        {
            // Police storyline
            Register(new MissionPolice_ThinBlueLine());
            Register(new MissionPolice_OperationLockdown());

            // Gangster storyline
            Register(new MissionGangster_FirstContact());
            Register(new MissionGangster_TheTakeover());

            // Side quests
            Register(new SideQuest_StreetRace());
            Register(new SideQuest_Delivery());
            Register(new SideQuest_AnimalRescue());
            Register(new SideQuest_CasinoChallenge());

            Debug.Log($"<color=#FFDD44>[MissionManager]</color> {allMissions.Count} missions registered.");
        }

        private void Register(IMission mission)
        {
            mission.OnCompleted += HandleMissionCompleted;
            mission.OnFailed    += HandleMissionFailed;
            allMissions.Add(mission);
        }

        private void UnlockStartingMissions()
        {
            // Police M1 and Gangster M1 are available from the start
            SetAvailable("police_01_thin_blue_line");
            SetAvailable("gangster_01_first_contact");

            // All side quests available immediately
            SetAvailable("side_race_01");
            SetAvailable("side_delivery_01");
            SetAvailable("side_animal_rescue_01");
            SetAvailable("side_casino_01");
        }

        private void SetAvailable(string id)
        {
            var m = Find(id);
            if (m is MissionBase mb)
                mb.Status = MissionStatus.Available;
        }

        // ------------------------------------------------------------------ //
        //  Public API
        // ------------------------------------------------------------------ //

        /// <summary>Start a mission by ID. Fails silently if not available.</summary>
        public bool StartMission(string missionId)
        {
            if (activeMission != null && activeMission.Status == MissionStatus.Active)
            {
                Debug.LogWarning($"[MissionManager] Cannot start '{missionId}' — '{activeMission.Title}' is already active.");
                return false;
            }

            var m = Find(missionId);
            if (m == null) { Debug.LogWarning($"[MissionManager] Mission not found: {missionId}"); return false; }
            if (m.Status != MissionStatus.Available) { Debug.LogWarning($"[MissionManager] Mission not available: {missionId}"); return false; }

            activeMission = m;
            m.Activate();
            return true;
        }

        public IMission Find(string id)
        {
            foreach (var m in allMissions)
                if (m.MissionId == id) return m;
            return null;
        }

        public List<IMission> GetByFaction(MissionFaction faction)
        {
            var result = new List<IMission>();
            foreach (var m in allMissions)
                if (m.Faction == faction) result.Add(m);
            return result;
        }

        public List<IMission> GetAvailable() => allMissions.FindAll(m => m.Status == MissionStatus.Available);
        public List<IMission> GetCompleted()  => allMissions.FindAll(m => m.Status == MissionStatus.Completed);

        // ------------------------------------------------------------------ //
        //  Event handlers
        // ------------------------------------------------------------------ //
        private void HandleMissionCompleted(IMission mission)
        {
            Debug.Log($"<color=#00FF88>[MissionManager]</color> '{mission.Title}' complete!");

            // Chain storyline unlocks
            if (mission.MissionId == "police_01_thin_blue_line")
                SetAvailable("police_02_operation_lockdown");

            if (mission.MissionId == "gangster_01_first_contact")
                SetAvailable("gangster_02_the_takeover");

            activeMission = null;
        }

        private void HandleMissionFailed(IMission mission, string reason)
        {
            Debug.Log($"<color=#FF4444>[MissionManager]</color> '{mission.Title}' failed: {reason}");
            activeMission = null;

            // Police rep penalty for failed gang missions and vice versa
            if (mission.Faction == MissionFaction.Gangster)
                FactionReputation.Instance?.Apply("Gangsters", -0.10f);
            else if (mission.Faction == MissionFaction.Police)
                FactionReputation.Instance?.Apply("Police", -0.10f);
        }
    }
}
