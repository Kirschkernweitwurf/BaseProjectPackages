using Base.AttributePackage.Editor.Core;
using Base.AttributePackage.Editor.Core.Interfaces;

namespace Base.AttributePackage.Editor.Handlers
{
    /// <summary>Disables the field unless the referenced bool members satisfy the condition mode.</summary>
    internal sealed class EnableIfHandler : IEnableHandler
    {
        public bool ShouldEnable(in MemberContext context)
        {
            EnableIfAttribute attribute = context.GetAttribute<EnableIfAttribute>();
            return attribute == null || ConditionEvaluator.ResolveAll(context, attribute.Mode, attribute.Members);
        }
    }
}