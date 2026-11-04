using System.Collections.Generic;
using Base.ToolsPackage.Editor.AssetZoo.Builder;
using UnityEditor;
using UnityEngine;

namespace Base.ToolsPackage.Editor.AssetZoo.UI
{
    /// <summary>
    /// Gets the picture of an asset for the group list. Unity renders previews in the background, so a thumbnail
    /// can come back empty at first and the window has to keep repainting until <see cref="IsLoading"/> is false.
    /// </summary>
    internal static class ZooThumbnails
    {
        /// <summary>
        /// Enough for every thumbnail on screen at once. Unity evicts previews beyond its cache size, which shows
        /// as thumbnails flickering back to icons.
        /// </summary>
        private const int PreviewCacheSize = 512;

        private static readonly Dictionary<GameObject, DecalPicture> DecalPictures = new();

        private static bool _hasRaisedCache;

        /// <summary>True while Unity is still rendering previews.</summary>
        public static bool IsLoading => AssetPreview.IsLoadingAssetPreviews();

        /// <summary>Draws the best picture of an asset available right now.</summary>
        /// <param name="rect">Where it goes.</param>
        /// <param name="asset">A prefab or model.</param>
        /// <param name="alpha">How solid it is drawn.</param>
        /// <param name="radius">The corner radius.</param>
        public static void Draw(Rect rect, GameObject asset, float alpha, float radius)
        {
            RaiseCacheOnce();

            if (TryGetDecalPicture(asset, out DecalPicture decal))
            {
                DrawDecal(rect, decal, alpha);
                return;
            }

            Texture preview = AssetPreview.GetAssetPreview(asset);

            if (preview == null)
                preview = AssetPreview.GetMiniThumbnail(asset);

            if (preview != null)
            {
                GUI.DrawTexture(rect, preview, ScaleMode.ScaleToFit, true, 0f, new Color(1f, 1f, 1f, alpha), 0f,
                    radius);
            }
        }

        // Cut to the shape the decal has in its atlas, so a poster looks tall and a sticker looks small.
        private static void DrawDecal(Rect rect, DecalPicture decal, float alpha)
        {
            float width = decal.Uv.width * decal.Texture.width;
            float height = decal.Uv.height * decal.Texture.height;
            float fit = Mathf.Min(rect.width / width, rect.height / height);

            Rect fitted = new(rect.center.x - width * fit * 0.5f, rect.center.y - height * fit * 0.5f, width * fit,
                height * fit);

            Color previous = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, alpha);
            GUI.DrawTextureWithTexCoords(fitted, decal.Texture, decal.Uv);
            GUI.color = previous;
        }

        private static void RaiseCacheOnce()
        {
            if (_hasRaisedCache)
                return;

            _hasRaisedCache = true;
            AssetPreview.SetPreviewTextureCacheSize(PreviewCacheSize);
        }

        // A decal has no renderer, so Unity's preview of it is empty. Its picture is the next best thing.
        private static bool TryGetDecalPicture(GameObject asset, out DecalPicture picture)
        {
            if (DecalPictures.TryGetValue(asset, out picture))
                return picture.Texture != null;

            if (ZooVisuals.TryGetDecal(asset, out Behaviour decal))
            {
                Texture texture = ZooVisuals.FindDecalPicture(decal, out Rect uv);
                picture = new DecalPicture(texture, uv);
            }

            DecalPictures[asset] = picture;

            return picture.Texture != null;
        }

        private readonly struct DecalPicture
        {
            /// <summary>The texture, which may be an atlas of many decals.</summary>
            public Texture Texture { get; }

            /// <summary>The part of the texture this decal shows.</summary>
            public Rect Uv { get; }

            /// <summary>Records what a decal shows.</summary>
            /// <param name="texture">The texture.</param>
            /// <param name="uv">The part of it this decal shows.</param>
            public DecalPicture(Texture texture, Rect uv)
            {
                Texture = texture;
                Uv = uv;
            }
        }
    }
}