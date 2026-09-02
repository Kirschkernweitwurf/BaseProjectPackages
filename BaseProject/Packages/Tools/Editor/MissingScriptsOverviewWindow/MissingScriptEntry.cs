using System;

namespace Base.ToolsPackage.Editor.MissingScriptsOverviewWindow
{
    /// <summary>
    /// One found missing script, holding enough data to navigate back to it.
    /// </summary>
    internal sealed class MissingScriptEntry
    {
        /// <summary>
        /// Whether the missing script sits in a scene, a prefab, or another asset. This decides how the
        /// entry is navigated back to.
        /// </summary>
        internal EMissingScriptSource Source { get; }

        /// <summary>Scene, prefab, or asset path the object lives in.</summary>
        internal string AssetPath { get; }

        /// <summary>Sibling index chain from the root down to the object. Empty for assets.</summary>
        internal int[] SiblingPath { get; }

        /// <summary>Human-readable hierarchy path or asset name.</summary>
        internal string DisplayPath { get; }

        /// <summary>Number of missing script components on the object.</summary>
        internal int MissingCount { get; }

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