using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using NeuralBattalion.Core;
using NeuralBattalion.Core.Events;
using NeuralBattalion.Utility;

namespace NeuralBattalion.UI
{
    /// <summary>
    /// Controls the game over screen display.
    /// Responsibilities:
    /// - Show victory or defeat screen
    /// - Display final score
    /// - Handle restart/menu buttons
    /// </summary>
    public class GameOverScreen : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI finalScoreText;
        [SerializeField] private TextMeshProUGUI highScoreText;
        [SerializeField] private GameObject newHighScoreIndicator;

        [Header("Buttons")]
        [SerializeField] private Button restartButton;
        [SerializeField] private Button nextLevelButton;
        [SerializeField] private Button mainMenuButton;

        [Header("Visual Settings")]
        [SerializeField] private string victoryTitle = "VICTORY!";
        [SerializeField] private string defeatTitle = "GAME OVER";
        [SerializeField] private Color victoryColor = Color.green;
        [SerializeField] private Color defeatColor = Color.red;

        [Header("Animation")]
        [SerializeField] private Animator screenAnimator;
        [SerializeField] private float showDelay = 1f;

        private bool isVictory;
        private int finalScore;

        private void Start()
        {
            SetupButtonListeners();
            SubscribeToEvents();
            HideScreen();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void SetupButtonListeners()
        {
            if (restartButton != null) restartButton.onClick.AddListener(OnRestartClicked);
            if (nextLevelButton != null) nextLevelButton.onClick.AddListener(OnNextLevelClicked);
            if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }

        private void SubscribeToEvents()
        {
            EventBus.Subscribe<GameOverEvent>(OnGameOver);
            EventBus.Subscribe<NewHighScoreEvent>(OnNewHighScore);
        }

        private void UnsubscribeFromEvents()
        {
            EventBus.Unsubscribe<GameOverEvent>(OnGameOver);
            EventBus.Unsubscribe<NewHighScoreEvent>(OnNewHighScore);
        }

        private void OnGameOver(GameOverEvent evt)
        {
            isVictory = evt.Victory;
            finalScore = evt.FinalScore;

            StartCoroutine(ShowScreenDelayed());
        }

        private void OnNewHighScore(NewHighScoreEvent evt)
        {
            if (newHighScoreIndicator != null)
            {
                newHighScoreIndicator.SetActive(true);
            }
        }

        private System.Collections.IEnumerator ShowScreenDelayed()
        {
            yield return new WaitForSeconds(showDelay);
            ShowScreen();
        }

        private void ShowScreen()
        {
            if (gameOverPanel != null) gameOverPanel.SetActive(true);

            // Set title and color
            if (titleText != null)
            {
                titleText.text = isVictory ? victoryTitle : defeatTitle;
                titleText.color = isVictory ? victoryColor : defeatColor;
            }

            // Set score
            if (finalScoreText != null)
            {
                finalScoreText.text = $"Score: {finalScore:N0}";
            }

            // Set high score
            int highScore = PlayerPrefs.GetInt(Constants.PlayerPrefsKeys.HighScore, 0);
            if (highScoreText != null)
            {
                highScoreText.text = $"High Score: {highScore:N0}";
            }

            // Show/hide next level button
            if (nextLevelButton != null)
            {
                nextLevelButton.gameObject.SetActive(isVictory);
            }

            // Reset high score indicator
            if (newHighScoreIndicator != null)
            {
                newHighScoreIndicator.SetActive(false);
            }

            // Play animation
            if (screenAnimator != null)
            {
                screenAnimator.SetTrigger("Show");
            }
        }

        private void HideScreen()
        {
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
        }

        #region Button Handlers

        private void OnRestartClicked()
        {
            HideScreen();

            // Restart current level
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void OnNextLevelClicked()
        {
            HideScreen();

            // Go to next level
            GameManager.Instance?.NextLevel();
        }

        private void OnMainMenuClicked()
        {
            HideScreen();

            GameManager.Instance?.ReturnToMainMenu();
        }

        #endregion

        /// <summary>
        /// Show game over screen programmatically.
        /// </summary>
        public void Show(bool victory, int score)
        {
            isVictory = victory;
            finalScore = score;
            ShowScreen();
        }
    }
}
