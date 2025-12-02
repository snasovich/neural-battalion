namespace NeuralBattalion.Core.Events
{
    /// <summary>
    /// Game state events.
    /// </summary>
    public struct GameStartedEvent
    {
        public int Level;
    }

    public struct GamePausedEvent { }

    public struct GameResumedEvent { }

    public struct GameOverEvent
    {
        public bool Victory;
        public int FinalScore;
    }

    public struct GameStateChangedEvent
    {
        public GameState PreviousState;
        public GameState NewState;
    }

    /// <summary>
    /// Level events.
    /// </summary>
    public struct LevelStartedEvent
    {
        public int Level;
        public int TotalEnemies;
    }

    public struct LevelCompletedEvent
    {
        public int Level;
    }

    public struct LevelLoadingEvent
    {
        public float Progress;
    }

    /// <summary>
    /// Player events.
    /// </summary>
    public struct PlayerSpawnedEvent
    {
        public int Lives;
    }

    public struct PlayerDamagedEvent
    {
        public int CurrentHealth;
        public int MaxHealth;
        public int DamageAmount;
    }

    public struct PlayerDeathEvent
    {
        public int RemainingLives;
    }

    public struct PlayerRespawnEvent
    {
        public int RemainingLives;
    }

    public struct PlayerUpgradeEvent
    {
        public UpgradeType UpgradeType;
        public int Level;
    }

    public enum UpgradeType
    {
        Speed,
        FireRate,
        Damage,
        Shield
    }

    /// <summary>
    /// Enemy events.
    /// </summary>
    public struct EnemySpawnedEvent
    {
        public int EnemyId;
        public int EnemyType;
    }

    public struct EnemyDestroyedEvent
    {
        public int EnemyId;
        public int EnemyType;
        public int ScoreValue;
    }

    public struct EnemyWaveStartedEvent
    {
        public int WaveNumber;
        public int EnemyCount;
    }

    /// <summary>
    /// Combat events.
    /// </summary>
    public struct ProjectileFiredEvent
    {
        public bool IsPlayer;
        public UnityEngine.Vector2 Position;
        public UnityEngine.Vector2 Direction;
    }

    public struct ProjectileHitEvent
    {
        public UnityEngine.Vector2 Position;
        public bool HitTerrain;
        public bool HitTank;
    }

    public struct ExplosionEvent
    {
        public UnityEngine.Vector2 Position;
        public float Radius;
    }

    /// <summary>
    /// Terrain events.
    /// </summary>
    public struct TerrainDestroyedEvent
    {
        public UnityEngine.Vector2Int GridPosition;
        public TileDestroyedType TileType;
    }

    public enum TileDestroyedType
    {
        Brick,
        Steel
    }

    public struct BaseDestroyedEvent { }

    /// <summary>
    /// Score events.
    /// </summary>
    public struct ScoreChangedEvent
    {
        public int NewScore;
        public int PointsAdded;
    }

    public struct NewHighScoreEvent
    {
        public int Score;
    }

    /// <summary>
    /// Power-up events.
    /// </summary>
    public struct PowerUpSpawnedEvent
    {
        public PowerUpType Type;
        public UnityEngine.Vector2 Position;
    }

    public struct PowerUpCollectedEvent
    {
        public PowerUpType Type;
    }

    public enum PowerUpType
    {
        ExtraLife,
        SpeedBoost,
        FireRateBoost,
        Shield,
        Bomb,
        Timer,
        Shovel
    }
}
