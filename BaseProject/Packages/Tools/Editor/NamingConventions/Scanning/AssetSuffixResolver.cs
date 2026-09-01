using Base.ToolPackage.Editor.NamingConventions.Data;
using UnityEditor;

namespace Base.ToolPackage.Editor.NamingConventions.Scanning
{
    /// <summary>
    /// Reads the sub type of a texture from its importer and turns it into the suffix the rule
    /// asks for. Artists usually mark a normal map with _N, and the importer already knows which
    /// texture is one, so the tool can demand and suggest the right suffix instead of leaving it
    /// to everyone's memory.
    /// </summary>
    internal static class AssetSuffixResolver
    {
        private const string LightmapSuffix = "_L";
        private const string NormalSuffix = "_N";

        /// <summary>
        /// Suffix this asset has to carry, or an empty string when the sub type is unknown or the
        /// rule does not list the suffix at all.
        /// </summary>
        public static string Resolve(string assetPath, NamingRule rule)
        {
            if (rule.Suffixes.Count == 0)
                return string.Empty;

            // Only textures carry a sub type the importer knows, so everything else skips the
            // importer lookup entirely.
            if (!rule.Suffixes.Contains(NormalSuffix)
                && !rule.Suffixes.Contains(LightmapSuffix))
                return string.Empty;

            if (AssetImporter.GetAtPath(assetPath) is not TextureImporter importer)
                return string.Empty;

            string suffix = SuffixOf(importer.textureType);

            return rule.Suffixes.Contains(suffix)
                ? suffix
                : string.Empty;
        }

        private static string SuffixOf(TextureImporterType type) => type switch
        {
            TextureImporterType.NormalMap => NormalSuffix,
            TextureImporterType.Lightmap => LightmapSuffix,
            _ => string.Empty
        };
    }
}