using System;
using System.Collections.Generic;
using UnityEngine;

namespace NeuralBattalion.Core.Events
{
    /// <summary>
    /// Simple publish/subscribe event bus for game events.
    /// Responsibilities:
    /// - Decouple event publishers from subscribers
    /// - Manage event subscriptions
    /// - Dispatch events to all subscribers
    /// 
    /// Usage:
    /// // Subscribe
    /// EventBus.Subscribe<PlayerDamagedEvent>(OnPlayerDamaged);
    /// 
    /// // Publish
    /// EventBus.Publish(new PlayerDamagedEvent { CurrentHealth = 50 });
    /// 
    /// // Unsubscribe (important for cleanup!)
    /// EventBus.Unsubscribe<PlayerDamagedEvent>(OnPlayerDamaged);
    /// </summary>
    public static class EventBus
    {
        private static readonly Dictionary<Type, HashSet<Delegate>> subscribers = new();

        /// <summary>
        /// Subscribe to an event type.
        /// </summary>
        /// <typeparam name="T">Event type.</typeparam>
        /// <param name="handler">Handler method.</param>
        public static void Subscribe<T>(Action<T> handler) where T : struct
        {
            var eventType = typeof(T);

            if (!subscribers.ContainsKey(eventType))
            {
                subscribers[eventType] = new HashSet<Delegate>();
            }

            subscribers[eventType].Add(handler);
        }

        /// <summary>
        /// Unsubscribe from an event type.
        /// </summary>
        /// <typeparam name="T">Event type.</typeparam>
        /// <param name="handler">Handler method to remove.</param>
        public static void Unsubscribe<T>(Action<T> handler) where T : struct
        {
            var eventType = typeof(T);

            if (subscribers.ContainsKey(eventType))
            {
                subscribers[eventType].Remove(handler);
            }
        }

        /// <summary>
        /// Publish an event to all subscribers.
        /// </summary>
        /// <typeparam name="T">Event type.</typeparam>
        /// <param name="eventData">Event data to publish.</param>
        public static void Publish<T>(T eventData) where T : struct
        {
            var eventType = typeof(T);

            if (!subscribers.ContainsKey(eventType)) return;

            // Create a copy to avoid modification during iteration
            var handlersCopy = new List<Delegate>(subscribers[eventType]);

            foreach (var handler in handlersCopy)
            {
                try
                {
                    ((Action<T>)handler)?.Invoke(eventData);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[EventBus] Error invoking handler for {eventType.Name}: {e}");
                }
            }
        }

        /// <summary>
        /// Clear all subscriptions. Use with caution - typically only for testing or shutdown.
        /// </summary>
        public static void ClearAll()
        {
            subscribers.Clear();
        }

        /// <summary>
        /// Clear subscriptions for a specific event type.
        /// </summary>
        /// <typeparam name="T">Event type to clear.</typeparam>
        public static void Clear<T>() where T : struct
        {
            var eventType = typeof(T);
            if (subscribers.ContainsKey(eventType))
            {
                subscribers[eventType].Clear();
            }
        }

        /// <summary>
        /// Get the number of subscribers for an event type (for debugging).
        /// </summary>
        /// <typeparam name="T">Event type.</typeparam>
        /// <returns>Number of subscribers.</returns>
        public static int GetSubscriberCount<T>() where T : struct
        {
            var eventType = typeof(T);
            return subscribers.ContainsKey(eventType) ? subscribers[eventType].Count : 0;
        }
    }
}
