using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Base.AttributesPackage.Editor.Core
{
    /// <summary>
    /// Caches which serializable types have a registered <see cref="CustomPropertyDrawer"/>. The
    /// pipeline must not descend into those types, since Unity draws them through the custom drawer.
    /// </summary>
    internal static class PropertyDrawerCache
    {
        private const string TargetTypeField = "m_Type";
        private const string UseForChildrenField = "m_UseForChildren";

        private static readonly Dictionary<Type, bool> Results = new();

        private static HashSet<Type> _exactTypes;
        private static List<Type> _baseTypes;

        /// <summary>Returns true when the given type is drawn by a registered property drawer.</summary>
        internal static bool HasDrawer(Type type)
        {
            if (Results.TryGetValue(type, out bool cached))
                return cached;

            Build();

            bool result = Resolve(type);
            Results[type] = result;
            return result;
        }

        private static bool Resolve(Type type)
        {
            if (_exactTypes.Contains(type))
                return true;

            // A drawer for a generic type is registered against the open definition, for example
            // SerializableDictionary<,>, while the inspected field carries a closed one. Without this
            // step the pipeline would descend into the type and draw its raw backing list instead.
            if (type.IsGenericType && _exactTypes.Contains(type.GetGenericTypeDefinition()))
                return true;

            foreach (Type baseType in _baseTypes)
            {
                if (IsCoveredBy(baseType, type))
                    return true;
            }

            return false;
        }

        // Open generic base types cannot answer IsAssignableFrom, so the hierarchy is walked and each
        // level compared against the definition instead.
        private static bool IsCoveredBy(Type registered, Type type)
        {
            if (registered.IsAssignableFrom(type))
                return true;

            if (!registered.IsGenericTypeDefinition)
                return false;

            Type current = type;
            while (current != null)
            {
                if (current.IsGenericType && current.GetGenericTypeDefinition() == registered)
                    return true;

                current = current.BaseType;
            }

            return false;
        }

        private static void Build()
        {
            if (_exactTypes != null)
                return;

            _exactTypes = new HashSet<Type>();
            _baseTypes = new List<Type>();

            FieldInfo targetField =
                typeof(CustomPropertyDrawer).GetField(TargetTypeField, BindingFlags.Instance | BindingFlags.NonPublic);

            FieldInfo childrenField = typeof(CustomPropertyDrawer)
                .GetField(UseForChildrenField, BindingFlags.Instance | BindingFlags.NonPublic);

            if (targetField == null || childrenField == null)
                return;

            foreach (Type drawer in TypeCache.GetTypesWithAttribute<CustomPropertyDrawer>())
            {
                foreach (CustomPropertyDrawer attribute in drawer.GetCustomAttributes<CustomPropertyDrawer>())
                    Register(attribute, targetField, childrenField);
            }
        }

        private static void Register(CustomPropertyDrawer attribute, FieldInfo targetField, FieldInfo childrenField)
        {
            if (targetField.GetValue(attribute) is not Type target)
                return;

            if (typeof(PropertyAttribute).IsAssignableFrom(target))
                return;

            if (childrenField.GetValue(attribute) is true)
                _baseTypes.Add(target);
            else
                _exactTypes.Add(target);
        }
    }
}