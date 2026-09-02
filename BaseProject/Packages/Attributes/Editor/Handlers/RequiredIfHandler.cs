using Base.AttributesPackage.Editor.Core;
using Base.AttributesPackage.Editor.Core.Interfaces;
using Base.AttributesPackage.Editor.Drawers;
using UnityEditor;

namespace Base.AttributesPackage.Editor.Handlers
{
    /// <summary>
    /// Shows a compact error when a <see cref="RequiredIfAttribute"/> reference is null while its
    /// condition holds. Stays silent otherwise, so configurations that never use the field are quiet.
    /// </summary>
    internal sealed class RequiredIfHandler : IAfterFieldHandler
    {
        /// <inheritdoc/>
        public int Order => 20;

        /// <inheritdoc/>
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