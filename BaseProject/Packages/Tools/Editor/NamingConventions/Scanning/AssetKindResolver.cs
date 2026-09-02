using System;
using UnityEditor;

namespace Base.ToolsPackage.Editor.NamingConventions.Scanning
{
    /// <summary>
    /// Resolves the kind of an asset. Prefabs and model files both load as GameObject, and every
    /// texture loads as Texture2D no matter what it is used for, so the kind comes from the file
    /// extension and the texture importer instead. That is what lets a rule target prefabs with a
    /// P_ prefix or sprites with their own one without hitting everything else.
    /// </summary>
    internal static class AssetKindResolver
    {
        /// <summary>Kind of textures imported as a cookie.</summary>
        public const string CookieKind = "Cookie";

        /// <summary>Kind of textures imported as a cursor.</summary>
        public const string CursorKind = "Cursor";

        /// <summary>Kind of textures imported as a lightmap.</summary>
        public const string LightmapKind = "Lightmap";

        /// <summary>Kind of imported model files like FBX.</summary>
        public const string ModelKind = "Model";

        /// <summary>Kind of textures imported as a normal map.</summary>
        public const string NormalMapKind = "NormalMap";

        /// <summary>Kind of prefab assets.</summary>
        public const string PrefabKind = "Prefab";

        /// <summary>Kind of textures imported as a sprite, which covers 2D and UI.</summary>
        public const string SpriteKind = "Sprite";

        private const string PrefabExtension = ".prefab";
        private const string UnknownKind = "Unknown";

        private static readonly string[] ModelExtensions =
        {
            ".fbx",
            ".obj",
            ".blend",
            ".dae",
            ".3ds"
        };

        private static readonly string[] TextureExtensions =
        {
            ".png",
            ".jpg",
            ".jpeg",
            ".tga",
            ".psd",
            ".tif",
            ".tiff",
            ".exr",
            ".bmp",
            ".gif",
            ".hdr"
        };

        /// <summary>Kinds that come from the file itself instead of from the asset type.</summary>
        private static readonly string[] ImportedKinds =
        {
            CookieKind,
            CursorKind,
            LightmapKind,
            ModelKind,
            NormalMapKind,
            PrefabKind,
            SpriteKind
        };

        /// <summary>Short kind of the asset, used for rule matching and display.</summary>
        public static string Resolve(string assetPath, Type assetType)
        {
            if (assetPath.EndsWith(PrefabExtension, StringComparison.OrdinalIgnoreCase))
                return PrefabKind;

            if (HasExtension(assetPath, ModelExtensions))
                return ModelKind;

            string textureKind = ResolveTextureKind(assetPath);

            if (textureKind.Length > 0)
                return textureKind;

            return assetType == null
                ? UnknownKind
                : assetType.Name;
        }

        /// <summary>
        /// Type name a detected rule should store: the kind for everything the file itself
        /// decides, otherwise the full type name so the type hierarchy matching applies.
        /// </summary>
        public static string ResolveRuleTypeName(string assetPath, Type assetType)
        {
            string kind = Resolve(assetPath, assetType);

            if (IsImportedKind(kind))
                return kind;

            return assetType == null
                ? string.Empty
                : assetType.FullName;
        }

        /// <summary>True for kinds that are read from the file, not from the asset type.</summary>
        public static bool IsImportedKind(string kind)
        {
            foreach (string imported in ImportedKinds)
            {
                if (imported == kind)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Kind of a texture, taken from its importer. An empty string means the file is not a
        /// texture or is imported as a plain one, which keeps it on the Texture2D rule.
        /// </summary>
        private static string ResolveTextureKind(string assetPath)
        {
            // The importer lookup is the expensive part of a scan, so files that cannot be a
            // texture never reach it.
            if (!HasExtension(assetPath, TextureExtensions))
                return string.Empty;

            if (AssetImporter.GetAtPath(assetPath) is not TextureImporter importer)
                return string.Empty;

            return importer.textureType switch
            {
                TextureImporterType.Sprite => SpriteKind,
                TextureImporterType.NormalMap => NormalMapKind,
                TextureImporterType.Lightmap => LightmapKind,
                TextureImporterType.Cursor => CursorKind,
                TextureImporterType.Cookie => CookieKind,
                _ => string.Empty
            };
        }

        private static bool HasExtension(string assetPath, string[] extensions)
        {
            foreach (string extension in extensions)
            {
                if (assetPath.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }
    }
}