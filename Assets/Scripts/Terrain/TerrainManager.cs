using UnityEngine;
using UnityEngine.Tilemaps;
using NeuralBattalion.Data;
using NeuralBattalion.Core.Events;
using NeuralBattalion.Utility;

namespace NeuralBattalion.Terrain
{
    /// <summary>
    /// Manages the terrain grid and tile interactions.
    /// Responsibilities:
    /// - Manage tile grid
    /// - Handle terrain queries
    /// - Build levels from data
    /// - Coordinate terrain destruction
    /// </summary>
    public class TerrainManager : MonoBehaviour
    {
        [Header("Tilemaps")]
        [SerializeField] private Tilemap groundTilemap;
        [SerializeField] private Tilemap obstacleTilemap;
        [SerializeField] private Tilemap decorationTilemap;

        [Header("Tile Assets")]
        [SerializeField] private TileBase brickTile;
        [SerializeField] private TileBase steelTile;
        [SerializeField] private TileBase waterTile;
        [SerializeField] private TileBase treeTile;
        [SerializeField] private TileBase iceTile;
        [SerializeField] private TileBase groundTile;

        [Header("Grid Settings")]
        [SerializeField] private int gridWidth = 26;
        [SerializeField] private int gridHeight = 26;
        [SerializeField] private Vector2 cellSize = Vector2.one;

        [Header("Destructible Terrain")]
        [SerializeField] private GameObject destructibleTerrainPrefab;

        [Header("Level Boundary")]
        [SerializeField] private bool createBoundary = true;
        [SerializeField] private Color boundaryColor = new Color(0.3f, 0.3f, 0.3f, 1f);

        // Internal grid data
        private TileType[,] tileGrid;
        private int[,] tileHealth; // For destructible tiles
        private GameObject[,] destructibleObjects; // Track destructible terrain GameObjects
        private LevelBoundary levelBoundary;

        // Pre-allocated buffer for tank collision checks
        private Vector2[] cornerBuffer = new Vector2[4];

        public int GridWidth => gridWidth;
        public int GridHeight => gridHeight;

        private void Awake()
        {
            Debug.Log("[TerrainManager] Awake - Initializing");
            InitializeGrid();
            // Create fallback tiles if assets are not assigned
            // This must happen in Awake() before any Start() methods call BuildLevel()
            CreateFallbackTilesIfNeeded();
            Debug.Log("[TerrainManager] Awake - Complete");
        }

        private void OnEnable()
        {
            // Subscribe to terrain destruction events
            EventBus.Subscribe<TerrainDestroyedEvent>(OnTerrainDestroyed);
            Debug.Log("[TerrainManager] Subscribed to TerrainDestroyedEvent");
        }

        private void OnDisable()
        {
            // Unsubscribe from events
            EventBus.Unsubscribe<TerrainDestroyedEvent>(OnTerrainDestroyed);
            Debug.Log("[TerrainManager] Unsubscribed from TerrainDestroyedEvent");
        }

        /// <summary>
        /// Handle terrain destroyed event - removes tile from tilemap.
        /// </summary>
        private void OnTerrainDestroyed(TerrainDestroyedEvent evt)
        {
            // Remove the tile from the tilemap
            Vector3Int tilemapPos = new Vector3Int(evt.GridPosition.x, evt.GridPosition.y, 0);
            
            if (obstacleTilemap != null)
            {
                obstacleTilemap.SetTile(tilemapPos, null);
            }
            
            // Update internal grid
            if (IsValidGridPosition(evt.GridPosition))
            {
                tileGrid[evt.GridPosition.x, evt.GridPosition.y] = TileType.Empty;
                tileHealth[evt.GridPosition.x, evt.GridPosition.y] = 0;
                destructibleObjects[evt.GridPosition.x, evt.GridPosition.y] = null;
            }
        }

        /// <summary>
        /// Initialize the internal grid data.
        /// </summary>
        private void InitializeGrid()
        {
            tileGrid = new TileType[gridWidth, gridHeight];
            tileHealth = new int[gridWidth, gridHeight];
            destructibleObjects = new GameObject[gridWidth, gridHeight];

            // Initialize all tiles as empty
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    tileGrid[x, y] = TileType.Empty;
                    tileHealth[x, y] = 0;
                    destructibleObjects[x, y] = null;
                }
            }
            
            // Setup colliders on tilemaps
            SetupTilemapColliders();
        }
        
        /// <summary>
        /// Setup colliders on tilemaps for projectile collision.
        /// </summary>
        private void SetupTilemapColliders()
        {
            // NOTE: We are NOT adding TilemapCollider2D to the obstacle tilemap
            // because we use individual DestructibleTerrain GameObjects with their own colliders
            // for brick and steel tiles. The TilemapCollider2D would interfere with those
            // individual colliders and prevent proper collision detection.
            
            if (obstacleTilemap != null)
            {
                // Remove TilemapCollider2D if it exists (might have been added in editor)
                var collider = obstacleTilemap.GetComponent<TilemapCollider2D>();
                if (collider != null)
                {
                    Debug.Log("[TerrainManager] Removing TilemapCollider2D from obstacle tilemap (using individual terrain colliders instead)");
                    Destroy(collider);
                }
                
                // Also remove CompositeCollider2D if it exists
                var compositeCollider = obstacleTilemap.GetComponent<CompositeCollider2D>();
                if (compositeCollider != null)
                {
                    Debug.Log("[TerrainManager] Removing CompositeCollider2D from obstacle tilemap");
                    Destroy(compositeCollider);
                }
                
                Debug.Log("[TerrainManager] Obstacle tilemap configured for individual terrain colliders");
            }
        }

        /// <summary>
        /// Build the level from level data.
        /// </summary>
        /// <param name="levelData">Level data to build from.</param>
        public void BuildLevel(LevelData levelData)
        {
            if (levelData == null)
            {
                Debug.LogError("[TerrainManager] No level data provided");
                return;
            }

            Debug.Log($"[TerrainManager] BuildLevel started - Tiles available: brick={brickTile != null}, steel={steelTile != null}, water={waterTile != null}, tree={treeTile != null}, ice={iceTile != null}, ground={groundTile != null}");

            ClearLevel();

            // Parse level data and place tiles
            TileType[,] grid = levelData.ParseTileData();
            
            Debug.Log($"[TerrainManager] Building level: {levelData.LevelName}");
            Debug.Log($"[TerrainManager] Grid size: {levelData.GridWidth}x{levelData.GridHeight}");
            
            int tilesSet = 0;
            for (int x = 0; x < levelData.GridWidth; x++)
            {
                for (int y = 0; y < levelData.GridHeight; y++)
                {
                    TileType tileType = grid[x, y];
                    if (tileType != TileType.Empty)
                    {
                        SetTile(new Vector2Int(x, y), tileType);
                        tilesSet++;
                    }
                }
            }
            
            // Create level boundary
            if (createBoundary)
            {
                CreateLevelBoundary(levelData.GridWidth, levelData.GridHeight);
            }
            
            Debug.Log($"[TerrainManager] Level built successfully - {tilesSet} tiles placed");
        }

        /// <summary>
        /// Clear all tiles from the level.
        /// </summary>
        public void ClearLevel()
        {
            groundTilemap?.ClearAllTiles();
            obstacleTilemap?.ClearAllTiles();
            decorationTilemap?.ClearAllTiles();

            InitializeGrid();
        }

        /// <summary>
        /// Set a tile at the specified grid position.
        /// </summary>
        /// <param name="gridPos">Grid position.</param>
        /// <param name="tileType">Type of tile to place.</param>
        public void SetTile(Vector2Int gridPos, TileType tileType)
        {
            if (!IsValidGridPosition(gridPos)) return;

            tileGrid[gridPos.x, gridPos.y] = tileType;
            tileHealth[gridPos.x, gridPos.y] = GetTileMaxHealth(tileType);

            Vector3Int tilemapPos = new Vector3Int(gridPos.x, gridPos.y, 0);

            // Remove existing destructible object if present
            if (destructibleObjects[gridPos.x, gridPos.y] != null)
            {
                Destroy(destructibleObjects[gridPos.x, gridPos.y]);
                destructibleObjects[gridPos.x, gridPos.y] = null;
            }

            switch (tileType)
            {
                case TileType.Brick:
                    obstacleTilemap?.SetTile(tilemapPos, brickTile);
                    CreateDestructibleTerrainObject(gridPos, TileType.Brick);
                    break;
                case TileType.Steel:
                    obstacleTilemap?.SetTile(tilemapPos, steelTile);
                    CreateDestructibleTerrainObject(gridPos, TileType.Steel);
                    break;
                case TileType.Water:
                    groundTilemap?.SetTile(tilemapPos, waterTile);
                    break;
                case TileType.Trees:
                    decorationTilemap?.SetTile(tilemapPos, treeTile);
                    break;
                case TileType.Ice:
                    groundTilemap?.SetTile(tilemapPos, iceTile);
                    break;
                case TileType.Empty:
                    obstacleTilemap?.SetTile(tilemapPos, null);
                    groundTilemap?.SetTile(tilemapPos, groundTile);
                    break;
            }
        }
        
        /// <summary>
        /// Create a destructible terrain GameObject at the specified position.
        /// </summary>
        private void CreateDestructibleTerrainObject(Vector2Int gridPos, TileType tileType)
        {
            Vector2 worldPos = GridToWorldPosition(gridPos);
            
            GameObject terrainObj = new GameObject($"DestructibleTerrain_{gridPos.x}_{gridPos.y}");
            terrainObj.transform.position = worldPos;
            terrainObj.transform.parent = obstacleTilemap?.transform;
            
            // Add Rigidbody2D - needed for trigger collision detection with projectiles
            Rigidbody2D rb = terrainObj.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static; // Static since terrain doesn't move
            rb.gravityScale = 0f;
            
            // Add BoxCollider2D as trigger
            BoxCollider2D collider = terrainObj.AddComponent<BoxCollider2D>();
            collider.size = cellSize * 0.95f; // Slightly smaller to prevent overlaps
            collider.isTrigger = true;
            
            // Add SpriteRenderer for visual feedback
            SpriteRenderer spriteRenderer = terrainObj.AddComponent<SpriteRenderer>();
            spriteRenderer.sortingLayerName = "Default";
            spriteRenderer.sortingOrder = 5; // Above ground, below tanks
            
            // Create damage state sprites
            Color baseColor = tileType == TileType.Brick ? new Color(0.8f, 0.3f, 0.2f) : new Color(0.6f, 0.6f, 0.6f);
            Color damagedColor = tileType == TileType.Brick ? new Color(0.5f, 0.2f, 0.1f) : new Color(0.4f, 0.4f, 0.4f);
            
            Sprite[] damageSprites = new Sprite[2];
            damageSprites[0] = SpriteGenerator.CreateColoredSprite(baseColor, 16, 16, 100f); // Full health
            damageSprites[1] = SpriteGenerator.CreateColoredSprite(damagedColor, 16, 16, 100f); // Damaged
            
            // Set initial sprite
            spriteRenderer.sprite = damageSprites[0];
            
            // Add DestructibleTerrain component and initialize it
            DestructibleTerrain destructible = terrainObj.AddComponent<DestructibleTerrain>();
            int health = GetTileMaxHealth(tileType);
            destructible.Initialize(tileType, health, spriteRenderer, damageSprites);
            
            // Store reference
            destructibleObjects[gridPos.x, gridPos.y] = terrainObj;
            
            // Tag appropriately
            terrainObj.tag = "DestructibleTerrain";
        }

        /// <summary>
        /// Get the tile type at a grid position.
        /// </summary>
        /// <param name="gridPos">Grid position.</param>
        /// <returns>Tile type at position.</returns>
        public TileType GetTile(Vector2Int gridPos)
        {
            if (!IsValidGridPosition(gridPos)) return TileType.Empty;
            return tileGrid[gridPos.x, gridPos.y];
        }

        /// <summary>
        /// Get the tile type at a world position.
        /// </summary>
        /// <param name="worldPos">World position.</param>
        /// <returns>Tile type at position.</returns>
        public TileType GetTileAtWorldPosition(Vector2 worldPos)
        {
            Vector2Int gridPos = WorldToGridPosition(worldPos);
            return GetTile(gridPos);
        }

        /// <summary>
        /// Damage a tile at the specified position.
        /// </summary>
        /// <param name="gridPos">Grid position.</param>
        /// <param name="damage">Amount of damage.</param>
        /// <param name="canDestroySteel">Whether steel can be destroyed.</param>
        /// <returns>True if tile was destroyed.</returns>
        public bool DamageTile(Vector2Int gridPos, int damage = 1, bool canDestroySteel = false)
        {
            if (!IsValidGridPosition(gridPos)) return false;

            TileType tileType = tileGrid[gridPos.x, gridPos.y];

            // Check if tile is destructible
            if (!IsTileDestructible(tileType, canDestroySteel)) return false;

            tileHealth[gridPos.x, gridPos.y] -= damage;

            if (tileHealth[gridPos.x, gridPos.y] <= 0)
            {
                DestroyTile(gridPos);
                return true;
            }

            // Update tile visual for partial damage
            UpdateTileVisual(gridPos);
            return false;
        }

        /// <summary>
        /// Destroy a tile at the specified position.
        /// </summary>
        /// <param name="gridPos">Grid position.</param>
        private void DestroyTile(Vector2Int gridPos)
        {
            TileType previousType = tileGrid[gridPos.x, gridPos.y];

            // Destroy the destructible terrain object
            if (destructibleObjects[gridPos.x, gridPos.y] != null)
            {
                Destroy(destructibleObjects[gridPos.x, gridPos.y]);
                destructibleObjects[gridPos.x, gridPos.y] = null;
            }

            SetTile(gridPos, TileType.Empty);

            // Map TileType to TileDestroyedType for the event
            TileDestroyedType destroyedType = previousType switch
            {
                TileType.Steel => TileDestroyedType.Steel,
                _ => TileDestroyedType.Brick
            };

            EventBus.Publish(new TerrainDestroyedEvent
            {
                GridPosition = gridPos,
                TileType = destroyedType
            });
        }

        /// <summary>
        /// Update tile visual for partial damage.
        /// </summary>
        private void UpdateTileVisual(Vector2Int gridPos)
        {
            // TODO: Update tile sprite to show damage
            // This could use different tiles for damaged states
        }

        /// <summary>
        /// Check if a tile type is destructible.
        /// </summary>
        public bool IsTileDestructible(TileType tileType, bool canDestroySteel = false)
        {
            return tileType switch
            {
                TileType.Brick => true,
                TileType.Steel => canDestroySteel,
                _ => false
            };
        }

        /// <summary>
        /// Check if a tile type is passable (tanks can move through).
        /// </summary>
        public bool IsTilePassable(TileType tileType)
        {
            return tileType switch
            {
                TileType.Empty => true,
                TileType.Trees => true,
                TileType.Ice => true,
                _ => false
            };
        }

        /// <summary>
        /// Check if a tile type allows bullets to pass through.
        /// </summary>
        public bool IsTileBulletPassable(TileType tileType)
        {
            return tileType switch
            {
                TileType.Empty => true,
                TileType.Water => true,
                TileType.Trees => true,
                TileType.Ice => true,
                _ => false
            };
        }

        /// <summary>
        /// Get maximum health for a tile type.
        /// </summary>
        private int GetTileMaxHealth(TileType tileType)
        {
            return tileType switch
            {
                TileType.Brick => 2, // Two hits to fully destroy
                TileType.Steel => 4, // Four hits with power-up
                _ => 0
            };
        }

        /// <summary>
        /// Check if a grid position is valid.
        /// </summary>
        public bool IsValidGridPosition(Vector2Int gridPos)
        {
            return gridPos.x >= 0 && gridPos.x < gridWidth &&
                   gridPos.y >= 0 && gridPos.y < gridHeight;
        }

        /// <summary>
        /// Convert world position to grid position.
        /// </summary>
        public Vector2Int WorldToGridPosition(Vector2 worldPos)
        {
            return new Vector2Int(
                Mathf.FloorToInt(worldPos.x / cellSize.x),
                Mathf.FloorToInt(worldPos.y / cellSize.y)
            );
        }

        /// <summary>
        /// Convert grid position to world position (center of cell).
        /// </summary>
        public Vector2 GridToWorldPosition(Vector2Int gridPos)
        {
            return new Vector2(
                (gridPos.x + 0.5f) * cellSize.x,
                (gridPos.y + 0.5f) * cellSize.y
            );
        }

        /// <summary>
        /// Check if a world position is passable.
        /// </summary>
        public bool IsPositionPassable(Vector2 worldPos)
        {
            TileType tileType = GetTileAtWorldPosition(worldPos);
            return IsTilePassable(tileType);
        }

        /// <summary>
        /// Check if a tank-sized area at a world position is passable.
        /// Checks multiple points around the tank's bounds.
        /// </summary>
        /// <param name="worldPos">Center position of the tank.</param>
        /// <param name="tankSize">Size of the tank (width and height).</param>
        /// <returns>True if all checked positions are passable.</returns>
        public bool IsTankPositionPassable(Vector2 worldPos, float tankSize = 0.8f)
        {
            // Check center
            if (!IsPositionPassable(worldPos))
                return false;

            // Check corners using pre-allocated buffer
            float halfSize = tankSize * 0.5f;
            cornerBuffer[0] = worldPos + new Vector2(halfSize, halfSize);
            cornerBuffer[1] = worldPos + new Vector2(halfSize, -halfSize);
            cornerBuffer[2] = worldPos + new Vector2(-halfSize, halfSize);
            cornerBuffer[3] = worldPos + new Vector2(-halfSize, -halfSize);

            for (int i = 0; i < 4; i++)
            {
                if (!IsPositionPassable(cornerBuffer[i]))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Fortify the base with steel walls (Shovel power-up).
        /// </summary>
        /// <param name="baseTiles">Array of grid positions around the base.</param>
        /// <param name="duration">Duration before reverting to brick.</param>
        public void FortifyBase(Vector2Int[] baseTiles, float duration)
        {
            foreach (var pos in baseTiles)
            {
                SetTile(pos, TileType.Steel);
            }

            StartCoroutine(RevertBaseFortification(baseTiles, duration));
        }

        private System.Collections.IEnumerator RevertBaseFortification(Vector2Int[] baseTiles, float duration)
        {
            yield return new WaitForSeconds(duration);

            foreach (var pos in baseTiles)
            {
                if (GetTile(pos) == TileType.Steel)
                {
                    SetTile(pos, TileType.Brick);
                }
            }
        }

        /// <summary>
        /// Create or update the level boundary.
        /// </summary>
        /// <param name="width">Grid width.</param>
        /// <param name="height">Grid height.</param>
        private void CreateLevelBoundary(int width, int height)
        {
            // Find existing boundary or create new one
            if (levelBoundary == null)
            {
                levelBoundary = FindObjectOfType<LevelBoundary>();
            }

            if (levelBoundary == null)
            {
                // Create new boundary game object
                GameObject boundaryObj = new GameObject("LevelBoundary");
                boundaryObj.transform.parent = transform;
                boundaryObj.transform.localPosition = Vector3.zero;
                levelBoundary = boundaryObj.AddComponent<LevelBoundary>();
            }

            // Initialize boundary with level dimensions
            levelBoundary.Initialize(width, height, cellSize.x);
            
            Debug.Log($"[TerrainManager] Level boundary created for {width}x{height} grid");
        }

        /// <summary>
        /// Create simple fallback tiles if tile assets are not assigned.
        /// This creates basic colored tiles using generated sprites.
        /// </summary>
        private void CreateFallbackTilesIfNeeded()
        {
            if (brickTile == null)
            {
                Tile tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = SpriteGenerator.CreateColoredSprite(new Color(0.8f, 0.4f, 0.2f), 16, 16, 16f); // Orange-brown
                brickTile = tile;
                Debug.Log("[TerrainManager] Created fallback brick tile");
            }

            if (steelTile == null)
            {
                Tile tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = SpriteGenerator.CreateColoredSprite(new Color(0.7f, 0.7f, 0.7f), 16, 16, 16f); // Gray
                steelTile = tile;
                Debug.Log("[TerrainManager] Created fallback steel tile");
            }

            if (waterTile == null)
            {
                Tile tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = SpriteGenerator.CreateColoredSprite(new Color(0.2f, 0.4f, 0.8f), 16, 16, 16f); // Blue
                waterTile = tile;
                Debug.Log("[TerrainManager] Created fallback water tile");
            }

            if (treeTile == null)
            {
                Tile tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = SpriteGenerator.CreateColoredSprite(new Color(0.2f, 0.6f, 0.2f), 16, 16, 16f); // Green
                treeTile = tile;
                Debug.Log("[TerrainManager] Created fallback tree tile");
            }

            if (iceTile == null)
            {
                Tile tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = SpriteGenerator.CreateColoredSprite(new Color(0.7f, 0.9f, 1.0f), 16, 16, 16f); // Light blue
                iceTile = tile;
                Debug.Log("[TerrainManager] Created fallback ice tile");
            }

            if (groundTile == null)
            {
                Tile tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = SpriteGenerator.CreateColoredSprite(new Color(0.2f, 0.2f, 0.2f), 16, 16, 16f); // Dark gray
                groundTile = tile;
                Debug.Log("[TerrainManager] Created fallback ground tile");
            }
        }
    }
}
