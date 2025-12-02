using UnityEngine;
using System.Collections.Generic;

namespace NeuralBattalion.Combat
{
    /// <summary>
    /// Centralized damage calculation and application system.
    /// Responsibilities:
    /// - Calculate damage based on modifiers
    /// - Queue and process damage events
    /// - Handle damage resistance/vulnerability
    /// </summary>
    public class DamageSystem : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool debugMode = false;

        // Queue of pending damage to process
        private Queue<DamageEvent> pendingDamage = new Queue<DamageEvent>();

        /// <summary>
        /// Queue damage to be processed.
        /// </summary>
        /// <param name="target">Target to damage.</param>
        /// <param name="baseDamage">Base damage amount.</param>
        /// <param name="source">Source of damage.</param>
        /// <param name="damageType">Type of damage.</param>
        public void QueueDamage(GameObject target, int baseDamage, GameObject source, DamageType damageType = DamageType.Normal)
        {
            if (target == null) return;

            pendingDamage.Enqueue(new DamageEvent
            {
                Target = target,
                BaseDamage = baseDamage,
                Source = source,
                DamageType = damageType
            });
        }

        /// <summary>
        /// Process all queued damage.
        /// </summary>
        public void ProcessPendingDamage()
        {
            while (pendingDamage.Count > 0)
            {
                var damageEvent = pendingDamage.Dequeue();
                ProcessDamage(damageEvent);
            }
        }

        /// <summary>
        /// Process a single damage event.
        /// </summary>
        private void ProcessDamage(DamageEvent damageEvent)
        {
            if (damageEvent.Target == null) return;

            int finalDamage = CalculateDamage(damageEvent);

            if (debugMode)
            {
                Debug.Log($"[DamageSystem] Applying {finalDamage} damage to {damageEvent.Target.name}");
            }

            // Apply damage based on target type
            ApplyDamage(damageEvent.Target, finalDamage, damageEvent.DamageType);
        }

        /// <summary>
        /// Calculate final damage with modifiers.
        /// </summary>
        private int CalculateDamage(DamageEvent damageEvent)
        {
            float damage = damageEvent.BaseDamage;

            // Apply damage type modifiers
            damage *= GetDamageTypeMultiplier(damageEvent.DamageType);

            // Get target resistance if applicable
            var damageable = damageEvent.Target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damage *= (1f - damageable.GetDamageResistance(damageEvent.DamageType));
            }

            return Mathf.Max(1, Mathf.RoundToInt(damage));
        }

        /// <summary>
        /// Get multiplier for damage type.
        /// </summary>
        private float GetDamageTypeMultiplier(DamageType damageType)
        {
            return damageType switch
            {
                DamageType.Normal => 1f,
                DamageType.Explosive => 1.5f,
                DamageType.Power => 2f,
                _ => 1f
            };
        }

        /// <summary>
        /// Apply damage to target.
        /// </summary>
        private void ApplyDamage(GameObject target, int damage, DamageType damageType)
        {
            // Try IDamageable interface first
            var damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
                return;
            }

            // Try specific components
            var playerController = target.GetComponent<Player.PlayerController>();
            if (playerController != null)
            {
                playerController.TakeDamage(damage);
                return;
            }

            var enemyController = target.GetComponent<Enemy.EnemyController>();
            if (enemyController != null)
            {
                enemyController.TakeDamage(damage);
                return;
            }

            var destructible = target.GetComponent<Terrain.DestructibleTerrain>();
            if (destructible != null)
            {
                destructible.TakeDamage(damage, damageType == DamageType.Power);
                return;
            }
        }

        /// <summary>
        /// Apply immediate damage (bypasses queue).
        /// </summary>
        public void ApplyImmediateDamage(GameObject target, int damage, DamageType damageType = DamageType.Normal)
        {
            if (target == null) return;

            var damageEvent = new DamageEvent
            {
                Target = target,
                BaseDamage = damage,
                DamageType = damageType
            };

            ProcessDamage(damageEvent);
        }

        /// <summary>
        /// Apply area damage.
        /// </summary>
        /// <param name="center">Center of explosion.</param>
        /// <param name="radius">Damage radius.</param>
        /// <param name="damage">Damage amount.</param>
        /// <param name="source">Source of explosion.</param>
        /// <param name="layerMask">Layers to affect.</param>
        public void ApplyAreaDamage(Vector2 center, float radius, int damage, GameObject source, LayerMask layerMask)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius, layerMask);

            foreach (var hit in hits)
            {
                if (hit.gameObject == source) continue; // Don't damage self

                // Calculate distance falloff
                float distance = Vector2.Distance(center, hit.transform.position);
                float falloff = 1f - (distance / radius);
                int scaledDamage = Mathf.Max(1, Mathf.RoundToInt(damage * falloff));

                QueueDamage(hit.gameObject, scaledDamage, source, DamageType.Explosive);
            }

            ProcessPendingDamage();
        }
    }

    /// <summary>
    /// Types of damage.
    /// </summary>
    public enum DamageType
    {
        Normal,     // Standard projectile damage
        Explosive,  // Area damage with falloff
        Power       // Can destroy steel walls
    }

    /// <summary>
    /// Damage event data.
    /// </summary>
    public struct DamageEvent
    {
        public GameObject Target;
        public int BaseDamage;
        public GameObject Source;
        public DamageType DamageType;
    }

    /// <summary>
    /// Interface for damageable objects.
    /// </summary>
    public interface IDamageable
    {
        void TakeDamage(int damage);
        float GetDamageResistance(DamageType damageType);
    }
}
