using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCultCity.Core
{
    /// <summary>
    /// Central decoupled Publish / Subscribe EventBus.
    /// Handles communication between game systems (Player actions, NPC memory,
    /// reputation, missions, UI, audio) without direct dependencies.
    /// </summary>
    public static class EventBus
    {
        private static readonly Dictionary<Type, List<Delegate>> subscribers = new Dictionary<Type, List<Delegate>>();

        /// <summary>
        /// Subscribe a listener callback for an event of type T.
        /// </summary>
        public static void Subscribe<T>(Action<T> listener) where T : IEvent
        {
            if (listener == null) return;
            Type eventType = typeof(T);

            if (!subscribers.TryGetValue(eventType, out var list))
            {
                list = new List<Delegate>();
                subscribers[eventType] = list;
            }

            if (!list.Contains(listener))
            {
                list.Add(listener);
            }
        }

        /// <summary>
        /// Unsubscribe a listener callback from an event of type T.
        /// </summary>
        public static void Unsubscribe<T>(Action<T> listener) where T : IEvent
        {
            if (listener == null) return;
            Type eventType = typeof(T);

            if (subscribers.TryGetValue(eventType, out var list))
            {
                list.Remove(listener);
                if (list.Count == 0)
                {
                    subscribers.Remove(eventType);
                }
            }
        }

        /// <summary>
        /// Publish an event to all subscribed listeners.
        /// Iterates over a snapshot to safely allow subscription changes during invocation.
        /// </summary>
        public static void Publish<T>(T eventData) where T : IEvent
        {
            Type eventType = typeof(T);

            if (subscribers.TryGetValue(eventType, out var list))
            {
                // Create a temporary snapshot array to avoid collection modification issues
                var snapshot = list.ToArray();
                for (int i = 0; i < snapshot.Length; i++)
                {
                    try
                    {
                        if (snapshot[i] is Action<T> action)
                        {
                            action.Invoke(eventData);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[EventBus] Exception while invoking listener for event {eventType.Name}: {ex}");
                    }
                }
            }
        }

        /// <summary>
        /// Clears all subscriptions across all event types.
        /// Useful when reloading scenes or resetting game state.
        /// </summary>
        public static void ClearAll()
        {
            subscribers.Clear();
        }

        /// <summary>
        /// Returns total number of registered listeners for an event type (for diagnostics).
        /// </summary>
        public static int GetSubscriberCount<T>() where T : IEvent
        {
            Type eventType = typeof(T);
            return subscribers.TryGetValue(eventType, out var list) ? list.Count : 0;
        }
    }
}
