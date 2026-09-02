using Base.AttributesPackage.Editor.Core;
using Base.AttributesPackage.Editor.Core.Interfaces;

namespace Base.AttributesPackage.Editor.Handlers
{
    /// <summary>Hides the field while the referenced bool members satisfy the condition mode.</summary>
    internal sealed class HideIfHandler : IVisibilityHandler
    {
        /// <inheritdoc/>
        public bool ShouldShow(in MemberContext context)
        {
            HideIfAttribute attribute = context.GetAttribute<HideIfAttribute>();
            return attribute == null || !ConditionEvaluator.ResolveAll(context, attribute.Mode, attribute.Members);
        }
    }
}