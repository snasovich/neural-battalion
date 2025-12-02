using UnityEngine;
using UnityEngine.UI;
using NeuralBattalion.Core;
using NeuralBattalion.Core.Events;

namespace NeuralBattalion.UI
{
    /// <summary>
    /// Controls the pause menu functionality.
    /// Responsibilities:
    /// - Show/hide pause menu
    /// - Handle pause menu buttons
    /// - Resume/quit game
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject pauseMenuPanel;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button optionsButton;
        [SerializeField] private Button mainMenuButton;

        [Header("Options Panel")]
        [SerializeField] private GameObject optionsPanel;
        [SerializeField] private Button optionsBackButton;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;

        [Header("Input")]
        [SerializeField] private KeyCode pauseKey = KeyCode.Escape;

        private bool isPaused = false;

        private void Start()
        {
            SetupButtonListeners();
            HidePauseMenu();
        }

        private void Update()
        {
            if (Input.GetKeyDown(pauseKey))
            {
                TogglePause();
            }
        }

        private void SetupButtonListeners()
        {
            if (resumeButton != null) resumeButton.onClick.AddListener(OnResumeClicked);
            if (restartButton != null) restartButton.onClick.AddListener(OnRestartClicked);
            if (optionsButton != null) optionsButton.onClick.AddListener(OnOptionsClicked);
            if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenuClicked);
            if (optionsBackButton != null) optionsBackButton.onClick.AddListener(OnOptionsBackClicked);
        }

        /// <summary>
        /// Toggle pause state.
        /// </summary>
        public void TogglePause()
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }

        /// <summary>
        /// Pause the game.
        /// </summary>
        public void Pause()
        {
            if (isPaused) return;

            isPaused = true;
            ShowPauseMenu();
            GameManager.Instance?.PauseGame();
        }

        /// <summary>
        /// Resume the game.
        /// </summary>
        public void Resume()
        {
            if (!isPaused) return;

            isPaused = false;
            HidePauseMenu();
            GameManager.Instance?.ResumeGame();
        }

        private void ShowPauseMenu()
        {
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
            if (optionsPanel != null) optionsPanel.SetActive(false);
        }

        private void HidePauseMenu()
        {
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
            if (optionsPanel != null) optionsPanel.SetActive(false);
        }

        #region Button Handlers

        private void OnResumeClicked()
        {
            Resume();
        }

        private void OnRestartClicked()
        {
            Resume();
            // TODO: Restart current level
            // GameManager.Instance?.RestartLevel();
        }

        private void OnOptionsClicked()
        {
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
            if (optionsPanel != null) optionsPanel.SetActive(true);
        }

        private void OnOptionsBackClicked()
        {
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
            if (optionsPanel != null) optionsPanel.SetActive(false);
        }

        private void OnMainMenuClicked()
        {
            Resume();
            GameManager.Instance?.ReturnToMainMenu();
        }

        #endregion
    }
}
