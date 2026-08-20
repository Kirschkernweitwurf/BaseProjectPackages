using Base.AttributePackage.Editor.Handlers;

namespace Base.AttributePackage.Editor.Drawers
{
    /// <summary>Hides the field while the referenced bool members satisfy the condition mode.</summary>
    internal sealed class HideIfHandler : IVisibilityHandler
    {
        public bool ShouldShow(in MemberContext context)
        {
            HideIfAttribute attribute = context.GetAttribute<HideIfAttribute>();
            return attribute == null || !ConditionEvaluator.ResolveAll(context, attribute.Mode, attribute.Members);
        }
    }
}