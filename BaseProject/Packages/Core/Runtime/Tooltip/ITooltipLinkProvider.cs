using System.Collections.Generic;

namespace Base.CorePackage.Tooltip
{
    /// <summary>
    /// Adds links to a tooltip at hover time: the things its text names, each shown beside the tooltip as a picture
    /// with its own panels, like the card a rule deals. A <see cref="TooltipTrigger"/> asks every provider on its
    /// GameObject, in component order.
    /// </summary>
    public interface ITooltipLinkProvider
    {
        /// <summary>Appends this provider's links to <paramref name="links"/>.</summary>
        /// <param name="links">The links collected so far. Add to it, never clear it.</param>
        void CollectLinks(List<TooltipLink> links);
    }
}