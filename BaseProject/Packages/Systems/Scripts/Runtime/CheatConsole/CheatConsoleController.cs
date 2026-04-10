using System;
using System.Collections.Generic;
using Systems.CheatConsole.Cheats;
using Systems.Input;
using Systems.Services;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Systems.CheatConsole
{
    /// <summary>
    /// Controller for the cheat console. Subscribes to input actions, coordinates the
    /// model and view, and handles command execution and navigation.
    /// </summary>
    [RequireComponent(typeof(CheatConsoleView))]
    public sealed class CheatConsoleController : MonoBehaviour
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private CheatConsoleView _view;
        private CheatConsoleModel _model;

        private void Awake()
        {
            _view = GetComponent<CheatConsoleView>();
            _model = new CheatConsoleModel(CheatCommandProvider.DiscoverAllCommands());
            BuiltinCheatCommands.Register(_model, _view);
            SceneManager.sceneLoaded += OnSceneLoaded;
            _view.InputField.onValueChanged.AddListener(OnInputChanged);
        }

        private void OnEnable()
        {
            if (!ServiceLocator.TryGet(out InputManager inputManager))
                return;

            inputManager.InputActions.Permanent.ToggleCheatConsole.performed += OnToggleConsole;
            inputManager.InputActions.Cheats.ExecuteCommand.started += OnExecuteCommand;
            inputManager.InputActions.Cheats.AutoComplete.started += OnAutoComplete;
            inputManager.InputActions.Cheats.PreviousCommand.started += OnPreviousCommand;
            inputManager.InputActions.Cheats.NextCommand.started += OnNextCommand;
        }

        private void OnDisable()
        {
            if (!ServiceLocator.TryGet(out InputManager inputManager))
                return;

            inputManager.InputActions.Permanent.ToggleCheatConsole.performed -= OnToggleConsole;
            inputManager.InputActions.Cheats.ExecuteCommand.started -= OnExecuteCommand;
            inputManager.InputActions.Cheats.AutoComplete.started -= OnAutoComplete;
            inputManager.InputActions.Cheats.PreviousCommand.started -= OnPreviousCommand;
            inputManager.InputActions.Cheats.NextCommand.started -= OnNextCommand;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            _view.InputField.onValueChanged.RemoveListener(OnInputChanged);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            List<CheatCommandInfo> discovered = CheatCommandProvider.DiscoverAllCommands();
            _model = new CheatConsoleModel(discovered);
            BuiltinCheatCommands.Register(_model, _view);
            _view.AppendLog($"[Scene] Cheat commands reloaded ({_model.Commands.Count} found).",
                CheatConsoleMessageType.Info);
        }

        private void OnExecuteCommand(InputAction.CallbackContext context)
        {
            string input = _view.GetInputText();
            if (string.IsNullOrWhiteSpace(input))
                return;

            _view.AppendLog(input, CheatConsoleMessageType.Command);

            CheatConsoleResult result = _model.Execute(input);
            _view.AppendLog(result.Message, result.MessageType);

            _view.SetInputText(string.Empty);
            _view.FocusInput();
        }

        private void OnAutoComplete(InputAction.CallbackContext context)
        {
            string current = _view.GetInputText();
            List<string> suggestions = _view.GetCurrentSuggestions();
            if (suggestions == null || suggestions.Count == 0)
                suggestions = _model.GetSuggestions(current);

            string completed = current;
            if (suggestions.Count > 0)
            {
                string first = suggestions[0];
                if (!string.Equals(current, first, StringComparison.OrdinalIgnoreCase))
                    completed = first;
            }

            _view.SetInputText(completed);
            _view.FocusInput();
            _view.ShowSuggestions(_model.GetSuggestions(completed));
        }

        private void OnPreviousCommand(InputAction.CallbackContext context)
        {
            string previous = _model.GetPreviousHistory();
            if (previous == null)
                return;

            _view.SetInputText(previous);
            _view.FocusInput();
        }

        private void OnNextCommand(InputAction.CallbackContext context)
        {
            string next = _model.GetNextHistory();
            if (next == null)
                return;

            _view.SetInputText(next);
            _view.FocusInput();
        }

        private void OnInputChanged(string newText)
        {
            if (string.IsNullOrWhiteSpace(newText))
                _view.ShowSuggestions(new List<string>());

            List<string> suggestions = _model.GetSuggestions(newText);
            _view.ShowSuggestions(suggestions);
        }

        private void OnToggleConsole(InputAction.CallbackContext ctx)
        {
            if (!_view.IsOpen)
                _view.Open();
            else
                _view.Close();
        }
#endif
    }
}