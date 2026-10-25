using System.Collections.Generic;
using System.Linq;
using Base.AttributesPackage;
using Base.CorePackage.MenuManaging;
using Base.UtilityPackage.Pooling;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Base.CoreDebugPackage.DebugMenu.CheatConsole
{
    /// <summary>
    /// Unity view implementation for the cheat console.
    /// </summary>
    public sealed class CheatConsoleView : Menu
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private const string CommandPrefix = "\u203A ";
        private const string ErrorPrefix = "[Error] ";
        private const string WarningPrefix = "[Warning] ";
        private const char WordSeparator = '.';

        private readonly List<TMP_Text> _activeSuggestions = new();

        [field: Header("Input")]

        [field: Required] [field: SerializeField] public TMP_InputField InputField { get; private set; }

        [Title("Log")]
        [Required] [SerializeField] private TMP_Text logText;
        [Required] [SerializeField] private ScrollRect scrollRect;

        [Title("Suggestions")]
        [Required] [SerializeField] private TMP_Text suggestionPrefab;
        [Required] [SerializeField] private Transform suggestionParent;
        [Min(1)] [SerializeField] private int maxSuggestions = 5;

        [Title("Colors")]
        [SerializeField] private Color infoColor = new(0.898f, 0.898f, 0.918f);
        [SerializeField] private Color warningColor = new(1f, 0.624f, 0.04f);
        [SerializeField] private Color errorColor = new(1f, 0.271f, 0.227f);
        [SerializeField] private Color commandColor = new(0.188f, 0.82f, 0.345f);

        private HashSetObjectPool<TMP_Text> _suggestionPool;

#region Unity Callbacks
        protected override void Awake()
        {
            base.Awake();

            _suggestionPool = new HashSetObjectPool<TMP_Text>(suggestionPrefab, suggestionParent, ResetSuggestion);
            InputField.onValidateInput = SkipShortcutLetter;
        }
#endregion

        protected override void OnOpened()
        {
            base.OnOpened();

            FocusInput();
        }

        protected override void OnClosed()
        {
            base.OnClosed();

            SetInputText(string.Empty);
        }

        /// <summary>
        /// Sets the text of the input field.
        /// </summary>
        /// <param name="text">The text to show in the input field.</param>
        public void SetInputText(string text)
        {
            InputField.text = text;
            InputField.caretPosition = InputField.text.Length;
        }

        /// <summary>
        /// Gets the current text of the input field.
        /// </summary>
        /// <returns>The text currently typed into the input field.</returns>
        public string GetInputText() => InputField.text;

        /// <summary>
        /// Appends a message line to the console log.
        /// </summary>
        /// <param name="message">The message to append.</param>
        /// <param name="messageType">The severity the message is colored and prefixed with.</param>
        public void AppendLog(string message, ECheatConsoleMessageType messageType)
        {
            string prefix = GetPrefix(messageType);
            string colored = $"<color=#{ColorUtility.ToHtmlStringRGB(GetColor(messageType))}>{prefix}{message}</color>";

            if (logText.text.Length == 0)
                logText.text = colored;
            else
                logText.text += "\n" + colored;

            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }

        /// <summary>
        /// Focuses the input field for user typing.
        /// </summary>
        public void FocusInput()
        {
            InputField.ActivateInputField();
            InputField.Select();
        }

        /// <summary>
        /// Deletes the word before the caret and the spaces after it, like Ctrl+W in a shell. A dot counts as a word of
        /// its own, so "date.affection.add" goes back one step at a time: "date.affection.", "date.affection", "date.".
        /// </summary>
        public void DeleteWordBeforeCaret() => DeleteBeforeCaret(FindWordStart(InputField.text,
            InputField.stringPosition));

        /// <summary>
        /// Deletes everything before the caret, like Ctrl+U in a shell.
        /// </summary>
        public void DeleteLineBeforeCaret() => DeleteBeforeCaret(0);

        /// <summary>
        /// Shows a list of suggestion texts below the input field.
        /// </summary>
        /// <param name="suggestions">The suggestions to show, capped by the configured maximum.</param>
        public void ShowSuggestions(List<string> suggestions)
        {
            foreach (TMP_Text suggestion in _activeSuggestions)
                _suggestionPool.Release(suggestion);

            _activeSuggestions.Clear();

            // An empty box would sit above the input as a bare bar.
            bool hasSuggestions = suggestions != null
                && suggestions.Count > 0;
            suggestionParent.gameObject.SetActive(hasSuggestions);

            if (!hasSuggestions)
                return;

            int count = Mathf.Min(maxSuggestions, suggestions.Count);
            for (int i = 0; i < count; i++)
            {
                TMP_Text item = _suggestionPool.Get();
                item.text = suggestions[i];
                item.gameObject.SetActive(true);
                item.transform.SetSiblingIndex(i);
                _activeSuggestions.Add(item);
            }
        }

        /// <summary>
        /// Gets the currently shown suggestion texts.
        /// </summary>
        /// <returns>The text of every visible suggestion.</returns>
        public List<string> GetCurrentSuggestions()
            => _activeSuggestions.Select(suggestion => suggestion.text).ToList();

        /// <summary>
        /// Clears the console log and scrolls back to the top.
        /// </summary>
        public void ClearLog()
        {
            logText.text = string.Empty;
            scrollRect.verticalNormalizedPosition = 1f;
        }

        private static string GetPrefix(ECheatConsoleMessageType messageType) => messageType switch
        {
            ECheatConsoleMessageType.Error => ErrorPrefix,
            ECheatConsoleMessageType.Warning => WarningPrefix,
            ECheatConsoleMessageType.Command => CommandPrefix,
            _ => string.Empty
        };

        // Some platforms send the letter of Ctrl+W or Ctrl+U along, which the field would type. Only those two keys
        // count, so Ctrl+V still pastes and AltGr, which is Ctrl+Alt on Windows, still types characters like @.
        private static char SkipShortcutLetter(string text, int index, char addedChar) => IsShortcutHeld()
            ? '\0'
            : addedChar;

        private static bool IsShortcutHeld()
        {
            Keyboard keyboard = Keyboard.current;

            return keyboard != null
                && keyboard.ctrlKey.isPressed
                && !keyboard.altKey.isPressed
                && (keyboard.wKey.isPressed || keyboard.uKey.isPressed);
        }

        // Like a shell: first the spaces right before the caret, then the word in front of them, where a dot is a word
        // of its own.
        private static int FindWordStart(string text, int caret)
        {
            int start = caret;

            while (start > 0
                && char.IsWhiteSpace(text[start - 1]))
                start--;

            if (start > 0
                && text[start - 1] == WordSeparator)
                return start - 1;

            while (start > 0
                && !char.IsWhiteSpace(text[start - 1])
                && text[start - 1] != WordSeparator)
                start--;

            return start;
        }

        private void DeleteBeforeCaret(int start)
        {
            int caret = InputField.stringPosition;

            if (start >= caret)
                return;

            InputField.text = InputField.text.Remove(start, caret - start);
            InputField.stringPosition = start;
        }

        private void ResetSuggestion(TMP_Text text)
        {
            // Pooled items are destroyed with the scene, so a missing one is a normal shutdown state.
            if (text == null)
                return;

            text.text = string.Empty;
            text.gameObject.SetActive(false);
            text.transform.SetParent(suggestionParent, false);
        }

        private Color GetColor(ECheatConsoleMessageType messageType) => messageType switch
        {
            ECheatConsoleMessageType.Error => errorColor,
            ECheatConsoleMessageType.Warning => warningColor,
            ECheatConsoleMessageType.Command => commandColor,
            _ => infoColor
        };
#endif
    }
}