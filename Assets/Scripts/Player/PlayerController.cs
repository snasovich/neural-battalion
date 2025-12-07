using UnityEngine;
using NeuralBattalion.Combat;
using NeuralBattalion.Data;
using NeuralBattalion.Core.Events;
using NeuralBattalion.Terrain;
using NeuralBattalion.Utility;
using NeuralBattalion.Enemy;

namespace NeuralBattalion.Player
{
    /// <summary>
    /// Main controller for the player tank.
    /// Responsibilities:
    /// - Handle tank movement and rotation
    /// - Manage shooting mechanics
    /// - Coordinate with other player components
    /// - Apply power-ups
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Tank Data")]
        [SerializeField] private TankData tankData;

        [Header("Components")]
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private Weapon weapon;
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float rotationSpeed = 180f;

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
        private bool canShoot = true;
        private float lastShotTime;
        private bool isInvulnerable;

        // Current modifiers from power-ups
        private float speedModifier = 1f;
        private float fireRateModifier = 1f;

        public TankData TankData => tankData;
        public bool IsInvulnerable => isInvulnerable;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f; // 2D top-down, no gravity
            rb.freezeRotation = true;
            rb.bodyType = RigidbodyType2D.Kinematic; // Prevent physics-based pushing
            
            // Keep transform rotation at zero - directional sprites handle visual direction
            transform.rotation = Quaternion.identity;
            rb.rotation = 0f;

            if (tankData != null)
            {
                ApplyTankData();
            }
        }

        private void Start()
        {
            if (playerInput == null)
            {
                playerInput = GetComponent<PlayerInput>();
            }

            // Find TerrainManager
            terrainManager = FindObjectOfType<TerrainManager>();
            if (terrainManager == null)
            {
                Debug.LogWarning("[PlayerController] TerrainManager not found - collision detection will be disabled");
            }

            // Find LevelBoundary
            levelBoundary = FindObjectOfType<LevelBoundary>();
            if (levelBoundary == null)
            {
                Debug.LogWarning("[PlayerController] LevelBoundary not found - boundary collision will be disabled");
            }

            // Create directional sprites
            CreateDirectionalSprites();

            EventBus.Publish(new PlayerSpawnedEvent { Lives = playerHealth?.CurrentLives ?? 3 });
        }

        private void Update()
        {
            if (playerInput == null) return;

            // Get input
            moveDirection = playerInput.GetMoveDirection();

            // Handle shooting
            if (playerInput.IsFirePressed() && CanShoot())
            {
                Fire();
            }
        }

        private void FixedUpdate()
        {
            Move();
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

            if (weapon != null)
            {
                weapon.Configure(tankData);
            }

            // Only recreate sprites if they haven't been created or color changed
            Color newColor = tankData?.TankColor ?? Color.green;
            bool needsRecreate = directionalSprites == null || currentSpriteColor != newColor;
            
            if (needsRecreate)
            {
                CreateDirectionalSprites();
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

            Color tankColor = tankData?.TankColor ?? Color.green;
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
            if (directionalSprites == null)
            {
                Debug.LogError("[PlayerController] UpdateSpriteDirection: directionalSprites is NULL!");
                return;
            }
            
            if (spriteRenderer == null)
            {
                Debug.LogError("[PlayerController] UpdateSpriteDirection: spriteRenderer is NULL!");
                return;
            }

            currentDirection = direction;
            Sprite sprite = directionalSprites.GetSprite(direction);
            if (sprite == null)
            {
                Debug.LogError($"[PlayerController] UpdateSpriteDirection: Got NULL sprite for direction {direction}!");
                return;
            }
            
            spriteRenderer.sprite = sprite;
            Debug.Log($"[PlayerController] Updated sprite to direction {direction}");
        }

        /// <summary>
        /// Set tank data at runtime.
        /// </summary>
        /// <param name="data">Tank data to apply.</param>
        public void SetTankData(TankData data)
        {
            tankData = data;
            if (tankData != null)
            {
                ApplyTankData();
            }
        }

        /// <summary>
        /// Move the tank based on input.
        /// </summary>
        private void Move()
        {
            // Always update facing direction if there's input, even if not moving
            if (moveDirection != Vector2.zero)
            {
                // Tank moves in 4 directions only (classic Battle City style)
                Vector2 snappedDirection = SnapToCardinalDirection(moveDirection);
                
                // Update facing direction
                lastFacingDirection = snappedDirection;

                // Update sprite direction based on facing
                TankSpriteManager.Direction newDirection = TankSpriteManager.GetDirectionFromVector(snappedDirection);
                Debug.Log($"[PlayerController] Move: snappedDirection={snappedDirection}, newDirection={newDirection}, currentDirection={currentDirection}");
                if (newDirection != currentDirection)
                {
                    Debug.Log($"[PlayerController] Direction changed from {currentDirection} to {newDirection}, calling UpdateSpriteDirection");
                    UpdateSpriteDirection(newDirection);
                }
                else
                {
                    Debug.Log($"[PlayerController] Direction unchanged, staying at {currentDirection}");
                }

                // Calculate intended movement
                Vector2 movement = snappedDirection * moveSpeed * speedModifier * Time.fixedDeltaTime;
                Vector2 targetPosition = rb.position + movement;

                // Check if movement is valid (no terrain, tank collision, or boundary)
                if (CanMoveTo(targetPosition))
                {
                    rb.MovePosition(targetPosition);
                }
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
        /// Check if the player can fire.
        /// </summary>
        private bool CanShoot()
        {
            if (!canShoot) return false;

            float fireRate = (tankData?.FireRate ?? 0.5f) / fireRateModifier;
            return Time.time - lastShotTime >= fireRate;
        }

        /// <summary>
        /// Fire a projectile.
        /// </summary>
        private void Fire()
        {
            lastShotTime = Time.time;
            
            if (weapon == null)
            {
                Debug.LogError("[PlayerController] Weapon is NULL! Cannot fire projectile.");
                return;
            }
            
            // Fire in the direction the tank is facing (from lastFacingDirection)
            weapon.Fire(transform.position, lastFacingDirection, true);

            EventBus.Publish(new ProjectileFiredEvent
            {
                IsPlayer = true,
                Position = transform.position,
                Direction = lastFacingDirection
            });
        }

        /// <summary>
        /// Take damage from an attack.
        /// </summary>
        /// <param name="damage">Amount of damage.</param>
        public void TakeDamage(int damage)
        {
            if (isInvulnerable) return;

            playerHealth?.TakeDamage(damage);
        }

        /// <summary>
        /// Apply a speed boost power-up.
        /// </summary>
        /// <param name="multiplier">Speed multiplier.</param>
        /// <param name="duration">Duration in seconds.</param>
        public void ApplySpeedBoost(float multiplier, float duration)
        {
            StartCoroutine(SpeedBoostCoroutine(multiplier, duration));
        }

        private System.Collections.IEnumerator SpeedBoostCoroutine(float multiplier, float duration)
        {
            speedModifier = multiplier;
            yield return new WaitForSeconds(duration);
            speedModifier = 1f;
        }

        /// <summary>
        /// Apply a fire rate boost power-up.
        /// </summary>
        /// <param name="multiplier">Fire rate multiplier.</param>
        /// <param name="duration">Duration in seconds.</param>
        public void ApplyFireRateBoost(float multiplier, float duration)
        {
            StartCoroutine(FireRateBoostCoroutine(multiplier, duration));
        }

        private System.Collections.IEnumerator FireRateBoostCoroutine(float multiplier, float duration)
        {
            fireRateModifier = multiplier;
            yield return new WaitForSeconds(duration);
            fireRateModifier = 1f;
        }

        /// <summary>
        /// Apply invulnerability (shield power-up).
        /// </summary>
        /// <param name="duration">Duration in seconds.</param>
        public void ApplyShield(float duration)
        {
            StartCoroutine(ShieldCoroutine(duration));
        }

        private System.Collections.IEnumerator ShieldCoroutine(float duration)
        {
            isInvulnerable = true;
            // TODO: Show shield visual effect
            yield return new WaitForSeconds(duration);
            isInvulnerable = false;
        }

        /// <summary>
        /// Enable or disable shooting.
        /// </summary>
        /// <param name="enabled">Whether shooting is enabled.</param>
        public void SetShootingEnabled(bool enabled)
        {
            canShoot = enabled;
        }

        /// <summary>
        /// Respawn the player at a specific position.
        /// </summary>
        /// <param name="position">Spawn position.</param>
        public void Respawn(Vector2 position)
        {
            transform.position = position;
            transform.rotation = Quaternion.identity;

            // Brief invulnerability on respawn
            ApplyShield(3f);

            EventBus.Publish(new PlayerRespawnEvent { RemainingLives = playerHealth?.CurrentLives ?? 0 });
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
