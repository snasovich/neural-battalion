using UnityEngine;
using UnityEngine.SceneManagement;
using NeuralBattalion.Data;

namespace NeuralBattalion.Core
{
    /// <summary>
    /// Manages level loading, transitions, and level data.
    /// Responsibilities:
    /// - Load and unload levels
    /// - Parse level data
    /// - Handle scene transitions
    /// - Manage level completion
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        [Header("Level Data")]
        [SerializeField] private LevelData[] levels;

        [Header("Scene Names")]
        [SerializeField] private string mainMenuScene = "MainMenu";
        [SerializeField] private string gameScene = "GameScene";

        public LevelData CurrentLevelData { get; private set; }
        public int CurrentLevelIndex { get; private set; } = -1;
        public int TotalLevels => levels?.Length ?? 0;

        /// <summary>
        /// Load a specific level by index.
        /// </summary>
        /// <param name="levelIndex">Zero-based level index.</param>
        public void LoadLevel(int levelIndex)
        {
            if (levels == null || levelIndex < 0 || levelIndex >= levels.Length)
            {
                Debug.LogError($"[LevelManager] Invalid level index: {levelIndex}");
                return;
            }

            CurrentLevelIndex = levelIndex;
            CurrentLevelData = levels[levelIndex];

            // TODO: Load game scene and build level from data
            // SceneManager.LoadScene(gameScene);
        }

        /// <summary>
        /// Load the next level.
        /// </summary>
        /// <returns>True if next level exists, false if all levels complete.</returns>
        public bool LoadNextLevel()
        {
            int nextIndex = CurrentLevelIndex + 1;

            if (nextIndex >= TotalLevels)
            {
                return false; // All levels complete
            }

            LoadLevel(nextIndex);
            return true;
        }

        /// <summary>
        /// Reload the current level.
        /// </summary>
        public void ReloadCurrentLevel()
        {
            if (CurrentLevelIndex >= 0)
            {
                LoadLevel(CurrentLevelIndex);
            }
        }

        /// <summary>
        /// Load the main menu scene.
        /// </summary>
        public void LoadMainMenu()
        {
            CurrentLevelIndex = -1;
            CurrentLevelData = null;
            SceneManager.LoadScene(mainMenuScene);
        }

        /// <summary>
        /// Async level loading with progress callback.
        /// </summary>
        /// <param name="levelIndex">Level to load.</param>
        /// <param name="onProgress">Progress callback (0-1).</param>
        public void LoadLevelAsync(int levelIndex, System.Action<float> onProgress = null)
        {
            if (levels == null || levelIndex < 0 || levelIndex >= levels.Length)
            {
                Debug.LogError($"[LevelManager] Invalid level index: {levelIndex}");
                return;
            }

            CurrentLevelIndex = levelIndex;
            CurrentLevelData = levels[levelIndex];

            StartCoroutine(LoadLevelCoroutine(onProgress));
        }

        private System.Collections.IEnumerator LoadLevelCoroutine(System.Action<float> onProgress)
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(gameScene);

            while (!asyncLoad.isDone)
            {
                onProgress?.Invoke(asyncLoad.progress);
                yield return null;
            }

            // Build level from data after scene loads
            BuildLevelFromData();
        }

        /// <summary>
        /// Build the level terrain and entities from LevelData.
        /// </summary>
        private void BuildLevelFromData()
        {
            if (CurrentLevelData == null)
            {
                Debug.LogError("[LevelManager] No level data to build from");
                return;
            }

            // TODO: Instantiate terrain tiles based on level data
            // TODO: Set up spawn points
            // TODO: Configure enemy waves
        }
    }
}
