using System.Collections.Generic;
using System.Linq;
using Base.AttributePackage;
using Base.CorePackage.MenuManaging;
using Base.CorePackage.ObjectPooling;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Base.CorePackage.DebugMenu.CheatConsole
{
    /// <summary>
    /// Unity view implementation for the cheat console.
    /// </summary>
    public sealed class CheatConsoleView : Menu
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private const string CommandPrefix = "> ";
        private const string ErrorPrefix = "[Error] ";
        private const string WarningPrefix = "[Warning] ";

        private readonly List<TMP_Text> _activeSuggestions = new();

        [field: Header("Input")]

        [field: Required] [field: SerializeField] public TMP_InputField InputField { get; private set; }

        [Header("Log")]

        [Required] [SerializeField] private TMP_Text logText;
        [Required] [SerializeField] private ScrollRect scrollRect;

        [Header("Suggestions")]

        [Required] [SerializeField] private TMP_Text suggestionPrefab;
        [Required] [SerializeField] private Transform suggestionParent;
        [Min(1)] [SerializeField] private int maxSuggestions = 5;

        [Header("Colors")]

        [SerializeField] private Color infoColor = Color.white;
        [SerializeField] private Color warningColor = new(1f, 0.6f, 0.2f);
        [SerializeField] private Color errorColor = Color.red;
        [SerializeField] private Color commandColor = new(0.5f, 1f, 0.5f);

        private HashSetObjectPool<TMP_Text> _suggestionPool;

#region Unity Callbacks
        protected override void Awake()
        {
            base.Awake();

            _suggestionPool = new HashSetObjectPool<TMP_Text>(suggestionPrefab, suggestionParent, ResetSuggestion);
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
        /// Shows a list of suggestion texts below the input field.
        /// </summary>
        /// <param name="suggestions">The suggestions to show, capped by the configured maximum.</param>
        public void ShowSuggestions(List<string> suggestions)
        {
            foreach (TMP_Text suggestion in _activeSuggestions)
                _suggestionPool.Release(suggestion);

            _activeSuggestions.Clear();

            if (suggestions == null || suggestions.Count == 0)
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