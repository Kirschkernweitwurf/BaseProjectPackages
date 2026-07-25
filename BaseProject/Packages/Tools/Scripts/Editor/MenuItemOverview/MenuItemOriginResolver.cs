#if UNITY_EDITOR
using System;

namespace Base.ToolPackage.Editor.MenuItemOverview
{
    /// <summary>Maps a script asset path onto the place its menu item comes from.</summary>
    public static class MenuItemOriginResolver
    {
        private const string PackagePrefix = "Packages/";
        private const string ProjectPrefix = "Assets/";

        /// <summary>Classifies a project relative script path. An empty path means built in.</summary>
        public static EMenuItemOrigin Classify(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
                return EMenuItemOrigin.BuiltIn;

            if (assetPath.StartsWith(PackagePrefix, StringComparison.Ordinal))
                return EMenuItemOrigin.Package;

            if (assetPath.StartsWith(ProjectPrefix, StringComparison.Ordinal))
                return EMenuItemOrigin.Project;

            return EMenuItemOrigin.BuiltIn;
        }
    }
}
#endif