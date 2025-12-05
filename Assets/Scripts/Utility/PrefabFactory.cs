using UnityEngine;
using NeuralBattalion.Combat;

namespace NeuralBattalion.Utility
{
    /// <summary>
    /// Factory class for creating game object prefabs at runtime.
    /// Used as a fallback when prefab assets are not available.
    /// </summary>
    public static class PrefabFactory
    {
        /// <summary>
        /// Create a projectile prefab with all necessary components.
        /// </summary>
        /// <param name="isPlayerProjectile">Whether this is for player projectiles.</param>
        /// <returns>A new projectile GameObject.</returns>
        public static GameObject CreateProjectilePrefab(bool isPlayerProjectile = true)
        {
            // Create GameObject
            GameObject projectileGO = new GameObject(isPlayerProjectile ? "PlayerProjectile" : "EnemyProjectile");
            
            // Add Rigidbody2D
            Rigidbody2D rb = projectileGO.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.isKinematic = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            
            // Add BoxCollider2D as trigger
            BoxCollider2D collider = projectileGO.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(0.2f, 0.3f);
            collider.isTrigger = true;
            
            // Add SpriteRenderer
            SpriteRenderer spriteRenderer = projectileGO.AddComponent<SpriteRenderer>();
            Color projectileColor = isPlayerProjectile ? Color.yellow : Color.red;
            spriteRenderer.sprite = SpriteGenerator.CreateColoredSprite(projectileColor, 8, 12, 100f);
            spriteRenderer.sortingLayerName = "Default";
            spriteRenderer.sortingOrder = 15;
            
            // Add Projectile component
            Projectile projectile = projectileGO.AddComponent<Projectile>();
            
            // Set appropriate tag
            projectileGO.tag = isPlayerProjectile ? "PlayerProjectile" : "EnemyProjectile";
            
            // Set to inactive (will be activated when fired)
            projectileGO.SetActive(false);
            
            Debug.Log($"[PrefabFactory] Created {(isPlayerProjectile ? "player" : "enemy")} projectile prefab");
            
            return projectileGO;
        }
        
        /// <summary>
        /// Create a hit effect prefab for projectile impacts.
        /// </summary>
        /// <returns>A new hit effect GameObject.</returns>
        public static GameObject CreateHitEffectPrefab()
        {
            GameObject effectGO = new GameObject("HitEffect");
            
            // Add SpriteRenderer for simple visual effect
            SpriteRenderer spriteRenderer = effectGO.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = SpriteGenerator.CreateColoredSprite(Color.white, 16, 16, 100f);
            spriteRenderer.sortingLayerName = "Default";
            spriteRenderer.sortingOrder = 20;
            
            // Add component to auto-destroy after delay
            var autoDestroy = effectGO.AddComponent<AutoDestroy>();
            
            effectGO.SetActive(false);
            
            return effectGO;
        }
        
        /// <summary>
        /// Create an explosion effect prefab.
        /// </summary>
        /// <returns>A new explosion effect GameObject.</returns>
        public static GameObject CreateExplosionPrefab()
        {
            GameObject explosionGO = new GameObject("Explosion");
            
            // Add SpriteRenderer
            SpriteRenderer spriteRenderer = explosionGO.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = SpriteGenerator.CreateColoredSprite(new Color(1f, 0.5f, 0f, 0.8f), 32, 32, 100f);
            spriteRenderer.sortingLayerName = "Default";
            spriteRenderer.sortingOrder = 20;
            
            // Add component to auto-destroy
            var autoDestroy = explosionGO.AddComponent<AutoDestroy>();
            
            explosionGO.SetActive(false);
            
            return explosionGO;
        }
    }
    
    /// <summary>
    /// Simple component to auto-destroy a GameObject after a delay.
    /// </summary>
    public class AutoDestroy : MonoBehaviour
    {
        [SerializeField] private float lifetime = 0.5f;
        
        private void OnEnable()
        {
            Invoke(nameof(DestroyThis), lifetime);
        }
        
        private void DestroyThis()
        {
            Destroy(gameObject);
        }
    }
}
