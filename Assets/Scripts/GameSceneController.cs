using UnityEngine;
using NeuralBattalion.Data;
using NeuralBattalion.Terrain;
using NeuralBattalion.Player;
using NeuralBattalion.Combat;
using NeuralBattalion.Enemy;
using NeuralBattalion.Utility;

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
    
    [Header("Player Settings")]
    [SerializeField] private GameObject playerTankPrefab;
    [SerializeField] private TankData playerTankData;
    
    [Header("Combat Settings")]
    [SerializeField] private GameObject projectilePrefab;
    
    private GameObject playerInstance;
    private LevelData currentLevel;
    
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
        
        currentLevel = levelData;
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
        
        // Spawn player tank
        SpawnPlayer(levelData);
        
        Debug.Log("[GameSceneController] Level initialized successfully");
    }
    
    /// <summary>
    /// Spawn the player tank at the level's spawn point.
    /// </summary>
    private void SpawnPlayer(LevelData levelData)
    {
        // Load player tank data if not assigned
        if (playerTankData == null)
        {
            playerTankData = Resources.Load<TankData>("TankData/PlayerTankData");
            if (playerTankData == null)
            {
                Debug.LogWarning("[GameSceneController] PlayerTankData not found in Resources. Player may not spawn correctly.");
            }
        }
        
        // Get spawn position from level data (grid coordinates)
        Vector2Int spawnGridPos = levelData.PlayerSpawnPoint;
        
        // Convert grid position to world position
        // Assuming each tile is 1 unit and grid starts at (0,0)
        Vector3 spawnWorldPos = new Vector3(spawnGridPos.x + 0.5f, spawnGridPos.y + 0.5f, 0f);
        
        // Create player tank (from prefab or at runtime)
        if (playerTankPrefab != null)
        {
            // Use assigned prefab
            playerInstance = Instantiate(playerTankPrefab, spawnWorldPos, Quaternion.identity);
            playerInstance.name = "PlayerTank";
        }
        else
        {
            // Create player tank at runtime if no prefab assigned
            playerInstance = CreatePlayerTankAtPosition(spawnWorldPos);
        }
        
        // Configure player controller with tank data
        PlayerController playerController = playerInstance.GetComponent<PlayerController>();
        if (playerController != null && playerTankData != null)
        {
            playerController.SetTankData(playerTankData);
        }
        
        if (playerController != null)
        {
            Debug.Log($"[GameSceneController] Player tank spawned at position {spawnWorldPos}");
        }
        else
        {
            Debug.LogWarning("[GameSceneController] PlayerController component not found on player tank instance!");
        }
    }
    
    /// <summary>
    /// Create a player tank at runtime at a specific position.
    /// This is a fallback for when no prefab is assigned in the inspector.
    /// </summary>
    private GameObject CreatePlayerTankAtPosition(Vector3 position)
    {
        // Create a new GameObject for the player tank
        GameObject tankGO = new GameObject("PlayerTank");
        tankGO.transform.position = position;
        
        // Add Rigidbody2D
        Rigidbody2D rb = tankGO.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        
        // Add BoxCollider2D
        BoxCollider2D collider = tankGO.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(0.8f, 0.8f);
        
        // Add SpriteRenderer
        SpriteRenderer spriteRenderer = tankGO.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = CreatePlayerSprite();
        spriteRenderer.sortingLayerName = "Default";
        spriteRenderer.sortingOrder = 10;
        
        // Add PlayerInput
        // PlayerInput will auto-configure using legacy Input Manager (WASD/Arrow keys)
        tankGO.AddComponent<PlayerInput>();
        
        // Add PlayerHealth
        // PlayerHealth uses default values: 3 starting lives, 2s respawn delay
        PlayerHealth health = tankGO.AddComponent<PlayerHealth>();
        
        // Add Weapon
        GameObject weaponGO = new GameObject("Weapon");
        weaponGO.transform.SetParent(tankGO.transform);
        weaponGO.transform.localPosition = Vector3.zero;
        Weapon weapon = weaponGO.AddComponent<Weapon>();
        
        // Configure weapon with projectile prefab
        ConfigureWeapon(weapon);
        
        // Add PlayerController (this should be last as it references other components)
        PlayerController controller = tankGO.AddComponent<PlayerController>();
        
        // Set player tag for collision detection
        tankGO.tag = "Player";
        
        Debug.Log("[GameSceneController] Created player tank at runtime");
        
        return tankGO;
    }
    
    /// <summary>
    /// Create a simple sprite for the player tank.
    /// </summary>
    private Sprite CreatePlayerSprite()
    {
        // Create a simple colored sprite (yellow for player)
        int size = 32;
        Texture2D texture = new Texture2D(size, size);
        Color playerColor = new Color(1f, 0.92f, 0.016f, 1f); // Yellow
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // Create a simple tank shape
                bool isEdge = x < 2 || x >= size - 2 || y < 2 || y >= size - 2;
                bool isTurret = x >= size / 2 - 4 && x < size / 2 + 4 && y >= size / 2;
                
                if (isTurret || (isEdge && !isTurret))
                {
                    texture.SetPixel(x, y, playerColor);
                }
                else
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }
        
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
    
    /// <summary>
    /// Configure the weapon with a projectile prefab.
    /// </summary>
    private void ConfigureWeapon(Weapon weapon)
    {
        if (weapon == null) return;
        
        // Create or load projectile prefab
        if (projectilePrefab == null)
        {
            Debug.Log("[GameSceneController] Creating projectile prefab at runtime");
            projectilePrefab = PrefabFactory.CreateProjectilePrefab(true);
        }
        
        // Use reflection to set the private projectilePrefab field
        var weaponType = typeof(Weapon);
        var projectilePrefabField = weaponType.GetField("projectilePrefab", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (projectilePrefabField != null)
        {
            projectilePrefabField.SetValue(weapon, projectilePrefab);
            Debug.Log("[GameSceneController] Configured weapon with projectile prefab");
        }
        else
        {
            Debug.LogError("[GameSceneController] Failed to configure weapon - projectilePrefab field not found");
        }
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
                Debug.Log("[GameSceneController] EnemySpawner not found - creating one");
                GameObject spawnerObj = new GameObject("EnemySpawner");
                enemySpawner = spawnerObj.AddComponent<EnemySpawner>();
                Debug.Log("[GameSceneController] EnemySpawner created successfully");
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
    /// Configure the enemy spawner with prefabs and waves.
    /// </summary>
    private void ConfigureEnemySpawner()
    {
        // Configure spawn points
        if (enemySpawnPoints != null && enemySpawnPoints.Length > 0)
        {
            enemySpawner.ConfigureSpawnPoints(enemySpawnPoints);
            Debug.Log($"[GameSceneController] Configured {enemySpawnPoints.Length} spawn points");
        }
        
        // Configure enemy prefabs
        if (enemyPrefabs != null && enemyPrefabs.Length > 0)
        {
            enemySpawner.ConfigureEnemyPrefabs(enemyPrefabs);
            Debug.Log($"[GameSceneController] Configured {enemyPrefabs.Length} enemy prefabs");
        }
        
        // Configure waves
        if (waves != null && waves.Length > 0)
        {
            enemySpawner.ConfigureWaves(waves);
            Debug.Log($"[GameSceneController] Configured {waves.Length} waves");
        }
    }
}
