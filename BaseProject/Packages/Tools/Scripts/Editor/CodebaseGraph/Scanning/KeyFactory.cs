using System;
using System.Reflection;
using Base.ToolPackage.Editor.CodebaseGraph.Model;

namespace Base.ToolPackage.Editor.CodebaseGraph.Scanning
{
    /// <summary>
    /// Turns reflection objects into graph keys. Constructed generics, arrays and by-ref wrappers are
    /// normalized back to their definition, so a usage of List&lt;int&gt; and List&lt;string&gt; lands on
    /// the same node.
    /// </summary>
    public static class KeyFactory
    {
        /// <summary>Builds the key of a type, normalizing wrappers and generic instantiations.</summary>
        /// <param name="type">Type to build a key for.</param>
        /// <param name="key">The resulting key when the type can be keyed.</param>
        /// <returns>True when a key could be built.</returns>
        public static bool TryForType(Type type, out TypeKey key)
        {
            key = default;

            Type normalized = Normalize(type);
            if (normalized == null)
                return false;

            try
            {
                key = new TypeKey(normalized.Module.Name, normalized.MetadataToken);
                return true;
            }
            catch (Exception)
            {
                // Dynamic and synthesized types have no metadata token. They are simply not part of the graph.
                return false;
            }
        }

        /// <summary>Builds the key of a member, normalizing generic method instantiations.</summary>
        /// <param name="member">Member to build a key for.</param>
        /// <param name="key">The resulting key when the member can be keyed.</param>
        /// <returns>True when a key could be built.</returns>
        public static bool TryForMember(MemberInfo member, out MemberKey key)
        {
            key = default;
            if (member == null)
                return false;

            MemberInfo normalized = member;
            if (normalized is MethodInfo method
                && method.IsGenericMethod
                && !method.IsGenericMethodDefinition)
                normalized = method.GetGenericMethodDefinition();

            try
            {
                key = new MemberKey(normalized.Module.Name, normalized.MetadataToken);
                return true;
            }
            catch (Exception)
            {
                // Same as above: no metadata token means the member cannot take part in the graph.
                return false;
            }
        }

        private static Type Normalize(Type type)
        {
            Type current = type;

            while (current != null
                && (current.IsArray || current.IsByRef || current.IsPointer))
                current = current.GetElementType();

            if (current == null || current.IsGenericParameter)
                return null;

            return current.IsConstructedGenericType
                ? current.GetGenericTypeDefinition()
                : current;
        }
    }
}
