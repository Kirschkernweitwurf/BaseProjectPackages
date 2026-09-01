using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Base.AttributePackage.Editor.Collections
{
    /// <summary>
    /// Keeps one Unity reorderable list alive per drawn array, configured from the field's
    /// <see cref="ListDrawerSettingsAttribute"/>.
    /// </summary>
    /// <remarks>
    /// Instances are cached because a reorderable list holds the drag in progress; building a new one
    /// each repaint would drop the drag on the frame after it started. Staleness is judged by comparing
    /// the SerializedObject the list was built against, because a disposed one throws from every member
    /// including the equality check that would otherwise be the obvious test.
    /// <para>
    /// A filtered row is given a height of zero rather than removed, so the list stays the same control
    /// with the same indices and dragging is simply switched off while a filter is on.
    /// </para>
    /// </remarks>
    internal static class ReorderableListCache
    {
        private const float FoldoutInset = 12f;
        private const float RowPadding = 2f;

        /// <summary>How far a striped row is tinted from the row beneath it.</summary>
        private const float StripeStrength = 0.05f;

        private static readonly Dictionary<string, ReorderableList> Lists = new();

        private static readonly Dictionary<ReorderableList, ListEntryState> States = new();

        private static readonly Dictionary<ReorderableList, SerializedObject> Owners = new();

        static ReorderableListCache() => AssemblyReloadEvents.beforeAssemblyReload += Drop;

        /// <summary>Returns the list for the given array, building it on first use.</summary>
        /// <param name="property">The array being drawn.</param>
        /// <param name="settings">The settings that shape the list.</param>
        /// <param name="canResize">False when [ArraySize] fixes the element count.</param>
        /// <param name="hidden">Indices the filter is hiding this draw.</param>
        /// <returns>The cached list, configured for this draw.</returns>
        internal static ReorderableList Get(SerializedProperty property,
            ListDrawerSettingsAttribute settings, bool canResize, HashSet<int> hidden)
        {
            string key = property.serializedObject.targetObject.GetInstanceID() + property.propertyPath;

            if (Lists.TryGetValue(key, out ReorderableList cached)
                && Owners.TryGetValue(cached, out SerializedObject owner)
                && ReferenceEquals(owner, property.serializedObject))
            {
                Configure(cached, settings, canResize, hidden);
                return cached;
            }

            ReorderableList created = Build(property);

            Owners[created] = property.serializedObject;
            Lists[key] = created;

            Configure(created, settings, canResize, hidden);
            return created;
        }

        private static void Drop()
        {
            Lists.Clear();
            Owners.Clear();
            States.Clear();
        }

        private static ReorderableList Build(SerializedProperty property)
        {
            ReorderableList list = new(property.serializedObject, property.Copy(), true, false, true, true)
            {
                headerHeight = 0f
            };

            list.elementHeightCallback = index => HeightOf(list, index);
            list.drawElementCallback = (rect, index, active, focused) => DrawElement(list, rect, index);

            list.drawElementBackgroundCallback = (rect, index, active, focused)
                => DrawBackground(list, rect, index, active);

            // Unity removes without asking. The confirmation is the whole reason the setting exists, so
            // the default callback is replaced rather than wrapped.
            list.onRemoveCallback = target => Remove(list, target);

            return list;
        }

        private static void Configure(ReorderableList list, ListDrawerSettingsAttribute settings,
            bool canResize, HashSet<int> hidden)
        {
            States[list] = new ListEntryState(settings, hidden);

            list.displayAdd = canResize;
            list.displayRemove = canResize;

            // A filtered list is not contiguous, so the row above is not the element above and a dragged
            // row would land somewhere the pointer never went.
            list.draggable = hidden.Count == 0;
        }

        private static ListEntryState StateOf(ReorderableList list)
            => States.TryGetValue(list, out ListEntryState state)
                ? state
                : default(ListEntryState);

        private static float HeightOf(ReorderableList list, int index)
        {
            ListEntryState state = StateOf(list);

            if (state.IsHidden(index))
                return 0f;

            return EditorGUI.GetPropertyHeight(list.serializedProperty.GetArrayElementAtIndex(index), true)
                + RowPadding;
        }

        private static void DrawElement(ReorderableList list, Rect rect, int index)
        {
            ListEntryState state = StateOf(list);

            if (state.IsHidden(index))
                return;

            SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index);
            string label = ElementLabel.For(element, index);

            // An element that can expand draws its own foldout arrow at the left edge of the rect, which
            // is where the drag handle already is. The inset gives the arrow its own room.
            float inset = element.hasVisibleChildren
                ? FoldoutInset
                : 0f;

            Rect row = new(rect.x + inset, rect.y + RowPadding * 0.5f, rect.width - inset,
                rect.height - RowPadding);

            EditorGUI.PropertyField(row, element, ScratchContent.For(label), true);
        }

        private static void DrawBackground(ReorderableList list, Rect rect, int index, bool active)
        {
            if (index < 0 || Event.current.type != EventType.Repaint || StateOf(list).IsHidden(index))
                return;

            // Focused is forced. An unfocused selection is drawn in a light grey within a few percent of
            // the stripe, so clicking elsewhere in the inspector made a selected row look like an
            // ordinary striped one.
            if (active)
            {
                ReorderableList.defaultBehaviours.DrawElementBackground(rect, index, true, true, true);
                return;
            }

            if (!StateOf(list).Striped || index % 2 != 0)
                return;

            EditorGUI.DrawRect(rect, EditorGUIUtility.isProSkin
                ? new Color(1f, 1f, 1f, StripeStrength)
                : new Color(0f, 0f, 0f, StripeStrength));
        }

        private static void Remove(ReorderableList list, ReorderableList target)
        {
            ListEntryState state = StateOf(list);

            int index = target.index >= 0 && target.index < target.serializedProperty.arraySize
                ? target.index
                : target.serializedProperty.arraySize - 1;

            if (index < 0)
                return;

            SerializedProperty element = target.serializedProperty.GetArrayElementAtIndex(index);
            string label = ElementLabel.For(element, index);

            if (CollectionGui.ConfirmRemoval(label, state.ConfirmDelete))
                CollectionGui.DeleteElement(target.serializedProperty, index);
        }
    }
}