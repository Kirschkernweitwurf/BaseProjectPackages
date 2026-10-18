namespace Base.CorePackage.Tooltip
{
    /// <summary>Where a <see cref="TooltipTrigger"/> puts its tooltip.</summary>
    public enum ETooltipPlacement : byte
    {
        /// <summary>Next to where the cursor is when it shows. Beside the target when there is no mouse.</summary>
        AtCursor = 0,

        /// <summary>Beside the hovered object, so the panels line up next to what they explain.</summary>
        BesideTarget = 1
    }
}