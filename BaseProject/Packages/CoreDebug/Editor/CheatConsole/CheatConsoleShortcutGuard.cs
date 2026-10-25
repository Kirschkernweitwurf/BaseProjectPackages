using System.Reflection;
using Base.CoreDebugPackage.DebugMenu.CheatConsole;
using UnityEditor;
using UnityEngine;

namespace Base.CoreDebugPackage.Editor.CheatConsole
{
    /// <summary>
    /// Keeps Unity's main menu shortcuts from firing while someone types in the <see cref="CheatConsoleView"/> during
    /// play mode, so Ctrl+U deletes in the console instead of opening Undo History. It switches on the Game view's own
    /// option to ignore those shortcuts while the console input has focus, and puts the user's choice back afterwards.
    /// </summary>
    /// <remarks>
    /// The option has no public API, so it is reached by reflection. Should a Unity version drop it, the guard does
    /// nothing and the shortcuts behave as before.
    /// </remarks>
    [InitializeOnLoad]
    internal static class CheatConsoleShortcutGuard
    {
        private const string IgnoreProperty = "ignoreWhenPlayModeFocused";
        private const string IntegrationType = "UnityEditor.ShortcutManagement.ShortcutIntegration";

        private static readonly PropertyInfo IgnoreWhenPlayModeFocused = typeof(EditorWindow).Assembly
            .GetType(IntegrationType)
            ?.GetProperty(IgnoreProperty, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

        private static CheatConsoleView _view;
        private static bool _isGuarding;
        private static bool _wasIgnoring;

        static CheatConsoleShortcutGuard()
        {
            if (IgnoreWhenPlayModeFocused == null)
                return;

            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

            // A domain reload during play mode skips the play mode event, so the guard picks up here.
            if (EditorApplication.isPlaying)
                Watch();
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.EnteredPlayMode)
            {
                Watch();
                return;
            }

            if (change != PlayModeStateChange.ExitingPlayMode)
                return;

            EditorApplication.update -= OnUpdate;
            SetGuarding(false);
            _view = null;
        }

        private static void Watch()
        {
            EditorApplication.update -= OnUpdate;
            EditorApplication.update += OnUpdate;
        }

        // Leaving play mode can happen inside this update, after the restore, so a closing play mode never guards.
        private static void OnUpdate() => SetGuarding(EditorApplication.isPlayingOrWillChangePlaymode
            && IsTyping());

        private static bool IsTyping()
        {
            if (_view == null)
                _view = Object.FindAnyObjectByType<CheatConsoleView>();

            return _view != null
                && _view.InputField.isFocused;
        }

        private static void SetGuarding(bool isGuarding)
        {
            if (isGuarding == _isGuarding)
                return;

            _isGuarding = isGuarding;

            if (isGuarding)
                _wasIgnoring = (bool)IgnoreWhenPlayModeFocused.GetValue(null);

            IgnoreWhenPlayModeFocused.SetValue(null, isGuarding || _wasIgnoring);
        }
    }
}