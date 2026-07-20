namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Draws the bold title and underline for a plain <see cref="TitleAttribute"/>. Collapsible titles
    /// are drawn by <see cref="AttributePackageEditor"/> instead, which also folds the fields below them.
    /// </summary>
    public sealed class TitleHandler : IBeforeFieldHandler
    {
        public int Order => 0;

        public void BeforeField(in MemberContext context)
        {
            TitleAttribute attribute = context.GetAttribute<TitleAttribute>();
            if (attribute == null || attribute.Foldout)
                return;

            TitleRenderer.DrawPlain(attribute);
        }
    }
}