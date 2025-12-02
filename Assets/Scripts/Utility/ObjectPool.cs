using UnityEngine;
using System.Collections.Generic;

namespace NeuralBattalion.Utility
{
    /// <summary>
    /// Generic object pool for performance optimization.
    /// Responsibilities:
    /// - Pre-instantiate objects
    /// - Recycle objects instead of destroying
    /// - Reduce garbage collection
    /// 
    /// Usage:
    /// - Use for frequently spawned objects (projectiles, effects)
    /// - Call Get() to retrieve object from pool
    /// - Call Return() when done with object
    /// </summary>
    public class ObjectPool : MonoBehaviour
    {
        public static ObjectPool Instance { get; private set; }

        [System.Serializable]
        public class PoolItem
        {
            public string id;
            public GameObject prefab;
            public int initialSize = 10;
            public bool expandable = true;
        }

        [Header("Pool Settings")]
        [SerializeField] private PoolItem[] poolItems;
        [SerializeField] private bool initializeOnAwake = true;

        private Dictionary<string, Queue<GameObject>> pools = new Dictionary<string, Queue<GameObject>>();
        private Dictionary<string, GameObject> prefabMap = new Dictionary<string, GameObject>();
        private Dictionary<GameObject, string> instanceToPoolId = new Dictionary<GameObject, string>();

        private void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (initializeOnAwake)
            {
                InitializePools();
            }
        }

        /// <summary>
        /// Initialize all pools with initial objects.
        /// </summary>
        public void InitializePools()
        {
            if (poolItems == null) return;

            foreach (var item in poolItems)
            {
                CreatePool(item.id, item.prefab, item.initialSize);
            }
        }

        /// <summary>
        /// Create a new pool.
        /// </summary>
        /// <param name="id">Pool identifier.</param>
        /// <param name="prefab">Prefab to pool.</param>
        /// <param name="initialSize">Initial pool size.</param>
        public void CreatePool(string id, GameObject prefab, int initialSize = 10)
        {
            if (pools.ContainsKey(id))
            {
                Debug.LogWarning($"[ObjectPool] Pool '{id}' already exists");
                return;
            }

            pools[id] = new Queue<GameObject>();
            prefabMap[id] = prefab;

            // Pre-instantiate objects
            for (int i = 0; i < initialSize; i++)
            {
                GameObject obj = CreateNewObject(id);
                obj.SetActive(false);
                pools[id].Enqueue(obj);
            }
        }

        /// <summary>
        /// Get an object from the pool.
        /// </summary>
        /// <param name="id">Pool identifier.</param>
        /// <returns>Pooled object or null.</returns>
        public GameObject Get(string id)
        {
            if (!pools.ContainsKey(id))
            {
                Debug.LogError($"[ObjectPool] Pool '{id}' does not exist");
                return null;
            }

            GameObject obj;

            if (pools[id].Count > 0)
            {
                obj = pools[id].Dequeue();
            }
            else
            {
                // Check if pool is expandable
                var poolItem = GetPoolItem(id);
                if (poolItem != null && !poolItem.expandable)
                {
                    Debug.LogWarning($"[ObjectPool] Pool '{id}' is empty and not expandable");
                    return null;
                }

                obj = CreateNewObject(id);
            }

            obj.SetActive(true);
            return obj;
        }

        /// <summary>
        /// Get an object from pool using prefab reference.
        /// </summary>
        /// <param name="prefab">Prefab to get from pool.</param>
        /// <returns>Pooled object or null.</returns>
        public GameObject Get(GameObject prefab)
        {
            // Find pool by prefab
            foreach (var kvp in prefabMap)
            {
                if (kvp.Value == prefab)
                {
                    return Get(kvp.Key);
                }
            }

            // No pool exists, create temporary
            Debug.LogWarning($"[ObjectPool] No pool for prefab '{prefab.name}', creating temporary");
            return Instantiate(prefab);
        }

        /// <summary>
        /// Return an object to the pool.
        /// </summary>
        /// <param name="obj">Object to return.</param>
        public void Return(GameObject obj)
        {
            if (obj == null) return;

            // Find which pool this belongs to
            if (!instanceToPoolId.TryGetValue(obj, out string poolId))
            {
                // Object not from pool, destroy it
                Destroy(obj);
                return;
            }

            obj.SetActive(false);
            obj.transform.SetParent(transform);

            if (pools.ContainsKey(poolId))
            {
                pools[poolId].Enqueue(obj);
            }
        }

        /// <summary>
        /// Return an object to pool after delay.
        /// </summary>
        /// <param name="obj">Object to return.</param>
        /// <param name="delay">Delay in seconds.</param>
        public void Return(GameObject obj, float delay)
        {
            StartCoroutine(ReturnDelayed(obj, delay));
        }

        private System.Collections.IEnumerator ReturnDelayed(GameObject obj, float delay)
        {
            yield return new WaitForSeconds(delay);
            Return(obj);
        }

        /// <summary>
        /// Create a new object for the pool.
        /// </summary>
        private GameObject CreateNewObject(string id)
        {
            if (!prefabMap.ContainsKey(id)) return null;

            GameObject obj = Instantiate(prefabMap[id], transform);
            instanceToPoolId[obj] = id;
            return obj;
        }

        /// <summary>
        /// Get pool item configuration.
        /// </summary>
        private PoolItem GetPoolItem(string id)
        {
            if (poolItems == null) return null;

            foreach (var item in poolItems)
            {
                if (item.id == id) return item;
            }

            return null;
        }

        /// <summary>
        /// Get count of available objects in pool.
        /// </summary>
        public int GetAvailableCount(string id)
        {
            return pools.ContainsKey(id) ? pools[id].Count : 0;
        }

        /// <summary>
        /// Clear all pools and destroy objects.
        /// </summary>
        public void ClearAllPools()
        {
            foreach (var pool in pools.Values)
            {
                while (pool.Count > 0)
                {
                    var obj = pool.Dequeue();
                    if (obj != null) Destroy(obj);
                }
            }

            pools.Clear();
            prefabMap.Clear();
            instanceToPoolId.Clear();
        }

        /// <summary>
        /// Clear a specific pool.
        /// </summary>
        public void ClearPool(string id)
        {
            if (!pools.ContainsKey(id)) return;

            while (pools[id].Count > 0)
            {
                var obj = pools[id].Dequeue();
                if (obj != null)
                {
                    instanceToPoolId.Remove(obj);
                    Destroy(obj);
                }
            }
        }
    }
}
