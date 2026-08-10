using System;
using System.Collections.Generic;
using Base.AttributePackage;
using Base.CorePackage.Input;
using Base.CorePackage.Services;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Base.CorePackage.DebugMenu.CheatConsole
{
    /// <summary>
    /// Controller for the cheat console. Subscribes to input actions, coordinates the
    /// model and view, and handles command execution and navigation.
    /// </summary>
    [RequireComponent(typeof(CheatConsoleView))]
    public sealed class CheatConsoleController : MonoBehaviour
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private const string ReloadedFormat = "[Scene] Cheat commands reloaded ({0} found).";

        [GetComponent] [SerializeField] private CheatConsoleView view;

        private CheatConsoleModel _model;

#region Unity Callbacks
        private void Awake()
        {
            RebuildCommands();

            SceneManager.sceneLoaded += OnSceneLoaded;
            view.InputField.onValueChanged.AddListener(OnInputChanged);
        }

        private void OnEnable()
        {
            if (!ServiceLocator.TryGet(out InputManager inputManager))
                return;

            inputManager.BaseInputActions.Cheats.ExecuteCommand.started += OnExecuteCommand;
            inputManager.BaseInputActions.Cheats.AutoComplete.started += OnAutoComplete;
            inputManager.BaseInputActions.Cheats.PreviousCommand.started += OnPreviousCommand;
            inputManager.BaseInputActions.Cheats.NextCommand.started += OnNextCommand;
        }

        private void OnDisable()
        {
            if (!ServiceLocator.TryGet(out InputManager inputManager))
                return;

            inputManager.BaseInputActions.Cheats.ExecuteCommand.started -= OnExecuteCommand;
            inputManager.BaseInputActions.Cheats.AutoComplete.started -= OnAutoComplete;
            inputManager.BaseInputActions.Cheats.PreviousCommand.started -= OnPreviousCommand;
            inputManager.BaseInputActions.Cheats.NextCommand.started -= OnNextCommand;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            view.InputField.onValueChanged.RemoveListener(OnInputChanged);
        }
#endregion

        // A new scene brings new MonoBehaviours, so instance commands have to be discovered again.
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            RebuildCommands();

            view.AppendLog(string.Format(ReloadedFormat, _model.Commands.Count), ECheatConsoleMessageType.Info);
        }

        private void RebuildCommands()
        {
            _model = new CheatConsoleModel(CheatCommandProvider.DiscoverAllCommands());
            BuiltinCheatCommands.Register(_model, view);
        }

        private void OnExecuteCommand(InputAction.CallbackContext _)
        {
            string input = view.GetInputText();
            if (string.IsNullOrWhiteSpace(input))
                return;

            view.AppendLog(input, ECheatConsoleMessageType.Command);

            CheatConsoleResult result = _model.Execute(input);
            view.AppendLog(result.Message, result.MessageType);

            view.SetInputText(string.Empty);
            view.FocusInput();
        }

        private void OnAutoComplete(InputAction.CallbackContext _)
        {
            string current = view.GetInputText();

            List<string> suggestions = view.GetCurrentSuggestions();
            if (suggestions.Count == 0)
                suggestions = _model.GetSuggestions(current);

            string completed = current;
            if (suggestions.Count > 0
                && !string.Equals(current, suggestions[0], StringComparison.OrdinalIgnoreCase))
                completed = suggestions[0];

            view.SetInputText(completed);
            view.FocusInput();
            view.ShowSuggestions(_model.GetSuggestions(completed));
        }

        private void OnPreviousCommand(InputAction.CallbackContext _)
        {
            string previous = _model.GetPreviousHistory();
            if (previous == null)
                return;

            view.SetInputText(previous);
            view.FocusInput();
        }

        private void OnNextCommand(InputAction.CallbackContext _)
        {
            string next = _model.GetNextHistory();
            if (next == null)
                return;

            view.SetInputText(next);
            view.FocusInput();
        }

        private void OnInputChanged(string newText) => view.ShowSuggestions(_model.GetSuggestions(newText));
#endif
    }
}