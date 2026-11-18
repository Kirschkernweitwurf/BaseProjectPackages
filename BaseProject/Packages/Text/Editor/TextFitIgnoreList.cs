using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Base.TextPackage.Editor
{
    /// <summary>
    /// The text boxes the team marked as fine to leave unchecked, like a debug label or a clock. Saved in the
    /// project settings, so everyone shares the list through version control.
    /// </summary>
    [FilePath(SettingsPath, FilePathAttribute.Location.ProjectFolder)]
    internal sealed class TextFitIgnoreList : ScriptableSingleton<TextFitIgnoreList>
    {
        private const string SettingsPath = "ProjectSettings/TextFitIgnoreList.asset";

        [SerializeField] private List<string> keys = new();

        /// <summary>How many boxes are ignored.</summary>
        internal int Count => keys.Count;

        /// <summary>True when the box at <paramref name="location"/> is ignored.</summary>
        internal bool IsIgnored(TextBoxLocation location) => keys.Contains(location.Key);

        /// <summary>Ignores or checks the box at <paramref name="location"/> again and saves the list.</summary>
        internal void SetIgnored(TextBoxLocation location, bool isIgnored)
        {
            if (isIgnored)
                keys.Add(location.Key);
            else
                keys.Remove(location.Key);

            keys.Sort();
            Save(true);
        }
    }
}