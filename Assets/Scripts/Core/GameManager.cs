using UnityEngine;
using NeuralBattalion.Core.Events;

namespace NeuralBattalion.Core
{
    /// <summary>
    /// Singleton managing overall game state and high-level game flow.
    /// Responsibilities:
    /// - Manage game states (MainMenu, Playing, Paused, GameOver)
    /// - Coordinate between major systems
    /// - Handle scene transitions
    /// - Persist game settings
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private LevelManager levelManager;
        [SerializeField] private ScoreManager scoreManager;

        public GameState CurrentState { get; private set; } = GameState.MainMenu;
        public int CurrentLevel { get; private set; } = 1;
        public bool IsPaused => CurrentState == GameState.Paused;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSystems();
        }

        private void InitializeSystems()
        {
            // TODO: Initialize all game systems
            // - Audio system
            // - Input system
            // - Save/Load system
        }

        /// <summary>
        /// Start a new game from level 1.
        /// </summary>
        public void StartGame()
        {
            CurrentLevel = 1;
            ChangeState(GameState.Playing);
            EventBus.Publish(new GameStartedEvent { Level = CurrentLevel });

            // TODO: Load first level
        }

        /// <summary>
        /// Continue to the next level.
        /// </summary>
        public void NextLevel()
        {
            CurrentLevel++;
            EventBus.Publish(new LevelCompletedEvent { Level = CurrentLevel - 1 });

            // TODO: Load next level
        }

        /// <summary>
        /// Pause the game.
        /// </summary>
        public void PauseGame()
        {
            if (CurrentState != GameState.Playing) return;

            ChangeState(GameState.Paused);
            Time.timeScale = 0f;
            EventBus.Publish(new GamePausedEvent());
        }

        /// <summary>
        /// Resume the game from pause.
        /// </summary>
        public void ResumeGame()
        {
            if (CurrentState != GameState.Paused) return;

            ChangeState(GameState.Playing);
            Time.timeScale = 1f;
            EventBus.Publish(new GameResumedEvent());
        }

        /// <summary>
        /// End the current game.
        /// </summary>
        /// <param name="victory">True if player won, false if defeated.</param>
        public void EndGame(bool victory)
        {
            ChangeState(GameState.GameOver);
            Time.timeScale = 1f;
            EventBus.Publish(new GameOverEvent { Victory = victory, FinalScore = scoreManager?.CurrentScore ?? 0 });
        }

        /// <summary>
        /// Return to main menu.
        /// </summary>
        public void ReturnToMainMenu()
        {
            ChangeState(GameState.MainMenu);
            Time.timeScale = 1f;

            // TODO: Load main menu scene
        }

        private void ChangeState(GameState newState)
        {
            var previousState = CurrentState;
            CurrentState = newState;
            EventBus.Publish(new GameStateChangedEvent
            {
                PreviousState = previousState,
                NewState = newState
            });
        }
    }

    /// <summary>
    /// Possible game states.
    /// </summary>
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver,
        Loading
    }
}
