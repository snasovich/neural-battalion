using UnityEngine;
using NeuralBattalion.Utility;
using NeuralBattalion.Core.Events;

namespace NeuralBattalion.Combat
{
    /// <summary>
    /// Projectile behavior for bullets.
    /// Responsibilities:
    /// - Move in a straight line
    /// - Detect collisions
    /// - Apply damage on hit
    /// - Self-destruct after lifetime or hit
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class Projectile : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float speed = 10f;
        [SerializeField] private int damage = 1;
        [SerializeField] private float lifetime = 3f;
        [SerializeField] private bool canDestroySteel = false;

        [Header("Visuals")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private GameObject hitEffectPrefab;

        [Header("Audio")]
        [SerializeField] private AudioClip hitSound;

        private Rigidbody2D rb;
        private Vector2 direction;
        private bool isPlayerProjectile;
        private bool hasHit = false;
        private float spawnTime;
        private Weapon ownerWeapon;

        public bool IsPlayerProjectile => isPlayerProjectile;
        public int Damage => damage;
        public bool CanDestroySteel => canDestroySteel;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.isKinematic = true;
        }

        private void Start()
        {
            spawnTime = Time.time;
        }

        private void Update()
        {
            // Check lifetime
            if (Time.time - spawnTime > lifetime)
            {
                Despawn();
            }
        }

        private void FixedUpdate()
        {
            if (hasHit) return;

            // Move projectile
            Vector2 movement = direction * speed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + movement);
        }

        /// <summary>
        /// Initialize and fire the projectile.
        /// </summary>
        /// <param name="startPosition">Starting position.</param>
        /// <param name="fireDirection">Direction to travel.</param>
        /// <param name="fromPlayer">Whether fired by player.</param>
        /// <param name="projectileSpeed">Override speed (optional).</param>
        /// <param name="projectileDamage">Override damage (optional).</param>
        public void Fire(Vector2 startPosition, Vector2 fireDirection, bool fromPlayer,
                        float projectileSpeed = -1, int projectileDamage = -1)
        {
            transform.position = startPosition;
            direction = fireDirection.normalized;
            isPlayerProjectile = fromPlayer;
            hasHit = false;
            spawnTime = Time.time;

            if (projectileSpeed > 0) speed = projectileSpeed;
            if (projectileDamage > 0) damage = projectileDamage;

            // Rotate sprite to face direction
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            // Set tag for collision detection
            gameObject.tag = fromPlayer ? "PlayerProjectile" : "EnemyProjectile";

            gameObject.SetActive(true);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (hasHit) return;

            // Ignore collision with shooter
            if (isPlayerProjectile && other.CompareTag("Player")) return;
            if (!isPlayerProjectile && other.CompareTag("Enemy")) return;

            // Check what we hit
            HandleCollision(other);
        }

        /// <summary>
        /// Handle collision with various objects.
        /// </summary>
        private void HandleCollision(Collider2D other)
        {
            hasHit = true;

            Vector2 hitPosition = transform.position;

            // Hit player
            if (other.CompareTag("Player") && !isPlayerProjectile)
            {
                var playerController = other.GetComponent<Player.PlayerController>();
                playerController?.TakeDamage(damage);
                OnHit(hitPosition, false, true);
                return;
            }

            // Hit enemy
            if (other.CompareTag("Enemy") && isPlayerProjectile)
            {
                var enemyController = other.GetComponent<Enemy.EnemyController>();
                enemyController?.TakeDamage(damage);
                OnHit(hitPosition, false, true);
                return;
            }

            // Hit destructible terrain
            var destructible = other.GetComponent<Terrain.DestructibleTerrain>();
            if (destructible != null)
            {
                destructible.TakeDamage(damage, canDestroySteel);
                OnHit(hitPosition, true, false);
                return;
            }

            // Hit base
            var baseController = other.GetComponent<Terrain.BaseController>();
            if (baseController != null)
            {
                baseController.TakeDamage(damage);
                OnHit(hitPosition, true, false);
                return;
            }

            // Hit other projectile (projectiles can destroy each other)
            if (other.CompareTag("PlayerProjectile") || other.CompareTag("EnemyProjectile"))
            {
                var otherProjectile = other.GetComponent<Projectile>();
                if (otherProjectile != null && otherProjectile.isPlayerProjectile != isPlayerProjectile)
                {
                    otherProjectile.Despawn();
                    OnHit(hitPosition, false, false);
                    return;
                }
            }

            // Hit wall or other obstacle
            if (other.CompareTag("Wall") || other.CompareTag("Obstacle"))
            {
                OnHit(hitPosition, true, false);
                return;
            }

            // Default: hit something, despawn
            OnHit(hitPosition, true, false);
        }

        /// <summary>
        /// Handle hit effects.
        /// </summary>
        /// <param name="position">Hit position.</param>
        /// <param name="hitTerrain">Whether hit terrain.</param>
        /// <param name="hitTank">Whether hit a tank.</param>
        private void OnHit(Vector2 position, bool hitTerrain, bool hitTank)
        {
            // Spawn hit effect
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, position, Quaternion.identity);
            }

            // Play sound
            if (hitSound != null)
            {
                AudioSource.PlayClipAtPoint(hitSound, position);
            }

            // Publish event
            EventBus.Publish(new ProjectileHitEvent
            {
                Position = position,
                HitTerrain = hitTerrain,
                HitTank = hitTank
            });

            Despawn();
        }

        /// <summary>
        /// Despawn the projectile.
        /// </summary>
        public void Despawn()
        {
            if (hasHit) return;

            hasHit = true;
            gameObject.SetActive(false);

            // Notify owner weapon
            ownerWeapon?.OnProjectileDestroyed();
            ownerWeapon = null;

            // Try to return to pool first, otherwise destroy
            var pool = ObjectPool.Instance;
            if (pool != null)
            {
                pool.Return(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Set whether this projectile can destroy steel.
        /// </summary>
        public void SetCanDestroySteel(bool canDestroy)
        {
            canDestroySteel = canDestroy;
        }

        /// <summary>
        /// Set the weapon that fired this projectile.
        /// Used to notify when projectile is destroyed for tracking.
        /// </summary>
        public void SetOwnerWeapon(Weapon weapon)
        {
            ownerWeapon = weapon;
        }
    }
}
