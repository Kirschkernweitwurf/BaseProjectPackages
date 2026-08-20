using System;
using Base.AttributePackage.Editor.Core.Interfaces;
using Base.AttributePackage.Editor.Drawers;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.AttributePackage.Editor.Handlers
{
    /// <summary>
    /// Fills an empty <see cref="GetPrefabWithComponentAttribute"/> field with the first prefab asset
    /// carrying the required component. On a GameObject field the prefab root is assigned; on a
    /// component field the component on that root is.
    /// </summary>
    internal sealed class GetPrefabWithComponentHandler : IAfterFieldHandler
    {
        private const int HandlerOrder = 7;
        private const string PrefabFilter = "t:Prefab";

        public int Order => HandlerOrder;

        public void AfterField(in MemberContext context)
        {
            GetPrefabWithComponentAttribute attribute =
                context.GetAttribute<GetPrefabWithComponentAttribute>();

            if (attribute == null)
                return;

            if (!AutoAssign.IsFillable(context, out Type fieldType))
                return;

            Type componentType = attribute.ComponentType ?? fieldType;
            if (!typeof(Component).IsAssignableFrom(componentType) && !componentType.IsInterface)
                return;

            Object found = AutoAssignCache.GetAsset(componentType, FindFirst);
            if (found == null)
                return;

            context.Property.objectReferenceValue = Narrow(found, fieldType, componentType);
        }

        private static Object FindFirst(Type componentType)
        {
            foreach (string guid in AssetDatabase.FindAssets(PrefabFilter))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (prefab != null && prefab.GetComponent(componentType) != null)
                    return prefab;
            }

            return null;
        }

        // The cache stores the prefab root, because that is what the search found. Which of the two the
        // field actually wants is decided here rather than by caching two entries per type.
        private static Object Narrow(Object found, Type fieldType, Type componentType)
        {
            if (fieldType == typeof(GameObject))
                return found;

            return found is GameObject prefab
                ? prefab.GetComponent(componentType)
                : found;
        }
    }
}