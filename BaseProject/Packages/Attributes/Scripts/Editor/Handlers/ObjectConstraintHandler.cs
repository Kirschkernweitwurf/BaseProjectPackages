using Base.AttributePackage.Editor.Core;
using Base.AttributePackage.Editor.Core.Interfaces;
using Base.AttributePackage.Validation;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.AttributePackage.Editor.Handlers
{
    /// <summary>
    /// Enforces <see cref="AssetOnlyAttribute"/> and <see cref="SceneObjectOnlyAttribute"/>.
    /// Reverts a newly assigned invalid reference and reports a pre-existing invalid one.
    /// </summary>
    public sealed class ObjectConstraintHandler : IAfterFieldHandler
    {
        public int Order => 0;

        public void AfterField(in MemberContext context)
        {
            if (context.Property.propertyType != SerializedPropertyType.ObjectReference)
                return;

            bool assetOnly = context.GetAttribute<AssetOnlyAttribute>() != null;
            bool sceneOnly = context.GetAttribute<SceneObjectOnlyAttribute>() != null;
            if (!assetOnly && !sceneOnly)
                return;

            Object current = context.Property.objectReferenceValue;
            if (current == null)
                return;

            if (assetOnly && !IsAsset(current))
                Reject(context, current, "Only project assets are allowed here.");
            else if (sceneOnly && !IsSceneObject(current))
                Reject(context, current, "Only scene objects are allowed here.");
        }

        private static void Reject(in MemberContext context, Object current, string message)
        {
            if (current != context.ObjectReferenceBefore)
                context.Property.objectReferenceValue = context.ObjectReferenceBefore;
            else
                EditorGUILayout.HelpBox(message, MessageType.Error);
        }

        private static bool IsAsset(Object value) => value != null && EditorUtility.IsPersistent(value);

        private static bool IsSceneObject(Object value) => value != null
            && !EditorUtility.IsPersistent(value)
            && (value is GameObject || value is Component);
    }
}