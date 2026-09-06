using Base.ToolsPackage.Editor.MenuManagerModel;
using UnityEditor;

namespace Base.ToolsPackage.Editor.MenuManagerWindows
{
    /// <summary>
    /// The temporary highlight an overview window asks for when it links straight to one entry.
    /// <para>
    /// It is temporary on purpose. The window is opened to answer a question about one entry, and a
    /// highlight that stayed would still be pointing at it the next time the window is opened for a
    /// different reason.
    /// </para>
    /// </summary>
    internal sealed class MenuRowFocus
    {
        // Long enough to find the row after the window opens, short enough that it is gone by the
        // time anybody comes back to the window for something else.
        private const double FocusSeconds = 4d;

        private string _entryId;
        private double _expiresAt;

        /// <summary>Whether the row still has to be scrolled into view.</summary>
        internal bool IsScrollPending { get; private set; }

        /// <summary>Whether an entry is currently highlighted at all.</summary>
        internal bool IsActive => _entryId != null;

        /// <summary>Starts highlighting the entry with the given id and asks for it to be scrolled to.</summary>
        /// <param name="entryId">Stable id of the entry to highlight.</param>
        internal void Begin(string entryId)
        {
            _entryId = entryId;
            _expiresAt = EditorApplication.timeSinceStartup + FocusSeconds;
            IsScrollPending = true;
        }

        /// <summary>Whether the given entry is the highlighted one.</summary>
        /// <param name="entry">The entry a row is drawing.</param>
        /// <returns>True while that entry is the one being highlighted.</returns>
        internal bool Matches(MenuEntry entry) => _entryId != null
            && entry != null
            && entry.Id == _entryId;

        /// <summary>Drops the highlight once its time is up.</summary>
        internal void Expire()
        {
            if (_entryId == null)
                return;

            if (EditorApplication.timeSinceStartup <= _expiresAt)
                return;

            _entryId = null;
            IsScrollPending = false;
        }

        /// <summary>Records that the row has been scrolled to, so it is not scrolled to again.</summary>
        internal void ScrollDone() => IsScrollPending = false;
    }
}