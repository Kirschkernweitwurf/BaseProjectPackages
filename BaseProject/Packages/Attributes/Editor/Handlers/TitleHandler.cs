using Base.AttributesPackage.Editor.Core;
using Base.AttributesPackage.Editor.Core.Interfaces;
using Base.AttributesPackage.Editor.Inspectors;

namespace Base.AttributesPackage.Editor.Handlers
{
    /// <summary>
    /// Draws the bold title and underline for a plain <see cref="TitleAttribute"/>. Collapsible titles
    /// are drawn by <see cref="AttributesPackageEditor"/> instead, which also folds the fields below them.
    /// </summary>
    internal sealed class TitleHandler : IBeforeFieldHandler
    {
        /// <inheritdoc/>
        public int Order => 0;

        /// <inheritdoc/>
        public void BeforeField(in MemberContext context)
        {
            TitleAttribute attribute = context.GetAttribute<TitleAttribute>();
            if (attribute == null || attribute.Foldout)
                return;

            TitleRenderer.DrawPlain(attribute, ValueResolver.Text(context, attribute.Title));
        }
    }
}