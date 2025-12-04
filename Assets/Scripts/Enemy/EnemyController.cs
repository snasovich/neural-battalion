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
        [SerializeField] private float collisionCheckRadius = 0.4f;

        private Rigidbody2D rb;
        private TerrainManager terrainManager;
        private Collider2D[] overlapBuffer = new Collider2D[10];
        private Vector2 moveDirection;
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

            // Apply visual properties
            if (spriteRenderer != null)
            {
                if (tankData.TankSprite != null)
                {
                    spriteRenderer.sprite = tankData.TankSprite;
                }
                else
                {
                    // Create a simple colored sprite if no sprite is provided
                    spriteRenderer.sprite = CreateSimpleSprite();
                }
                spriteRenderer.color = tankData.TankColor;
            }

            if (weapon != null)
            {
                weapon.Configure(tankData);
            }
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

            // Rotate to face movement direction
            float targetAngle = Mathf.Atan2(snappedDirection.y, snappedDirection.x) * Mathf.Rad2Deg - 90f;
            float currentAngle = rb.rotation;
            float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, rotationSpeed * Time.fixedDeltaTime);
            rb.MoveRotation(newAngle);

            // Calculate intended movement
            Vector2 movement = snappedDirection * moveSpeed * Time.fixedDeltaTime;
            Vector2 targetPosition = rb.position + movement;

            // Check if movement is valid (no terrain or tank collision)
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
            // Check terrain collision for tank-sized area
            if (terrainManager != null)
            {
                if (!terrainManager.IsTankPositionPassable(targetPosition))
                {
                    return false;
                }
            }

            // Check tank-to-tank collision using overlap check (non-allocating)
            int numOverlaps = Physics2D.OverlapCircleNonAlloc(targetPosition, collisionCheckRadius, overlapBuffer);
            for (int i = 0; i < numOverlaps; i++)
            {
                Collider2D overlap = overlapBuffer[i];
                
                // Skip self
                if (overlap.gameObject == gameObject)
                    continue;

                // Check if it's another tank by checking for tank components
                if (overlap.GetComponent<PlayerController>() != null || 
                    overlap.GetComponent<EnemyController>() != null)
                {
                    return false;
                }
            }

            return true;
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
            weapon?.Fire(transform.position, transform.up, false);

            EventBus.Publish(new ProjectileFiredEvent
            {
                IsPlayer = false,
                Position = transform.position,
                Direction = transform.up
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
    }
}
