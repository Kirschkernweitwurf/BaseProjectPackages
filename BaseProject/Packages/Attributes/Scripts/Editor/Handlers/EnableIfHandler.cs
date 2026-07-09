using Base.AttributePackage.Conditional;
using Base.AttributePackage.Editor.Core;
using Base.AttributePackage.Editor.Core.Interfaces;

namespace Base.AttributePackage.Editor.Handlers
{
    /// <summary>Disables the field unless the referenced bool member is true.</summary>
    public sealed class EnableIfHandler : IEnableHandler
    {
        public bool ShouldEnable(in MemberContext context)
        {
            EnableIfAttribute attribute = context.GetAttribute<EnableIfAttribute>();
            return attribute == null || ConditionEvaluator.ResolveBool(context, attribute.Member);
        }
    }
}