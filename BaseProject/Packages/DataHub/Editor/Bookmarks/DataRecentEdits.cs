using System;
using System.Collections.Generic;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Base.DataHubPackage.Editor.Bookmarks
{
    /// <summary>
    /// The data assets changed last in this editor session, newest first, wherever the change was made: in the
    /// table, the side panel or Unity's Inspector. One click in the sidebar goes back to the card that was being
    /// tuned before a detour.
    /// </summary>
    internal sealed class DataRecentEdits
    {
        private const int MaxCount = 8;
        private const string RecentKey = "Base.DataHubPackage.Recent";
        private const char Separator = ';';

        private readonly List<Object> _assets = new();
        private readonly List<string> _guids = new();
        private readonly Action _changed;
        private readonly Func<Type, bool> _isListed;

        private bool _isLoaded;

        /// <summary>Creates the list.</summary>
        /// <param name="isListed">True for an asset type the hub has a table for; other edits are ignored.</param>
        /// <param name="changed">Called when an edit moved an asset to the top, so the sidebar redraws.</param>
        internal DataRecentEdits(Func<Type, bool> isListed, Action changed)
        {
            _isListed = isListed;
            _changed = changed;
        }

        /// <summary>The changed assets that still exist, newest first.</summary>
        internal IReadOnlyList<Object> Assets
        {
            get
            {
                Load();

                return _assets;
            }
        }

        /// <summary>Starts listening for changes. Pair with <see cref="Stop"/>.</summary>
        internal void Start() => ObjectChangeEvents.changesPublished += OnChangesPublished;

        /// <summary>Stops listening for changes.</summary>
        internal void Stop() => ObjectChangeEvents.changesPublished -= OnChangesPublished;

        /// <summary>Reads the list again on next use, after assets were deleted or moved.</summary>
        internal void Invalidate() => _isLoaded = false;

        private void OnChangesPublished(ref ObjectChangeEventStream stream)
        {
            bool isChanged = false;

            for (int i = 0; i < stream.length; i++)
            {
                if (stream.GetEventType(i) != ObjectChangeKind.ChangeAssetObjectProperties)
                    continue;

                stream.GetChangeAssetObjectPropertiesEvent(i, out ChangeAssetObjectPropertiesEventArgs change);

                if (Add(change.guid.ToString()))
                    isChanged = true;
            }

            if (!isChanged)
                return;

            SessionState.SetString(RecentKey, string.Join(Separator.ToString(), _guids));
            Invalidate();
            _changed();
        }

        private bool Add(string guid)
        {
            Type type = AssetDatabase.GetMainAssetTypeFromGUID(new GUID(guid));

            if (type == null
                || !_isListed(type))
                return false;

            Load();

            if (_guids.Count > 0
                && _guids[0] == guid)
                return false;

            _guids.Remove(guid);
            _guids.Insert(0, guid);

            if (_guids.Count > MaxCount)
                _guids.RemoveAt(_guids.Count - 1);

            return true;
        }

        // In the session state, so the list survives a script reload but starts empty with the next editor start.
        private void Load()
        {
            if (_isLoaded)
                return;

            _isLoaded = true;
            _guids.Clear();
            _assets.Clear();

            string saved = SessionState.GetString(RecentKey, string.Empty);

            if (saved.Length == 0)
                return;

            foreach (string guid in saved.Split(Separator))
            {
                Object asset = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(guid));

                if (asset == null)
                    continue;

                _guids.Add(guid);
                _assets.Add(asset);
            }
        }
    }
}