using System;
using Base.AttributesPackage.Editor.Core;
using Base.AttributesPackage.Editor.Core.Interfaces;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.AttributesPackage.Editor.Handlers
{
    /// <summary>
    /// Fills an empty <see cref="GetInSceneAttribute"/> field from anywhere in the open scenes. The
    /// search runs through <see cref="AutoAssignCache"/> and is dropped whenever the hierarchy changes.
    /// </summary>
    internal sealed class GetInSceneHandler : IAfterFieldHandler
    {
        private const int HandlerOrder = 8;

        /// <inheritdoc/>
        public int Order => HandlerOrder;

        /// <inheritdoc/>
        public void AfterField(in MemberContext context)
        {
            GetInSceneAttribute attribute = context.GetAttribute<GetInSceneAttribute>();
            if (attribute == null)
                return;

            if (!AutoAssign.IsFillable(context, out Type type))
                return;

            if (!typeof(Component).IsAssignableFrom(type))
                return;

            FindObjectsInactive inactive = attribute.IncludeInactive
                ? FindObjectsInactive.Include
                : FindObjectsInactive.Exclude;

            Object found = AutoAssignCache.GetSceneObject(type,
                search: searched => FindFirst(searched, inactive));

            if (found != null)
                context.Property.objectReferenceValue = found;
        }

        private static Object FindFirst(Type type, FindObjectsInactive inactive)
        {
            Object[] found = Object.FindObjectsByType(type, inactive, FindObjectsSortMode.None);

            return found.Length > 0
                ? found[0]
                : null;
        }
    }
}