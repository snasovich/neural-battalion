using UnityEngine;
using System;
using System.Collections.Generic;

namespace NeuralBattalion.Utility
{
    /// <summary>
    /// Service Locator pattern for accessing game services.
    /// Responsibilities:
    /// - Provide centralized access to services
    /// - Decouple systems from direct references
    /// - Allow service registration/replacement
    /// 
    /// Usage:
    /// - Register services: ServiceLocator.Register<IGameManager>(gameManager);
    /// - Get services: var manager = ServiceLocator.Get<IGameManager>();
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> services = new Dictionary<Type, object>();
        private static readonly Dictionary<Type, Func<object>> factories = new Dictionary<Type, Func<object>>();

        /// <summary>
        /// Register a service instance.
        /// </summary>
        /// <typeparam name="T">Service type.</typeparam>
        /// <param name="service">Service instance.</param>
        public static void Register<T>(T service) where T : class
        {
            var type = typeof(T);

            if (services.ContainsKey(type))
            {
                Debug.LogWarning($"[ServiceLocator] Service '{type.Name}' already registered, replacing");
            }

            services[type] = service;
        }

        /// <summary>
        /// Register a service factory for lazy initialization.
        /// </summary>
        /// <typeparam name="T">Service type.</typeparam>
        /// <param name="factory">Factory function.</param>
        public static void RegisterFactory<T>(Func<T> factory) where T : class
        {
            var type = typeof(T);
            factories[type] = () => factory();
        }

        /// <summary>
        /// Get a registered service.
        /// </summary>
        /// <typeparam name="T">Service type.</typeparam>
        /// <returns>Service instance or null.</returns>
        public static T Get<T>() where T : class
        {
            var type = typeof(T);

            // Check for direct registration
            if (services.TryGetValue(type, out object service))
            {
                return service as T;
            }

            // Check for factory
            if (factories.TryGetValue(type, out Func<object> factory))
            {
                T newService = factory() as T;
                services[type] = newService; // Cache the created instance
                return newService;
            }

            Debug.LogWarning($"[ServiceLocator] Service '{type.Name}' not registered");
            return null;
        }

        /// <summary>
        /// Try to get a service without warning.
        /// </summary>
        /// <typeparam name="T">Service type.</typeparam>
        /// <param name="service">Output service instance.</param>
        /// <returns>True if service found.</returns>
        public static bool TryGet<T>(out T service) where T : class
        {
            var type = typeof(T);

            if (services.TryGetValue(type, out object obj))
            {
                service = obj as T;
                return service != null;
            }

            if (factories.TryGetValue(type, out Func<object> factory))
            {
                service = factory() as T;
                if (service != null)
                {
                    services[type] = service;
                    return true;
                }
            }

            service = null;
            return false;
        }

        /// <summary>
        /// Unregister a service.
        /// </summary>
        /// <typeparam name="T">Service type.</typeparam>
        public static void Unregister<T>() where T : class
        {
            var type = typeof(T);
            services.Remove(type);
            factories.Remove(type);
        }

        /// <summary>
        /// Check if a service is registered.
        /// </summary>
        /// <typeparam name="T">Service type.</typeparam>
        /// <returns>True if registered.</returns>
        public static bool IsRegistered<T>() where T : class
        {
            var type = typeof(T);
            return services.ContainsKey(type) || factories.ContainsKey(type);
        }

        /// <summary>
        /// Clear all registered services.
        /// </summary>
        public static void ClearAll()
        {
            services.Clear();
            factories.Clear();
        }

        /// <summary>
        /// Get or create a MonoBehaviour service.
        /// </summary>
        /// <typeparam name="T">MonoBehaviour type.</typeparam>
        /// <returns>Service instance.</returns>
        public static T GetOrCreate<T>() where T : MonoBehaviour
        {
            T service = Get<T>();

            if (service == null)
            {
                // Try to find in scene
                service = UnityEngine.Object.FindObjectOfType<T>();

                if (service == null)
                {
                    // Create new
                    var go = new GameObject(typeof(T).Name);
                    service = go.AddComponent<T>();
                }

                Register<T>(service);
            }

            return service;
        }
    }
}
