namespace Base.AttributePackage.Editor
{
    /// <summary>Hides the field unless the referenced bool members satisfy the condition mode.</summary>
    public sealed class ShowIfHandler : IVisibilityHandler
    {
        public bool ShouldShow(in MemberContext context)
        {
            ShowIfAttribute attribute = context.GetAttribute<ShowIfAttribute>();
            return attribute == null || ConditionEvaluator.ResolveAll(context, attribute.Mode, attribute.Members);
        }
    }
}
