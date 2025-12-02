using UnityEngine;
using NeuralBattalion.Core.Events;

namespace NeuralBattalion.Combat
{
    /// <summary>
    /// Explosion effect handler.
    /// Responsibilities:
    /// - Visual explosion effect
    /// - Audio explosion sound
    /// - Area damage (if enabled)
    /// - Self-destruct after animation
    /// </summary>
    public class Explosion : MonoBehaviour
    {
        [Header("Visual Settings")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite[] explosionSprites;
        [SerializeField] private float frameDuration = 0.1f;
        [SerializeField] private bool loop = false;

        [Header("Audio")]
        [SerializeField] private AudioClip explosionSound;
        [SerializeField] private float volume = 1f;

        [Header("Damage Settings")]
        [SerializeField] private bool causesAreaDamage = false;
        [SerializeField] private float damageRadius = 1f;
        [SerializeField] private int damage = 1;
        [SerializeField] private LayerMask damageLayers;

        [Header("Particle System")]
        [SerializeField] private ParticleSystem particles;

        private int currentFrame = 0;
        private float frameTimer = 0f;
        private bool isPlaying = false;

        private void Start()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            Play();
        }

        private void Update()
        {
            if (!isPlaying) return;

            UpdateAnimation();
        }

        /// <summary>
        /// Play the explosion.
        /// </summary>
        public void Play()
        {
            isPlaying = true;
            currentFrame = 0;
            frameTimer = 0f;

            // Play sound
            PlaySound();

            // Apply area damage
            if (causesAreaDamage)
            {
                ApplyAreaDamage();
            }

            // Start particles
            if (particles != null)
            {
                particles.Play();
            }

            // Publish event
            EventBus.Publish(new ExplosionEvent
            {
                Position = transform.position,
                Radius = damageRadius
            });

            // Set first frame
            if (explosionSprites != null && explosionSprites.Length > 0 && spriteRenderer != null)
            {
                spriteRenderer.sprite = explosionSprites[0];
            }
        }

        /// <summary>
        /// Update sprite animation.
        /// </summary>
        private void UpdateAnimation()
        {
            if (explosionSprites == null || explosionSprites.Length == 0) return;

            frameTimer += Time.deltaTime;

            if (frameTimer >= frameDuration)
            {
                frameTimer = 0f;
                currentFrame++;

                if (currentFrame >= explosionSprites.Length)
                {
                    if (loop)
                    {
                        currentFrame = 0;
                    }
                    else
                    {
                        OnAnimationComplete();
                        return;
                    }
                }

                if (spriteRenderer != null && currentFrame < explosionSprites.Length)
                {
                    spriteRenderer.sprite = explosionSprites[currentFrame];
                }
            }
        }

        /// <summary>
        /// Called when animation completes.
        /// </summary>
        private void OnAnimationComplete()
        {
            isPlaying = false;

            // Wait for particles to finish if present
            if (particles != null && particles.isPlaying)
            {
                spriteRenderer.enabled = false;
                Invoke(nameof(DestroySelf), particles.main.duration);
            }
            else
            {
                DestroySelf();
            }
        }

        /// <summary>
        /// Play explosion sound.
        /// </summary>
        private void PlaySound()
        {
            if (explosionSound == null) return;

            AudioSource.PlayClipAtPoint(explosionSound, transform.position, volume);
        }

        /// <summary>
        /// Apply damage to objects in radius.
        /// </summary>
        private void ApplyAreaDamage()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, damageRadius, damageLayers);

            foreach (var hit in hits)
            {
                // Calculate distance falloff
                float distance = Vector2.Distance(transform.position, hit.transform.position);
                float falloff = 1f - (distance / damageRadius);
                int scaledDamage = Mathf.Max(1, Mathf.RoundToInt(damage * falloff));

                // Apply damage
                var damageable = hit.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(scaledDamage);
                }

                // Try specific components
                var playerController = hit.GetComponent<Player.PlayerController>();
                if (playerController != null)
                {
                    playerController.TakeDamage(scaledDamage);
                    continue;
                }

                var enemyController = hit.GetComponent<Enemy.EnemyController>();
                if (enemyController != null)
                {
                    enemyController.TakeDamage(scaledDamage);
                    continue;
                }

                var destructible = hit.GetComponent<Terrain.DestructibleTerrain>();
                if (destructible != null)
                {
                    destructible.TakeDamage(scaledDamage);
                }
            }
        }

        /// <summary>
        /// Destroy this explosion object.
        /// </summary>
        private void DestroySelf()
        {
            Destroy(gameObject);
        }

        /// <summary>
        /// Initialize explosion with custom parameters.
        /// </summary>
        /// <param name="position">Explosion position.</param>
        /// <param name="radius">Damage radius.</param>
        /// <param name="explosionDamage">Damage amount.</param>
        public void Initialize(Vector2 position, float radius, int explosionDamage)
        {
            transform.position = position;
            damageRadius = radius;
            damage = explosionDamage;
            causesAreaDamage = true;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Draw damage radius in editor.
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (!causesAreaDamage) return;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, damageRadius);
        }
#endif
    }
}
