using System;
using Base.EditorUIPackage.Editor;
using Base.ToolsPackage.Editor.AssetZoo.Config;
using Base.UtilityPackage.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.ToolsPackage.Editor.AssetZoo.UI
{
    /// <summary>
    /// The categories of a config, drawn as an <see cref="EditorList"/>. Each row is the category
    /// itself, drawn by Unity, so folding one open gives the name, the label color and the prefabs as
    /// a nested reorderable list with its own drag handles and its own add and remove tab.
    /// </summary>
    /// <remarks>
    /// The rows used to be laid out by hand, with a swatch, a badge, a ping button and an empty
    /// object field to add through. That looked like a list without behaving like one: the prefabs
    /// could not be dragged, there was no add or remove on them, and dropping into an empty slot is
    /// not how anything else in the editor works. Handing the row back to Unity restores all of it at
    /// once, and the tint around the list carries the theme into the nested list too.
    /// </remarks>
    internal sealed class ZooCategoryListView
    {
        private const string ElementLabelFormat = "Element {0}";
        private const string EmptyLabel = "No categories yet";
        private const string ListTitle = "Categories";
        private const string NewCategoryName = "Category";

        /// <summary>How many categories the config holds.</summary>
        public int Count => _categories.arraySize;

        private readonly EditorList _list;
        private readonly SerializedProperty _categories;

        private string _filter = string.Empty;

        /// <summary>Creates the view over the categories array of one config.</summary>
        /// <param name="categories">The categories array property.</param>
        public ZooCategoryListView(SerializedProperty categories)
        {
            _categories = categories;

            _list = new EditorList(categories)
            {
                EmptyLabel = EmptyLabel,
                Title = ListTitle
            };

            _list.DrawElement = (rect, index, isActive) => DrawRow(rect, index);
            _list.ElementHeight = HeightOf;
            _list.OnAdd = AddCategory;
        }

        /// <summary>Draws the list.</summary>
        /// <param name="styles">The built chrome styles.</param>
        /// <param name="filter">Text a category or one of its prefabs has to contain, or empty.</param>
        public void Draw(EditorWindowStyles styles, string filter)
        {
            _filter = filter;

            // A filtered row is collapsed to nothing rather than dropped, so dragging one while the
            // filter is on would move it somewhere nobody can see, and Unity paints the drag handle
            // before it knows how tall a row is, which would leave grips over the hidden ones.
            _list.Draggable = string.IsNullOrEmpty(filter);

            _list.DrawLayout(styles);
        }

        /// <summary>Folds every category open or closed at once.</summary>
        /// <param name="isExpanded">True to open all, false to close all.</param>
        public void SetAllExpanded(bool isExpanded)
        {
            for (int index = 0; index < _categories.arraySize; index++)
                _categories.GetArrayElementAtIndex(index).isExpanded = isExpanded;
        }

        /// <summary>True while at least one category is folded open.</summary>
        public bool HasExpanded()
        {
            for (int index = 0; index < _categories.arraySize; index++)
            {
                if (_categories.GetArrayElementAtIndex(index).isExpanded)
                    return true;
            }

            return false;
        }

        /// <summary>The total number of prefabs across every category.</summary>
        public int EntryCount()
        {
            int total = 0;

            for (int index = 0; index < _categories.arraySize; index++)
                total += EntriesOf(index).arraySize;

            return total;
        }

        private static SerializedProperty ChildOf(SerializedProperty element, string name)
            => CustomEditorUtility.FindProp(element, name);

        private SerializedProperty NameOf(int index)
            => ChildOf(_categories.GetArrayElementAtIndex(index), nameof(ZooCategory.Name));

        private SerializedProperty ColorOf(int index)
            => ChildOf(_categories.GetArrayElementAtIndex(index), nameof(ZooCategory.LabelColor));

        private SerializedProperty EntriesOf(int index)
            => ChildOf(_categories.GetArrayElementAtIndex(index), nameof(ZooCategory.Entries));

        // The category name rather than "Element 3", so a collapsed list can be read. Falls back to
        // Unity's own wording while a category has no name yet.
        private GUIContent LabelOf(int index)
        {
            string name = NameOf(index).stringValue;

            if (string.IsNullOrEmpty(name))
                return EditorGUIUtility.TrTempContent(string.Format(ElementLabelFormat, index));

            return EditorGUIUtility.TrTempContent(name);
        }

        private bool Matches(int index)
        {
            if (string.IsNullOrEmpty(_filter))
                return true;

            if (NameOf(index).stringValue.IndexOf(_filter, StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            SerializedProperty entries = EntriesOf(index);

            // A prefab name matching is enough, so searching for one asset finds the group it sits in.
            for (int entry = 0; entry < entries.arraySize; entry++)
            {
                Object prefab = entries.GetArrayElementAtIndex(entry).objectReferenceValue;

                if (prefab != null
                    && prefab.name.IndexOf(_filter, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }

            return false;
        }

        private float HeightOf(int index)
        {
            // A filtered out row is collapsed to nothing rather than skipped, because the list numbers
            // its rows itself and dropping one would shift every index behind it.
            if (!Matches(index))
                return 0f;

            return EditorGUI.GetPropertyHeight(_categories.GetArrayElementAtIndex(index), LabelOf(index),
                true);
        }

        private void DrawRow(Rect rect, int index)
        {
            if (!Matches(index))
                return;

            EditorGUI.PropertyField(rect, _categories.GetArrayElementAtIndex(index), LabelOf(index), true);
        }

        private void AddCategory()
        {
            _categories.InsertArrayElementAtIndex(_categories.arraySize);

            // Unity copies the element before it, so the new one is reset rather than arriving as a
            // second copy of whatever sat at the end of the list.
            int index = _categories.arraySize - 1;

            NameOf(index).stringValue = NewCategoryName;
            ColorOf(index).colorValue = Color.cyan;
            EntriesOf(index).ClearArray();

            _categories.GetArrayElementAtIndex(index).isExpanded = true;

            _list.Select(index);
        }
    }
}