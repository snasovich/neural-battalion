using UnityEngine;
using NeuralBattalion.Combat;
using NeuralBattalion.Data;
using NeuralBattalion.Core.Events;

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

        private Rigidbody2D rb;
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

            if (weapon != null)
            {
                weapon.Configure(tankData);
            }
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

            // Move forward
            Vector2 movement = snappedDirection * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + movement);
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
