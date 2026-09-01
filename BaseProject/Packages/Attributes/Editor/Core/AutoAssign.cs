using System;
using UnityEditor;

namespace Base.AttributePackage.Editor.Core
{
    /// <summary>
    /// The guard every auto-getter shares: only fill an object reference that is empty, only on a single
    /// object, and only when the field type is known. Without the empty check a search would run on
    /// every repaint of every selected object, which is what makes the expensive getters affordable.
    /// </summary>
    internal static class AutoAssign
    {
        /// <summary>Returns whether the member is an empty object reference worth searching for.</summary>
        /// <param name="context">The member currently being drawn.</param>
        /// <param name="fieldType">The type the field holds.</param>
        /// <returns>True when a search should run.</returns>
        internal static bool IsFillable(in MemberContext context, out Type fieldType)
        {
            fieldType = context.Field?.FieldType;

            if (fieldType == null)
                return false;

            if (context.Property.propertyType != SerializedPropertyType.ObjectReference)
                return false;

            if (context.Property.objectReferenceValue != null)
                return false;

            // Filling several objects at once from one search would assign them all the same reference,
            // which is almost never what a multi-object edit means.
            return !context.Editor.serializedObject.isEditingMultipleObjects;
        }
    }
}