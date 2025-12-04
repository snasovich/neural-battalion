using UnityEngine;
using NeuralBattalion.Data;
using NeuralBattalion.Terrain;
using NeuralBattalion.Player;
using NeuralBattalion.Combat;

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
    
    [Header("Player Settings")]
    [SerializeField] private GameObject playerTankPrefab;
    [SerializeField] private TankData playerTankData;
    
    private GameObject playerInstance;
    private LevelData currentLevel;
    
    private void Start()
    {
        Debug.Log("[GameSceneController] GameScene started");
        LoadLevel();
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
        
        // Add PlayerController (this should be last as it references other components)
        PlayerController controller = tankGO.AddComponent<PlayerController>();
        
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
}
