namespace Base.CorePackage.GameActions
{
    /// <summary>
    /// Defines when a reaction handler runs relative to an action's resolution.
    /// </summary>
    public enum EReactionTiming : byte
    {
        /// <summary>
        /// Runs BEFORE the action executes. Handlers may modify the action's data
        /// (for example reduce damage) or cancel it entirely via
        /// <see cref="GameAction.Cancel"/>.
        /// </summary>
        Pre = 0,

        /// <summary>
        /// Runs AS the action executes: after the pre-window has confirmed the action is happening,
        /// but before its effect and animation start. Handlers here cannot cancel and fire only for
        /// actions that actually resolve. Use it for reactions that must sync with the start of the
        /// action instead of waiting for its animation to finish (for example sound).
        /// </summary>
        On = 2,

        /// <summary>
        /// Runs AFTER the action has executed. The effect is already applied and
        /// can no longer be canceled. This is where follow-up reactions are
        /// triggered (for example "draw a card").
        /// </summary>
        Post = 1
    }
}