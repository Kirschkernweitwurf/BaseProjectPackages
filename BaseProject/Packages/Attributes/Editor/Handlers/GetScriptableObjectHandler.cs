using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Fills an empty <see cref="GetScriptableObjectAttribute"/> field with the first matching asset in
    /// the project. The search runs through <see cref="AutoAssignCache"/>, since asking the asset
    /// database anything on every repaint is not affordable.
    /// </summary>
    internal sealed class GetScriptableObjectHandler : IAfterFieldHandler
    {
        private const int HandlerOrder = 6;
        private const string TypeFilter = "t:";

        public int Order => HandlerOrder;

        public void AfterField(in MemberContext context)
        {
            if (context.GetAttribute<GetScriptableObjectAttribute>() == null)
                return;

            if (!AutoAssign.IsFillable(context, out Type type))
                return;

            if (!typeof(ScriptableObject).IsAssignableFrom(type))
                return;

            Object found = AutoAssignCache.GetAsset(type, FindFirst);

            if (found != null)
                context.Property.objectReferenceValue = found;
        }

        private static Object FindFirst(Type type)
        {
            foreach (string guid in AssetDatabase.FindAssets(TypeFilter + type.Name))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Object asset = AssetDatabase.LoadAssetAtPath(path, type);

                if (asset != null)
                    return asset;
            }

            return null;
        }
    }
}