using System;
using System.Collections.Generic;
using System.Reflection;
using Base.UtilityPackage;
using UnityEditor;

namespace Base.AttributePackage.Editor.SceneHandles
{
    /// <summary>
    /// Discovers every <see cref="IHandleDrawer"/> and caches which fields carry handle attributes.
    /// The scene view redraws constantly, so nothing here may run reflection per frame.
    /// </summary>
    internal static class HandleRegistry
    {
        private const BindingFlags FieldFlags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private static Dictionary<Type, IHandleDrawer> Drawers => _drawers ??= CreateDrawers();

        private static readonly HandleBinding[] None = Array.Empty<HandleBinding>();

        private static readonly Dictionary<FieldInfo, HandleBinding[]> Bindings = new();

        private static readonly Dictionary<Type, bool> TypesWithHandles = new();

        private static Dictionary<Type, IHandleDrawer> _drawers;

        /// <summary>Returns the handle bindings on the given field, empty when it carries none.</summary>
        /// <param name="field">The field to inspect.</param>
        /// <returns>The bindings, in attribute declaration order.</returns>
        internal static HandleBinding[] GetBindings(FieldInfo field)
        {
            if (Bindings.TryGetValue(field, out HandleBinding[] cached))
                return cached;

            List<HandleBinding> bindings = new();

            foreach (Attribute attribute in field.GetCustomAttributes())
            {
                if (Drawers.TryGetValue(attribute.GetType(), out IHandleDrawer drawer))
                    bindings.Add(new HandleBinding(attribute, drawer));
            }

            HandleBinding[] result = bindings.Count == 0
                ? None
                : bindings.ToArray();

            Bindings[field] = result;
            return result;
        }

        /// <summary>
        /// Returns whether the type carries any handle attribute, directly or inside a nested
        /// serializable type. Lets the scene hook skip the property walk entirely for the vast majority
        /// of components, which have no handles at all.
        /// </summary>
        /// <param name="type">The type to inspect.</param>
        /// <returns>True when at least one handle attribute exists somewhere in the type.</returns>
        internal static bool HasAny(Type type)
        {
            if (TypesWithHandles.TryGetValue(type, out bool cached))
                return cached;

            // Written before the scan so a type that contains itself cannot recurse forever.
            TypesWithHandles[type] = false;

            bool found = Scan(type);
            TypesWithHandles[type] = found;
            return found;
        }

        private static Dictionary<Type, IHandleDrawer> CreateDrawers()
        {
            Dictionary<Type, IHandleDrawer> drawers = new();

            foreach (Type type in TypeCache.GetTypesDerivedFrom<IHandleDrawer>())
            {
                if (type.IsAbstract || type.IsInterface)
                    continue;

                IHandleDrawer drawer = (IHandleDrawer)Activator.CreateInstance(type);
                drawers[drawer.AttributeType] = drawer;
            }

            return drawers;
        }

        private static bool Scan(Type type)
        {
            foreach (FieldInfo field in type.GetFields(FieldFlags))
            {
                if (GetBindings(field).Length > 0)
                    return true;

                Type nested = ElementType(field.FieldType);

                if (nested == null || nested.IsPrimitive || nested.IsEnum)
                    continue;

                if (!FrameworkAssemblies.Contains(nested) && HasAny(nested))
                    return true;
            }

            return false;
        }

        private static Type ElementType(Type type)
        {
            if (type.IsArray)
                return type.GetElementType();

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                return type.GetGenericArguments()[0];

            return type;
        }
    }
}