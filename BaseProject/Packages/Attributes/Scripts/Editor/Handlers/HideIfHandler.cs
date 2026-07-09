using Base.AttributePackage.Conditional;
using Base.AttributePackage.Editor.Core;

namespace Base.AttributePackage.Editor.Handlers
{
    /// <summary>Hides the field while the referenced bool member is true.</summary>
    public sealed class HideIfHandler : IVisibilityHandler
    {
        public bool ShouldShow(in MemberContext context)
        {
            HideIfAttribute attribute = context.GetAttribute<HideIfAttribute>();
            return attribute == null || !ConditionEvaluator.ResolveBool(context, attribute.Member);
        }
    }
}