using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Base.AttributePackage.Editor.Collections
{
    /// <summary>
    /// Keeps one Unity reorderable list alive per drawn array, so a list can be dragged exactly the way
    /// every other list in the editor is.
    /// </summary>
    /// <remarks>
    /// Unity's own list is used rather than a hand-rolled drag, because reordering is a thing people
    /// already know how to do and a bespoke version of it is only ever going to be a worse copy. The
    /// cost is that the list owns its own layout: it draws every element and cannot be told to skip
    /// some, which is why the search box and the pager fall back to the plain rows.
    /// <para>
    /// Instances are cached because a reorderable list holds the drag in progress. Building a new one
    /// each repaint would drop that drag on the frame after it started.
    /// </para>
    /// </remarks>
    internal static class ReorderableListCache
    {
        private const float FoldoutInset = 12f;
        private const float RowPadding = 2f;

        private static readonly Dictionary<string, ReorderableList> Lists = new();

        // The callbacks are built once and close over the list rather than over the settings, so both
        // are looked up per list instead of captured at construction.
        private static readonly Dictionary<ReorderableList, string> LabelMembers = new();

        private static readonly Dictionary<ReorderableList, bool> Confirmations = new();

        static ReorderableListCache() => AssemblyReloadEvents.beforeAssemblyReload += Drop;

        private static void Drop()
        {
            Lists.Clear();
            LabelMembers.Clear();
            Confirmations.Clear();
        }

        /// <summary>Returns the list for the given array, building it on first use.</summary>
        /// <param name="property">The array being drawn.</param>
        /// <param name="settings">The settings that shape the list.</param>
        /// <returns>The cached list, configured for this draw.</returns>
        /// <param name="canResize">False when [ArraySize] fixes the element count.</param>
        internal static ReorderableList Get(SerializedProperty property, ListDrawerSettingsAttribute settings,
            bool canResize = true)
        {
            string key = KeyFor(property);

            if (Lists.TryGetValue(key, out ReorderableList cached)
                && cached.serializedProperty != null
                && SerializedProperty.EqualContents(cached.serializedProperty, property))
            {
                Configure(cached, settings, canResize);
                return cached;
            }

            ReorderableList created = Build(property, settings, canResize);
            Lists[key] = created;
            return created;
        }

        private static string KeyFor(SerializedProperty property)
            => property.serializedObject.targetObject.GetInstanceID() + property.propertyPath;

        private static ReorderableList Build(SerializedProperty property, ListDrawerSettingsAttribute settings,
            bool canResize)
        {
            ReorderableList list = new(property.serializedObject, property.Copy(), settings.Draggable,
                false, !settings.HideAddButton, !settings.HideRemoveButton);

            // The header row is switched off, because the drawer already draws its own foldout with the
            // element count and the search box in it. The footer is left alone: its add and remove
            // buttons are Unity's, they sit where everyone expects them, and they keep working on an
            // empty list, which a button in the header does not.
            list.headerHeight = 0f;
            list.showDefaultBackground = true;

            list.elementHeightCallback = index =>
                EditorGUI.GetPropertyHeight(list.serializedProperty.GetArrayElementAtIndex(index), true)
                + RowPadding;

            list.drawElementCallback = (rect, index, active, focused) =>
            {
                SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index);
                string label = ElementLabel.For(element, index, ElementLabelMember(list));

                // An element that can expand draws its own foldout arrow at the left edge of the rect,
                // which is where the drag handle already is. The inset gives the arrow its own room.
                float inset = element.hasVisibleChildren
                    ? FoldoutInset
                    : 0f;

                Rect row = new(rect.x + inset, rect.y + RowPadding * 0.5f, rect.width - inset,
                    rect.height - RowPadding);

                EditorGUI.PropertyField(row, element, ScratchContent.For(label), true);
            };

            // Unity removes without asking. The confirmation is the whole reason the setting exists, so
            // the default callback is replaced rather than wrapped.
            list.onRemoveCallback = target =>
            {
                int index = target.index >= 0 && target.index < target.serializedProperty.arraySize
                    ? target.index
                    : target.serializedProperty.arraySize - 1;

                if (index < 0)
                    return;

                SerializedProperty element = target.serializedProperty.GetArrayElementAtIndex(index);
                string label = ElementLabel.For(element, index, ElementLabelMember(list));

                if (CollectionGui.ConfirmRemoval(label, ConfirmsRemoval(list)))
                    CollectionGui.DeleteElement(target.serializedProperty, index);
            };

            Configure(list, settings, canResize);
            return list;
        }

        // The settings can change between repaints, since they come from an attribute that a domain
        // reload may have replaced, so they are reapplied rather than baked in at construction.
        private static void Configure(ReorderableList list, ListDrawerSettingsAttribute settings,
            bool canResize)
        {
            list.draggable = settings.Draggable;
            list.displayAdd = canResize && !settings.HideAddButton;
            list.displayRemove = canResize && !settings.HideRemoveButton;
            list.showDefaultBackground = settings.ShowAlternatingBackground;

            LabelMembers[list] = settings.LabelMember;
            Confirmations[list] = settings.ConfirmDelete;
        }

        private static string ElementLabelMember(ReorderableList list)
            => LabelMembers.TryGetValue(list, out string member)
                ? member
                : null;

        private static bool ConfirmsRemoval(ReorderableList list)
            => Confirmations.TryGetValue(list, out bool confirms) && confirms;
    }
}