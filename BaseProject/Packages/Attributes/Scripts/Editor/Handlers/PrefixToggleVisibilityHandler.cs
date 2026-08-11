namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Hides a bool that some other field draws as its prefix toggle. Without this the same value would
    /// appear twice, once as a checkbox in front of the field it drives and once on a row of its own.
    /// </summary>
    public sealed class PrefixToggleVisibilityHandler : IVisibilityHandler
    {
        public bool ShouldShow(in MemberContext context)
            => !PrefixToggleState.IsDrivenBySomeone(context.DeclaringType, context.Property.name);
    }
}
