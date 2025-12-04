using UnityEngine;
using NeuralBattalion.Combat;
using NeuralBattalion.Data;
using NeuralBattalion.Core.Events;

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

        private Rigidbody2D rb;
        private Vector2 moveDirection;
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
            if (moveDirection == Vector2.zero) return;

            // Tank moves in 4 directions only (classic Battle City style)
            Vector2 snappedDirection = SnapToCardinalDirection(moveDirection);

            // Rotate to face movement direction
            float targetAngle = Mathf.Atan2(snappedDirection.y, snappedDirection.x) * Mathf.Rad2Deg - 90f;
            float currentAngle = rb.rotation;
            float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, rotationSpeed * Time.fixedDeltaTime);
            rb.MoveRotation(newAngle);

            // Move forward
            Vector2 movement = snappedDirection * moveSpeed * speedModifier * Time.fixedDeltaTime;
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
            weapon?.Fire(transform.position, transform.up, true);

            EventBus.Publish(new ProjectileFiredEvent
            {
                IsPlayer = true,
                Position = transform.position,
                Direction = transform.up
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
    }
}
