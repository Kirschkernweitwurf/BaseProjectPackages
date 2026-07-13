#if UNITY_EDITOR
using System;

namespace Base.ToolPackage.Editor.MenuManagerWindow
{
    /// <summary>Live scan result for one entry. Holds the delegates and defaults needed to register a menu.</summary>
    public sealed class ResolvedMenu
    {
        /// <summary>Kind of the entry.</summary>
        public EMenuEntryKind Kind { get; }

        /// <summary>Full default path used when the entry is first discovered.</summary>
        public string DefaultPath { get; }

        /// <summary>Action invoked when the menu item is clicked.</summary>
        public Action Execute { get; }

        /// <summary>Optional validate function, or null.</summary>
        public Func<bool> Validate { get; }

        /// <summary>Creates a resolved entry.</summary>
        public ResolvedMenu(EMenuEntryKind kind, string defaultPath, Action execute, Func<bool> validate)
        {
            Kind = kind;
            DefaultPath = defaultPath;
            Execute = execute;
            Validate = validate;
        }
    }
}
#endif