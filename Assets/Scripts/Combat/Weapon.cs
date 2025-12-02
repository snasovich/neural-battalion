using UnityEngine;
using NeuralBattalion.Data;
using NeuralBattalion.Utility;

namespace NeuralBattalion.Combat
{
    /// <summary>
    /// Weapon component for tanks.
    /// Responsibilities:
    /// - Fire projectiles
    /// - Manage fire rate
    /// - Handle weapon upgrades
    /// - Spawn projectiles from pool
    /// </summary>
    public class Weapon : MonoBehaviour
    {
        [Header("Projectile Settings")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float projectileSpeed = 10f;
        [SerializeField] private int projectileDamage = 1;

        [Header("Weapon Settings")]
        [SerializeField] private float fireRate = 0.5f;
        [SerializeField] private int maxActiveProjectiles = 1;
        [SerializeField] private bool canDestroySteel = false;

        [Header("Audio")]
        [SerializeField] private AudioClip fireSound;
        [SerializeField] private AudioSource audioSource;

        private int activeProjectiles = 0;
        private float nextFireTime = 0f;

        // Upgrade modifiers
        private float damageMultiplier = 1f;
        private float speedMultiplier = 1f;
        private float fireRateMultiplier = 1f;

        private void Awake()
        {
            if (firePoint == null)
            {
                firePoint = transform;
            }

            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }
        }

        /// <summary>
        /// Configure weapon from tank data.
        /// </summary>
        /// <param name="tankData">Tank configuration data.</param>
        public void Configure(TankData tankData)
        {
            if (tankData == null) return;

            projectileSpeed = tankData.ProjectileSpeed;
            projectileDamage = tankData.Damage;
            fireRate = tankData.FireRate;
            maxActiveProjectiles = tankData.MaxProjectiles;
        }

        /// <summary>
        /// Fire a projectile.
        /// </summary>
        /// <param name="position">Fire position.</param>
        /// <param name="direction">Fire direction.</param>
        /// <param name="isPlayer">Whether fired by player.</param>
        /// <returns>True if projectile was fired.</returns>
        public bool Fire(Vector2 position, Vector2 direction, bool isPlayer)
        {
            // Check fire rate
            if (Time.time < nextFireTime) return false;

            // Check max active projectiles
            if (activeProjectiles >= maxActiveProjectiles) return false;

            // Get projectile from pool or instantiate
            GameObject projectileObj = GetProjectile(position);
            if (projectileObj == null) return false;

            // Initialize projectile
            Projectile projectile = projectileObj.GetComponent<Projectile>();
            if (projectile != null)
            {
                float speed = projectileSpeed * speedMultiplier;
                int damage = Mathf.RoundToInt(projectileDamage * damageMultiplier);

                projectile.Fire(position, direction, isPlayer, speed, damage);
                projectile.SetCanDestroySteel(canDestroySteel);
            }

            // Update state
            nextFireTime = Time.time + (fireRate / fireRateMultiplier);
            activeProjectiles++;

            // Play sound
            PlayFireSound();

            return true;
        }

        /// <summary>
        /// Get a projectile from pool or create new.
        /// </summary>
        private GameObject GetProjectile(Vector2 position)
        {
            // Try to get from object pool
            // var projectile = ObjectPool.Instance?.Get(projectilePrefab);
            // if (projectile != null) return projectile;

            // Fallback: instantiate
            if (projectilePrefab == null)
            {
                Debug.LogError("[Weapon] No projectile prefab assigned");
                return null;
            }

            return Instantiate(projectilePrefab, position, Quaternion.identity);
        }

        /// <summary>
        /// Called when a projectile is destroyed (for tracking).
        /// </summary>
        public void OnProjectileDestroyed()
        {
            activeProjectiles = Mathf.Max(0, activeProjectiles - 1);
        }

        /// <summary>
        /// Play fire sound effect.
        /// </summary>
        private void PlayFireSound()
        {
            if (fireSound == null) return;

            if (audioSource != null)
            {
                audioSource.PlayOneShot(fireSound);
            }
            else
            {
                AudioSource.PlayClipAtPoint(fireSound, transform.position);
            }
        }

        /// <summary>
        /// Apply damage upgrade.
        /// </summary>
        public void UpgradeDamage(float multiplier)
        {
            damageMultiplier = multiplier;
        }

        /// <summary>
        /// Apply fire rate upgrade.
        /// </summary>
        public void UpgradeFireRate(float multiplier)
        {
            fireRateMultiplier = multiplier;
        }

        /// <summary>
        /// Apply speed upgrade.
        /// </summary>
        public void UpgradeSpeed(float multiplier)
        {
            speedMultiplier = multiplier;
        }

        /// <summary>
        /// Enable steel destruction (power-up).
        /// </summary>
        public void EnableSteelDestruction()
        {
            canDestroySteel = true;
        }

        /// <summary>
        /// Disable steel destruction.
        /// </summary>
        public void DisableSteelDestruction()
        {
            canDestroySteel = false;
        }

        /// <summary>
        /// Increase max active projectiles.
        /// </summary>
        public void IncreaseMaxProjectiles(int amount = 1)
        {
            maxActiveProjectiles += amount;
        }

        /// <summary>
        /// Reset all upgrades.
        /// </summary>
        public void ResetUpgrades()
        {
            damageMultiplier = 1f;
            speedMultiplier = 1f;
            fireRateMultiplier = 1f;
            canDestroySteel = false;
            maxActiveProjectiles = 1;
        }

        /// <summary>
        /// Check if weapon can fire.
        /// </summary>
        public bool CanFire()
        {
            return Time.time >= nextFireTime && activeProjectiles < maxActiveProjectiles;
        }

        /// <summary>
        /// Get time until next shot.
        /// </summary>
        public float GetCooldownRemaining()
        {
            return Mathf.Max(0, nextFireTime - Time.time);
        }
    }
}
