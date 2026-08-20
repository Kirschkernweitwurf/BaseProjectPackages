using System;
using Base.AttributePackage.Editor.Core.Interfaces;
using Base.AttributePackage.Editor.Drawers;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.AttributePackage.Editor.Handlers
{
    /// <summary>
    /// Enforces <see cref="MustImplementAttribute"/>. A newly assigned object that does not satisfy the
    /// required types is resolved to a matching component on the same GameObject when possible, and
    /// otherwise reverted. A pre-existing violation is reported instead of silently cleared.
    /// </summary>
    internal sealed class MustImplementHandler : IAfterFieldHandler
    {
        public int Order => 0;

        public void AfterField(in MemberContext context)
        {
            MustImplementAttribute attribute = context.GetAttribute<MustImplementAttribute>();
            if (attribute == null || attribute.Types == null || attribute.Types.Length == 0)
                return;

            if (context.Property.propertyType != SerializedPropertyType.ObjectReference)
                return;

            Object current = context.Property.objectReferenceValue;
            if (current == null)
                return;

            if (Satisfies(current, attribute.Types))
                return;

            Object resolved = Resolve(current, attribute.Types);
            if (resolved != null)
            {
                context.Property.objectReferenceValue = resolved;
                return;
            }

            // Reverting a fresh assignment gives immediate feedback. An object that was already stored
            // is kept and reported, so an interface removed later does not wipe authored data.
            if (current != context.ObjectReferenceBefore)
                context.Property.objectReferenceValue = context.ObjectReferenceBefore;
            else
                CompactHelpBox.Error($"{current.name} {Describe(attribute.Types)}");
        }

        private static bool Satisfies(Object value, Type[] types)
        {
            foreach (Type required in types)
            {
                if (required != null && !required.IsInstanceOfType(value))
                    return false;
            }

            return true;
        }

        // Dragging a whole GameObject is the common case, so the first component satisfying every
        // required type is used instead of rejecting the drop.
        private static Object Resolve(Object assigned, Type[] types)
        {
            GameObject gameObject = assigned switch
            {
                GameObject direct => direct,
                Component component => component.gameObject,
                _ => null
            };

            if (gameObject == null)
                return null;

            foreach (Component candidate in gameObject.GetComponents<Component>())
            {
                if (candidate != null && Satisfies(candidate, types))
                    return candidate;
            }

            return null;
        }

        private static string Describe(Type[] types)
        {
            string[] names = new string[types.Length];
            for (int i = 0; i < types.Length; i++)
            {
                names[i] = types[i] == null
                    ? string.Empty
                    : types[i].Name;
            }

            return $"does not implement {string.Join(", ", names)}.";
        }
    }
}