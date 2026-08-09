namespace Base.AttributePackage.Editor
{
    /// <summary>Hides the field while the referenced bool members satisfy the condition mode.</summary>
    public sealed class HideIfHandler : IVisibilityHandler
    {
        public bool ShouldShow(in MemberContext context)
        {
            HideIfAttribute attribute = context.GetAttribute<HideIfAttribute>();
            return attribute == null || !ConditionEvaluator.ResolveAll(context, attribute.Mode, attribute.Members);
        }
    }
}
