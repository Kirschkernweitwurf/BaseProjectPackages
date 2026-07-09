namespace Base.AttributePackage.Editor
{
    /// <summary>Disables the field while the referenced bool member is true.</summary>
    public sealed class DisableIfHandler : IEnableHandler
    {
        public bool ShouldEnable(in MemberContext context)
        {
            DisableIfAttribute attribute = context.GetAttribute<DisableIfAttribute>();
            return attribute == null || !ConditionEvaluator.ResolveBool(context, attribute.Member);
        }
    }
}