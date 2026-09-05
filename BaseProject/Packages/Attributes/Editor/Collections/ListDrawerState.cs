using System.Collections.Generic;
using Base.AttributesPackage.Editor.Core;
using UnityEditor;

namespace Base.AttributesPackage.Editor.Collections
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
    }
}