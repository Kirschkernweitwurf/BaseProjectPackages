using System.Collections.Generic;
using System.Linq;
using Systems.MenuManaging;
using Systems.ObjectPooling;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utility.Logging;

namespace Systems.CheatConsole
{
    /// <summary>
    /// Unity view implementation for the cheat console.
    /// </summary>
    public sealed class CheatConsoleView : Menu
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        [field: Header("Input")]
        [field: SerializeField] public TMP_InputField InputField { get; private set; }

        [Header("Log")]
        [SerializeField] private TMP_Text logText;
        [SerializeField] private ScrollRect scrollRect;

        [Header("Suggestions")]
        [SerializeField] private TMP_Text suggestionPrefab;
        [SerializeField] private Transform suggestionParent;
        [SerializeField] private int maxSuggestions = 5;

        [Header("Colors")]
        [SerializeField] private Color infoColor = Color.white;
        [SerializeField] private Color warningColor = new(1f, 0.6f, 0.2f);
        [SerializeField] private Color errorColor = Color.red;
        [SerializeField] private Color commandColor = new(0.5f, 1f, 0.5f);

        private readonly List<TMP_Text> _activeSuggestions = new();
        private HashSetObjectPool<TMP_Text> _suggestionPool;

        protected override void Awake()
        {
            base.Awake();

            _suggestionPool = new HashSetObjectPool<TMP_Text>(suggestionPrefab, suggestionParent, ResetSuggestion);
        }

        /// <summary>
        /// Sets the text of the input field.
        /// </summary>
        public void SetInputText(string text)
        {
            InputField.text = text;
            InputField.caretPosition = InputField.text.Length;
        }

        /// <summary>
        /// Gets the current text of the input field.
        /// </summary>
        public string GetInputText() => InputField.text;

        /// <summary>
        /// Appends a message line to the console log.
        /// </summary>
        public void AppendLog(string message, CheatConsoleMessageType messageType)
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
        public void ShowSuggestions(List<string> suggestions)
        {
            foreach (TMP_Text s in _activeSuggestions)
                _suggestionPool.Release(s);

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
        /// Gets the current suggestion texts.
        /// </summary>
        public List<string> GetCurrentSuggestions() => _activeSuggestions.Select(s => s.text).ToList();

        public void ClearLog()
        {
            logText.text = string.Empty;
            scrollRect.verticalNormalizedPosition = 1f;
        }

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

        private static string GetPrefix(CheatConsoleMessageType type)
        {
            return type switch
            {
                CheatConsoleMessageType.Error => "[Error] ",
                CheatConsoleMessageType.Warning => "[Warning] ",
                CheatConsoleMessageType.Command => "> ",
                _ => string.Empty
            };
        }

        private void ResetSuggestion(TMP_Text text)
        {
            if (text == null)
            {
                CustomLogger.LogWarning("Suggestion text is null. Cannot reset.", this);
                return;
            }

            text.text = string.Empty;
            text.gameObject.SetActive(false);
            text.transform.SetParent(suggestionParent, false);
        }

        private Color GetColor(CheatConsoleMessageType type)
        {
            return type switch
            {
                CheatConsoleMessageType.Error => errorColor,
                CheatConsoleMessageType.Warning => warningColor,
                CheatConsoleMessageType.Command => commandColor,
                _ => infoColor
            };
        }
#endif
    }
}