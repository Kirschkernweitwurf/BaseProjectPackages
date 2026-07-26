#if UNITY_EDITOR
using System;

namespace Base.ToolPackage.Editor.MenuManagerWindows.CreateAssetMenuOverview
{
    /// <summary>Maps a script asset path onto the place its asset creation entry comes from.</summary>
    public static class CreateAssetOriginResolver
    {
        private const string PackagePrefix = "Packages/";
        private const string ProjectPrefix = "Assets/";

        /// <summary>Classifies a project relative script path. An empty path means built in.</summary>
        public static ECreateAssetOrigin Classify(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
                return ECreateAssetOrigin.BuiltIn;

            if (assetPath.StartsWith(PackagePrefix, StringComparison.Ordinal))
                return ECreateAssetOrigin.Package;

            if (assetPath.StartsWith(ProjectPrefix, StringComparison.Ordinal))
                return ECreateAssetOrigin.Project;

            return ECreateAssetOrigin.BuiltIn;
        }
    }
}
#endif