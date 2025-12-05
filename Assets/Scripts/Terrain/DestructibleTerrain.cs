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
        /// Initialize the destructible terrain with type and health.
        /// Call this after instantiation to properly set up the component.
        /// </summary>
        /// <param name="type">Tile type.</param>
        /// <param name="health">Maximum health.</param>
        public void Initialize(TileType type, int health)
        {
            tileType = type;
            maxHealth = health;
            currentHealth = maxHealth;
            isDestroyed = false;
        }

        /// <summary>
        /// Take damage from a projectile.
        /// </summary>
        /// <param name="damage">Amount of damage.</param>
        /// <param name="canDestroySteel">Whether this can destroy steel.</param>
        /// <returns>True if terrain was destroyed.</returns>
        public bool TakeDamage(int damage = 1, bool canDestroySteel = false)
        {
            Debug.Log($"[DestructibleTerrain] TakeDamage called on {gameObject.name}. Type: {tileType}, Health: {currentHealth}/{maxHealth}, Damage: {damage}, CanDestroySteel: {canDestroySteel}");
            
            if (isDestroyed)
            {
                Debug.Log($"[DestructibleTerrain] {gameObject.name} already destroyed, ignoring damage");
                return false;
            }

            // Steel can only be destroyed if the projectile has power-up
            if (tileType == TileType.Steel && !canDestroySteel)
            {
                Debug.Log($"[DestructibleTerrain] Steel tile hit without power-up, resisting damage");
                // Play hit sound but don't take damage
                PlayHitEffect();
                return false;
            }

            currentHealth -= damage;
            Debug.Log($"[DestructibleTerrain] Health reduced to {currentHealth}/{maxHealth}");

            if (currentHealth <= 0)
            {
                Debug.Log($"[DestructibleTerrain] Destroying {gameObject.name}");
                Destroy();
                return true;
            }
            else
            {
                Debug.Log($"[DestructibleTerrain] Tile damaged but not destroyed");
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
            if (isDestroyed)
            {
                Debug.Log($"[DestructibleTerrain] {gameObject.name} already destroyed");
                return;
            }

            Debug.Log($"[DestructibleTerrain] Destroying terrain {gameObject.name} at {transform.position}");
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
            Debug.Log($"[DestructibleTerrain] Disabling GameObject {gameObject.name}");
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
