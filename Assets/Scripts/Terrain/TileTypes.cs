namespace NeuralBattalion.Terrain
{
    /// <summary>
    /// Defines the types of terrain tiles in the game.
    /// 
    /// Tile Properties:
    /// - Empty: Passable, bullets pass through
    /// - Brick: Destructible, blocks movement and bullets
    /// - Steel: Indestructible (except with power-up), blocks movement and bullets
    /// - Water: Blocks movement, bullets pass through
    /// - Trees: Passable, bullets pass through, hides tanks
    /// - Ice: Passable, tanks slide, bullets pass through
    /// - Base: Player base, game over if destroyed
    /// </summary>
    public enum TileType
    {
        /// <summary>
        /// Empty ground - fully passable.
        /// </summary>
        Empty = 0,

        /// <summary>
        /// Brick wall - destructible obstacle.
        /// Can be destroyed by any projectile.
        /// </summary>
        Brick = 1,

        /// <summary>
        /// Steel wall - normally indestructible.
        /// Only destroyed by power projectile.
        /// </summary>
        Steel = 2,

        /// <summary>
        /// Water - blocks tanks, bullets pass through.
        /// </summary>
        Water = 3,

        /// <summary>
        /// Trees/Bushes - tanks can pass and hide.
        /// Provides visual cover.
        /// </summary>
        Trees = 4,

        /// <summary>
        /// Ice - tanks slide when moving.
        /// Reduces traction.
        /// </summary>
        Ice = 5,

        /// <summary>
        /// Player base - must be protected.
        /// Game over if destroyed.
        /// </summary>
        Base = 6,

        /// <summary>
        /// Spawn point for player.
        /// </summary>
        PlayerSpawn = 7,

        /// <summary>
        /// Spawn point for enemies.
        /// </summary>
        EnemySpawn = 8
    }

    /// <summary>
    /// Static helper class for tile type properties.
    /// </summary>
    public static class TileTypeHelper
    {
        /// <summary>
        /// Check if a tile blocks tank movement.
        /// </summary>
        public static bool BlocksMovement(TileType type)
        {
            return type switch
            {
                TileType.Brick => true,
                TileType.Steel => true,
                TileType.Water => true,
                TileType.Base => true,
                _ => false
            };
        }

        /// <summary>
        /// Check if a tile blocks projectiles.
        /// </summary>
        public static bool BlocksProjectiles(TileType type)
        {
            return type switch
            {
                TileType.Brick => true,
                TileType.Steel => true,
                TileType.Base => true,
                _ => false
            };
        }

        /// <summary>
        /// Check if a tile can be destroyed by normal projectiles.
        /// </summary>
        public static bool IsDestructible(TileType type)
        {
            return type == TileType.Brick || type == TileType.Base;
        }

        /// <summary>
        /// Check if a tile provides visual cover.
        /// </summary>
        public static bool ProvidesCover(TileType type)
        {
            return type == TileType.Trees;
        }

        /// <summary>
        /// Check if a tile affects movement (ice).
        /// </summary>
        public static bool AffectsMovement(TileType type)
        {
            return type == TileType.Ice;
        }

        /// <summary>
        /// Get the character representation for level editor.
        /// </summary>
        public static char ToChar(TileType type)
        {
            return type switch
            {
                TileType.Empty => '.',
                TileType.Brick => '#',
                TileType.Steel => '@',
                TileType.Water => '~',
                TileType.Trees => '*',
                TileType.Ice => '-',
                TileType.Base => 'B',
                TileType.PlayerSpawn => 'P',
                TileType.EnemySpawn => 'E',
                _ => '?'
            };
        }

        /// <summary>
        /// Parse a character to tile type.
        /// </summary>
        public static TileType FromChar(char c)
        {
            return c switch
            {
                '.' => TileType.Empty,
                '#' => TileType.Brick,
                '@' => TileType.Steel,
                '~' => TileType.Water,
                '*' => TileType.Trees,
                '-' => TileType.Ice,
                'B' => TileType.Base,
                'P' => TileType.PlayerSpawn,
                'E' => TileType.EnemySpawn,
                _ => TileType.Empty
            };
        }
    }
}
