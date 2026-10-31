using System.Collections.Generic;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Base.DataHubPackage.Editor.Bookmarks
{
    /// <summary>
    /// The assets the designer pinned to the top of the sidebar, like the deck they are tuning this week. Kept in
    /// the user settings of the project, so the pins stay with this project and this person and are never checked
    /// in.
    /// </summary>
    internal sealed class DataPins
    {
        private const string PinsKey = "Base.DataHubPackage.Pins";
        private const char Separator = ';';

        private readonly List<string> _guids = new();
        private readonly List<Object> _assets = new();

        private bool _isLoaded;

        /// <summary>The pinned assets that still exist, in the order they were pinned.</summary>
        internal IReadOnlyList<Object> Assets
        {
            get
            {
                Load();

                return _assets;
            }
        }

        /// <summary>True when <paramref name="asset"/> is pinned.</summary>
        /// <param name="asset">The asset to look for.</param>
        /// <returns>True for a pinned asset.</returns>
        internal bool IsPinned(Object asset)
        {
            Load();

            return _guids.Contains(GuidOf(asset));
        }

        /// <summary>Pins an asset that is not pinned yet, and unpins one that is.</summary>
        /// <param name="asset">The asset to pin or unpin.</param>
        internal void Toggle(Object asset)
        {
            string guid = GuidOf(asset);

            if (string.IsNullOrEmpty(guid))
                return;

            Load();

            if (!_guids.Remove(guid))
                _guids.Add(guid);

            EditorUserSettings.SetConfigValue(PinsKey, string.Join(Separator.ToString(), _guids));
            Invalidate();
        }

        /// <summary>Reads the pins again on next use, after assets were deleted or moved.</summary>
        internal void Invalidate() => _isLoaded = false;

        private static string GuidOf(Object asset) => asset == null
            ? string.Empty
            : AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(asset));

        private void Load()
        {
            if (_isLoaded)
                return;

            _isLoaded = true;
            _guids.Clear();
            _assets.Clear();

            string saved = EditorUserSettings.GetConfigValue(PinsKey);

            if (string.IsNullOrEmpty(saved))
                return;

            foreach (string guid in saved.Split(Separator))
            {
                Object asset = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(guid));

                // A deleted asset keeps its pin, so undoing the delete brings it back.
                _guids.Add(guid);

                if (asset != null)
                    _assets.Add(asset);
            }
        }
    }
}