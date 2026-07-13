using System;
using System.IO;

namespace Base.ToolPackage.Editor.MissingScriptsOverviewWindow
{
    /// <summary>
    /// One found missing script, holding enough data to navigate back to it.
    /// </summary>
    public sealed class MissingScriptEntry
    {
        public EMissingScriptSource Source { get; }

        /// <summary>Scene, prefab, or asset path the object lives in.</summary>
        public string AssetPath { get; }

        /// <summary>Sibling index chain from the root down to the object. Empty for assets.</summary>
        public int[] SiblingPath { get; }

        /// <summary>Human readable hierarchy path or asset name.</summary>
        public string DisplayPath { get; }

        /// <summary>Number of missing script components on the object.</summary>
        public int MissingCount { get; }

        /// <summary>True when the asset itself could not be loaded at all.</summary>
        public bool AssetFailedToLoad { get; }

        public string FileName => Path.GetFileName(AssetPath);

        public MissingScriptEntry(EMissingScriptSource source,
            string assetPath,
            int[] siblingPath,
            string displayPath,
            int missingCount,
            bool assetFailedToLoad = false)
        {
            Source = source;
            AssetPath = assetPath;
            SiblingPath = siblingPath ?? Array.Empty<int>();
            DisplayPath = displayPath;
            MissingCount = missingCount;
            AssetFailedToLoad = assetFailedToLoad;
        }
    }
}