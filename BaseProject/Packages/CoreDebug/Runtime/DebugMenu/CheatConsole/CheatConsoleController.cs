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
        private const string ReloadedFormat = "[Scene] Cheat commands reloaded ({0} found).";

        // Text editing keys belong to the console alone, so they live here instead of in the shared input asset.
        private readonly InputAction _deleteWord = CreateShortcut(DeleteWordKeyPath);
        private readonly InputAction _deleteLine = CreateShortcut(DeleteLineKeyPath);

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
            CheatMacroRunner.Register(_model);
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
            string completed = _model.Complete(view.GetInputText());

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