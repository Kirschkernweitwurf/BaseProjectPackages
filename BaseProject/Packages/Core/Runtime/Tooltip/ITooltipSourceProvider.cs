namespace Base.CorePackage.Tooltip
{
    /// <summary>
    /// Names what a tooltip explains, for the header of a <see cref="TooltipView"/> that has one. Put it next to an
    /// <see cref="ITooltipProvider"/> on the trigger's GameObject when the panels explain parts of something, like the
    /// terms on a card, so the header says which card.
    /// </summary>
    public interface ITooltipSourceProvider
    {
        /// <summary>Gets the header: a title, an icon and an eyebrow; its body is ignored.</summary>
        /// <param name="source">The header to show.</param>
        /// <returns><c>true</c> when there is something to name.</returns>
        bool TryGetSource(out TooltipEntry source);
    }
}
