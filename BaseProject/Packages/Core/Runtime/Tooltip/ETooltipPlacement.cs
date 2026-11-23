namespace Base.CorePackage.Tooltip
{
    /// <summary>Where a <see cref="TooltipTrigger"/> puts its tooltip.</summary>
    public enum ETooltipPlacement : byte
    {
        /// <summary>Next to where the cursor is when it shows. Beside the target when there is no mouse.</summary>
        AtCursor = 0,

        /// <summary>Beside the hovered object, so the panels line up next to what they explain.</summary>
        BesideTarget = 1,

        /// <summary>
        /// Above the hovered object with the tail pointing down at it, growing away from the screen's middle, so
        /// what lies beside the object stays readable, like the other cards in a hand. Beside it, as high as it can
        /// go, only when there is no room above: an area kept visible gets covered rather than the object's
        /// neighbors.
        /// </summary>
        AboveTarget = 2
    }
}