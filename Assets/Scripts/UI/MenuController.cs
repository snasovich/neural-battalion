using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using NeuralBattalion.Core;
using NeuralBattalion.Utility;

namespace NeuralBattalion.UI
{
    /// <summary>
    /// Controls main menu navigation.
    /// Responsibilities:
    /// - Handle menu button clicks
    /// - Navigate between menu screens
    /// - Load game scenes
    /// </summary>
    public class MenuController : MonoBehaviour
    {
        [Header("Menu Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject optionsPanel;
        [SerializeField] private GameObject levelSelectPanel;
        [SerializeField] private GameObject creditsPanel;

        [Header("Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button optionsButton;
        [SerializeField] private Button creditsButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private Button backButton;

        [Header("Options")]
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;

        [Header("Animation")]
        [SerializeField] private Animator menuAnimator;

        private GameObject currentPanel;

        private void Start()
        {
            SetupButtonListeners();
            ShowMainMenu();
            LoadSettings();
        }

        private void SetupButtonListeners()
        {
            if (playButton != null) playButton.onClick.AddListener(OnPlayClicked);
            if (optionsButton != null) optionsButton.onClick.AddListener(OnOptionsClicked);
            if (creditsButton != null) creditsButton.onClick.AddListener(OnCreditsClicked);
            if (quitButton != null) quitButton.onClick.AddListener(OnQuitClicked);
            if (backButton != null) backButton.onClick.AddListener(OnBackClicked);

            if (musicVolumeSlider != null) musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            if (sfxVolumeSlider != null) sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }

        private void LoadSettings()
        {
            float musicVolume = PlayerPrefs.GetFloat(Constants.PlayerPrefsKeys.MusicVolume, 1f);
            float sfxVolume = PlayerPrefs.GetFloat(Constants.PlayerPrefsKeys.SFXVolume, 1f);

            if (musicVolumeSlider != null) musicVolumeSlider.value = musicVolume;
            if (sfxVolumeSlider != null) sfxVolumeSlider.value = sfxVolume;
        }

        private void SaveSettings()
        {
            if (musicVolumeSlider != null)
                PlayerPrefs.SetFloat(Constants.PlayerPrefsKeys.MusicVolume, musicVolumeSlider.value);
            if (sfxVolumeSlider != null)
                PlayerPrefs.SetFloat(Constants.PlayerPrefsKeys.SFXVolume, sfxVolumeSlider.value);
            PlayerPrefs.Save();
        }

        #region Button Handlers

        public void OnPlayClicked()
        {
            // Start the game
            GameManager.Instance?.StartGame();

            // Load game scene
            SceneManager.LoadScene(Constants.Scenes.GameScene);
        }

        public void OnOptionsClicked()
        {
            ShowPanel(optionsPanel);
        }

        public void OnCreditsClicked()
        {
            ShowPanel(creditsPanel);
        }

        public void OnQuitClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void OnBackClicked()
        {
            ShowMainMenu();
            SaveSettings();
        }

        public void OnLevelSelectClicked()
        {
            ShowPanel(levelSelectPanel);
        }

        public void OnLevelSelected(int levelIndex)
        {
            // TODO: Start game at specific level
            SceneManager.LoadScene(Constants.Scenes.GameScene);
        }

        #endregion

        #region Volume Handlers

        private void OnMusicVolumeChanged(float value)
        {
            // TODO: Update audio manager
            // AudioManager.Instance?.SetMusicVolume(value);
        }

        private void OnSFXVolumeChanged(float value)
        {
            // TODO: Update audio manager
            // AudioManager.Instance?.SetSFXVolume(value);
        }

        #endregion

        #region Panel Management

        private void ShowMainMenu()
        {
            ShowPanel(mainMenuPanel);
        }

        private void ShowPanel(GameObject panel)
        {
            // Hide all panels
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (optionsPanel != null) optionsPanel.SetActive(false);
            if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
            if (creditsPanel != null) creditsPanel.SetActive(false);

            // Show selected panel
            if (panel != null)
            {
                panel.SetActive(true);
                currentPanel = panel;
            }

            // Show/hide back button
            if (backButton != null)
            {
                backButton.gameObject.SetActive(panel != mainMenuPanel);
            }
        }

        #endregion

        /// <summary>
        /// Handle ESC key to go back.
        /// </summary>
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (currentPanel != mainMenuPanel)
                {
                    OnBackClicked();
                }
            }
        }
    }
}
