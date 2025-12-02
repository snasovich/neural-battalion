using UnityEngine;
using NeuralBattalion.Player;
using NeuralBattalion.Enemy;
using NeuralBattalion.Combat;

namespace NeuralBattalion.Core
{
    /// <summary>
    /// Orchestrates the main game update loop.
    /// Responsibilities:
    /// - Coordinate update order of game systems
    /// - Handle fixed vs variable timestep updates
    /// - Manage update priorities
    /// 
    /// Update Order:
    /// 1. ProcessInput - Handle player input
    /// 2. UpdateAI - Update enemy decisions
    /// 3. UpdatePhysics - Movement and collisions (FixedUpdate)
    /// 4. UpdateCombat - Projectiles and damage
    /// 5. UpdateTerrain - Terrain destruction
    /// 6. CheckWinConditions - Evaluate game state
    /// 7. UpdateUI - Refresh HUD
    /// </summary>
    public class GameLoop : MonoBehaviour
    {
        [Header("System References")]
        [SerializeField] private PlayerController playerController;
        [SerializeField] private EnemySpawner enemySpawner;
        [SerializeField] private DamageSystem damageSystem;

        [Header("Settings")]
        [SerializeField] private bool debugMode = false;

        private bool isRunning = false;
        private int enemiesRemaining;
        private int enemiesToSpawn;

        private void Start()
        {
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            // TODO: Subscribe to game events
            // EventBus.Subscribe<GameStartedEvent>(OnGameStarted);
            // EventBus.Subscribe<GamePausedEvent>(OnGamePaused);
        }

        private void UnsubscribeFromEvents()
        {
            // TODO: Unsubscribe from game events
        }

        private void Update()
        {
            if (!isRunning) return;
            if (GameManager.Instance?.IsPaused ?? true) return;

            ProcessInput();
            UpdateAI();
            UpdateCombat();
            UpdateTerrain();
            CheckWinConditions();
            UpdateUI();
        }

        private void FixedUpdate()
        {
            if (!isRunning) return;
            if (GameManager.Instance?.IsPaused ?? true) return;

            UpdatePhysics();
        }

        /// <summary>
        /// Start the game loop for a new level.
        /// </summary>
        /// <param name="totalEnemies">Total enemies in this level.</param>
        public void StartLevel(int totalEnemies)
        {
            enemiesToSpawn = totalEnemies;
            enemiesRemaining = totalEnemies;
            isRunning = true;

            if (debugMode)
            {
                Debug.Log($"[GameLoop] Level started with {totalEnemies} enemies");
            }
        }

        /// <summary>
        /// Stop the game loop.
        /// </summary>
        public void StopLevel()
        {
            isRunning = false;
        }

        #region Update Methods

        /// <summary>
        /// Process player input each frame.
        /// </summary>
        private void ProcessInput()
        {
            // Input is handled by PlayerInput component
            // This is a hook for any input pre-processing if needed
        }

        /// <summary>
        /// Update all enemy AI decisions.
        /// </summary>
        private void UpdateAI()
        {
            // TODO: Update enemy AI through EnemySpawner or AIManager
            // enemySpawner?.UpdateAllEnemyAI();
        }

        /// <summary>
        /// Physics update for movement and collisions.
        /// </summary>
        private void UpdatePhysics()
        {
            // Physics handled by Unity's physics engine
            // This is a hook for any custom physics if needed
        }

        /// <summary>
        /// Update combat systems (projectiles, damage).
        /// </summary>
        private void UpdateCombat()
        {
            // TODO: Update projectile movement and collision checks
            // damageSystem?.ProcessPendingDamage();
        }

        /// <summary>
        /// Update terrain destruction effects.
        /// </summary>
        private void UpdateTerrain()
        {
            // TODO: Process pending terrain destruction
        }

        /// <summary>
        /// Check win/lose conditions.
        /// </summary>
        private void CheckWinConditions()
        {
            // Victory: All enemies destroyed
            if (enemiesRemaining <= 0 && enemiesToSpawn <= 0)
            {
                OnLevelComplete();
                return;
            }

            // Defeat: Player out of lives or base destroyed
            // This is typically triggered by events, not polling
        }

        /// <summary>
        /// Update UI elements.
        /// </summary>
        private void UpdateUI()
        {
            // TODO: Update HUD via events or direct reference
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Called when an enemy is destroyed.
        /// </summary>
        public void OnEnemyDestroyed()
        {
            enemiesRemaining--;

            if (debugMode)
            {
                Debug.Log($"[GameLoop] Enemy destroyed. Remaining: {enemiesRemaining}");
            }
        }

        /// <summary>
        /// Called when the player loses all lives.
        /// </summary>
        public void OnPlayerDefeated()
        {
            isRunning = false;
            GameManager.Instance?.EndGame(false);
        }

        /// <summary>
        /// Called when the player base is destroyed.
        /// </summary>
        public void OnBaseDestroyed()
        {
            isRunning = false;
            GameManager.Instance?.EndGame(false);
        }

        /// <summary>
        /// Called when all enemies are destroyed.
        /// </summary>
        private void OnLevelComplete()
        {
            isRunning = false;
            GameManager.Instance?.NextLevel();
        }

        #endregion
    }
}
