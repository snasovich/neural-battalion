namespace NeuralBattalion.Enemy
{
    /// <summary>
    /// Definitions for different enemy tank types.
    /// Each type has different stats and behaviors.
    /// 
    /// Classic Battle City Enemy Types:
    /// - Basic: Standard tank, slow speed, low health
    /// - Fast: Increased speed, low health
    /// - Power: Standard speed, can destroy steel walls
    /// - Armor: Slow speed, high health (4 hits)
    /// </summary>
    public static class EnemyTypes
    {
        /// <summary>
        /// Basic tank - Standard enemy.
        /// Speed: Normal
        /// Health: 1
        /// Score: 100
        /// </summary>
        public const int Basic = 0;

        /// <summary>
        /// Fast tank - High speed enemy.
        /// Speed: Fast
        /// Health: 1
        /// Score: 200
        /// </summary>
        public const int Fast = 1;

        /// <summary>
        /// Power tank - Can destroy steel walls.
        /// Speed: Normal
        /// Health: 1
        /// Score: 300
        /// Ability: Destroys steel walls
        /// </summary>
        public const int Power = 2;

        /// <summary>
        /// Armor tank - Heavy tank with high health.
        /// Speed: Slow
        /// Health: 4
        /// Score: 400
        /// Note: Changes color with each hit
        /// </summary>
        public const int Armor = 3;

        /// <summary>
        /// Total number of enemy types.
        /// </summary>
        public const int Count = 4;

        /// <summary>
        /// Get display name for enemy type.
        /// </summary>
        /// <param name="type">Enemy type index.</param>
        /// <returns>Display name.</returns>
        public static string GetName(int type)
        {
            return type switch
            {
                Basic => "Basic",
                Fast => "Fast",
                Power => "Power",
                Armor => "Armor",
                _ => "Unknown"
            };
        }

        /// <summary>
        /// Get base score value for enemy type.
        /// </summary>
        /// <param name="type">Enemy type index.</param>
        /// <returns>Score value.</returns>
        public static int GetScoreValue(int type)
        {
            return type switch
            {
                Basic => 100,
                Fast => 200,
                Power => 300,
                Armor => 400,
                _ => 100
            };
        }

        /// <summary>
        /// Get base health for enemy type.
        /// </summary>
        /// <param name="type">Enemy type index.</param>
        /// <returns>Health value.</returns>
        public static int GetHealth(int type)
        {
            return type switch
            {
                Armor => 4,
                _ => 1
            };
        }

        /// <summary>
        /// Get speed multiplier for enemy type.
        /// </summary>
        /// <param name="type">Enemy type index.</param>
        /// <returns>Speed multiplier.</returns>
        public static float GetSpeedMultiplier(int type)
        {
            return type switch
            {
                Fast => 1.5f,
                Armor => 0.7f,
                _ => 1f
            };
        }

        /// <summary>
        /// Check if enemy type can destroy steel walls.
        /// </summary>
        /// <param name="type">Enemy type index.</param>
        /// <returns>True if can destroy steel.</returns>
        public static bool CanDestroySteel(int type)
        {
            return type == Power;
        }
    }
}
