using UnityEngine;

namespace NeuralBattalion.Utility
{
    /// <summary>
    /// Utility class for generating simple colored sprites at runtime.
    /// Used as a fallback when sprite assets are not available.
    /// </summary>
    public static class SpriteGenerator
    {
        /// <summary>
        /// Create a simple 1x1 colored sprite.
        /// </summary>
        /// <param name="color">The color of the sprite.</param>
        /// <returns>A new sprite with the specified color.</returns>
        public static Sprite CreateColoredSprite(Color color)
        {
            // Create a 1x1 texture
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            texture.filterMode = FilterMode.Point;

            // Create a sprite from the texture
            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0, 0, 1, 1),
                new Vector2(0.5f, 0.5f),
                1f // pixels per unit
            );

            return sprite;
        }

        /// <summary>
        /// Create a colored sprite with specified dimensions.
        /// </summary>
        /// <param name="color">The color of the sprite.</param>
        /// <param name="width">Width in pixels.</param>
        /// <param name="height">Height in pixels.</param>
        /// <param name="pixelsPerUnit">Pixels per unit for the sprite.</param>
        /// <returns>A new sprite with the specified color and dimensions.</returns>
        public static Sprite CreateColoredSprite(Color color, int width, int height, float pixelsPerUnit = 100f)
        {
            // Create a texture with specified dimensions
            Texture2D texture = new Texture2D(width, height);
            
            // Fill with color
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            texture.SetPixels(pixels);
            texture.Apply();
            texture.filterMode = FilterMode.Point;

            // Create a sprite from the texture
            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0, 0, width, height),
                new Vector2(0.5f, 0.5f),
                pixelsPerUnit
            );

            return sprite;
        }
    }
}
