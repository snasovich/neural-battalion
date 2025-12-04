using UnityEngine;
using System.Collections.Generic;
using NeuralBattalion.Data;
using NeuralBattalion.Core.Events;

namespace NeuralBattalion.Enemy
{
    /// <summary>
    /// Manages enemy spawning and wave progression.
    /// Responsibilities:
    /// - Spawn enemies based on wave data
    /// - Track active enemies
    /// - Handle spawn timing and positioning
    /// - Trigger wave events
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [Header("Spawn Points")]
        [SerializeField] private Transform[] spawnPoints;

        [Header("Enemy Prefabs")]
        [SerializeField] private GameObject[] enemyPrefabs;

        [Header("Wave Settings")]
        [SerializeField] private WaveData[] waves;
        [SerializeField] private float initialSpawnDelay = 3f;
        [SerializeField] private float spawnInterval = 5f;
        [SerializeField] private int maxActiveEnemies = 4;

        [Header("Debug")]
        [SerializeField] private bool debugMode = false;

        // Current wave state
        private int currentWaveIndex = 0;
        private int enemiesSpawnedThisWave = 0;
        private int enemiesToSpawnThisWave = 0;
        private float spawnTimer;
        private bool isSpawning = false;

        // Enemy tracking
        private List<EnemyController> activeEnemies = new List<EnemyController>();
        private int nextEnemyId = 0;

        public int CurrentWave => currentWaveIndex + 1;
        public int TotalWaves => waves?.Length ?? 0;
        public int ActiveEnemyCount => activeEnemies.Count;
        public int RemainingEnemiesToSpawn => enemiesToSpawnThisWave - enemiesSpawnedThisWave;

        private void Start()
        {
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            EventBus.Subscribe<EnemyDestroyedEvent>(OnEnemyDestroyed);
        }

        private void UnsubscribeFromEvents()
        {
            EventBus.Unsubscribe<EnemyDestroyedEvent>(OnEnemyDestroyed);
        }

        private void Update()
        {
            if (!isSpawning) return;

            UpdateSpawnTimer();
        }

        /// <summary>
        /// Start spawning enemies for the level.
        /// </summary>
        public void StartSpawning()
        {
            currentWaveIndex = 0;
            StartWave(0);
        }

        /// <summary>
        /// Stop all spawning.
        /// </summary>
        public void StopSpawning()
        {
            isSpawning = false;
        }

        /// <summary>
        /// Start a specific wave.
        /// </summary>
        /// <param name="waveIndex">Wave index to start.</param>
        private void StartWave(int waveIndex)
        {
            if (waves == null || waveIndex >= waves.Length)
            {
                if (debugMode) Debug.Log("[EnemySpawner] No more waves");
                isSpawning = false;
                return;
            }

            currentWaveIndex = waveIndex;
            WaveData wave = waves[waveIndex];

            enemiesSpawnedThisWave = 0;
            enemiesToSpawnThisWave = wave.TotalEnemies;
            spawnTimer = initialSpawnDelay;
            spawnInterval = wave.SpawnInterval;
            isSpawning = true;

            EventBus.Publish(new EnemyWaveStartedEvent
            {
                WaveNumber = waveIndex + 1,
                EnemyCount = enemiesToSpawnThisWave
            });

            if (debugMode)
            {
                Debug.Log($"[EnemySpawner] Starting wave {waveIndex + 1} with {enemiesToSpawnThisWave} enemies");
            }
        }

        /// <summary>
        /// Update spawn timer and spawn enemies.
        /// </summary>
        private void UpdateSpawnTimer()
        {
            // Check if wave is complete
            if (enemiesSpawnedThisWave >= enemiesToSpawnThisWave)
            {
                // Check if all enemies are destroyed to progress to next wave
                if (activeEnemies.Count == 0)
                {
                    ProgressToNextWave();
                }
                return;
            }

            // Check max active enemies
            if (activeEnemies.Count >= maxActiveEnemies) return;

            // Update timer
            spawnTimer -= Time.deltaTime;

            if (spawnTimer <= 0)
            {
                SpawnEnemy();
                spawnTimer = spawnInterval;
            }
        }

        /// <summary>
        /// Spawn a single enemy.
        /// </summary>
        private void SpawnEnemy()
        {
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                Debug.LogError("[EnemySpawner] No spawn points defined");
                return;
            }

            if (enemyPrefabs == null || enemyPrefabs.Length == 0)
            {
                Debug.LogError("[EnemySpawner] No enemy prefabs defined");
                return;
            }

            // Select spawn point
            Transform spawnPoint = SelectSpawnPoint();
            if (spawnPoint == null)
            {
                if (debugMode)
                {
                    Debug.LogWarning("[EnemySpawner] No available spawn points - will retry next cycle");
                }
                return;
            }

            // Determine enemy type from wave data
            int enemyType = DetermineEnemyType();

            // Spawn enemy
            GameObject prefab = enemyPrefabs[Mathf.Clamp(enemyType, 0, enemyPrefabs.Length - 1)];
            if (prefab == null)
            {
                Debug.LogError($"[EnemySpawner] Enemy prefab for type {enemyType} is null!");
                return;
            }
            
            GameObject enemyObj = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
            enemyObj.name = $"Enemy_{nextEnemyId}_{EnemyTypes.GetName(enemyType)}";

            EnemyController enemy = enemyObj.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.Initialize(nextEnemyId++, enemyType);
                activeEnemies.Add(enemy);
                
                if (debugMode)
                {
                    Debug.Log($"[EnemySpawner] Spawned enemy {enemy.EnemyId} ({EnemyTypes.GetName(enemyType)}) " +
                             $"at {spawnPoint.position} - Wave progress: {enemiesSpawnedThisWave}/{enemiesToSpawnThisWave}");
                }
            }
            else
            {
                Debug.LogError("[EnemySpawner] Spawned enemy has no EnemyController component!");
                Destroy(enemyObj);
                return;
            }

            enemiesSpawnedThisWave++;
        }

        /// <summary>
        /// Select a valid spawn point.
        /// </summary>
        private Transform SelectSpawnPoint()
        {
            // Try to find unoccupied spawn point
            List<Transform> validPoints = new List<Transform>();

            foreach (var point in spawnPoints)
            {
                if (!IsSpawnPointOccupied(point))
                {
                    validPoints.Add(point);
                }
            }

            if (validPoints.Count == 0)
            {
                // All points occupied, wait for next cycle
                return null;
            }

            return validPoints[Random.Range(0, validPoints.Count)];
        }

        /// <summary>
        /// Check if a spawn point is occupied.
        /// </summary>
        private bool IsSpawnPointOccupied(Transform point)
        {
            Collider2D hit = Physics2D.OverlapCircle(point.position, 1f);
            return hit != null;
        }

        /// <summary>
        /// Determine enemy type to spawn based on wave data.
        /// </summary>
        private int DetermineEnemyType()
        {
            if (waves == null || currentWaveIndex >= waves.Length)
            {
                return 0;
            }

            WaveData wave = waves[currentWaveIndex];

            // Simple weighted random selection based on wave enemy types
            if (wave.EnemyTypes != null && wave.EnemyTypes.Length > 0)
            {
                return wave.EnemyTypes[Random.Range(0, wave.EnemyTypes.Length)];
            }

            return 0;
        }

        /// <summary>
        /// Progress to the next wave.
        /// </summary>
        private void ProgressToNextWave()
        {
            currentWaveIndex++;

            if (currentWaveIndex < waves?.Length)
            {
                StartWave(currentWaveIndex);
            }
            else
            {
                // All waves complete
                isSpawning = false;
                if (debugMode) Debug.Log("[EnemySpawner] All waves complete");
            }
        }

        /// <summary>
        /// Handle enemy destroyed event.
        /// </summary>
        private void OnEnemyDestroyed(EnemyDestroyedEvent evt)
        {
            // Remove from active list
            activeEnemies.RemoveAll(e => e == null || e.EnemyId == evt.EnemyId);
        }

        /// <summary>
        /// Destroy all active enemies (Bomb power-up).
        /// </summary>
        public void DestroyAllEnemies()
        {
            foreach (var enemy in activeEnemies)
            {
                if (enemy != null && enemy.IsAlive)
                {
                    enemy.ForceDestroy();
                }
            }

            activeEnemies.Clear();
        }

        /// <summary>
        /// Freeze all active enemies (Timer power-up).
        /// </summary>
        /// <param name="duration">Freeze duration in seconds.</param>
        public void FreezeAllEnemies(float duration)
        {
            foreach (var enemy in activeEnemies)
            {
                if (enemy != null && enemy.IsAlive)
                {
                    enemy.Freeze(duration);
                }
            }
        }

        /// <summary>
        /// Get all active enemies.
        /// </summary>
        public List<EnemyController> GetActiveEnemies()
        {
            // Clean up null references
            activeEnemies.RemoveAll(e => e == null);
            return new List<EnemyController>(activeEnemies);
        }

        /// <summary>
        /// Get total enemies for current level.
        /// </summary>
        public int GetTotalLevelEnemies()
        {
            int total = 0;
            if (waves != null)
            {
                foreach (var wave in waves)
                {
                    total += wave.TotalEnemies;
                }
            }
            return total;
        }
    }
}
