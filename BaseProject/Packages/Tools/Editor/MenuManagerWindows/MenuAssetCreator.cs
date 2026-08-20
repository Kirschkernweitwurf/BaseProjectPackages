using System;
using Base.UtilityPackage.Logging;
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.MenuManagerWindows
{
    /// <summary>
    /// Creates a ScriptableObject asset the way Unity's own create menu does, including the inline
    /// rename in the project window. Shared by the menu registration and the command palette so
    /// both produce the exact same result.
    /// </summary>
    internal static class MenuAssetCreator
    {
        private const string AssetExtension = ".asset";
        private const string NewAssetPrefix = "New ";

        /// <summary>Creates a new asset and starts the inline rename in the project window.</summary>
        /// <param name="type">Concrete ScriptableObject type to create.</param>
        /// <param name="fileName">File name without extension. Falls back to the type name.</param>
        public static void Create(Type type, string fileName)
        {
            if (type == null)
            {
                CustomLogger.LogError("Cannot create an asset without a type.", null);
                return;
            }

            ScriptableObject instance = ScriptableObject.CreateInstance(type);

            if (instance == null)
            {
                CustomLogger.LogError($"Could not create an instance of {type.Name}.", null);
                return;
            }

            string resolved = string.IsNullOrWhiteSpace(fileName)
                ? NewAssetPrefix + type.Name
                : fileName;

            ProjectWindowUtil.CreateAsset(instance, resolved + AssetExtension);
        }
    }
}