using System;

namespace Base.DataHubPackage.Editor
{
    /// <summary>One data asset of the project, known by path and main type without being loaded.</summary>
    internal readonly struct DataAssetEntry
    {
        /// <summary>Project path of the asset.</summary>
        internal readonly string Path;

        /// <summary>Type of the main asset.</summary>
        internal readonly Type Type;

        /// <summary>Creates an entry.</summary>
        /// <param name="path">Project path of the asset.</param>
        /// <param name="type">Type of the main asset.</param>
        internal DataAssetEntry(string path, Type type)
        {
            Path = path;
            Type = type;
        }
    }
}