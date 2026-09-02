using System.Collections.Generic;
using UnityEngine;

namespace Base.EditorUIPackage.Editor
{
    /// <summary>
    /// Generates and owns the flat and rounded background textures a window draws its cards, pills
    /// and hand-drawn buttons with. Textures created here are hidden and not saved, so the owning
    /// window has to <see cref="Release"/> them when it closes or when the editor skin changes.
    /// </summary>
    public sealed class EditorTextureCache
    {
        private readonly List<Texture2D> _owned = new();

        /// <summary>
        /// A one pixel texture of a flat color.
        /// </summary>
        /// <param name="color">The color to fill with.</param>
        /// <returns>The generated texture.</returns>
        public Texture2D Solid(Color color)
        {
            Texture2D texture = Create(1);

            texture.SetPixels(new[]
            {
                color
            });

            texture.Apply();

            return texture;
        }

        /// <summary>
        /// A nine-sliced rounded rectangle: a square of <c>2 * radius + 1</c> pixels whose single
        /// center pixel stretches, so the corners keep their true size at any target rectangle.
        /// Assign it to a style's background and set the style's border to the same radius.
        /// </summary>
        /// <param name="color">The fill color.</param>
        /// <param name="radius">The corner radius in pixels.</param>
        /// <returns>The generated texture.</returns>
        public Texture2D Rounded(Color color, int radius)
        {
            int size = radius * 2 + 1;
            Texture2D texture = Create(size);
            Color[] pixels = new Color[size * size];

            for (int index = 0; index < pixels.Length; index++)
                pixels[index] = CornerAlpha(color, index % size, index / size, size, radius);

            texture.SetPixels(pixels);
            texture.Apply();

            return texture;
        }

        /// <summary>Destroys every texture handed out so far. Call when the owner closes.</summary>
        public void Release()
        {
            foreach (Texture2D texture in _owned)
            {
                if (texture != null)
                    Object.DestroyImmediate(texture);
            }

            _owned.Clear();
        }

        private static Color CornerAlpha(Color color, int x, int y, int size, int radius)
        {
            // Distance from the pixel center to the nearest point of the rectangle inset by the
            // radius. Inside that core the pixel is solid; near a corner it fades over one pixel.
            float pointX = x + 0.5f;
            float pointY = y + 0.5f;

            float nearestX = Mathf.Clamp(pointX, radius, size - radius);
            float nearestY = Mathf.Clamp(pointY, radius, size - radius);

            float distance = Mathf.Sqrt(Square(pointX - nearestX) + Square(pointY - nearestY));
            float coverage = Mathf.Clamp01(radius + 0.5f - distance);

            return new Color(color.r, color.g, color.b, color.a * coverage);
        }

        private static float Square(float value) => value * value;

        private Texture2D Create(int size)
        {
            Texture2D texture = new(size, size, TextureFormat.RGBA32, false)
            {
                hideFlags = HideFlags.HideAndDontSave,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            _owned.Add(texture);

            return texture;
        }
    }
}