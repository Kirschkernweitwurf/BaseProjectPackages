using System;
using System.Reflection;

namespace Base.AttributePackage.Editor.Windows.AttributeTroubleshoot.Checks
{
    /// <summary>
    /// Enumerates the members a check inspects. Only members declared directly on the type are
    /// returned, so a field on a base class is reported once, on the base, instead of once per subclass.
    /// </summary>
    internal static class ScannedMembers
    {
        // Static members are excluded on purpose. The renderers and the reflection cache only look at
        // instance members, so an attribute on a static member is invisible either way.
        private const BindingFlags DeclaredFlags = BindingFlags.Instance
            | BindingFlags.Public
            | BindingFlags.NonPublic
            | BindingFlags.DeclaredOnly;

        /// <summary>Returns the fields declared directly on the given type.</summary>
        /// <param name="type">The type to enumerate.</param>
        /// <returns>The declared fields.</returns>
        public static FieldInfo[] DeclaredFields(Type type) => type.GetFields(DeclaredFlags);

        /// <summary>Returns the methods declared directly on the given type.</summary>
        /// <param name="type">The type to enumerate.</param>
        /// <returns>The declared methods.</returns>
        public static MethodInfo[] DeclaredMethods(Type type) => type.GetMethods(DeclaredFlags);
    }
}