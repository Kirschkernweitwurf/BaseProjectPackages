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

        /// <summary>How far a striped row is tinted from the row beneath it.</summary>
        private const float StripeStrength = 0.05f;

        private static readonly Dictionary<string, ReorderableList> Lists = new();

        // The SerializedObject a cached list was built against, kept so staleness can be judged without
        // touching the cached property. A SerializedObject is disposed when the inspector rebuilds, and
        // every member of a property belonging to it throws from that moment on, including the equality
        // check that would otherwise be the obvious way to ask whether the cache still applies.
        private static readonly Dictionary<ReorderableList, SerializedObject> Owners = new();

        // The callbacks are built once and close over the list rather than over the settings, so both
        // are looked up per list instead of captured at construction.
        private static readonly Dictionary<ReorderableList, string> LabelMembers = new();

        private static readonly Dictionary<ReorderableList, bool> Stripes = new();

        private static readonly Dictionary<ReorderableList, bool> Confirmations = new();

        static ReorderableListCache() => AssemblyReloadEvents.beforeAssemblyReload += Drop;

        private static void Drop()
        {
            Lists.Clear();
            LabelMembers.Clear();
            Owners.Clear();
            Confirmations.Clear();
            Stripes.Clear();
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
                && Owners.TryGetValue(cached, out SerializedObject owner)
                && ReferenceEquals(owner, property.serializedObject))
            {
                Configure(cached, settings, canResize);
                return cached;
            }

            ReorderableList created = Build(property, settings, canResize);
            Owners[created] = property.serializedObject;
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

            list.drawElementBackgroundCallback = (rect, index, active, focused) =>
            {
                if (index < 0 || Event.current.type != EventType.Repaint)
                    return;

                // Setting this callback replaces Unity's background entirely, selection included, so the
                // selected row is drawn by Unity's own behaviour rather than approximated here. Doing it
                // the other way round is what made a selected light stripe indistinguishable from an
                // unselected one: two tints of similar strength, one on top of the other.
                // Focused is forced. An unfocused selection is drawn in a light grey that sits within a
                // few percent of the stripe, so clicking elsewhere in the inspector made the selected
                // row look like an ordinary striped one. Keeping it blue costs a small lie about focus
                // and buys a selection you can still see.
                if (active)
                {
                    ReorderableList.defaultBehaviours.DrawElementBackground(rect, index, true, true, true);
                    return;
                }

                if (!Striped(list) || index % 2 != 0)
                    return;

                EditorGUI.DrawRect(rect, EditorGUIUtility.isProSkin
                    ? new Color(1f, 1f, 1f, StripeStrength)
                    : new Color(0f, 0f, 0f, StripeStrength));
            };

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
            list.showDefaultBackground = true;

            LabelMembers[list] = settings.LabelMember;
            Stripes[list] = settings.ShowAlternatingBackground;
            Confirmations[list] = settings.ConfirmDelete;
        }

        private static string ElementLabelMember(ReorderableList list)
            => LabelMembers.TryGetValue(list, out string member)
                ? member
                : null;

        private static bool Striped(ReorderableList list)
            => Stripes.TryGetValue(list, out bool striped) && striped;

        private static bool ConfirmsRemoval(ReorderableList list)
            => Confirmations.TryGetValue(list, out bool confirms) && confirms;
    }
}