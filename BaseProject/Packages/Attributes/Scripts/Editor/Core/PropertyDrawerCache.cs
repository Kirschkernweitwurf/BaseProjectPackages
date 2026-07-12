using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Caches which serializable types have a registered <see cref="CustomPropertyDrawer"/>. The
    /// pipeline must not descend into those types, since Unity draws them through the custom drawer.
    /// </summary>
    public static class PropertyDrawerCache
    {
        private const string TargetTypeField = "m_Type";
        private const string UseForChildrenField = "m_UseForChildren";

        private static HashSet<Type> _exactTypes;
        private static List<Type> _baseTypes;

        /// <summary>Returns true when the given type is drawn by a registered property drawer.</summary>
        public static bool HasDrawer(Type type)
        {
            Build();

            if (_exactTypes.Contains(type))
                return true;

            foreach (Type baseType in _baseTypes)
            {
                if (baseType.IsAssignableFrom(type))
                    return true;
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