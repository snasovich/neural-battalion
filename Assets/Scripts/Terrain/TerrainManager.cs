using UnityEngine;
using UnityEngine.Tilemaps;
using NeuralBattalion.Data;
using NeuralBattalion.Core.Events;

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

        // Internal grid data
        private TileType[,] tileGrid;
        private int[,] tileHealth; // For destructible tiles

        public int GridWidth => gridWidth;
        public int GridHeight => gridHeight;

        private void Awake()
        {
            InitializeGrid();
        }

        /// <summary>
        /// Initialize the internal grid data.
        /// </summary>
        private void InitializeGrid()
        {
            tileGrid = new TileType[gridWidth, gridHeight];
            tileHealth = new int[gridWidth, gridHeight];

            // Initialize all tiles as empty
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    tileGrid[x, y] = TileType.Empty;
                    tileHealth[x, y] = 0;
                }
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

            ClearLevel();

            // TODO: Parse level data and place tiles
            // levelData.TileData contains the grid information
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

            switch (tileType)
            {
                case TileType.Brick:
                    obstacleTilemap?.SetTile(tilemapPos, brickTile);
                    break;
                case TileType.Steel:
                    obstacleTilemap?.SetTile(tilemapPos, steelTile);
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
    }
}
