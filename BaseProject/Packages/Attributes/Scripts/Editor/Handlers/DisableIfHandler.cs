namespace Base.AttributePackage.Editor
{
    /// <summary>Disables the field while the referenced bool members satisfy the condition mode.</summary>
    public sealed class DisableIfHandler : IEnableHandler
    {
        public bool ShouldEnable(in MemberContext context)
        {
            DisableIfAttribute attribute = context.GetAttribute<DisableIfAttribute>();
            return attribute == null || !ConditionEvaluator.ResolveAll(context, attribute.Mode, attribute.Members);
        }
    }
}