using System.Collections.Generic;
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
        private const char KeySeparator = ':';

        private static readonly Dictionary<string, ListDrawerState> States = new();

        private static readonly HashSet<string> Seen = new();

        /// <summary>Current filter text, empty when nothing is filtered.</summary>
        public string Search { get; set; } = string.Empty;

        /// <summary>Zero-based index of the page being shown.</summary>
        public int Page { get; set; }

        /// <summary>
        /// Returns whether this is the first time the property is drawn this session. Used to apply a
        /// default expanded state once, since a serialized property has no way to say it was never set.
        /// </summary>
        /// <param name="property">The list property being drawn.</param>
        /// <returns>True on the first draw only.</returns>
        public static bool IsFirstDraw(SerializedProperty property) => Seen.Add(KeyFor(property));

        /// <summary>Returns the state belonging to the given property, creating it on first use.</summary>
        /// <param name="property">The list property being drawn.</param>
        /// <returns>The state for that property.</returns>
        public static ListDrawerState For(SerializedProperty property)
        {
            string key = KeyFor(property);

            if (States.TryGetValue(key, out ListDrawerState state))
                return state;

            state = new ListDrawerState();
            States[key] = state;
            return state;
        }

        private static string KeyFor(SerializedProperty property)
            => property.serializedObject.targetObject.GetInstanceID() + KeySeparator + property.propertyPath;
    }
}