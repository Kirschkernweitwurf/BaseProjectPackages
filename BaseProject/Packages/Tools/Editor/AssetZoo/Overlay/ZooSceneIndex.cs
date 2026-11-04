using System;
using System.Collections.Generic;
using Base.ToolsPackage.AssetZoo;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Base.ToolsPackage.Editor.AssetZoo.Overlay
{
    /// <summary>
    /// Knows what zoo stands in the open scenes, so the window and the Scene view do not have to search for it
    /// on every repaint. A search only runs after the hierarchy changed.
    /// </summary>
    [InitializeOnLoad]
    internal static class ZooSceneIndex
    {
        private static ZooGroup[] _groups = Array.Empty<ZooGroup>();
        private static bool _isDirty = true;
        private static ZooItem[] _items = Array.Empty<ZooItem>();
        private static ZooRootMarker _root;

        static ZooSceneIndex() => EditorApplication.hierarchyChanged += Invalidate;

        /// <summary>Raised when what stands in the scene may have changed.</summary>
        public static event Action Invalidated;

        /// <summary>The root of the zoo in the open scenes, or null when there is none.</summary>
        public static ZooRootMarker Root
        {
            get
            {
                RefreshIfNeeded();
                return _root;
            }
        }

        /// <summary>The groups of the zoo, showing or not.</summary>
        public static IReadOnlyList<ZooGroup> Groups
        {
            get
            {
                RefreshIfNeeded();
                return _groups;
            }
        }

        /// <summary>The items of the zoo, showing or not.</summary>
        public static IReadOnlyList<ZooItem> Items
        {
            get
            {
                RefreshIfNeeded();
                return _items;
            }
        }

        /// <summary>Asks for a new search the next time anything is read.</summary>
        public static void Invalidate()
        {
            _isDirty = true;
            Invalidated?.Invoke();
        }

        private static void RefreshIfNeeded()
        {
            if (!_isDirty)
                return;

            _isDirty = false;
            _root = Object.FindAnyObjectByType<ZooRootMarker>(UnityEngine.FindObjectsInactive.Include);

            if (_root == null)
            {
                _groups = Array.Empty<ZooGroup>();
                _items = Array.Empty<ZooItem>();

                return;
            }

            _groups = _root.GetComponentsInChildren<ZooGroup>(true);
            _items = _root.GetComponentsInChildren<ZooItem>(true);
        }
    }
}