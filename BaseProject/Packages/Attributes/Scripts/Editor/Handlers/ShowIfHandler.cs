using Base.AttributePackage.Conditional;
using Base.AttributePackage.Editor.Core;

namespace Base.AttributePackage.Editor.Handlers
{
    /// <summary>Hides the field unless the referenced bool member is true.</summary>
    public sealed class ShowIfHandler : IVisibilityHandler
    {
        public bool ShouldShow(in MemberContext context)
        {
            ShowIfAttribute attribute = context.GetAttribute<ShowIfAttribute>();
            return attribute == null || ConditionEvaluator.ResolveBool(context, attribute.Member);
        }
    }
}