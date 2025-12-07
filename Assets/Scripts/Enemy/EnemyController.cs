using UnityEngine;
using NeuralBattalion.Combat;
using NeuralBattalion.Data;
using NeuralBattalion.Core.Events;
using NeuralBattalion.Terrain;
using NeuralBattalion.Utility;
using NeuralBattalion.Player;

namespace NeuralBattalion.Enemy
{
    /// <summary>
    /// Base controller for enemy tanks.
    /// Responsibilities:
    /// - Handle enemy movement and rotation
    /// - Manage shooting mechanics
    /// - Respond to AI commands
    /// - Handle destruction
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyController : MonoBehaviour
    {
        [Header("Tank Data")]
        [SerializeField] private TankData tankData;

        [Header("Components")]
        [SerializeField] private EnemyAI enemyAI;
        [SerializeField] private Weapon weapon;
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Header("Settings")]
        [SerializeField] private int enemyId;
        [SerializeField] private int enemyType;
        [SerializeField] private int scoreValue = 100;

        [Header("Collision Settings")]
        [SerializeField] private float collisionCheckRadius = 0.45f;
        
        // Buffer size for overlap detection - 10 is sufficient for typical scenarios where
        // a tank would rarely have more than 2-3 other tanks within collision radius
        private const int OVERLAP_BUFFER_SIZE = 10;

        private Rigidbody2D rb;
        private TerrainManager terrainManager;
        private LevelBoundary levelBoundary;
        private Collider2D[] overlapBuffer = new Collider2D[OVERLAP_BUFFER_SIZE];
        
        // Directional sprites
        private TankSpriteManager.DirectionalSprites directionalSprites;
        private TankSpriteManager.Direction currentDirection = TankSpriteManager.Direction.Up;
        
        // Cache for tank component checks
        // Note: This cache grows as new tanks are encountered. In typical gameplay with limited tanks,
        // this is not an issue. For games with many dynamically spawned/destroyed tanks, consider
        // implementing cache cleanup or using a different collision identification strategy.
        private static System.Collections.Generic.Dictionary<GameObject, bool> tankCache = 
            new System.Collections.Generic.Dictionary<GameObject, bool>();
        private Vector2 moveDirection;
        private Vector2 lastFacingDirection = Vector2.up; // Track facing direction independently
        private float moveSpeed = 3f;
        private float rotationSpeed = 180f;
        private int health = 1;
        private int maxHealth = 1;
        private bool isAlive = true;
        private bool isFrozen = false;
        private float lastShotTime;

        public int EnemyId => enemyId;
        public int EnemyType => enemyType;
        public int ScoreValue => scoreValue;
        public bool IsAlive => isAlive;
        public bool IsFrozen => isFrozen;
        public TankData TankData => tankData;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.bodyType = RigidbodyType2D.Kinematic; // Prevent physics-based pushing
            
            // Keep transform rotation at zero - directional sprites handle visual direction
            transform.rotation = Quaternion.identity;
            rb.rotation = 0f;

            // Find sprite renderer if not assigned
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (tankData != null)
            {
                ApplyTankData();
            }
        }

        private void Start()
        {
            if (enemyAI == null)
            {
                enemyAI = GetComponent<EnemyAI>();
            }

            // Find TerrainManager
            terrainManager = FindObjectOfType<TerrainManager>();
            if (terrainManager == null)
            {
                Debug.LogWarning("[EnemyController] TerrainManager not found - collision detection will be disabled");
            }

            // Find LevelBoundary
            levelBoundary = FindObjectOfType<LevelBoundary>();
            if (levelBoundary == null)
            {
                Debug.LogWarning("[EnemyController] LevelBoundary not found - boundary collision will be disabled");
            }
        }

        private void FixedUpdate()
        {
            if (!isAlive || isFrozen) return;

            Move();
        }

        /// <summary>
        /// Initialize the enemy with data.
        /// </summary>
        /// <param name="id">Unique enemy ID.</param>
        /// <param name="type">Enemy type index.</param>
        /// <param name="data">Tank configuration data.</param>
        public void Initialize(int id, int type, TankData data = null)
        {
            enemyId = id;
            enemyType = type;

            if (data != null)
            {
                tankData = data;
                ApplyTankData();
            }

            health = maxHealth;
            isAlive = true;
            isFrozen = false;

            Debug.Log($"[EnemyController] Enemy {enemyId} initialized - Type: {EnemyTypes.GetName(type)}, " +
                     $"Health: {health}, Speed: {moveSpeed}, Position: {transform.position}");

            EventBus.Publish(new EnemySpawnedEvent { EnemyId = enemyId, EnemyType = enemyType });
        }

        // Track the color used to create sprites
        private Color currentSpriteColor = Color.clear;

        /// <summary>
        /// Apply tank data configuration.
        /// </summary>
        private void ApplyTankData()
        {
            moveSpeed = tankData.MoveSpeed;
            rotationSpeed = tankData.RotationSpeed;
            maxHealth = tankData.Health;
            health = maxHealth;
            scoreValue = tankData.ScoreValue;

            // Apply visual properties and create directional sprites
            if (spriteRenderer != null)
            {
                // Only recreate sprites if they haven't been created or color changed
                Color newColor = tankData?.TankColor ?? Color.red;
                bool needsRecreate = directionalSprites == null || currentSpriteColor != newColor;
                
                if (needsRecreate)
                {
                    CreateDirectionalSprites();
                }
            }

            if (weapon != null)
            {
                weapon.Configure(tankData);
            }
        }

        /// <summary>
        /// Create directional sprites for the tank.
        /// </summary>
        private void CreateDirectionalSprites()
        {
            if (spriteRenderer == null) return;

            // Clean up old sprites before creating new ones
            if (directionalSprites != null)
            {
                TankSpriteManager.DestroyDirectionalSprites(directionalSprites);
            }

            Color tankColor = tankData?.TankColor ?? Color.red;
            currentSpriteColor = tankColor;
            directionalSprites = TankSpriteManager.CreateDirectionalSprites(tankColor, 32);

            // Set initial sprite
            UpdateSpriteDirection(currentDirection);
        }

        /// <summary>
        /// Update the sprite to match the current direction.
        /// </summary>
        private void UpdateSpriteDirection(TankSpriteManager.Direction direction)
        {
            if (directionalSprites == null || spriteRenderer == null) return;

            currentDirection = direction;
            spriteRenderer.sprite = directionalSprites.GetSprite(direction);
        }

        /// <summary>
        /// Create a simple colored square sprite for the tank.
        /// </summary>
        private Sprite CreateSimpleSprite()
        {
            int size = 32;
            Texture2D texture = new Texture2D(size, size);
            
            // Create a simple tank shape
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Create a border and fill
                    bool isEdge = x < 2 || x >= size - 2 || y < 2 || y >= size - 2;
                    bool isTurret = x >= size / 2 - 3 && x < size / 2 + 3 && y >= size / 2;
                    
                    if (isTurret || isEdge)
                    {
                        texture.SetPixel(x, y, Color.white);
                    }
                    else
                    {
                        texture.SetPixel(x, y, new Color(1f, 1f, 1f, 0.8f));
                    }
                }
            }
            
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        /// <summary>
        /// Set the movement direction (called by AI).
        /// </summary>
        /// <param name="direction">Desired movement direction.</param>
        public void SetMoveDirection(Vector2 direction)
        {
            moveDirection = direction.normalized;
        }

        /// <summary>
        /// Move the tank based on current direction.
        /// </summary>
        private void Move()
        {
            if (moveDirection == Vector2.zero) return;

            // Snap to cardinal direction
            Vector2 snappedDirection = SnapToCardinalDirection(moveDirection);
            
            // Update facing direction
            lastFacingDirection = snappedDirection;

            // Update sprite direction based on movement
            TankSpriteManager.Direction newDirection = TankSpriteManager.GetDirectionFromVector(snappedDirection);
            if (newDirection != currentDirection)
            {
                UpdateSpriteDirection(newDirection);
            }

            // Calculate intended movement
            Vector2 movement = snappedDirection * moveSpeed * Time.fixedDeltaTime;
            Vector2 targetPosition = rb.position + movement;

            // Check if movement is valid (no terrain, tank collision, or boundary)
            if (CanMoveTo(targetPosition))
            {
                rb.MovePosition(targetPosition);
            }
        }

        /// <summary>
        /// Check if the tank can move to a target position without colliding.
        /// </summary>
        /// <param name="targetPosition">The target position to check.</param>
        /// <returns>True if the position is valid.</returns>
        private bool CanMoveTo(Vector2 targetPosition)
        {
            // Check level boundaries
            if (levelBoundary != null)
            {
                if (!levelBoundary.IsTankWithinBounds(targetPosition, collisionCheckRadius * 2))
                {
                    return false;
                }
            }

            // Check terrain collision for tank-sized area
            if (terrainManager != null)
            {
                if (!terrainManager.IsTankPositionPassable(targetPosition))
                {
                    return false;
                }
            }

            // Check if target position would overlap with any tank
            int numOverlaps = Physics2D.OverlapCircleNonAlloc(targetPosition, collisionCheckRadius, overlapBuffer);
            for (int i = 0; i < numOverlaps; i++)
            {
                Collider2D overlap = overlapBuffer[i];
                if (overlap.gameObject != gameObject)
                {
                    if (IsTank(overlap.gameObject))
                    {
                        // Found a tank at target position - check if moving would decrease distance
                        Vector2 otherPos = overlap.transform.position;
                        float currentDist = Vector2.Distance(rb.position, otherPos);
                        float targetDist = Vector2.Distance(targetPosition, otherPos);
                        
                        // Only block if target position would be closer to the other tank
                        // This allows separation while preventing overlap
                        if (targetDist < currentDist)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Check if a GameObject is a tank (with caching for performance).
        /// </summary>
        private static bool IsTank(GameObject obj)
        {
            // Check cache first
            if (tankCache.TryGetValue(obj, out bool isTank))
                return isTank;

            // Not in cache, check components
            bool result = obj.GetComponent<PlayerController>() != null || 
                          obj.GetComponent<EnemyController>() != null;
            
            // Cache the result
            tankCache[obj] = result;
            return result;
        }

        /// <summary>
        /// Snap movement to 4 cardinal directions.
        /// </summary>
        private Vector2 SnapToCardinalDirection(Vector2 input)
        {
            if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
            {
                return new Vector2(Mathf.Sign(input.x), 0);
            }
            else if (Mathf.Abs(input.y) > 0)
            {
                return new Vector2(0, Mathf.Sign(input.y));
            }

            return Vector2.zero;
        }

        /// <summary>
        /// Attempt to fire a projectile (called by AI).
        /// </summary>
        /// <returns>True if shot was fired.</returns>
        public bool TryFire()
        {
            if (!isAlive || isFrozen) return false;

            float fireRate = tankData?.FireRate ?? 1f;
            if (Time.time - lastShotTime < fireRate) return false;

            lastShotTime = Time.time;
            // Fire in the direction the tank is facing (from lastFacingDirection)
            weapon?.Fire(transform.position, lastFacingDirection, false);

            EventBus.Publish(new ProjectileFiredEvent
            {
                IsPlayer = false,
                Position = transform.position,
                Direction = lastFacingDirection
            });

            return true;
        }

        /// <summary>
        /// Take damage from an attack.
        /// </summary>
        /// <param name="damage">Amount of damage.</param>
        public void TakeDamage(int damage)
        {
            if (!isAlive) return;

            health -= damage;

            if (health <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// Handle enemy death.
        /// </summary>
        private void Die()
        {
            if (!isAlive) return;

            isAlive = false;

            EventBus.Publish(new EnemyDestroyedEvent
            {
                EnemyId = enemyId,
                EnemyType = enemyType,
                ScoreValue = scoreValue
            });

            // TODO: Play death animation/effect
            // TODO: Return to pool or destroy
            Destroy(gameObject);
        }

        /// <summary>
        /// Freeze the enemy (Timer power-up).
        /// </summary>
        /// <param name="duration">Freeze duration in seconds.</param>
        public void Freeze(float duration)
        {
            if (!isAlive) return;

            StartCoroutine(FreezeCoroutine(duration));
        }

        private System.Collections.IEnumerator FreezeCoroutine(float duration)
        {
            isFrozen = true;
            yield return new WaitForSeconds(duration);
            isFrozen = false;
        }

        /// <summary>
        /// Unfreeze the enemy immediately.
        /// </summary>
        public void Unfreeze()
        {
            isFrozen = false;
            StopCoroutine(nameof(FreezeCoroutine));
        }

        /// <summary>
        /// Force destroy the enemy (Bomb power-up).
        /// </summary>
        public void ForceDestroy()
        {
            Die();
        }

        /// <summary>
        /// Get health as a percentage (0-1).
        /// </summary>
        /// <returns>Health percentage.</returns>
        public float GetHealthPercentage()
        {
            return (float)health / maxHealth;
        }

        /// <summary>
        /// Clean up resources when destroyed.
        /// </summary>
        private void OnDestroy()
        {
            // Clean up directional sprites to prevent memory leaks
            if (directionalSprites != null)
            {
                TankSpriteManager.DestroyDirectionalSprites(directionalSprites);
                directionalSprites = null;
            }
        }
    }
}
