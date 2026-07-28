namespace Base.AttributePackage.Editor
{
    /// <summary>Disables <see cref="ReadOnlyAttribute"/> fields while keeping them visible.</summary>
    public sealed class ReadOnlyHandler : IEnableHandler
    {
        public bool ShouldEnable(in MemberContext context) => context.GetAttribute<ReadOnlyAttribute>() == null;
    }
}