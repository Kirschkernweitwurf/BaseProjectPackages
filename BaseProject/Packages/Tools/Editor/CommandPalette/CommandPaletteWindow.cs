using Base.UtilityPackage.Menus;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace Base.ToolsPackage.Editor.CommandPalette
{
    /// <summary>
    /// Searchable list of every editor menu item and every asset creation entry, static or
    /// arranged in the menu manager, plus the settings pages the project and its packages declare.
    /// Opens on a shortcut or from the main toolbar, ranks by fuzzy match over the whole path plus
    /// tags, pins and recent use, and runs the selection on Enter.
    /// <para>
    /// The window owns the state and routes every request through <see cref="Handle"/>, so the
    /// keyboard, the mouse and the context menu all end up in the same implementation.
    /// </para>
    /// </summary>
    internal sealed class CommandPaletteWindow : EditorWindow
    {
        private const string PaletteMenuPath = "Tools/Base Packages/Command Palette";

        /// <summary>Width of both filter pills. They share it so the pair reads as one control.</summary>
        private const float PillWidth = 68f;

        private const string Placeholder = "Search any menu.    #tag    > menu    + asset    @ settings";
        private const string RescanFormat = "Rescanned, {0} commands";
        private const string RescanLabel = "Rescan";
        private const string ResultFormat = "{0} of {1}";
        private const string SearchControlName = "BaseCommandPaletteSearch";
        private const string SearchIconName = "Search Icon";
        private const string ShortcutId = "Base/Command Palette";
        private const float VerticalBias = 0.24f;
        private const float WindowHeight = 520f;
        private const string WindowTitle = "Command Palette";
        private const float WindowWidth = 760f;

        private static readonly GUIContent ProjectContent =
            new("Project", "Hide commands that come from packages or from Unity itself");

        private static readonly GUIContent RescanContent = new(RescanLabel, "Build the command index again");

        private readonly CommandResultList _list = new();
        private readonly CommandTagEditor _tags = new();

        private ECommandPaletteAction _pending;
        private string _search = string.Empty;
        private string _status;
        private string _term = string.Empty;
        private bool _closing;
        private bool _focusRequested;
        private bool _menuOpen;
        private bool _needsQuery;
        private bool _projectOnly;

#region Unity Callbacks
        private void OnEnable()
        {
            wantsMouseMove = true;
            _focusRequested = true;
            _needsQuery = true;
        }

        private void OnGUI()
        {
            CommandPaletteStyles.EnsureFresh();

            if (_needsQuery)
                RunQuery();

            if (Event.current.type == EventType.MouseMove)
                Repaint();

            CommandPaletteLayout layout = new(new Rect(0f, 0f, position.width, position.height));

            DrawBackground();
            HandleKeyboard();

            DrawSearchRow(layout.Search);
            CommandPaletteChrome.DrawSeparator(layout.TopLine);

            Handle(_list.Draw(layout.List, _term));

            CommandPaletteChrome.DrawSeparator(layout.BottomLine);
            DrawFooter(layout.Footer);

            ApplyFocus();
            Finish();
        }

        private void OnFocus() => _menuOpen = false;

        private void OnLostFocus()
        {
            // A context menu takes the focus away without the palette being done.
            if (_closing || _menuOpen)
                return;

            CloseNow();
        }
#endregion

        /// <summary>Opens the palette centered on the main editor window.</summary>
        [Shortcut(ShortcutId, KeyCode.K, ShortcutModifiers.Action | ShortcutModifiers.Shift)]
        [DynamicMenuItem(PaletteMenuPath)]
        public static void Open()
        {
            CommandPaletteWindow window = CreateInstance<CommandPaletteWindow>();

            window.titleContent = new GUIContent(WindowTitle);
            window.ShowUtility();

            // Utility windows pick their own spot when shown, so the placement is applied after.
            window.position = CenteredRect();
            window.Focus();
        }

        private static Rect CenteredRect()
        {
            Rect main = EditorGUIUtility.GetMainWindowPosition();

            return new Rect(main.x + (main.width - WindowWidth) * 0.5f,
                main.y + (main.height - WindowHeight) * VerticalBias, WindowWidth, WindowHeight);
        }

        private void ApplyFocus()
        {
            if (!_focusRequested)
                return;

            EditorGUI.FocusTextInControl(_tags.IsActive
                ? CommandTagEditor.ControlName
                : SearchControlName);

            if (Event.current.type == EventType.Repaint)
                _focusRequested = false;
        }

        private void CloseNow()
        {
            _closing = true;
            Close();
        }

        private void DrawBackground()
        {
            if (Event.current.type != EventType.Repaint)
                return;

            EditorGUI.DrawRect(new Rect(0f, 0f, position.width, position.height),
                CommandPaletteStyles.BackgroundColor());
        }

        private void DrawFooter(Rect row)
        {
            if (_tags.IsActive)
            {
                CommandPaletteFooter.DrawText(row, CommandTagEditor.Hint());
                return;
            }

            CommandPaletteFooter.Draw(row, _status
                ?? string.Format(ResultFormat, _list.Count,
                    CommandIndex.Entries.Count));
        }

        private void DrawSearchBox(Rect box)
        {
            CommandPaletteChrome.DrawFill(box, CommandPaletteStyles.FieldColor(), CommandPaletteStyles.CornerRadius);
            CommandPaletteChrome.DrawBorder(box, CommandPaletteStyles.BorderColor(),
                CommandPaletteStyles.CornerRadius, CommandPaletteStyles.BorderWidth);

            Rect inner = CommandPaletteChrome.Inset(box, CommandPaletteStyles.RowInset);
            float iconSize = CommandPaletteStyles.SearchIconSize;
            Rect icon = new(inner.x, inner.y + (inner.height - iconSize) * 0.5f, iconSize, iconSize);

            if (Event.current.type == EventType.Repaint)
                GUI.DrawTexture(icon, EditorGUIUtility.FindTexture(SearchIconName), ScaleMode.ScaleToFit);

            Rect field = new(icon.xMax + CommandPaletteStyles.Gap, inner.y,
                inner.xMax - icon.xMax - CommandPaletteStyles.Gap, inner.height);

            EditorGUI.BeginChangeCheck();

            GUI.SetNextControlName(SearchControlName);
            _search = EditorGUI.TextField(field, _search, CommandPaletteStyles.SearchField);

            if (EditorGUI.EndChangeCheck())
            {
                _list.Reset();
                RunQuery();
            }

            if (_search.Length == 0)
                GUI.Label(field, Placeholder, CommandPaletteStyles.Placeholder);
        }

        private void DrawSearchRow(Rect row)
        {
            float gap = CommandPaletteStyles.Gap;
            float pillHeight = CommandPaletteStyles.PillHeight;
            float pillY = row.y + (row.height - pillHeight) * 0.5f;

            Rect rescan = new(row.xMax - PillWidth, pillY, PillWidth, pillHeight);
            Rect project = new(rescan.x - gap - PillWidth, pillY, PillWidth, pillHeight);
            Rect box = new(row.x, row.y, project.x - gap - row.x, row.height);

            if (_tags.IsActive)
                _tags.Draw(box);
            else
                DrawSearchBox(box);

            if (CommandPaletteChrome.DrawPill(project, ProjectContent, _projectOnly))
            {
                _projectOnly = !_projectOnly;

                _list.Reset();
                RunQuery();
            }

            if (CommandPaletteChrome.DrawPill(rescan, RescanContent, false))
                Handle(ECommandPaletteAction.Rescan);
        }

        private void Execute()
        {
            if (!_list.HasSelection)
                return;

            CommandEntry entry = _list.Selected;

            CommandUsageStore.instance.Register(entry.Id);
            CloseNow();

            // The command can open its own window or dialog, so let the palette disappear first.
            EditorApplication.delayCall += entry.Execute;
        }

        // Running a command, opening a script and closing all destroy this window, so they wait
        // until the drawing of the current pass is done.
        private void Finish()
        {
            ECommandPaletteAction pending = _pending;
            _pending = ECommandPaletteAction.None;

            switch (pending)
            {
                case ECommandPaletteAction.Close:
                    CloseNow();
                    break;

                case ECommandPaletteAction.OpenScript:
                    OpenScript();
                    break;

                case ECommandPaletteAction.Run:
                    Execute();
                    break;
            }
        }

        private void Handle(ECommandPaletteAction action)
        {
            if (action == ECommandPaletteAction.None)
                return;

            if (action != ECommandPaletteAction.Rescan)
                _status = null;

            switch (action)
            {
                case ECommandPaletteAction.Close:
                case ECommandPaletteAction.OpenScript:
                case ECommandPaletteAction.Run:
                    _pending = action;

                    // A context menu pick lands outside the GUI pass, so ask for one more.
                    Repaint();
                    break;

                case ECommandPaletteAction.EditTags:
                    StartTagEdit();
                    break;

                case ECommandPaletteAction.MoveDown:
                case ECommandPaletteAction.MoveUp:
                case ECommandPaletteAction.PageDown:
                case ECommandPaletteAction.PageUp:
                    _list.Move(action);
                    Repaint();
                    break;

                case ECommandPaletteAction.Rescan:
                    Rescan();
                    break;

                case ECommandPaletteAction.ShowMenu:
                    ShowMenu();
                    break;

                case ECommandPaletteAction.TogglePin:
                    TogglePin();
                    break;
            }
        }

        private void HandleKeyboard()
        {
            Event current = Event.current;

            if (_tags.IsActive)
            {
                HandleTagKeyboard(current);
                return;
            }

            ECommandPaletteAction action = CommandPaletteInput.Read(current);

            if (action == ECommandPaletteAction.None)
                return;

            current.Use();
            Handle(action);
        }

        private void HandleTagKeyboard(Event current)
        {
            if (CommandPaletteInput.IsCancel(current))
            {
                _tags.Cancel();
                _focusRequested = true;

                current.Use();
                return;
            }

            if (!CommandPaletteInput.IsSubmit(current))
                return;

            _tags.Commit();
            _focusRequested = true;

            current.Use();
            RunQuery();
        }

        private void OpenScript()
        {
            if (!_list.HasSelection)
                return;

            CommandScriptOpener.Open(_list.Selected);
            CloseNow();
        }

        private void Rescan()
        {
            CommandIndex.Invalidate();

            _list.Reset();
            RunQuery();

            _status = string.Format(RescanFormat, CommandIndex.Entries.Count);
        }

        private void RunQuery()
        {
            CommandFilter filter = CommandFilter.Parse(_search);

            _term = filter.Term;
            _needsQuery = false;

            _list.Fill(CommandIndex.Entries, filter, _projectOnly);

            Repaint();
        }

        private void ShowMenu()
        {
            if (!_list.HasSelection)
                return;

            _menuOpen = true;

            CommandRowMenu.Show(_list.Selected, Handle);
        }

        private void StartTagEdit()
        {
            if (!_list.HasSelection)
                return;

            _tags.Begin(_list.Selected);
            _focusRequested = true;

            Focus();
            Repaint();
        }

        private void TogglePin()
        {
            if (!_list.HasSelection)
                return;

            CommandTagStore.instance.TogglePinned(_list.Selected.Id);

            RunQuery();
        }
    }
}