using UnityEngine;
using NeuralBattalion.Core;
using NeuralBattalion.Core.Events;

namespace NeuralBattalion.Player
{
    /// <summary>
    /// Manages player health and lives.
    /// Responsibilities:
    /// - Track current health
    /// - Manage player lives
    /// - Handle death and respawning
    /// - Emit health-related events
    /// </summary>
    public class PlayerHealth : MonoBehaviour
    {
        [Header("Health Settings")]
        [SerializeField] private int maxHealth = 1;
        [SerializeField] private int startingLives = 3;
        [SerializeField] private float respawnDelay = 2f;

        [Header("Spawn Point")]
        [SerializeField] private Transform spawnPoint;

        [Header("References")]
        [SerializeField] private PlayerController playerController;

        public int CurrentHealth { get; private set; }
        public int MaxHealth => maxHealth;
        public int CurrentLives { get; private set; }
        public bool IsDead => CurrentHealth <= 0;

        private bool isRespawning;

        private void Awake()
        {
            CurrentHealth = maxHealth;
            CurrentLives = startingLives;
        }

        private void Start()
        {
            if (playerController == null)
            {
                playerController = GetComponent<PlayerController>();
            }
        }

        /// <summary>
        /// Reset health and lives for a new game.
        /// </summary>
        public void ResetForNewGame()
        {
            CurrentHealth = maxHealth;
            CurrentLives = startingLives;
            isRespawning = false;
        }

        /// <summary>
        /// Take damage from an attack.
        /// </summary>
        /// <param name="damage">Amount of damage to take.</param>
        public void TakeDamage(int damage)
        {
            if (IsDead || isRespawning) return;
            if (playerController != null && playerController.IsInvulnerable) return;

            CurrentHealth -= damage;
            CurrentHealth = Mathf.Max(0, CurrentHealth);

            EventBus.Publish(new PlayerDamagedEvent
            {
                CurrentHealth = CurrentHealth,
                MaxHealth = maxHealth,
                DamageAmount = damage
            });

            if (CurrentHealth <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// Heal the player.
        /// </summary>
        /// <param name="amount">Amount to heal.</param>
        public void Heal(int amount)
        {
            int previousHealth = CurrentHealth;
            CurrentHealth = Mathf.Min(CurrentHealth + amount, maxHealth);

            if (CurrentHealth != previousHealth)
            {
                // Could emit a heal event if needed
            }
        }

        /// <summary>
        /// Add an extra life.
        /// </summary>
        public void AddLife()
        {
            CurrentLives++;
            // Could have a max lives limit

            EventBus.Publish(new PowerUpCollectedEvent { Type = PowerUpType.ExtraLife });
        }

        /// <summary>
        /// Handle player death.
        /// </summary>
        private void Die()
        {
            CurrentLives--;

            EventBus.Publish(new PlayerDeathEvent { RemainingLives = CurrentLives });

            if (CurrentLives > 0)
            {
                StartCoroutine(RespawnCoroutine());
            }
            else
            {
                // Game Over
                GameManager.Instance?.EndGame(false);
            }
        }

        /// <summary>
        /// Respawn after delay.
        /// </summary>
        private System.Collections.IEnumerator RespawnCoroutine()
        {
            isRespawning = true;

            // Hide player or play death animation
            // TODO: Death effect

            yield return new WaitForSeconds(respawnDelay);

            // Reset health
            CurrentHealth = maxHealth;
            isRespawning = false;

            // Respawn at spawn point
            Vector2 respawnPosition = spawnPoint != null ?
                (Vector2)spawnPoint.position :
                Vector2.zero;

            playerController?.Respawn(respawnPosition);
        }

        /// <summary>
        /// Force immediate respawn (for testing/debugging).
        /// </summary>
        public void ForceRespawn()
        {
            if (!IsDead && !isRespawning) return;

            StopAllCoroutines();
            CurrentHealth = maxHealth;
            isRespawning = false;

            Vector2 respawnPosition = spawnPoint != null ?
                (Vector2)spawnPoint.position :
                Vector2.zero;

            playerController?.Respawn(respawnPosition);
        }

        /// <summary>
        /// Set the spawn point for respawning.
        /// </summary>
        /// <param name="newSpawnPoint">New spawn point transform.</param>
        public void SetSpawnPoint(Transform newSpawnPoint)
        {
            spawnPoint = newSpawnPoint;
        }

        /// <summary>
        /// Get health as a percentage (0-1).
        /// </summary>
        /// <returns>Health percentage.</returns>
        public float GetHealthPercentage()
        {
            return (float)CurrentHealth / maxHealth;
        }
    }
}
