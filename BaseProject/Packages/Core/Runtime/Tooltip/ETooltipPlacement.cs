namespace Base.CorePackage.Tooltip
{
    /// <summary>Where a <see cref="TooltipTrigger"/> puts its tooltip.</summary>
    public enum ETooltipPlacement : byte
    {
        /// <summary>Next to the cursor, following it. Falls back to beside the target without a mouse.</summary>
        FollowCursor = 0,

        /// <summary>Beside the hovered object, so the panels line up next to what they explain.</summary>
        BesideTarget = 1
    }
}