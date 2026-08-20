using Base.AttributePackage.Editor.Core.Interfaces;
using Base.AttributePackage.Editor.Drawers;

namespace Base.AttributePackage.Editor.Handlers
{
    /// <summary>Disables the field while the referenced bool members satisfy the condition mode.</summary>
    internal sealed class DisableIfHandler : IEnableHandler
    {
        public bool ShouldEnable(in MemberContext context)
        {
            DisableIfAttribute attribute = context.GetAttribute<DisableIfAttribute>();
            return attribute == null || !ConditionEvaluator.ResolveAll(context, attribute.Mode, attribute.Members);
        }
    }
}