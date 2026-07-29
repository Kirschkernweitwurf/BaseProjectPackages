using UnityEditor;

namespace Base.CorePackage.Editor.MenuManaging.Identifier
{
    /// <summary>
    /// Coalesces the regeneration requests coming from the asset callbacks into a single deferred run.
    /// </summary>
    /// <remarks>
    /// Asset callbacks must not create or refresh assets while an import is in flight, and a pending
    /// delete is only finished after the callback returns. Deferring also merges a batch of changes
    /// into one run.
    /// </remarks>
    internal static class MenuIdentifierRegenerationScheduler
    {
        private static bool _pending;

        /// <summary>Queues a regeneration for the next editor tick.</summary>
        internal static void Schedule()
        {
            if (_pending)
                return;

            _pending = true;
            EditorApplication.delayCall += Run;
        }

        private static void Run()
        {
            _pending = false;
            MenuIdentifierGenerator.Regenerate();
        }
    }
}