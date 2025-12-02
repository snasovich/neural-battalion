using UnityEngine;
using NeuralBattalion.Terrain;

namespace NeuralBattalion.Data
{
    /// <summary>
    /// ScriptableObject for level configuration.
    /// Contains tile grid, spawn points, and level settings.
    /// </summary>
    [CreateAssetMenu(fileName = "NewLevelData", menuName = "Neural Battalion/Level Data")]
    public class LevelData : ScriptableObject
    {
        [Header("Level Info")]
        [SerializeField] private string levelName = "Stage 1";
        [SerializeField] private int levelNumber = 1;
        [SerializeField] private Sprite previewImage;

        [Header("Grid Settings")]
        [SerializeField] private int gridWidth = 26;
        [SerializeField] private int gridHeight = 26;

        [Header("Tile Data")]
        [SerializeField, TextArea(10, 30)] private string tileDataString;

        [Header("Spawn Points")]
        [SerializeField] private Vector2Int playerSpawnPoint = new Vector2Int(9, 0);
        [SerializeField] private Vector2Int[] enemySpawnPoints = new Vector2Int[]
        {
            new Vector2Int(0, 25),
            new Vector2Int(12, 25),
            new Vector2Int(25, 25)
        };

        [Header("Base Position")]
        [SerializeField] private Vector2Int basePosition = new Vector2Int(12, 0);

        [Header("Wave Data")]
        [SerializeField] private WaveData[] waves;

        [Header("Level Settings")]
        [SerializeField] private float timeLimit = 0f; // 0 = no limit
        [SerializeField] private int bonusScore = 1000;

        // Properties
        public string LevelName => levelName;
        public int LevelNumber => levelNumber;
        public Sprite PreviewImage => previewImage;
        public int GridWidth => gridWidth;
        public int GridHeight => gridHeight;
        public string TileDataString => tileDataString;
        public Vector2Int PlayerSpawnPoint => playerSpawnPoint;
        public Vector2Int[] EnemySpawnPoints => enemySpawnPoints;
        public Vector2Int BasePosition => basePosition;
        public WaveData[] Waves => waves;
        public float TimeLimit => timeLimit;
        public int BonusScore => bonusScore;

        /// <summary>
        /// Parse tile data string to 2D grid.
        /// Each character represents a tile type.
        /// </summary>
        public TileType[,] ParseTileData()
        {
            TileType[,] grid = new TileType[gridWidth, gridHeight];

            if (string.IsNullOrEmpty(tileDataString))
            {
                return grid;
            }

            string[] rows = tileDataString.Split('\n');

            for (int y = 0; y < Mathf.Min(rows.Length, gridHeight); y++)
            {
                string row = rows[gridHeight - 1 - y]; // Flip Y for Unity coordinates
                for (int x = 0; x < Mathf.Min(row.Length, gridWidth); x++)
                {
                    grid[x, y] = TileTypeHelper.FromChar(row[x]);
                }
            }

            return grid;
        }

        /// <summary>
        /// Get total number of enemies in this level.
        /// </summary>
        public int GetTotalEnemies()
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

        /// <summary>
        /// Validate level data.
        /// </summary>
        public bool IsValid()
        {
            if (gridWidth <= 0 || gridHeight <= 0) return false;
            if (waves == null || waves.Length == 0) return false;
            if (enemySpawnPoints == null || enemySpawnPoints.Length == 0) return false;

            return true;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Generate tile data string from grid (for editor).
        /// </summary>
        public void SetTileDataFromGrid(TileType[,] grid)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            for (int y = gridHeight - 1; y >= 0; y--)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    sb.Append(TileTypeHelper.ToChar(grid[x, y]));
                }
                if (y > 0) sb.AppendLine();
            }

            tileDataString = sb.ToString();
        }
#endif
    }
}
