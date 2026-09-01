using System;
using System.Collections.Generic;
using Base.AttributePackage.Editor.Core;
using UnityEditor;

namespace Base.AttributePackage.Editor.Collections
{
    /// <summary>
    /// Search text and page position of one drawn list. Kept outside the renderer because handlers and
    /// renderers are shared across every inspector, and this state belongs to one field on one object.
    /// It lives only for the editor session, which is the right lifetime for a filter.
    /// </summary>
    internal sealed class ListDrawerState
    {
        /// <summary>Current filter text, empty when nothing is filtered.</summary>
        internal string Search { get; set; } = string.Empty;

        private static readonly Dictionary<string, ListDrawerState> States = new();

        private static readonly HashSet<string> Seen = new();

        /// <summary>
        /// Returns whether this is the first time the property is drawn this session. Used to apply a
        /// default expanded state once, since a serialized property has no way to say it was never set.
        /// </summary>
        /// <param name="property">The list property being drawn.</param>
        /// <returns>True on the first draw only.</returns>
        /// <remarks>
        /// Keyed by type rather than by instance, unlike the filter state below. The expanded flag this
        /// guards is shared by Unity across every object of a type, so a per-instance key would force it
        /// open again on the next object and undo the fold on the one before it.
        /// </remarks>
        internal static bool IsFirstDraw(SerializedProperty property) => Seen.Add(TypeKeyFor(property));

        /// <summary>
        /// Treats every property of the given type as never drawn, so a default expanded state is
        /// applied once more on the next draw.
        /// </summary>
        /// <param name="owner">The type to forget.</param>
        internal static void Forget(Type owner)
        {
            if (owner == null)
                return;

            string prefix = StateKey.For(owner, string.Empty);

            Seen.RemoveWhere(key => key.StartsWith(prefix, StringComparison.Ordinal));
        }

        /// <summary>Returns the state belonging to the given property, creating it on first use.</summary>
        /// <param name="property">The list property being drawn.</param>
        /// <returns>The state for that property.</returns>
        internal static ListDrawerState For(SerializedProperty property)
        {
            string key = InstanceKeyFor(property);

            if (States.TryGetValue(key, out ListDrawerState state))
                return state;

            state = new ListDrawerState();
            States[key] = state;
            return state;
        }

        // A filter is something one person typed into one field on one object, so it stays per instance.
        private static string InstanceKeyFor(SerializedProperty property)
            => StateKey.For(property.serializedObject.targetObject.GetInstanceID(), property.propertyPath);

        private static string TypeKeyFor(SerializedProperty property)
            => StateKey.For(property.serializedObject.targetObject.GetType(), property.propertyPath);
    }
}