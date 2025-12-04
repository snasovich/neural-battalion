using UnityEngine;
using NeuralBattalion.Data;
using NeuralBattalion.Terrain;
using NeuralBattalion.Enemy;

/// <summary>
/// Controller for the GameScene that initializes and loads the level.
/// This script should be attached to a GameObject in the GameScene.
/// </summary>
public class GameSceneController : MonoBehaviour
{
    [Header("Level Settings")]
    [SerializeField] private string levelToLoad = "Level1";
    
    [Header("References")]
    [SerializeField] private TerrainManager terrainManager;
    [SerializeField] private EnemySpawner enemySpawner;
    
    [Header("Enemy Settings")]
    [SerializeField] private Transform[] enemySpawnPoints;
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private WaveData[] waves;
    
    private void Start()
    {
        Debug.Log("[GameSceneController] GameScene started");
        LoadLevel();
        InitializeEnemySystem();
    }
    
    /// <summary>
    /// Load the specified level from Resources.
    /// </summary>
    private void LoadLevel()
    {
        // Load level data from Resources
        string levelPath = $"Levels/{levelToLoad}";
        LevelData levelData = Resources.Load<LevelData>(levelPath);
        
        if (levelData == null)
        {
            Debug.LogError($"[GameSceneController] Failed to load level data from: {levelPath}");
            return;
        }
        
        Debug.Log($"[GameSceneController] Loaded level: {levelData.LevelName}");
        
        // Find TerrainManager if not assigned
        if (terrainManager == null)
        {
            terrainManager = FindObjectOfType<TerrainManager>();
            
            if (terrainManager == null)
            {
                Debug.LogError("[GameSceneController] TerrainManager not found in scene!");
                return;
            }
        }
        
        // Build the level
        terrainManager.BuildLevel(levelData);
        
        Debug.Log("[GameSceneController] Level initialized successfully");
    }
    
    /// <summary>
    /// Initialize the enemy spawning system.
    /// </summary>
    private void InitializeEnemySystem()
    {
        // Find or create enemy spawner
        if (enemySpawner == null)
        {
            enemySpawner = FindObjectOfType<EnemySpawner>();
            
            if (enemySpawner == null)
            {
                Debug.LogWarning("[GameSceneController] EnemySpawner not found in scene - enemies will not spawn");
                return;
            }
        }
        
        // Load enemy prefabs from Resources if not assigned
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.Log("[GameSceneController] Loading enemy prefabs from Resources");
            enemyPrefabs = new GameObject[4];
            enemyPrefabs[0] = Resources.Load<GameObject>("Prefabs/EnemyBasicTank");
            enemyPrefabs[1] = Resources.Load<GameObject>("Prefabs/EnemyFastTank");
            enemyPrefabs[2] = Resources.Load<GameObject>("Prefabs/EnemyPowerTank");
            enemyPrefabs[3] = Resources.Load<GameObject>("Prefabs/EnemyArmorTank");
            
            // Validate loading
            for (int i = 0; i < enemyPrefabs.Length; i++)
            {
                if (enemyPrefabs[i] == null)
                {
                    Debug.LogError($"[GameSceneController] Failed to load enemy prefab type {i}");
                }
                else
                {
                    Debug.Log($"[GameSceneController] Loaded enemy prefab: {enemyPrefabs[i].name}");
                }
            }
        }
        
        // Load waves from Resources if not assigned
        if (waves == null || waves.Length == 0)
        {
            Debug.Log("[GameSceneController] Loading wave data from Resources");
            WaveData wave1 = Resources.Load<WaveData>("Waves/Wave1");
            if (wave1 != null)
            {
                waves = new WaveData[] { wave1 };
                Debug.Log($"[GameSceneController] Loaded wave: {wave1.WaveName}");
            }
            else
            {
                Debug.LogError("[GameSceneController] Failed to load Wave1 from Resources");
            }
        }
        
        // Setup spawn points if not assigned
        if (enemySpawnPoints == null || enemySpawnPoints.Length == 0)
        {
            Debug.Log("[GameSceneController] Creating default enemy spawn points");
            CreateDefaultSpawnPoints();
        }
        
        // Configure spawner using reflection to set private fields
        ConfigureEnemySpawner();
        
        // Start spawning enemies
        Debug.Log("[GameSceneController] Starting enemy spawning");
        enemySpawner.StartSpawning();
    }
    
    /// <summary>
    /// Create default spawn points at the top of the level.
    /// </summary>
    private void CreateDefaultSpawnPoints()
    {
        GameObject spawnParent = new GameObject("EnemySpawnPoints");
        enemySpawnPoints = new Transform[3];
        
        // Classic Battle City has 3 spawn points at the top
        float[] xPositions = { -8f, 0f, 8f };
        float yPosition = 10f;
        
        for (int i = 0; i < 3; i++)
        {
            GameObject spawnPoint = new GameObject($"SpawnPoint_{i}");
            spawnPoint.transform.parent = spawnParent.transform;
            spawnPoint.transform.position = new Vector3(xPositions[i], yPosition, 0);
            enemySpawnPoints[i] = spawnPoint.transform;
            
            Debug.Log($"[GameSceneController] Created spawn point at {xPositions[i]}, {yPosition}");
        }
    }
    
    /// <summary>
    /// Configure the enemy spawner with prefabs and waves using reflection.
    /// </summary>
    private void ConfigureEnemySpawner()
    {
        var spawnerType = typeof(EnemySpawner);
        
        // Set spawn points
        var spawnPointsField = spawnerType.GetField("spawnPoints", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (spawnPointsField != null && enemySpawnPoints != null)
        {
            spawnPointsField.SetValue(enemySpawner, enemySpawnPoints);
            Debug.Log($"[GameSceneController] Configured {enemySpawnPoints.Length} spawn points");
        }
        
        // Set enemy prefabs
        var prefabsField = spawnerType.GetField("enemyPrefabs",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (prefabsField != null && enemyPrefabs != null)
        {
            prefabsField.SetValue(enemySpawner, enemyPrefabs);
            Debug.Log($"[GameSceneController] Configured {enemyPrefabs.Length} enemy prefabs");
        }
        
        // Set waves
        var wavesField = spawnerType.GetField("waves",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (wavesField != null && waves != null)
        {
            wavesField.SetValue(enemySpawner, waves);
            Debug.Log($"[GameSceneController] Configured {waves.Length} waves");
        }
    }
}
