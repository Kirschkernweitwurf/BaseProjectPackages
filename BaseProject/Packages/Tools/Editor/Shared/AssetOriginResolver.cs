using System;

namespace Base.ToolPackage.Editor.Shared
{
    /// <summary>Maps a project relative asset path onto the place its source file comes from.</summary>
    internal static class AssetOriginResolver
    {
        private const string PackagePrefix = "Packages/";
        private const string ProjectPrefix = "Assets/";

        /// <summary>
        /// Classifies a project relative asset path. An empty path means the source is built into
        /// Unity and has no file to open.
        /// </summary>
        /// <param name="assetPath">Project relative path of the asset, for example Assets/Foo.cs.</param>
        /// <returns>The origin the path points at.</returns>
        public static EAssetOrigin Classify(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
                return EAssetOrigin.BuiltIn;

            if (assetPath.StartsWith(PackagePrefix, StringComparison.Ordinal))
                return EAssetOrigin.Package;

            if (assetPath.StartsWith(ProjectPrefix, StringComparison.Ordinal))
                return EAssetOrigin.Project;

            return EAssetOrigin.BuiltIn;
        }
    }
}