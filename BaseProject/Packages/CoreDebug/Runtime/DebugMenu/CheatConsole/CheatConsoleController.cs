using System;
using System.Collections.Generic;
using Base.AttributesPackage;
using Base.CorePackage.Input;
using Base.ServicesPackage;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Base.CoreDebugPackage.DebugMenu.CheatConsole
{
    /// <summary>
    /// Controller for the cheat console. Subscribes to input actions, coordinates the
    /// model and view, and handles command execution and navigation. Ctrl+W and Ctrl+U edit the input like in a
    /// shell: they delete the word or everything before the caret.
    /// </summary>
    /// <remarks>
    /// Commands on scene objects are discovered when the console opens, and on a scene load only while it is open.
    /// Discovering on every scene load stalled each load, although the console is closed nearly all the time.
    /// </remarks>
    [RequireComponent(typeof(CheatConsoleView))]
    public sealed class CheatConsoleController : MonoBehaviour
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private const string BindingPart = "Binding";
        private const string CtrlPath = "<Keyboard>/ctrl";
        private const string DeleteLineKeyPath = "<Keyboard>/u";
        private const string DeleteWordKeyPath = "<Keyboard>/w";
        private const string ModifierComposite = "OneModifier";
        private const string ModifierPart = "Modifier";
        private const string DescriptionFormat = "{0}  <color=#8E8E93>{1}</color>";

        [GetComponent] [SerializeField] private CheatConsoleView view;

        private CheatConsoleModel _model;

        // Text editing keys belong to the console alone, so they live here instead of in the shared input asset.
        private InputAction _deleteWord;
        private InputAction _deleteLine;

#region Unity Callbacks
        // The shortcuts are built here, not in field initializers: those run in the constructor, where the Input
        // System is not ready and throws.
        private void Awake()
        {
            _deleteWord = CreateShortcut(DeleteWordKeyPath);
            _deleteLine = CreateShortcut(DeleteLineKeyPath);
            CreateModel();

            SceneManager.sceneLoaded += OnSceneLoaded;
            view.InputField.onValueChanged.AddListener(OnInputChanged);
        }

        private void OnEnable()
        {
            view.Opened += OnOpened;
            _deleteWord.started += OnDeleteWord;
            _deleteLine.started += OnDeleteLine;
            _deleteWord.Enable();
            _deleteLine.Enable();

            if (!ServiceLocator.TryGet(out InputManager inputManager))
                return;

            inputManager.BaseInputActions.Cheats.ExecuteCommand.started += OnExecuteCommand;
            inputManager.BaseInputActions.Cheats.AutoComplete.started += OnAutoComplete;
            inputManager.BaseInputActions.Cheats.PreviousCommand.started += OnPreviousCommand;
            inputManager.BaseInputActions.Cheats.NextCommand.started += OnNextCommand;
        }

        private void OnDisable()
        {
            view.Opened -= OnOpened;
            _deleteWord.started -= OnDeleteWord;
            _deleteLine.started -= OnDeleteLine;
            _deleteWord.Disable();
            _deleteLine.Disable();

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
            _deleteWord.Dispose();
            _deleteLine.Dispose();
        }
#endregion

        private static InputAction CreateShortcut(string keyPath)
        {
            InputAction action = new(type: InputActionType.Button);
            action.AddCompositeBinding(ModifierComposite)
                .With(ModifierPart, CtrlPath)
                .With(BindingPart, keyPath);

            return action;
        }

        // The model lives as long as the console, so the history survives scene loads.
        private void CreateModel()
        {
            _model = new CheatConsoleModel(Array.Empty<CheatCommandInfo>());
            BuiltinCheatCommands.Register(_model, view);
            CheatMacroRunner.Register(_model);
        }

        private void OnOpened() => DiscoverCommands();

        // A new scene brings new MonoBehaviours, so an open console has to discover its commands again.
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!view.IsOpen)
                return;

            DiscoverCommands();
        }

        private void DiscoverCommands() => _model.ReplaceDiscoveredCommands(CheatCommandProvider.DiscoverAllCommands());

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
            string completed = _model.Complete(view.GetInputText());

            view.SetInputText(completed);
            view.FocusInput();
            ShowSuggestions(completed);
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

        private void OnInputChanged(string newText) => ShowSuggestions(newText);

        // Each suggestion carries its description in grey, so the list also tells what a command does.
        private void ShowSuggestions(string input)
        {
            List<string> lines = new();

            foreach (string command in _model.GetSuggestions(input))
            {
                string description = _model.Commands[command].Attribute.Description;

                lines.Add(string.IsNullOrEmpty(description)
                    ? command
                    : string.Format(DescriptionFormat, command, description));
            }

            view.ShowSuggestions(lines);
        }

        // The actions run while the console is closed too, so they only edit a focused input.
        private void OnDeleteWord(InputAction.CallbackContext _)
        {
            if (!view.InputField.isFocused)
                return;

            view.DeleteWordBeforeCaret();
        }

        private void OnDeleteLine(InputAction.CallbackContext _)
        {
            if (!view.InputField.isFocused)
                return;

            view.DeleteLineBeforeCaret();
        }
#endif
    }
}