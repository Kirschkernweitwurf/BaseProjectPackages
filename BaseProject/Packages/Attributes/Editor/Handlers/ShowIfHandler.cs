using Base.AttributesPackage.Editor.Core;
using Base.AttributesPackage.Editor.Core.Interfaces;

namespace Base.AttributesPackage.Editor.Handlers
{
    /// <summary>Hides the field unless the referenced bool members satisfy the condition mode.</summary>
    internal sealed class ShowIfHandler : IVisibilityHandler
    {
        /// <inheritdoc/>
        public bool ShouldShow(in MemberContext context)
        {
            ShowIfAttribute attribute = context.GetAttribute<ShowIfAttribute>();
            return attribute == null || ConditionEvaluator.ResolveAll(context, attribute.Mode, attribute.Members);
        }
    }
}