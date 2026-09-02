using Base.AttributesPackage.Editor.Core;
using Base.AttributesPackage.Editor.Core.Interfaces;

namespace Base.AttributesPackage.Editor.Handlers
{
    /// <summary>Disables the field while the referenced bool members satisfy the condition mode.</summary>
    internal sealed class DisableIfHandler : IEnableHandler
    {
        /// <inheritdoc/>
        public bool ShouldEnable(in MemberContext context)
        {
            DisableIfAttribute attribute = context.GetAttribute<DisableIfAttribute>();
            return attribute == null || !ConditionEvaluator.ResolveAll(context, attribute.Mode, attribute.Members);
        }
    }
}