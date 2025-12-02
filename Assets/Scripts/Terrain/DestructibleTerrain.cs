using UnityEngine;
using NeuralBattalion.Core.Events;

namespace NeuralBattalion.Terrain
{
    /// <summary>
    /// Handles destructible terrain behavior.
    /// Responsibilities:
    /// - Track damage state
    /// - Handle destruction effects
    /// - Update visuals for damage states
    /// </summary>
    public class DestructibleTerrain : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private TileType tileType = TileType.Brick;
        [SerializeField] private int maxHealth = 2;

        [Header("Visuals")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite[] damageSprites; // Sprites for each damage level
        [SerializeField] private GameObject destructionEffectPrefab;

        [Header("Audio")]
        [SerializeField] private AudioClip hitSound;
        [SerializeField] private AudioClip destroySound;

        private int currentHealth;
        private bool isDestroyed = false;

        public TileType TileType => tileType;
        public bool IsDestroyed => isDestroyed;
        public int CurrentHealth => currentHealth;

        private void Awake()
        {
            currentHealth = maxHealth;

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }
        }

        /// <summary>
        /// Take damage from a projectile.
        /// </summary>
        /// <param name="damage">Amount of damage.</param>
        /// <param name="canDestroySteel">Whether this can destroy steel.</param>
        /// <returns>True if terrain was destroyed.</returns>
        public bool TakeDamage(int damage = 1, bool canDestroySteel = false)
        {
            if (isDestroyed) return false;

            // Steel can only be destroyed if the projectile has power-up
            if (tileType == TileType.Steel && !canDestroySteel)
            {
                // Play hit sound but don't take damage
                PlayHitEffect();
                return false;
            }

            currentHealth -= damage;

            if (currentHealth <= 0)
            {
                Destroy();
                return true;
            }
            else
            {
                UpdateDamageVisual();
                PlayHitEffect();
                return false;
            }
        }

        /// <summary>
        /// Update the sprite to show damage.
        /// </summary>
        private void UpdateDamageVisual()
        {
            if (damageSprites == null || damageSprites.Length == 0) return;
            if (spriteRenderer == null) return;

            // Calculate damage level (0 = full health, Length-1 = almost destroyed)
            int damageLevel = maxHealth - currentHealth;
            damageLevel = Mathf.Clamp(damageLevel, 0, damageSprites.Length - 1);

            if (damageSprites[damageLevel] != null)
            {
                spriteRenderer.sprite = damageSprites[damageLevel];
            }
        }

        /// <summary>
        /// Play hit effect (sound, particles).
        /// </summary>
        private void PlayHitEffect()
        {
            if (hitSound != null)
            {
                AudioSource.PlayClipAtPoint(hitSound, transform.position);
            }

            // TODO: Spawn particle effect
        }

        /// <summary>
        /// Destroy the terrain.
        /// </summary>
        private void Destroy()
        {
            if (isDestroyed) return;

            isDestroyed = true;

            // Play destruction effects
            if (destroySound != null)
            {
                AudioSource.PlayClipAtPoint(destroySound, transform.position);
            }

            if (destructionEffectPrefab != null)
            {
                Instantiate(destructionEffectPrefab, transform.position, Quaternion.identity);
            }

            // Publish event
            Vector2Int gridPos = new Vector2Int(
                Mathf.FloorToInt(transform.position.x),
                Mathf.FloorToInt(transform.position.y)
            );

            EventBus.Publish(new TerrainDestroyedEvent
            {
                GridPosition = gridPos,
                TileType = tileType == TileType.Steel ? TileDestroyedType.Steel : TileDestroyedType.Brick
            });

            // Disable or destroy game object
            gameObject.SetActive(false);
            // Or: Destroy(gameObject);
        }

        /// <summary>
        /// Reset the terrain to full health (for pooling).
        /// </summary>
        public void Reset()
        {
            currentHealth = maxHealth;
            isDestroyed = false;
            gameObject.SetActive(true);

            // Reset visual
            if (damageSprites != null && damageSprites.Length > 0 && spriteRenderer != null)
            {
                spriteRenderer.sprite = damageSprites[0];
            }
        }

        /// <summary>
        /// Force destroy without effects (for clearing level).
        /// </summary>
        public void ForceDestroy()
        {
            isDestroyed = true;
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Change the tile type (for Shovel power-up).
        /// </summary>
        /// <param name="newType">New tile type.</param>
        public void ChangeTileType(TileType newType)
        {
            tileType = newType;

            // Update health based on new type
            maxHealth = newType == TileType.Steel ? 4 : 2;
            currentHealth = maxHealth;

            // TODO: Update sprite
        }
    }
}
