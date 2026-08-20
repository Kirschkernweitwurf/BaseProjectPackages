using Base.AttributePackage.Editor.Core.Interfaces;
using Base.AttributePackage.Editor.Drawers;
using UnityEditor;

namespace Base.AttributePackage.Editor.Handlers
{
    /// <summary>
    /// Shows a compact error when a <see cref="RequiredIfAttribute"/> reference is null while its
    /// condition holds. Stays silent otherwise, so configurations that never use the field are quiet.
    /// </summary>
    internal sealed class RequiredIfHandler : IAfterFieldHandler
    {
        public int Order => 20;

        public void AfterField(in MemberContext context)
        {
            RequiredIfAttribute attribute = context.GetAttribute<RequiredIfAttribute>();
            if (attribute == null)
                return;

            if (context.Property.propertyType != SerializedPropertyType.ObjectReference)
                return;

            if (context.Property.objectReferenceValue != null)
                return;

            if (!ConditionEvaluator.ResolveAll(context, attribute.Mode, attribute.Members))
                return;

            CompactHelpBox.Error(ValueResolver.Text(context, attribute.Message)
                ?? context.DisplayName + " " + RequiredIfAttribute.DefaultReason);
        }
    }
}