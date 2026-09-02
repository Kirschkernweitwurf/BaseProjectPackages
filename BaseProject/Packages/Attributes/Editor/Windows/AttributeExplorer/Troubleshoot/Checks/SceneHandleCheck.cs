using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.AttributesPackage.Editor.Windows.AttributeExplorer.Troubleshoot.Checks
{
    /// <summary>
    /// Verifies that scene handles sit on a field type they can read, and on a type that can host them.
    /// A handle that cannot resolve either simply never draws, which is indistinguishable from a scene
    /// view that was never looked at.
    /// </summary>
    internal sealed class SceneHandleCheck : IAttributeCheck
    {
        private const string AssetMessage =
            "Handles are drawn by the component inspector, so an asset never shows them.";

        private static readonly Type[] VectorHandles =
        {
            typeof(PositionHandleAttribute),
            typeof(ScaleHandleAttribute),
            typeof(DrawLineAttribute),
            typeof(DrawLabelAttribute)
        };

        private static readonly Type[] FloatHandles =
        {
            typeof(RadiusHandleAttribute),
            typeof(DrawWireDiscAttribute)
        };

        private static readonly Type[] AllHandles =
        {
            typeof(PositionHandleAttribute),
            typeof(RotationHandleAttribute),
            typeof(ScaleHandleAttribute),
            typeof(RadiusHandleAttribute),
            typeof(DrawLineAttribute),
            typeof(DrawLabelAttribute),
            typeof(DrawWireDiscAttribute),
            typeof(SceneViewPickerAttribute)
        };

        /// <inheritdoc/>
        public void Inspect(Type type, List<AttributeIssue> issues)
        {
            // Only an asset is definitely wrong. A plain serializable class carrying a handle is fine,
            // because the walk descends into it from whichever component embeds it.
            bool isAsset = typeof(ScriptableObject).IsAssignableFrom(type);

            foreach (FieldInfo field in ScannedMembers.DeclaredFields(type))
            {
                Type fieldType = CheckedMembers.ElementType(field.FieldType);
                if (fieldType == null)
                    continue;

                if (isAsset)
                    VerifyHost(field, issues);

                Verify(field, fieldType, VectorHandles, typeof(Vector3), issues);
                Verify(field, fieldType, FloatHandles, typeof(float), issues);
                VerifyRotation(field, fieldType, issues);
                VerifyPicker(field, fieldType, issues);
                VerifyPositionMember(field, type, issues);
            }
        }

        private static void VerifyHost(FieldInfo field, List<AttributeIssue> issues)
        {
            foreach (Type attributeType in AllHandles)
            {
                if (field.GetCustomAttribute(attributeType) != null)
                    AttributeIssues.Error(issues, field, attributeType, AssetMessage);
            }
        }

        private static void Verify(FieldInfo field, Type fieldType, Type[] attributes, Type required,
            List<AttributeIssue> issues)
        {
            if (fieldType == required)
                return;

            foreach (Type attributeType in attributes)
            {
                if (field.GetCustomAttribute(attributeType) == null)
                    continue;

                AttributeIssues.Error(issues, field, attributeType,
                    $"{fieldType.Name} is not supported. This handle needs a {required.Name}.");
            }
        }

        private static void VerifyRotation(FieldInfo field, Type fieldType, List<AttributeIssue> issues)
        {
            if (field.GetCustomAttribute<RotationHandleAttribute>() == null)
                return;

            if (fieldType == typeof(Quaternion) || fieldType == typeof(Vector3))
                return;

            AttributeIssues.Error(issues, field, typeof(RotationHandleAttribute),
                $"{fieldType.Name} is not supported. This handle needs a Quaternion or a Vector3 of euler "
                + "angles.");
        }

        private static void VerifyPicker(FieldInfo field, Type fieldType, List<AttributeIssue> issues)
        {
            if (field.GetCustomAttribute<SceneViewPickerAttribute>() == null)
                return;

            if (typeof(Object).IsAssignableFrom(fieldType))
                return;

            AttributeIssues.Error(issues, field, typeof(SceneViewPickerAttribute),
                $"{fieldType.Name} is not an object reference, so there is nothing to pick into.");
        }

        // The anchor members are optional, but a stale name silently drops the gizmo back onto the
        // transform, which looks like the offset simply being ignored.
        private static void VerifyPositionMember(FieldInfo field, Type owner, List<AttributeIssue> issues)
        {
            VerifyMember<RotationHandleAttribute>(field, owner, issues, selector: a => a.PositionMember);
            VerifyMember<ScaleHandleAttribute>(field, owner, issues, selector: a => a.PositionMember);
            VerifyMember<RadiusHandleAttribute>(field, owner, issues, selector: a => a.PositionMember);
            VerifyMember<DrawWireDiscAttribute>(field, owner, issues, selector: a => a.PositionMember);
            VerifyMember<DrawLineAttribute>(field, owner, issues, selector: a => a.FromMember);
            VerifyMember<DrawLabelAttribute>(field, owner, issues, selector: a => a.TextMember);
        }

        private static void VerifyMember<T>(FieldInfo field, Type owner, List<AttributeIssue> issues,
            Func<T, string> selector) where T : Attribute
        {
            T attribute = field.GetCustomAttribute<T>();
            if (attribute == null)
                return;

            string member = selector(attribute);
            if (string.IsNullOrEmpty(member) || CheckedMembers.Exists(owner, member))
                return;

            AttributeIssues.Error(issues, field, typeof(T),
                $"'{member}' does not exist on {owner.Name}, so the handle falls back to the transform.");
        }
    }
}