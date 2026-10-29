using System.Collections.Generic;
using UnityEngine;

namespace DCultCity.Reputation
{
    /// <summary>
    /// Global faction-level reputation tracked for a specific actor (default: "Player_01").
    /// Factions: Civilians, Police, Gangsters, Casino.
    /// Each faction tracks a score [-1, +1] that affects NPC reactions, mission availability,
    /// and police wanted level. FactionReputation is a singleton for easy cross-system access.
    /// </summary>
    public class FactionReputation : MonoBehaviour
    {
        public static FactionReputation Instance { get; private set; }

        [Header("Starting Reputation Scores")]
        [SerializeField] private float civilians  = 0f;
        [SerializeField] private float police     = 0f;
        [SerializeField] private float gangsters  = 0f;
        [SerializeField] private float casino     = 0f;

        // Map from faction name → current score
        private readonly Dictionary<string, float> factionScores = new Dictionary<string, float>();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }

            DontDestroyOnLoad(gameObject);
            Reset();
        }

        public void Reset()
        {
            factionScores["Civilians"]  = civilians;
            factionScores["Police"]     = police;
            factionScores["Gangsters"]  = gangsters;
            factionScores["Casino"]     = casino;
        }

        public float Get(string faction)
        {
            return factionScores.TryGetValue(faction, out float val) ? val : 0f;
        }

        public void Apply(string faction, float delta)
        {
            if (!factionScores.ContainsKey(faction)) factionScores[faction] = 0f;
            factionScores[faction] = Mathf.Clamp(factionScores[faction] + delta, -1f, 1f);
            Debug.Log($"<color=#AAFFCC>[FactionReputation]</color> {faction}: {factionScores[faction]:+0.00;-0.00}");
        }

        /// <summary>Maps an ActionType to affected factions and applies deltas.</summary>
        public void ApplyFromAction(DCultCity.Core.ActionType action, float repDelta)
        {
            switch (action)
            {
                case DCultCity.Core.ActionType.HelpedNPC:
                    Apply("Civilians", repDelta);
                    Apply("Police",    repDelta * 0.3f);
                    break;

                case DCultCity.Core.ActionType.StoleCar:
                    Apply("Civilians", repDelta);
                    Apply("Police",    repDelta * 1.5f);
                    Apply("Gangsters", Mathf.Abs(repDelta) * 0.2f); // Gangsters respect crime
                    break;

                case DCultCity.Core.ActionType.HitPedestrian:
                    Apply("Civilians", repDelta * 1.2f);
                    Apply("Police",    repDelta * 2f);
                    break;

                case DCultCity.Core.ActionType.WonCasino:
                    Apply("Casino", repDelta);
                    break;

                case DCultCity.Core.ActionType.BribedOfficer:
                    Apply("Police",    repDelta);
                    Apply("Gangsters", Mathf.Abs(repDelta) * 0.15f);
                    break;

                case DCultCity.Core.ActionType.RescuedAnimal:
                    Apply("Civilians", repDelta * 1.4f);
                    break;

                case DCultCity.Core.ActionType.CompletedDelivery:
                    Apply("Gangsters", repDelta);
                    Apply("Civilians", repDelta * 0.3f);
                    break;
            }
        }

        public WantedLevel GetWantedLevel()
        {
            float policeRep = Get("Police");
            if (policeRep < -0.80f) return WantedLevel.FiveStars;
            if (policeRep < -0.60f) return WantedLevel.FourStars;
            if (policeRep < -0.40f) return WantedLevel.ThreeStars;
            if (policeRep < -0.20f) return WantedLevel.TwoStars;
            if (policeRep < -0.05f) return WantedLevel.OneStar;
            return WantedLevel.None;
        }

        /// <summary>Serializable snapshot for Milestone 5 save/load.</summary>
        public Dictionary<string, float> GetSnapshot() => new Dictionary<string, float>(factionScores);

        public void LoadSnapshot(Dictionary<string, float> snapshot)
        {
            factionScores.Clear();
            foreach (var kv in snapshot)
                factionScores[kv.Key] = Mathf.Clamp(kv.Value, -1f, 1f);
        }
    }

    public enum WantedLevel { None, OneStar, TwoStars, ThreeStars, FourStars, FiveStars }
}
