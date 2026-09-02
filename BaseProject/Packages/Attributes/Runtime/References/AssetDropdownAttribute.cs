using System;
using UnityEngine;

namespace Base.AttributesPackage
{
    /// <summary>
    /// Replaces an object field with a searchable dropdown of the project assets matching a filter, so
    /// a reference can be picked by name instead of found in the Project window and dragged.
    /// </summary>
    /// <remarks>
    /// The filter is the string the asset database itself takes, so anything that works in the Project
    /// window search works here. Restricting the folders is worth doing on a large project: the search
    /// runs against the whole database otherwise.
    /// <para>
    /// Unlike <see cref="ResourcesAssetAttribute"/> this stores a real object reference, so nothing has
    /// to live under a Resources folder and nothing is loaded by path at runtime.
    /// </para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class AssetDropdownAttribute : PropertyAttribute
    {
        /// <summary>Asset database filter, for example <c>t:Material</c> or <c>t:Prefab enemy</c>.</summary>
        public string Filter { get; }

        /// <summary>Folders to search in, or null for the whole project.</summary>
        public string[] SearchInFolders { get; }

        /// <summary>Creates the attribute.</summary>
        /// <param name="filter">Asset database filter. Null derives one from the field type.</param>
        /// <param name="searchInFolders">Folders to search in.</param>
        public AssetDropdownAttribute(string filter = null, params string[] searchInFolders)
        {
            Filter = filter;
            SearchInFolders = searchInFolders != null && searchInFolders.Length > 0
                ? searchInFolders
                : null;
        }
    }
}