using System.Collections.Generic;

namespace Base.CorePackage.Tooltip
{
    /// <summary>
    /// Builds tooltip panels at hover time, for content that changes at runtime or comes from localization.
    /// A <see cref="TooltipTrigger"/> asks every provider on its GameObject, in component order, so several
    /// providers can add their panels to one tooltip.
    /// </summary>
    public interface ITooltipProvider
    {
        /// <summary>Appends this provider's panels to <paramref name="entries"/>.</summary>
        /// <param name="entries">The panels collected so far. Add to it, never clear it.</param>
        void CollectEntries(List<TooltipEntry> entries);
    }
}