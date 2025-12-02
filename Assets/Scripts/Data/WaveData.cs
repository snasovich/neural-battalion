using UnityEngine;

namespace NeuralBattalion.Data
{
    /// <summary>
    /// ScriptableObject for enemy wave configuration.
    /// Defines which enemies appear and spawn timing.
    /// </summary>
    [CreateAssetMenu(fileName = "NewWaveData", menuName = "Neural Battalion/Wave Data")]
    public class WaveData : ScriptableObject
    {
        [Header("Wave Info")]
        [SerializeField] private string waveName = "Wave 1";
        [SerializeField] private int waveNumber = 1;

        [Header("Enemy Configuration")]
        [SerializeField] private int totalEnemies = 20;
        [SerializeField] private int[] enemyTypes = new int[] { 0, 0, 0, 1 };
        [SerializeField] private int[] enemyCounts = new int[] { 10, 5, 3, 2 };

        [Header("Spawn Settings")]
        [SerializeField] private float spawnInterval = 5f;
        [SerializeField] private float initialDelay = 3f;
        [SerializeField] private int maxActiveEnemies = 4;

        [Header("Special Enemies")]
        [SerializeField] private int[] bonusEnemyIndices; // Which enemies drop power-ups
        [SerializeField] private bool hasBossEnemy = false;
        [SerializeField] private int bossEnemyType = 3;

        // Properties
        public string WaveName => waveName;
        public int WaveNumber => waveNumber;
        public int TotalEnemies => totalEnemies;
        public int[] EnemyTypes => enemyTypes;
        public int[] EnemyCounts => enemyCounts;
        public float SpawnInterval => spawnInterval;
        public float InitialDelay => initialDelay;
        public int MaxActiveEnemies => maxActiveEnemies;
        public int[] BonusEnemyIndices => bonusEnemyIndices;
        public bool HasBossEnemy => hasBossEnemy;
        public int BossEnemyType => bossEnemyType;

        /// <summary>
        /// Get enemy type for a specific spawn index.
        /// </summary>
        public int GetEnemyTypeForIndex(int spawnIndex)
        {
            if (enemyTypes == null || enemyTypes.Length == 0)
            {
                return 0; // Default to basic
            }

            if (enemyCounts != null && enemyCounts.Length == enemyTypes.Length)
            {
                // Use weighted distribution
                int currentIndex = 0;
                for (int i = 0; i < enemyCounts.Length; i++)
                {
                    currentIndex += enemyCounts[i];
                    if (spawnIndex < currentIndex)
                    {
                        return enemyTypes[i];
                    }
                }
            }

            // Random selection
            return enemyTypes[Random.Range(0, enemyTypes.Length)];
        }

        /// <summary>
        /// Check if a spawn index should be a bonus enemy (drops power-up).
        /// </summary>
        public bool IsBonusEnemy(int spawnIndex)
        {
            if (bonusEnemyIndices == null) return false;

            foreach (int index in bonusEnemyIndices)
            {
                if (index == spawnIndex) return true;
            }

            return false;
        }

        /// <summary>
        /// Validate wave data.
        /// </summary>
        public bool IsValid()
        {
            if (totalEnemies <= 0) return false;
            if (spawnInterval <= 0) return false;
            if (maxActiveEnemies <= 0) return false;

            return true;
        }

        /// <summary>
        /// Create a wave data from parameters.
        /// </summary>
        public static WaveData Create(int enemies, int[] types, float interval = 5f)
        {
            WaveData wave = CreateInstance<WaveData>();
            wave.totalEnemies = enemies;
            wave.enemyTypes = types;
            wave.spawnInterval = interval;
            return wave;
        }
    }
}
