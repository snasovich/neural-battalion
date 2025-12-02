using UnityEngine;
using NeuralBattalion.Core.Events;

namespace NeuralBattalion.Core
{
    /// <summary>
    /// Manages score tracking and persistence.
    /// Responsibilities:
    /// - Track current score
    /// - Handle score multipliers
    /// - Save and load high scores
    /// - Emit score change events
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        private const string HIGH_SCORE_KEY = "NeuralBattalion_HighScore";

        [Header("Score Settings")]
        [SerializeField] private int baseEnemyScore = 100;
        [SerializeField] private int levelBonusScore = 1000;

        public int CurrentScore { get; private set; }
        public int HighScore { get; private set; }
        public float ScoreMultiplier { get; private set; } = 1f;

        private void Awake()
        {
            LoadHighScore();
        }

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
            // TODO: Subscribe to enemy destroyed events
            // EventBus.Subscribe<EnemyDestroyedEvent>(OnEnemyDestroyed);
        }

        private void UnsubscribeFromEvents()
        {
            // TODO: Unsubscribe from events
        }

        /// <summary>
        /// Reset score for a new game.
        /// </summary>
        public void ResetScore()
        {
            CurrentScore = 0;
            ScoreMultiplier = 1f;
            EventBus.Publish(new ScoreChangedEvent { NewScore = CurrentScore });
        }

        /// <summary>
        /// Add points to the current score.
        /// </summary>
        /// <param name="basePoints">Base points before multiplier.</param>
        public void AddScore(int basePoints)
        {
            int actualPoints = Mathf.RoundToInt(basePoints * ScoreMultiplier);
            CurrentScore += actualPoints;

            EventBus.Publish(new ScoreChangedEvent
            {
                NewScore = CurrentScore,
                PointsAdded = actualPoints
            });

            // Check for new high score
            if (CurrentScore > HighScore)
            {
                HighScore = CurrentScore;
                SaveHighScore();
                EventBus.Publish(new NewHighScoreEvent { Score = HighScore });
            }
        }

        /// <summary>
        /// Add points for destroying an enemy.
        /// </summary>
        /// <param name="enemyType">Type of enemy for bonus calculation.</param>
        public void AddEnemyScore(int enemyType = 0)
        {
            int points = baseEnemyScore * (enemyType + 1);
            AddScore(points);
        }

        /// <summary>
        /// Add bonus points for completing a level.
        /// </summary>
        public void AddLevelBonus()
        {
            AddScore(levelBonusScore);
        }

        /// <summary>
        /// Set the score multiplier.
        /// </summary>
        /// <param name="multiplier">New multiplier value.</param>
        public void SetMultiplier(float multiplier)
        {
            ScoreMultiplier = Mathf.Max(1f, multiplier);
        }

        /// <summary>
        /// Temporarily increase the score multiplier.
        /// </summary>
        /// <param name="bonus">Amount to add to multiplier.</param>
        /// <param name="duration">Duration in seconds.</param>
        public void AddTemporaryMultiplier(float bonus, float duration)
        {
            StartCoroutine(TemporaryMultiplierCoroutine(bonus, duration));
        }

        private System.Collections.IEnumerator TemporaryMultiplierCoroutine(float bonus, float duration)
        {
            ScoreMultiplier += bonus;
            yield return new WaitForSeconds(duration);
            ScoreMultiplier = Mathf.Max(1f, ScoreMultiplier - bonus);
        }

        private void LoadHighScore()
        {
            HighScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
        }

        private void SaveHighScore()
        {
            PlayerPrefs.SetInt(HIGH_SCORE_KEY, HighScore);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Clear high score (for testing/reset).
        /// </summary>
        public void ClearHighScore()
        {
            HighScore = 0;
            PlayerPrefs.DeleteKey(HIGH_SCORE_KEY);
            PlayerPrefs.Save();
        }
    }
}
