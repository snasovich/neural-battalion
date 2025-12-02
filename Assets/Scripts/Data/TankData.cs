using UnityEngine;

namespace NeuralBattalion.Data
{
    /// <summary>
    /// ScriptableObject for tank configuration.
    /// Used to define tank properties for both player and enemies.
    /// </summary>
    [CreateAssetMenu(fileName = "NewTankData", menuName = "Neural Battalion/Tank Data")]
    public class TankData : ScriptableObject
    {
        [Header("Tank Info")]
        [SerializeField] private string tankName = "Tank";
        [SerializeField] private Sprite tankSprite;
        [SerializeField] private Color tankColor = Color.white;

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float rotationSpeed = 180f;

        [Header("Combat")]
        [SerializeField] private int health = 1;
        [SerializeField] private int damage = 1;
        [SerializeField] private float fireRate = 0.5f;
        [SerializeField] private float projectileSpeed = 10f;
        [SerializeField] private int maxProjectiles = 1;

        [Header("Score")]
        [SerializeField] private int scoreValue = 100;

        [Header("Special Abilities")]
        [SerializeField] private bool canDestroySteel = false;

        // Properties
        public string TankName => tankName;
        public Sprite TankSprite => tankSprite;
        public Color TankColor => tankColor;
        public float MoveSpeed => moveSpeed;
        public float RotationSpeed => rotationSpeed;
        public int Health => health;
        public int Damage => damage;
        public float FireRate => fireRate;
        public float ProjectileSpeed => projectileSpeed;
        public int MaxProjectiles => maxProjectiles;
        public int ScoreValue => scoreValue;
        public bool CanDestroySteel => canDestroySteel;

        /// <summary>
        /// Create a modified copy of this tank data.
        /// Useful for applying upgrades.
        /// </summary>
        public TankData CreateModifiedCopy(float speedMod = 1f, float fireRateMod = 1f, int damageMod = 0)
        {
            TankData copy = CreateInstance<TankData>();
            copy.tankName = tankName;
            copy.tankSprite = tankSprite;
            copy.tankColor = tankColor;
            copy.moveSpeed = moveSpeed * speedMod;
            copy.rotationSpeed = rotationSpeed;
            copy.health = health;
            copy.damage = damage + damageMod;
            copy.fireRate = fireRate / fireRateMod;
            copy.projectileSpeed = projectileSpeed;
            copy.maxProjectiles = maxProjectiles;
            copy.scoreValue = scoreValue;
            copy.canDestroySteel = canDestroySteel;
            return copy;
        }
    }
}
