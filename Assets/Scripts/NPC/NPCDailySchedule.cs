using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCultCity.NPC
{
    /// <summary>
    /// Time-block based daily schedule for Story NPCs.
    /// Uses a normalized 0–24 in-game hour clock driven by GameClock.
    /// Each schedule entry specifies where the NPC should be and what they are doing
    /// during a given time window.
    /// </summary>
    [Serializable]
    public class ScheduleEntry
    {
        [Tooltip("Hour of the in-game day this block begins (0–24)")]
        public float startHour;
        [Tooltip("Hour this block ends")]
        public float endHour;
        [Tooltip("World-space position the NPC should be at during this block")]
        public Vector3 targetPosition;
        [Tooltip("Short description used for dialogue context: 'working', 'resting', 'shopping'")]
        public string activityLabel;
        [Tooltip("Animation state key (e.g. Idle, Sit, Browse) — used in Milestone 6 with Animator")]
        public string animationState = "Idle";
    }

    /// <summary>
    /// Manages a list of ScheduleEntry blocks. Returns the active entry
    /// for the current in-game hour and drives NavMeshAgent destination.
    /// </summary>
    [Serializable]
    public class NPCDailySchedule
    {
        [SerializeField] private List<ScheduleEntry> entries = new List<ScheduleEntry>();

        public IReadOnlyList<ScheduleEntry> Entries => entries;

        public NPCDailySchedule(List<ScheduleEntry> entries)
        {
            this.entries = entries ?? new List<ScheduleEntry>();
        }

        /// <summary>Returns the schedule entry active at the given in-game hour (0–24).</summary>
        public ScheduleEntry GetActiveEntry(float currentHour)
        {
            foreach (var e in entries)
                if (currentHour >= e.startHour && currentHour < e.endHour)
                    return e;

            // Default: return first entry (wraps overnight)
            return entries.Count > 0 ? entries[0] : null;
        }

        public bool HasEntries => entries.Count > 0;
    }

    /// <summary>
    /// Lightweight in-game clock. Drives NPC schedules.
    /// 1 real second = timeScale in-game minutes. Default 1 real sec = 1 game min.
    /// </summary>
    public class GameClock : MonoBehaviour
    {
        public static GameClock Instance { get; private set; }

        [Header("Time Configuration")]
        [SerializeField] private float startHour     = 8f;   // Day starts at 8 AM
        [SerializeField] private float minutesPerRealSecond = 1f;

        private float currentHour;

        public float CurrentHour => currentHour;
        public string TimeString => $"{Mathf.FloorToInt(currentHour):00}:{Mathf.FloorToInt((currentHour % 1f) * 60f):00}";

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }

            currentHour = startHour;
        }

        private void Update()
        {
            currentHour += (minutesPerRealSecond / 60f) * Time.deltaTime;
            if (currentHour >= 24f) currentHour -= 24f;
        }

        /// <summary>Directly set the in-game hour. Used by SaveLoadManager during load.</summary>
        public void SetHour(float hour)
        {
            currentHour = Mathf.Repeat(hour, 24f);
        }
    }
}
