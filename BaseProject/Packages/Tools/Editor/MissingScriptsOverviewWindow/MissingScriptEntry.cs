using System;

namespace Base.ToolPackage.Editor.MissingScriptsOverviewWindow
{
    /// <summary>
    /// One found missing script, holding enough data to navigate back to it.
    /// </summary>
    internal sealed class MissingScriptEntry
    {
        public EMissingScriptSource Source { get; }

        /// <summary>Scene, prefab, or asset path the object lives in.</summary>
        public string AssetPath { get; }

        /// <summary>Sibling index chain from the root down to the object. Empty for assets.</summary>
        public int[] SiblingPath { get; }

        /// <summary>Human-readable hierarchy path or asset name.</summary>
        public string DisplayPath { get; }

        /// <summary>Number of missing script components on the object.</summary>
        public int MissingCount { get; }

        /// <summary>Creates an entry pointing at one missing script occurrence.</summary>
        public MissingScriptEntry(EMissingScriptSource source, string assetPath, int[] siblingPath,
            string displayPath, int missingCount)
        {
            Source = source;
            AssetPath = assetPath;
            SiblingPath = siblingPath ?? Array.Empty<int>();
            DisplayPath = displayPath;
            MissingCount = missingCount;
        }
    }
}