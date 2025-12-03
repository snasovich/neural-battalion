using UnityEngine;
using NeuralBattalion.Data;
using NeuralBattalion.Terrain;

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
}
