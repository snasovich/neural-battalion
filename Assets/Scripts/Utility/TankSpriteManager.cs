using UnityEngine;

namespace NeuralBattalion.Utility
{
    /// <summary>
    /// Manages directional tank sprites.
    /// Creates sprites for 4 directions (Up, Down, Left, Right) with tank-like appearance.
    /// Provides fallback sprites when assets are not available.
    /// </summary>
    public class TankSpriteManager
    {
        /// <summary>
        /// Tank facing direction.
        /// </summary>
        public enum Direction
        {
            Up,     // 0 degrees (forward)
            Right,  // 90 degrees
            Down,   // 180 degrees
            Left    // 270 degrees
        }

        /// <summary>
        /// Container for directional sprites.
        /// </summary>
        public class DirectionalSprites
        {
            public Sprite Up;
            public Sprite Right;
            public Sprite Down;
            public Sprite Left;

            /// <summary>
            /// Get sprite for a specific direction.
            /// </summary>
            public Sprite GetSprite(Direction direction)
            {
                return direction switch
                {
                    Direction.Up => Up,
                    Direction.Right => Right,
                    Direction.Down => Down,
                    Direction.Left => Left,
                    _ => Up
                };
            }
        }

        /// <summary>
        /// Create directional sprites for a tank with a specific color.
        /// </summary>
        /// <param name="color">Base color for the tank.</param>
        /// <param name="size">Size of the sprite in pixels (default 32).</param>
        /// <returns>DirectionalSprites containing sprites for all 4 directions.</returns>
        public static DirectionalSprites CreateDirectionalSprites(Color color, int size = 32)
        {
            return new DirectionalSprites
            {
                Up = CreateTankSprite(color, Direction.Up, size),
                Right = CreateTankSprite(color, Direction.Right, size),
                Down = CreateTankSprite(color, Direction.Down, size),
                Left = CreateTankSprite(color, Direction.Left, size)
            };
        }

        /// <summary>
        /// Create a tank sprite facing a specific direction.
        /// NOTE: The created texture should be explicitly destroyed when no longer needed
        /// using DestroyDirectionalSprites() to prevent memory leaks.
        /// </summary>
        private static Sprite CreateTankSprite(Color color, Direction direction, int size)
        {
            Texture2D texture = new Texture2D(size, size);
            Color darkerColor = new Color(color.r * 0.4f, color.g * 0.4f, color.b * 0.4f, color.a);
            // Make the gun/turret much more visible - use black for high contrast
            Color lighterColor = Color.black;

            // Fill with transparent pixels
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }

            // Draw tank based on direction
            switch (direction)
            {
                case Direction.Up:
                    DrawTankUp(texture, size, color, darkerColor, lighterColor);
                    break;
                case Direction.Right:
                    DrawTankRight(texture, size, color, darkerColor, lighterColor);
                    break;
                case Direction.Down:
                    DrawTankDown(texture, size, color, darkerColor, lighterColor);
                    break;
                case Direction.Left:
                    DrawTankLeft(texture, size, color, darkerColor, lighterColor);
                    break;
            }

            texture.Apply();
            texture.filterMode = FilterMode.Point;

            return Sprite.Create(
                texture,
                new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f),
                size // pixels per unit
            );
        }

        /// <summary>
        /// Destroy directional sprites and their associated textures.
        /// Call this when sprites are no longer needed to free memory.
        /// </summary>
        public static void DestroyDirectionalSprites(DirectionalSprites sprites)
        {
            if (sprites == null) return;

            DestroySprite(sprites.Up);
            DestroySprite(sprites.Right);
            DestroySprite(sprites.Down);
            DestroySprite(sprites.Left);
        }

        /// <summary>
        /// Helper to destroy a sprite and its texture.
        /// </summary>
        private static void DestroySprite(Sprite sprite)
        {
            if (sprite == null) return;
            
            Texture2D texture = sprite.texture;
            Object.Destroy(sprite);
            if (texture != null)
            {
                Object.Destroy(texture);
            }
        }

        /// <summary>
        /// Draw tank facing up.
        /// Simple full-square design for clearer collision.
        /// </summary>
        private static void DrawTankUp(Texture2D texture, int size, Color color, Color darkerColor, Color lighterColor)
        {
            int center = size / 2;
            int fullSize = size - 2; // Leave 1px border for clarity
            int gunWidth = size / 4;
            int gunHeight = size / 3;

            // Draw full square tank body
            DrawRect(texture, 1, 1, fullSize, fullSize, color);
            
            // Draw tracks on sides (darker stripes)
            int trackWidth = size / 6;
            DrawRect(texture, 2, 1, trackWidth, fullSize, darkerColor);
            DrawRect(texture, size - trackWidth - 2, 1, trackWidth, fullSize, darkerColor);

            // Draw gun barrel (pointing up) - black for high contrast
            // In Texture2D, Y=0 is bottom, so high Y value = top of sprite = up direction
            DrawRect(texture, center - gunWidth / 2, size - gunHeight - 1, gunWidth, gunHeight, lighterColor);
        }

        /// <summary>
        /// Draw tank facing right.
        /// Simple full-square design for clearer collision.
        /// </summary>
        private static void DrawTankRight(Texture2D texture, int size, Color color, Color darkerColor, Color lighterColor)
        {
            int center = size / 2;
            int fullSize = size - 2; // Leave 1px border for clarity
            int gunWidth = size / 3;
            int gunHeight = size / 4;

            // Draw full square tank body
            DrawRect(texture, 1, 1, fullSize, fullSize, color);
            
            // Draw tracks on top and bottom (darker stripes)
            int trackHeight = size / 6;
            DrawRect(texture, 1, 2, fullSize, trackHeight, darkerColor);
            DrawRect(texture, 1, size - trackHeight - 2, fullSize, trackHeight, darkerColor);

            // Draw gun barrel (pointing right) - black for high contrast
            DrawRect(texture, size - gunWidth - 1, center - gunHeight / 2, gunWidth, gunHeight, lighterColor);
        }

        /// <summary>
        /// Draw tank facing down.
        /// Simple full-square design for clearer collision.
        /// </summary>
        private static void DrawTankDown(Texture2D texture, int size, Color color, Color darkerColor, Color lighterColor)
        {
            int center = size / 2;
            int fullSize = size - 2; // Leave 1px border for clarity
            int gunWidth = size / 4;
            int gunHeight = size / 3;

            // Draw full square tank body
            DrawRect(texture, 1, 1, fullSize, fullSize, color);
            
            // Draw tracks on sides (darker stripes)
            int trackWidth = size / 6;
            DrawRect(texture, 2, 1, trackWidth, fullSize, darkerColor);
            DrawRect(texture, size - trackWidth - 2, 1, trackWidth, fullSize, darkerColor);

            // Draw gun barrel (pointing down) - black for high contrast
            // In Texture2D, Y=0 is bottom, so low Y value = bottom of sprite = down direction
            DrawRect(texture, center - gunWidth / 2, 1, gunWidth, gunHeight, lighterColor);
        }

        /// <summary>
        /// Draw tank facing left.
        /// Simple full-square design for clearer collision.
        /// </summary>
        private static void DrawTankLeft(Texture2D texture, int size, Color color, Color darkerColor, Color lighterColor)
        {
            int center = size / 2;
            int fullSize = size - 2; // Leave 1px border for clarity
            int gunWidth = size / 3;
            int gunHeight = size / 4;

            // Draw full square tank body
            DrawRect(texture, 1, 1, fullSize, fullSize, color);
            
            // Draw tracks on top and bottom (darker stripes)
            int trackHeight = size / 6;
            DrawRect(texture, 1, 2, fullSize, trackHeight, darkerColor);
            DrawRect(texture, 1, size - trackHeight - 2, fullSize, trackHeight, darkerColor);

            // Draw gun barrel (pointing left) - black for high contrast
            DrawRect(texture, 1, center - gunHeight / 2, gunWidth, gunHeight, lighterColor);
        }

        /// <summary>
        /// Helper method to draw a filled rectangle on a texture.
        /// </summary>
        private static void DrawRect(Texture2D texture, int x, int y, int width, int height, Color color)
        {
            for (int dy = 0; dy < height; dy++)
            {
                for (int dx = 0; dx < width; dx++)
                {
                    int px = x + dx;
                    int py = y + dy;
                    if (px >= 0 && px < texture.width && py >= 0 && py < texture.height)
                    {
                        texture.SetPixel(px, py, color);
                    }
                }
            }
        }

        /// <summary>
        /// Get direction from an angle in degrees.
        /// </summary>
        /// <param name="angle">Angle in degrees (0 = up, 90 = right, 180 = down, 270 = left).</param>
        /// <returns>Direction enum value.</returns>
        public static Direction GetDirectionFromAngle(float angle)
        {
            // Normalize angle to 0-360 range
            angle = (angle % 360 + 360) % 360;

            // Determine direction based on angle ranges
            // 0° is up, 90° is right, 180° is down, 270° is left
            if (angle >= 315 || angle < 45)
                return Direction.Up;
            else if (angle >= 45 && angle < 135)
                return Direction.Right;
            else if (angle >= 135 && angle < 225)
                return Direction.Down;
            else
                return Direction.Left;
        }

        /// <summary>
        /// Get direction from a Vector2 direction vector.
        /// </summary>
        /// <param name="directionVector">Normalized direction vector.</param>
        /// <returns>Direction enum value.</returns>
        public static Direction GetDirectionFromVector(Vector2 directionVector)
        {
            // Calculate angle from vector
            float angle = Mathf.Atan2(directionVector.x, directionVector.y) * Mathf.Rad2Deg;
            return GetDirectionFromAngle(angle);
        }
    }
}
