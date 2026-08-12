using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Base.UtilityPackage.Serialization;
using UnityEditor;

namespace Base.UtilityPackage.Editor.Serialization
{
    /// <summary>
    /// Collects the types a <see cref="TypeReference"/> picker offers.
    /// </summary>
    /// <remarks>
    /// An unfiltered list is unusable. Every loaded assembly together holds tens of thousands of types,
    /// most of them things no field should ever point at: compiler-generated closures and iterator state
    /// machines, internal Unity plumbing, private nested helpers. Three filters cut that down.
    /// <para>
    /// Visibility comes first. A type that is not public all the way out cannot be named from another
    /// assembly anyway, so offering it would produce a reference the owning code could not use.
    /// </para>
    /// <para>
    /// Compiler-generated types go next. They are legal types with unpronounceable names and no meaning
    /// outside the method that produced them.
    /// </para>
    /// <para>
    /// Scope comes last and only for an unconstrained field, which is where the flood actually comes
    /// from. A constrained field is already narrowed by its base type, so everything assignable to that
    /// base is worth offering wherever it lives.
    /// </para>
    /// </remarks>
    public static class TypeCandidates
    {
        private const string CoreLibrary = "mscorlib";
        private const char GeneratedMarker = '<';
        private const string MonoPrefix = "Mono.";
        private const string NetStandardPrefix = "netstandard";
        private const string SystemPrefix = "System";
        private const string UnityPrefix = "Unity";

        private static readonly Dictionary<Type, Type[]> Constrained = new();

        private static readonly Dictionary<ETypeScope, Type[]> Unconstrained = new();

        /// <summary>Returns the types offered for the given base, sorted and cached.</summary>
        /// <param name="baseType">The base type every candidate has to satisfy.</param>
        /// <param name="scope">Which assemblies an unconstrained picker draws from.</param>
        /// <returns>The candidate types.</returns>
        public static Type[] For(Type baseType, ETypeScope scope)
        {
            return baseType == typeof(object)
                ? Everything(scope)
                : DerivedFrom(baseType);
        }

        /// <summary>Drops every cached list, so the next lookup collects again.</summary>
        public static void Clear()
        {
            Constrained.Clear();
            Unconstrained.Clear();
        }

        private static Type[] DerivedFrom(Type baseType)
        {
            if (Constrained.TryGetValue(baseType, out Type[] cached))
                return cached;

            List<Type> types = new();

            // The base itself is offered too, as long as it is nameable. A field may legitimately point
            // at the interface or the abstract class rather than at one of its implementations.
            if (IsOffered(baseType))
                types.Add(baseType);

            foreach (Type candidate in TypeCache.GetTypesDerivedFrom(baseType))
            {
                if (IsOffered(candidate))
                    types.Add(candidate);
            }

            Type[] result = Sorted(types);
            Constrained[baseType] = result;
            return result;
        }

        private static Type[] Everything(ETypeScope scope)
        {
            if (Unconstrained.TryGetValue(scope, out Type[] cached))
                return cached;

            List<Type> types = new();

            foreach (Type candidate in TypeCache.GetTypesDerivedFrom<object>())
            {
                if (!IsOffered(candidate))
                    continue;

                if (scope == ETypeScope.Project && IsFramework(candidate))
                    continue;

                types.Add(candidate);
            }

            Type[] result = Sorted(types);
            Unconstrained[scope] = result;
            return result;
        }

        // IsVisible is public all the way out, which is exactly the question: a type that cannot be named
        // from another assembly cannot be the answer to a serialized type reference either.
        private static bool IsOffered(Type type)
        {
            if (type == null || !type.IsVisible || type.IsGenericTypeDefinition)
                return false;

            if (string.IsNullOrEmpty(type.FullName) || type.Name.IndexOf(GeneratedMarker) >= 0)
                return false;

            return !Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false);
        }

        private static bool IsFramework(Type type)
        {
            string assembly = type.Assembly.GetName().Name;

            return assembly.StartsWith(UnityPrefix)
                || assembly.StartsWith(SystemPrefix)
                || assembly.StartsWith(MonoPrefix)
                || assembly.StartsWith(NetStandardPrefix)
                || assembly == CoreLibrary;
        }

        private static Type[] Sorted(List<Type> types)
        {
            types.Sort(comparison: (a, b) => string.CompareOrdinal(a.FullName, b.FullName));
            return types.ToArray();
        }
    }
}