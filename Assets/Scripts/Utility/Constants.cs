namespace NeuralBattalion.Utility
{
    /// <summary>
    /// Game-wide constants.
    /// Central location for magic numbers and configuration values.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Layer names for physics.
        /// </summary>
        public static class Layers
        {
            public const string Player = "Player";
            public const string Enemy = "Enemy";
            public const string Projectile = "Projectile";
            public const string Terrain = "Terrain";
            public const string Water = "Water";
            public const string Decoration = "Decoration";
        }

        /// <summary>
        /// Tag names for game objects.
        /// </summary>
        public static class Tags
        {
            public const string Player = "Player";
            public const string Enemy = "Enemy";
            public const string PlayerProjectile = "PlayerProjectile";
            public const string EnemyProjectile = "EnemyProjectile";
            public const string Wall = "Wall";
            public const string Obstacle = "Obstacle";
            public const string PowerUp = "PowerUp";
            public const string Base = "Base";
        }

        /// <summary>
        /// Sorting layer names.
        /// </summary>
        public static class SortingLayers
        {
            public const string Ground = "Ground";
            public const string Water = "Water";
            public const string Default = "Default";
            public const string Tanks = "Tanks";
            public const string Projectiles = "Projectiles";
            public const string Decoration = "Decoration";
            public const string Effects = "Effects";
            public const string UI = "UI";
        }

        /// <summary>
        /// Scene names.
        /// </summary>
        public static class Scenes
        {
            public const string MainMenu = "MainMenu";
            public const string GameScene = "GameScene";
            public const string LevelEditor = "LevelEditor";
        }

        /// <summary>
        /// Grid and level constants.
        /// </summary>
        public static class Grid
        {
            public const int DefaultWidth = 26;
            public const int DefaultHeight = 26;
            public const float CellSize = 1f;
        }

        /// <summary>
        /// Player constants.
        /// </summary>
        public static class Player
        {
            public const int DefaultLives = 3;
            public const int MaxLives = 9;
            public const float DefaultSpeed = 5f;
            public const float DefaultFireRate = 0.5f;
            public const float InvulnerabilityDuration = 3f;
        }

        /// <summary>
        /// Enemy constants.
        /// </summary>
        public static class Enemy
        {
            public const int MaxActiveEnemies = 4;
            public const float SpawnDelay = 3f;
            public const float SpawnInterval = 5f;
        }

        /// <summary>
        /// Combat constants.
        /// </summary>
        public static class Combat
        {
            public const float DefaultProjectileSpeed = 10f;
            public const int DefaultProjectileDamage = 1;
            public const float ProjectileLifetime = 3f;
        }

        /// <summary>
        /// Power-up durations.
        /// </summary>
        public static class PowerUps
        {
            public const float ShieldDuration = 10f;
            public const float SpeedBoostDuration = 10f;
            public const float FireRateBoostDuration = 10f;
            public const float TimerDuration = 10f;
            public const float ShovelDuration = 20f;
        }

        /// <summary>
        /// Score values.
        /// </summary>
        public static class Score
        {
            public const int BasicTankScore = 100;
            public const int FastTankScore = 200;
            public const int PowerTankScore = 300;
            public const int ArmorTankScore = 400;
            public const int LevelBonusScore = 1000;
        }

        /// <summary>
        /// Audio mixer group names.
        /// </summary>
        public static class AudioGroups
        {
            public const string Master = "Master";
            public const string Music = "Music";
            public const string SFX = "SFX";
            public const string UI = "UI";
        }

        /// <summary>
        /// Input action names.
        /// </summary>
        public static class InputActions
        {
            public const string Move = "Move";
            public const string Fire = "Fire";
            public const string Pause = "Pause";
        }

        /// <summary>
        /// Animation parameter names.
        /// </summary>
        public static class Animations
        {
            public const string Moving = "IsMoving";
            public const string Firing = "IsFiring";
            public const string Direction = "Direction";
            public const string Dead = "IsDead";
        }

        /// <summary>
        /// PlayerPrefs keys.
        /// </summary>
        public static class PlayerPrefsKeys
        {
            public const string HighScore = "NeuralBattalion_HighScore";
            public const string MusicVolume = "NeuralBattalion_MusicVolume";
            public const string SFXVolume = "NeuralBattalion_SFXVolume";
            public const string LastLevel = "NeuralBattalion_LastLevel";
        }
    }
}
