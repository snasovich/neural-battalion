using UnityEngine;

namespace NeuralBattalion.Terrain
{
    /// <summary>
    /// Manages visible level boundaries.
    /// Creates a visible border around the playable area that prevents tanks from leaving.
    /// </summary>
    public class LevelBoundary : MonoBehaviour
    {
        [Header("Boundary Settings")]
        [SerializeField] private int gridWidth = 26;
        [SerializeField] private int gridHeight = 26;
        [SerializeField] private float cellSize = 1f;
        [SerializeField] private float borderThickness = 0.5f;

        [Header("Visual Settings")]
        [SerializeField] private Color borderColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        [SerializeField] private int sortingOrder = -1; // Behind everything

        private GameObject boundaryContainer;

        public Bounds LevelBounds { get; private set; }

        private void Awake()
        {
            CreateBoundary();
        }

        /// <summary>
        /// Initialize the boundary with specific dimensions.
        /// </summary>
        /// <param name="width">Grid width.</param>
        /// <param name="height">Grid height.</param>
        /// <param name="tileSize">Size of each tile/cell.</param>
        public void Initialize(int width, int height, float tileSize = 1f)
        {
            gridWidth = width;
            gridHeight = height;
            cellSize = tileSize;

            // Clean up existing boundary if any
            if (boundaryContainer != null)
            {
                Destroy(boundaryContainer);
            }

            CreateBoundary();
        }

        /// <summary>
        /// Create the boundary visual and colliders.
        /// </summary>
        private void CreateBoundary()
        {
            boundaryContainer = new GameObject("LevelBoundary");
            boundaryContainer.transform.parent = transform;
            boundaryContainer.transform.localPosition = Vector3.zero;

            float levelWidth = gridWidth * cellSize;
            float levelHeight = gridHeight * cellSize;

            // Calculate level bounds (play area)
            Vector2 center = new Vector2(levelWidth / 2f, levelHeight / 2f);
            Vector2 size = new Vector2(levelWidth, levelHeight);
            LevelBounds = new Bounds(center, size);

            // Create four border walls
            CreateBorderWall("TopBorder", 
                new Vector2(levelWidth / 2f, levelHeight + borderThickness / 2f),
                new Vector2(levelWidth + borderThickness * 2, borderThickness));

            CreateBorderWall("BottomBorder",
                new Vector2(levelWidth / 2f, -borderThickness / 2f),
                new Vector2(levelWidth + borderThickness * 2, borderThickness));

            CreateBorderWall("LeftBorder",
                new Vector2(-borderThickness / 2f, levelHeight / 2f),
                new Vector2(borderThickness, levelHeight));

            CreateBorderWall("RightBorder",
                new Vector2(levelWidth + borderThickness / 2f, levelHeight / 2f),
                new Vector2(borderThickness, levelHeight));

            Debug.Log($"[LevelBoundary] Boundary created - Size: {levelWidth}x{levelHeight}, Center: {center}");
        }

        /// <summary>
        /// Create a single border wall segment.
        /// </summary>
        private void CreateBorderWall(string name, Vector2 position, Vector2 size)
        {
            GameObject wall = new GameObject(name);
            wall.transform.parent = boundaryContainer.transform;
            wall.transform.localPosition = position;
            wall.tag = "Wall"; // Use Wall tag for collision detection

            // Add sprite renderer for visual
            SpriteRenderer sr = wall.AddComponent<SpriteRenderer>();
            sr.sprite = CreateBorderSprite(Mathf.RoundToInt(size.x * 100), Mathf.RoundToInt(size.y * 100));
            sr.color = borderColor;
            sr.sortingOrder = sortingOrder;

            // Add box collider for physics
            BoxCollider2D collider = wall.AddComponent<BoxCollider2D>();
            collider.size = size;
            collider.isTrigger = false; // Solid wall
        }

        /// <summary>
        /// Create a simple colored sprite for the border.
        /// </summary>
        private Sprite CreateBorderSprite(int width, int height)
        {
            Texture2D texture = new Texture2D(width, height);
            Color[] pixels = new Color[width * height];
            
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.white; // Will be tinted by sprite renderer
            }
            
            texture.SetPixels(pixels);
            texture.Apply();
            texture.filterMode = FilterMode.Point;

            return Sprite.Create(
                texture,
                new Rect(0, 0, width, height),
                new Vector2(0.5f, 0.5f),
                100f // pixels per unit
            );
        }

        /// <summary>
        /// Check if a position is within the level bounds.
        /// </summary>
        /// <param name="position">World position to check.</param>
        /// <returns>True if position is within bounds.</returns>
        public bool IsWithinBounds(Vector2 position)
        {
            return LevelBounds.Contains(position);
        }

        /// <summary>
        /// Clamp a position to be within the level bounds.
        /// </summary>
        /// <param name="position">World position to clamp.</param>
        /// <returns>Clamped position within bounds.</returns>
        public Vector2 ClampToBounds(Vector2 position)
        {
            float minX = LevelBounds.min.x;
            float maxX = LevelBounds.max.x;
            float minY = LevelBounds.min.y;
            float maxY = LevelBounds.max.y;

            return new Vector2(
                Mathf.Clamp(position.x, minX, maxX),
                Mathf.Clamp(position.y, minY, maxY)
            );
        }

        /// <summary>
        /// Check if a tank-sized area would be within bounds.
        /// </summary>
        /// <param name="position">Center position of the tank.</param>
        /// <param name="tankSize">Size of the tank.</param>
        /// <returns>True if tank would be fully within bounds.</returns>
        public bool IsTankWithinBounds(Vector2 position, float tankSize = 0.8f)
        {
            float halfSize = tankSize * 0.5f;
            
            // Check if all corners would be within bounds
            Vector2[] corners = new Vector2[]
            {
                position + new Vector2(halfSize, halfSize),
                position + new Vector2(halfSize, -halfSize),
                position + new Vector2(-halfSize, halfSize),
                position + new Vector2(-halfSize, -halfSize)
            };

            foreach (var corner in corners)
            {
                if (!IsWithinBounds(corner))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Get the closest point on the boundary from a position.
        /// </summary>
        /// <param name="position">World position.</param>
        /// <returns>Closest point within bounds.</returns>
        public Vector2 GetClosestPointOnBoundary(Vector2 position)
        {
            return LevelBounds.ClosestPoint(position);
        }

#if UNITY_EDITOR
        /// <summary>
        /// Draw boundary gizmos in the editor.
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (LevelBounds.size == Vector3.zero)
            {
                float levelWidth = gridWidth * cellSize;
                float levelHeight = gridHeight * cellSize;
                Vector2 center = new Vector2(levelWidth / 2f, levelHeight / 2f);
                Vector2 size = new Vector2(levelWidth, levelHeight);
                LevelBounds = new Bounds(center, size);
            }

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(LevelBounds.center, LevelBounds.size);
        }
#endif
    }
}
