using UnityEngine;
using NeuralBattalion.Core;
using NeuralBattalion.Core.Events;

namespace NeuralBattalion.Terrain
{
    /// <summary>
    /// Controls the player base that must be protected.
    /// Responsibilities:
    /// - Track base health
    /// - Handle destruction (game over trigger)
    /// - Visual feedback for damage
    /// </summary>
    public class BaseController : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private int maxHealth = 1;
        [SerializeField] private bool isInvulnerable = false;

        [Header("Visuals")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite normalSprite;
        [SerializeField] private Sprite destroyedSprite;
        [SerializeField] private GameObject destructionEffectPrefab;

        [Header("Audio")]
        [SerializeField] private AudioClip destroyedSound;

        [Header("Collider")]
        [SerializeField] private Collider2D baseCollider;

        private int currentHealth;
        private bool isDestroyed = false;

        public bool IsDestroyed => isDestroyed;
        public int CurrentHealth => currentHealth;

        private void Awake()
        {
            currentHealth = maxHealth;

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (baseCollider == null)
            {
                baseCollider = GetComponent<Collider2D>();
            }
        }

        private void Start()
        {
            ResetBase();
        }

        /// <summary>
        /// Reset the base for a new level.
        /// </summary>
        public void ResetBase()
        {
            currentHealth = maxHealth;
            isDestroyed = false;
            isInvulnerable = false;

            if (spriteRenderer != null && normalSprite != null)
            {
                spriteRenderer.sprite = normalSprite;
            }

            if (baseCollider != null)
            {
                baseCollider.enabled = true;
            }

            gameObject.SetActive(true);
        }

        /// <summary>
        /// Take damage from a projectile.
        /// </summary>
        /// <param name="damage">Amount of damage.</param>
        /// <returns>True if base was destroyed.</returns>
        public bool TakeDamage(int damage = 1)
        {
            if (isDestroyed || isInvulnerable) return false;

            currentHealth -= damage;

            if (currentHealth <= 0)
            {
                Destroy();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Destroy the base (game over).
        /// </summary>
        private void Destroy()
        {
            if (isDestroyed) return;

            isDestroyed = true;

            // Visual feedback
            if (spriteRenderer != null && destroyedSprite != null)
            {
                spriteRenderer.sprite = destroyedSprite;
            }

            // Play effects
            if (destroyedSound != null)
            {
                AudioSource.PlayClipAtPoint(destroyedSound, transform.position);
            }

            if (destructionEffectPrefab != null)
            {
                Instantiate(destructionEffectPrefab, transform.position, Quaternion.identity);
            }

            // Disable collider
            if (baseCollider != null)
            {
                baseCollider.enabled = false;
            }

            // Publish event
            EventBus.Publish(new BaseDestroyedEvent());

            // Trigger game over
            GameManager.Instance?.EndGame(false);
        }

        /// <summary>
        /// Set invulnerability (for Shovel power-up).
        /// </summary>
        /// <param name="invulnerable">Whether base is invulnerable.</param>
        public void SetInvulnerable(bool invulnerable)
        {
            isInvulnerable = invulnerable;
        }

        /// <summary>
        /// Temporarily make base invulnerable.
        /// </summary>
        /// <param name="duration">Duration in seconds.</param>
        public void TemporaryInvulnerability(float duration)
        {
            StartCoroutine(InvulnerabilityCoroutine(duration));
        }

        private System.Collections.IEnumerator InvulnerabilityCoroutine(float duration)
        {
            isInvulnerable = true;
            yield return new WaitForSeconds(duration);
            isInvulnerable = false;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Check if hit by enemy projectile
            if (other.CompareTag("EnemyProjectile"))
            {
                TakeDamage(1);
            }
        }
    }
}
