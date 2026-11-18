using Base.ServicesPackage;

namespace Base.CorePackage.GameActions
{
    /// <summary>
    /// Lets cheat commands change the game through the <see cref="IGameActionSystem"/> like any card would,
    /// so rules and feedback still react to a cheat and the game state stays consistent.
    /// </summary>
    public static class CheatActionRunner
    {
        private const string MissingSystemMessage = "No action system in this scene. Cheat skipped.";

        /// <summary>Queues <paramref name="action"/> and returns the text the cheat console shows.</summary>
        /// <param name="action">The action the cheat performs.</param>
        /// <param name="queuedMessage">Shown when the action was queued.</param>
        public static string Perform(GameAction action, string queuedMessage)
        {
            if (!ServiceLocator.TryGet(out IGameActionSystem actions))
                return MissingSystemMessage;

            actions.Perform(action);
            return queuedMessage;
        }
    }
}