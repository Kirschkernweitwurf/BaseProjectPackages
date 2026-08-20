using Base.AttributePackage.Editor.Core.Interfaces;
using Base.AttributePackage.Editor.Drawers;
using Base.AttributePackage.Editor.Inspectors;

namespace Base.AttributePackage.Editor.Handlers
{
    /// <summary>
    /// Draws the bold title and underline for a plain <see cref="TitleAttribute"/>. Collapsible titles
    /// are drawn by <see cref="AttributePackageEditor"/> instead, which also folds the fields below them.
    /// </summary>
    internal sealed class TitleHandler : IBeforeFieldHandler
    {
        public int Order => 0;

        public void BeforeField(in MemberContext context)
        {
            TitleAttribute attribute = context.GetAttribute<TitleAttribute>();
            if (attribute == null || attribute.Foldout)
                return;

            TitleRenderer.DrawPlain(attribute, ValueResolver.Text(context, attribute.Title));
        }
    }
}