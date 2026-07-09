using UnityEditor;

namespace Base.AttributePackage.Editor
{
    /// <summary>Shows an error when a <see cref="RequiredAttribute"/> reference is null.</summary>
    public sealed class RequiredHandler : IAfterFieldHandler
    {
        public int Order => 20;

        public void AfterField(in MemberContext context)
        {
            RequiredAttribute attribute = context.GetAttribute<RequiredAttribute>();
            if (attribute == null)
                return;

            if (context.Property.propertyType == SerializedPropertyType.ObjectReference
                && context.Property.objectReferenceValue == null)
                EditorGUILayout.HelpBox(attribute.Message ?? context.DisplayName + " is required.",
                    MessageType.Error);
        }
    }
}