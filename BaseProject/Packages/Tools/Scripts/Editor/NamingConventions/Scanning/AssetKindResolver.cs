using System;

namespace Base.ToolPackage.Editor.NamingConventions.Scanning
{
    /// <summary>
    /// Resolves the kind of an asset. Prefabs and model files both load as GameObject, so their
    /// kind comes from the file extension instead. This is what lets a rule target prefabs with a
    /// P_ prefix without also hitting imported models.
    /// </summary>
    public static class AssetKindResolver
    {
        /// <summary>Kind of imported model files like FBX.</summary>
        public const string ModelKind = "Model";

        /// <summary>Kind of prefab assets.</summary>
        public const string PrefabKind = "Prefab";

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

        /// <summary>Short kind of the asset, used for rule matching and display.</summary>
        public static string Resolve(string assetPath, Type assetType)
        {
            if (assetPath.EndsWith(PrefabExtension, StringComparison.OrdinalIgnoreCase))
                return PrefabKind;

            if (IsModel(assetPath))
                return ModelKind;

            return assetType == null
                ? UnknownKind
                : assetType.Name;
        }

        /// <summary>
        /// Type name a detected rule should store: the kind for extension based kinds, otherwise
        /// the full type name so the type hierarchy matching applies.
        /// </summary>
        public static string ResolveRuleTypeName(string assetPath, Type assetType)
        {
            string kind = Resolve(assetPath, assetType);

            if (kind == PrefabKind
                || kind == ModelKind)
                return kind;

            return assetType == null
                ? string.Empty
                : assetType.FullName;
        }

        private static bool IsModel(string assetPath)
        {
            foreach (string extension in ModelExtensions)
            {
                if (assetPath.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }
    }
}
