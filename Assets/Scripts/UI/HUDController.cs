using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NeuralBattalion.Core;
using NeuralBattalion.Core.Events;

namespace NeuralBattalion.UI
{
    /// <summary>
    /// Controls the in-game HUD display.
    /// Responsibilities:
    /// - Display player lives
    /// - Display score
    /// - Display remaining enemies
    /// - Display power-up status
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        [Header("Score Display")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI highScoreText;

        [Header("Lives Display")]
        [SerializeField] private TextMeshProUGUI livesText;
        [SerializeField] private Image[] lifeIcons;

        [Header("Enemy Counter")]
        [SerializeField] private TextMeshProUGUI enemyCountText;
        [SerializeField] private Image[] enemyIcons;

        [Header("Level Display")]
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI waveText;

        [Header("Power-up Indicators")]
        [SerializeField] private GameObject shieldIndicator;
        [SerializeField] private GameObject speedBoostIndicator;
        [SerializeField] private GameObject fireRateIndicator;

        [Header("Animation")]
        [SerializeField] private Animator hudAnimator;

        private int displayedScore = 0;
        private int targetScore = 0;
        private float scoreAnimSpeed = 1000f;

        private void Start()
        {
            SubscribeToEvents();
            InitializeHUD();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void Update()
        {
            AnimateScore();
        }

        private void SubscribeToEvents()
        {
            EventBus.Subscribe<ScoreChangedEvent>(OnScoreChanged);
            EventBus.Subscribe<PlayerDamagedEvent>(OnPlayerDamaged);
            EventBus.Subscribe<PlayerDeathEvent>(OnPlayerDeath);
            EventBus.Subscribe<PlayerRespawnEvent>(OnPlayerRespawn);
            EventBus.Subscribe<EnemyDestroyedEvent>(OnEnemyDestroyed);
            EventBus.Subscribe<EnemyWaveStartedEvent>(OnWaveStarted);
            EventBus.Subscribe<LevelStartedEvent>(OnLevelStarted);
            EventBus.Subscribe<PowerUpCollectedEvent>(OnPowerUpCollected);
        }

        private void UnsubscribeFromEvents()
        {
            EventBus.Unsubscribe<ScoreChangedEvent>(OnScoreChanged);
            EventBus.Unsubscribe<PlayerDamagedEvent>(OnPlayerDamaged);
            EventBus.Unsubscribe<PlayerDeathEvent>(OnPlayerDeath);
            EventBus.Unsubscribe<PlayerRespawnEvent>(OnPlayerRespawn);
            EventBus.Unsubscribe<EnemyDestroyedEvent>(OnEnemyDestroyed);
            EventBus.Unsubscribe<EnemyWaveStartedEvent>(OnWaveStarted);
            EventBus.Unsubscribe<LevelStartedEvent>(OnLevelStarted);
            EventBus.Unsubscribe<PowerUpCollectedEvent>(OnPowerUpCollected);
        }

        private void InitializeHUD()
        {
            UpdateScoreDisplay(0);
            UpdateLivesDisplay(3);
            UpdateEnemyCount(20);

            // Hide power-up indicators
            if (shieldIndicator != null) shieldIndicator.SetActive(false);
            if (speedBoostIndicator != null) speedBoostIndicator.SetActive(false);
            if (fireRateIndicator != null) fireRateIndicator.SetActive(false);
        }

        private void AnimateScore()
        {
            if (displayedScore < targetScore)
            {
                displayedScore = Mathf.Min(displayedScore + Mathf.RoundToInt(scoreAnimSpeed * Time.deltaTime), targetScore);
                if (scoreText != null) scoreText.text = displayedScore.ToString("N0");
            }
        }

        #region Event Handlers

        private void OnScoreChanged(ScoreChangedEvent evt)
        {
            targetScore = evt.NewScore;
        }

        private void OnPlayerDamaged(PlayerDamagedEvent evt)
        {
            // Flash damage indicator
            // hudAnimator?.SetTrigger("Damage");
        }

        private void OnPlayerDeath(PlayerDeathEvent evt)
        {
            UpdateLivesDisplay(evt.RemainingLives);
        }

        private void OnPlayerRespawn(PlayerRespawnEvent evt)
        {
            UpdateLivesDisplay(evt.RemainingLives);
        }

        private void OnEnemyDestroyed(EnemyDestroyedEvent evt)
        {
            // Update enemy count would be handled by GameLoop
        }

        private void OnWaveStarted(EnemyWaveStartedEvent evt)
        {
            if (waveText != null) waveText.text = $"Wave {evt.WaveNumber}";
            UpdateEnemyCount(evt.EnemyCount);
        }

        private void OnLevelStarted(LevelStartedEvent evt)
        {
            if (levelText != null) levelText.text = $"Stage {evt.Level}";
            UpdateEnemyCount(evt.TotalEnemies);
        }

        private void OnPowerUpCollected(PowerUpCollectedEvent evt)
        {
            // Show power-up indicator based on type
            switch (evt.Type)
            {
                case PowerUpType.Shield:
                    if (shieldIndicator != null) shieldIndicator.SetActive(true);
                    break;
                case PowerUpType.SpeedBoost:
                    if (speedBoostIndicator != null) speedBoostIndicator.SetActive(true);
                    break;
                case PowerUpType.FireRateBoost:
                    if (fireRateIndicator != null) fireRateIndicator.SetActive(true);
                    break;
            }
        }

        #endregion

        #region Display Updates

        private void UpdateScoreDisplay(int score)
        {
            displayedScore = score;
            targetScore = score;
            if (scoreText != null) scoreText.text = score.ToString("N0");
        }

        private void UpdateLivesDisplay(int lives)
        {
            if (livesText != null) livesText.text = lives.ToString();

            // Update life icons
            if (lifeIcons != null)
            {
                for (int i = 0; i < lifeIcons.Length; i++)
                {
                    lifeIcons[i].enabled = i < lives;
                }
            }
        }

        private void UpdateEnemyCount(int count)
        {
            if (enemyCountText != null) enemyCountText.text = count.ToString();

            // Update enemy icons
            if (enemyIcons != null)
            {
                for (int i = 0; i < enemyIcons.Length; i++)
                {
                    enemyIcons[i].enabled = i < count;
                }
            }
        }

        #endregion

        /// <summary>
        /// Set enemy count externally.
        /// </summary>
        public void SetEnemyCount(int count)
        {
            UpdateEnemyCount(count);
        }

        /// <summary>
        /// Hide a power-up indicator.
        /// </summary>
        public void HidePowerUpIndicator(PowerUpType type)
        {
            switch (type)
            {
                case PowerUpType.Shield:
                    if (shieldIndicator != null) shieldIndicator.SetActive(false);
                    break;
                case PowerUpType.SpeedBoost:
                    if (speedBoostIndicator != null) speedBoostIndicator.SetActive(false);
                    break;
                case PowerUpType.FireRateBoost:
                    if (fireRateIndicator != null) fireRateIndicator.SetActive(false);
                    break;
            }
        }
    }
}
