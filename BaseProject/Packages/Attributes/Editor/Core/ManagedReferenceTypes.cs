using System;
using System.Collections.Generic;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Base.AttributePackage.Editor.Core
{
    /// <summary>
    /// Resolves the declared type of a <c>[SerializeReference]</c> field and the concrete types that can
    /// be assigned to it. Unity exposes the declared type only as an assembly-qualified string, so it is
    /// parsed here once and the candidate list is cached per declared type.
    /// </summary>
    internal static class ManagedReferenceTypes
    {
        private const char TypeNameSeparator = ' ';

        private static readonly Dictionary<Type, Type[]> Candidates = new();

        /// <summary>
        /// Resolves the declared type of a managed reference property. Returns false when the property
        /// is not a managed reference or the type no longer exists.
        /// </summary>
        /// <param name="property">The managed reference property.</param>
        /// <param name="type">The declared field type.</param>
        /// <returns>True when the type could be resolved.</returns>
        public static bool TryResolveFieldType(SerializedProperty property, out Type type)
        {
            type = null;

            if (property.propertyType != SerializedPropertyType.ManagedReference)
                return false;

            type = Parse(property.managedReferenceFieldTypename);
            return type != null;
        }

        /// <summary>
        /// Returns every concrete type that can be stored in a managed reference of the given type.
        /// Unity objects are excluded, since managed references may only hold plain C# objects.
        /// </summary>
        /// <param name="declaredType">The declared field type.</param>
        /// <returns>The assignable concrete types, sorted by name.</returns>
        public static Type[] GetAssignable(Type declaredType)
        {
            if (Candidates.TryGetValue(declaredType, out Type[] cached))
                return cached;

            List<Type> types = new();

            if (IsAssignable(declaredType))
                types.Add(declaredType);

            foreach (Type candidate in TypeCache.GetTypesDerivedFrom(declaredType))
            {
                if (IsAssignable(candidate))
                    types.Add(candidate);
            }

            types.Sort(comparison: (a, b) => string.CompareOrdinal(a.Name, b.Name));

            Type[] result = types.ToArray();
            Candidates[declaredType] = result;
            return result;
        }

        /// <summary>Builds the dropdown label of a candidate type, grouped by its namespace.</summary>
        /// <param name="type">The candidate type.</param>
        /// <returns>A slash-separated label, so the dropdown groups by namespace.</returns>
        public static string LabelFor(Type type) => string.IsNullOrEmpty(type.Namespace)
            ? type.Name
            : type.Namespace.Replace('.', '/') + "/" + type.Name;

        // Unity stores the declared type as "AssemblyName Namespace.TypeName".
        private static Type Parse(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
                return null;

            int separator = typeName.IndexOf(TypeNameSeparator);
            if (separator < 0)
                return Type.GetType(typeName);

            string assembly = typeName.Substring(0, separator);
            string fullName = typeName.Substring(separator + 1);

            return Type.GetType($"{fullName}, {assembly}");
        }

        private static bool IsAssignable(Type type) => type.IsClass
            && !type.IsAbstract
            && !type.IsGenericTypeDefinition
            && !typeof(Object).IsAssignableFrom(type)
            && type.GetConstructor(Type.EmptyTypes) != null;
    }
}