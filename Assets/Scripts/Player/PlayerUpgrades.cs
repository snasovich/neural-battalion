using UnityEngine;
using NeuralBattalion.Core.Events;

namespace NeuralBattalion.Player
{
    /// <summary>
    /// Manages player upgrades and power-ups.
    /// Responsibilities:
    /// - Track upgrade levels
    /// - Apply upgrade effects
    /// - Handle power-up collection
    /// - Persist upgrade state during level
    /// </summary>
    public class PlayerUpgrades : MonoBehaviour
    {
        [Header("Upgrade Settings")]
        [SerializeField] private int maxUpgradeLevel = 4;

        [Header("Power-up Durations")]
        [SerializeField] private float speedBoostDuration = 10f;
        [SerializeField] private float fireRateBoostDuration = 10f;
        [SerializeField] private float shieldDuration = 10f;

        [Header("Power-up Multipliers")]
        [SerializeField] private float speedBoostMultiplier = 1.5f;
        [SerializeField] private float fireRateBoostMultiplier = 2f;

        [Header("References")]
        [SerializeField] private PlayerController playerController;
        [SerializeField] private PlayerHealth playerHealth;

        // Current upgrade levels
        public int SpeedLevel { get; private set; } = 0;
        public int FireRateLevel { get; private set; } = 0;
        public int DamageLevel { get; private set; } = 0;

        // Active power-ups
        public bool HasShield { get; private set; }
        public bool HasSpeedBoost { get; private set; }
        public bool HasFireRateBoost { get; private set; }

        private void Awake()
        {
            if (playerController == null)
            {
                playerController = GetComponent<PlayerController>();
            }

            if (playerHealth == null)
            {
                playerHealth = GetComponent<PlayerHealth>();
            }
        }

        /// <summary>
        /// Reset all upgrades for a new game.
        /// </summary>
        public void ResetUpgrades()
        {
            SpeedLevel = 0;
            FireRateLevel = 0;
            DamageLevel = 0;
            HasShield = false;
            HasSpeedBoost = false;
            HasFireRateBoost = false;

            StopAllCoroutines();
        }

        /// <summary>
        /// Collect a power-up.
        /// </summary>
        /// <param name="powerUpType">Type of power-up collected.</param>
        public void CollectPowerUp(PowerUpType powerUpType)
        {
            EventBus.Publish(new PowerUpCollectedEvent { Type = powerUpType });

            switch (powerUpType)
            {
                case PowerUpType.ExtraLife:
                    playerHealth?.AddLife();
                    break;

                case PowerUpType.SpeedBoost:
                    ApplySpeedBoost();
                    break;

                case PowerUpType.FireRateBoost:
                    ApplyFireRateBoost();
                    break;

                case PowerUpType.Shield:
                    ApplyShield();
                    break;

                case PowerUpType.Bomb:
                    DestroyAllEnemies();
                    break;

                case PowerUpType.Timer:
                    FreezeAllEnemies();
                    break;

                case PowerUpType.Shovel:
                    FortifyBase();
                    break;
            }
        }

        /// <summary>
        /// Upgrade a specific stat permanently (until death).
        /// </summary>
        /// <param name="upgradeType">Type of upgrade.</param>
        public void ApplyPermanentUpgrade(UpgradeType upgradeType)
        {
            switch (upgradeType)
            {
                case UpgradeType.Speed:
                    if (SpeedLevel < maxUpgradeLevel)
                    {
                        SpeedLevel++;
                        EventBus.Publish(new PlayerUpgradeEvent { UpgradeType = UpgradeType.Speed, Level = SpeedLevel });
                    }
                    break;

                case UpgradeType.FireRate:
                    if (FireRateLevel < maxUpgradeLevel)
                    {
                        FireRateLevel++;
                        EventBus.Publish(new PlayerUpgradeEvent { UpgradeType = UpgradeType.FireRate, Level = FireRateLevel });
                    }
                    break;

                case UpgradeType.Damage:
                    if (DamageLevel < maxUpgradeLevel)
                    {
                        DamageLevel++;
                        EventBus.Publish(new PlayerUpgradeEvent { UpgradeType = UpgradeType.Damage, Level = DamageLevel });
                    }
                    break;
            }
        }

        /// <summary>
        /// Apply temporary speed boost.
        /// </summary>
        private void ApplySpeedBoost()
        {
            if (HasSpeedBoost)
            {
                // Refresh duration
                StopCoroutine(nameof(SpeedBoostCoroutine));
            }

            StartCoroutine(SpeedBoostCoroutine());
        }

        private System.Collections.IEnumerator SpeedBoostCoroutine()
        {
            HasSpeedBoost = true;
            playerController?.ApplySpeedBoost(speedBoostMultiplier, speedBoostDuration);
            yield return new WaitForSeconds(speedBoostDuration);
            HasSpeedBoost = false;
        }

        /// <summary>
        /// Apply temporary fire rate boost.
        /// </summary>
        private void ApplyFireRateBoost()
        {
            if (HasFireRateBoost)
            {
                StopCoroutine(nameof(FireRateBoostCoroutine));
            }

            StartCoroutine(FireRateBoostCoroutine());
        }

        private System.Collections.IEnumerator FireRateBoostCoroutine()
        {
            HasFireRateBoost = true;
            playerController?.ApplyFireRateBoost(fireRateBoostMultiplier, fireRateBoostDuration);
            yield return new WaitForSeconds(fireRateBoostDuration);
            HasFireRateBoost = false;
        }

        /// <summary>
        /// Apply temporary shield.
        /// </summary>
        private void ApplyShield()
        {
            if (HasShield)
            {
                StopCoroutine(nameof(ShieldCoroutine));
            }

            StartCoroutine(ShieldCoroutine());
        }

        private System.Collections.IEnumerator ShieldCoroutine()
        {
            HasShield = true;
            playerController?.ApplyShield(shieldDuration);
            yield return new WaitForSeconds(shieldDuration);
            HasShield = false;
        }

        /// <summary>
        /// Destroy all enemies on screen (Bomb power-up).
        /// </summary>
        private void DestroyAllEnemies()
        {
            // TODO: Implement through EventBus or direct reference to EnemySpawner
            // EventBus.Publish(new BombActivatedEvent());
            Debug.Log("[PlayerUpgrades] Bomb activated - destroy all enemies");
        }

        /// <summary>
        /// Freeze all enemies temporarily (Timer power-up).
        /// </summary>
        private void FreezeAllEnemies()
        {
            // TODO: Implement through EventBus or direct reference to EnemySpawner
            // EventBus.Publish(new TimerActivatedEvent { Duration = 10f });
            Debug.Log("[PlayerUpgrades] Timer activated - freeze all enemies");
        }

        /// <summary>
        /// Fortify the base with steel walls (Shovel power-up).
        /// </summary>
        private void FortifyBase()
        {
            // TODO: Implement through EventBus or direct reference to TerrainManager
            // EventBus.Publish(new ShovelActivatedEvent { Duration = 15f });
            Debug.Log("[PlayerUpgrades] Shovel activated - fortify base");
        }

        /// <summary>
        /// Get current speed multiplier including upgrades.
        /// </summary>
        /// <returns>Speed multiplier.</returns>
        public float GetSpeedMultiplier()
        {
            float baseMultiplier = 1f + (SpeedLevel * 0.1f);
            return HasSpeedBoost ? baseMultiplier * speedBoostMultiplier : baseMultiplier;
        }

        /// <summary>
        /// Get current fire rate multiplier including upgrades.
        /// </summary>
        /// <returns>Fire rate multiplier.</returns>
        public float GetFireRateMultiplier()
        {
            float baseMultiplier = 1f + (FireRateLevel * 0.15f);
            return HasFireRateBoost ? baseMultiplier * fireRateBoostMultiplier : baseMultiplier;
        }

        /// <summary>
        /// Get current damage multiplier including upgrades.
        /// </summary>
        /// <returns>Damage multiplier.</returns>
        public float GetDamageMultiplier()
        {
            return 1f + (DamageLevel * 0.25f);
        }
    }
}
