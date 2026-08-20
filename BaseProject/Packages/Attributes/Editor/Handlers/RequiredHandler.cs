using Base.AttributePackage.Editor.Core.Interfaces;
using Base.AttributePackage.Editor.Drawers;
using UnityEditor;

namespace Base.AttributePackage.Editor.Handlers
{
    /// <summary>
    /// Shows a compact error when a <see cref="RequiredAttribute"/> reference is null, with a fix button
    /// when the attribute names a repair method.
    /// </summary>
    internal sealed class RequiredHandler : IAfterFieldHandler
    {
        private const int HandlerOrder = 20;

        public int Order => HandlerOrder;

        public void AfterField(in MemberContext context)
        {
            RequiredAttribute attribute = context.GetAttribute<RequiredAttribute>();
            if (attribute == null)
                return;

            if (context.Property.propertyType != SerializedPropertyType.ObjectReference)
                return;

            if (context.Property.objectReferenceValue != null)
                return;

            string message = ValueResolver.Text(context, attribute.Message)
                ?? context.DisplayName + " " + RequiredAttribute.DefaultReason;

            FixableHelpBox.Draw(context, message, EInfoBoxType.Error, attribute.FixAction,
                attribute.FixActionName ?? RequiredAttribute.DefaultFixLabel);
        }
    }
}