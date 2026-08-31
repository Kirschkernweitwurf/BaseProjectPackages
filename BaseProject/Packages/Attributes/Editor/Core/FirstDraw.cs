using System;
using System.Collections.Generic;
using UnityEditor;

namespace Base.AttributePackage.Editor.Core
{
    /// <summary>
    /// Tracks which properties have been drawn at least once this editor session, so a default expanded
    /// state can be applied exactly once. A SerializedProperty has no way to say whether its expanded
    /// flag was ever set, and forcing it every repaint would make the field impossible to fold away.
    /// </summary>
    /// <remarks>
    /// Keyed by target type and property path, which is how Unity stores the expanded flag itself:
    /// folding a field on one object folds it on every object of the same type. Keying per instance
    /// instead made the next object of that type count as a first draw, which forced the shared flag
    /// open again and reopened the field on the object it had just been folded on.
    /// </remarks>
    internal static class FirstDraw
    {
        private static readonly HashSet<string> Seen = new();

        // Selecting a different object does not forget the ones before it, so the set grows for as long
        // as the session lasts. Entering play mode is the natural point to forget: an object drawn again
        // afterwards is being looked at fresh, which is exactly when a default expanded state should
        // apply again.
        static FirstDraw() => EditorApplication.playModeStateChanged += _ => Seen.Clear();

        /// <summary>Returns true the first time it is called for a given property, false afterwards.</summary>
        /// <param name="property">The property being drawn.</param>
        /// <returns>True on the first draw only.</returns>
        public static bool IsFirst(SerializedProperty property) => Seen.Add(KeyFor(property));

        /// <summary>
        /// Treats every property of the given type as unseen again, so the defaults its attributes
        /// declare are applied once more on the next draw.
        /// </summary>
        /// <param name="owner">The type to forget.</param>
        public static void Forget(Type owner)
        {
            if (owner == null)
                return;

            string prefix = StateKey.For(owner, string.Empty);

            Seen.RemoveWhere(key => key.StartsWith(prefix, StringComparison.Ordinal));
        }

        private static string KeyFor(SerializedProperty property)
            => StateKey.For(property.serializedObject.targetObject.GetType(), property.propertyPath);
    }
}