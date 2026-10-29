using System;
using System.Collections.Generic;
using DCultCity.Vehicles;
using UnityEngine;

namespace DCultCity.Core
{
    /// <summary>
    /// Listener component that registers with the EventBus to capture and log
    /// player actions and world events. Keeps a local buffer for in-game/UI display.
    /// </summary>
    public class EventBusTestListener : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private bool logToConsole = true;
        [SerializeField] private int maxHistoryCount = 20;

        [Header("Diagnostics")]
        [SerializeField] private int totalEventsReceived = 0;

        // In-memory log buffer for on-screen debug display
        private readonly List<string> eventLogHistory = new List<string>();

        public IReadOnlyList<string> EventLogHistory => eventLogHistory;
        public int TotalEventsReceived => totalEventsReceived;

        public event Action<PlayerActionEvent> OnEventLogged;

        private void OnEnable()
        {
            // Subscribe to PlayerActionEvent and CharacterCustomizedEvent
            EventBus.Subscribe<PlayerActionEvent>(HandlePlayerAction);
            EventBus.Subscribe<CharacterCustomizedEvent>(HandleCharacterCustomized);
            EventBus.Subscribe<VehicleOccupancyEvent>(HandleVehicleOccupancy);
            if (logToConsole)
            {
                Debug.Log("<color=#00FF88>[EventBusTestListener]</color> Subscribed to PlayerActionEvent, CharacterCustomizedEvent & VehicleOccupancyEvent.");
            }
        }

        private void OnDisable()
        {
            // Unsubscribe when disabled/destroyed to prevent memory leaks
            EventBus.Unsubscribe<PlayerActionEvent>(HandlePlayerAction);
            EventBus.Unsubscribe<CharacterCustomizedEvent>(HandleCharacterCustomized);
            EventBus.Unsubscribe<VehicleOccupancyEvent>(HandleVehicleOccupancy);
            if (logToConsole)
            {
                Debug.Log("<color=#FFAA00>[EventBusTestListener]</color> Unsubscribed from all events.");
            }
        }

        private void HandleVehicleOccupancy(VehicleOccupancyEvent evt)
        {
            totalEventsReceived++;
            string action = evt.change == VehicleOccupancyEvent.OccupancyChange.Entered ? "Entered" : "Exited";
            string colorTag = evt.change == VehicleOccupancyEvent.OccupancyChange.Entered ? "#FFDD44" : "#44DDFF";
            string formatted = $"[{DateTime.Now:HH:mm:ss}] <color={colorTag}><b>Vehicle {action}</b></color>: <i>{evt.vehicleId}</i> by {evt.actorId}";

            if (logToConsole)
                Debug.Log($"<color={colorTag}>[EventBus Log #{totalEventsReceived}]</color> {evt}");

            eventLogHistory.Insert(0, formatted);
            if (eventLogHistory.Count > maxHistoryCount)
                eventLogHistory.RemoveAt(eventLogHistory.Count - 1);
        }

        private void HandleCharacterCustomized(CharacterCustomizedEvent evt)
        {
            totalEventsReceived++;
            string formatted = $"[{DateTime.Now:HH:mm:ss}] <color=#DD88FF><b>Character Created</b></color>: <i>{evt.characterName}</i> ({evt.gender}, Skin #{evt.skinToneIndex + 1})";
            if (logToConsole)
            {
                Debug.Log($"<color=#DD88FF>[EventBus Log #{totalEventsReceived}]</color> {evt}");
            }
            eventLogHistory.Insert(0, formatted);
            if (eventLogHistory.Count > maxHistoryCount)
            {
                eventLogHistory.RemoveAt(eventLogHistory.Count - 1);
            }
        }

        private void HandlePlayerAction(PlayerActionEvent evt)
        {
            totalEventsReceived++;

            string colorTag = evt.trustDelta switch
            {
                > 0 => "#00FFAA", // Positive / Friendly
                < 0 => "#FF4444", // Hostile / Crime
                _ => "#FFDD00"    // Neutral / Gambling
            };

            string formattedEntry = $"[{DateTime.Now:HH:mm:ss}] <color={colorTag}><b>{evt.actionType}</b></color> by <i>{evt.actorId}</i> on <i>{evt.targetId}</i> (Rep: {evt.reputationDelta:+0.0;-0.0;0}, Trust: {evt.trustDelta:+0.0;-0.0;0}) - Witnesses: {evt.witnesses.Count}";

            if (logToConsole)
            {
                Debug.Log($"<color=#33CCFF>[EventBus Log #{totalEventsReceived}]</color> {evt}");
            }

            eventLogHistory.Insert(0, formattedEntry);
            if (eventLogHistory.Count > maxHistoryCount)
            {
                eventLogHistory.RemoveAt(eventLogHistory.Count - 1);
            }

            OnEventLogged?.Invoke(evt);
        }

        public void ClearHistory()
        {
            eventLogHistory.Clear();
            totalEventsReceived = 0;
        }
    }
}
