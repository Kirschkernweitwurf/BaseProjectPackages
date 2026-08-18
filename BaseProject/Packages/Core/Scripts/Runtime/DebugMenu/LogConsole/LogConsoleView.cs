using System;
using System.Collections.Generic;
using System.Globalization;
using Base.AttributePackage;
using Base.CorePackage.MenuManaging;
using Base.UtilityPackage.Logging;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Base.CorePackage.DebugMenu.LogConsole
{
    /// <summary>
    /// Menu that mirrors Unity's log stream, including <see cref="CustomLogger"/> output. Capturing
    /// starts before the first scene loads, so every log is buffered even while the menu is closed.
    /// Each entry is prefixed with a timestamp: the timestamp is tinted for warnings and errors,
    /// while the message itself always keeps the default color.
    /// </summary>
    public sealed class LogConsoleView : Menu
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private const string DefaultColor = "#FFFFFF";
        private const string ErrorColor = "#FF4040";
        private const int MaxLines = 200;
        private const string TimestampFormat = "HH:mm:ss";
        private const string WarningColor = "#FF9933";

        private static readonly Queue<string> BufferedLines = new();

        private static event Action LinesChanged;

        [Title("Log")]
        [Required] [SerializeField] private TMP_Text logText;
        [Required] [SerializeField] private ScrollRect scrollRect;
        [Required] [SerializeField] private Button clearButton;

#region Unity Callbacks
        protected override void Awake()
        {
            base.Awake();

            LinesChanged += OnLinesChanged;
            clearButton.onClick.AddListener(ClearLog);
        }

        protected override void OnDestroy()
        {
            LinesChanged -= OnLinesChanged;
            clearButton.onClick.RemoveListener(ClearLog);

            base.OnDestroy();
        }
#endregion

        protected override void OnOpened()
        {
            base.OnOpened();

            Redraw();
        }

        // Capturing has to start before any menu exists, otherwise early logs would be lost.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeCapture()
        {
            BufferedLines.Clear();
            Application.logMessageReceived -= Capture;
            Application.logMessageReceived += Capture;
        }

        private static void Capture(string message, string stackTrace, LogType type)
        {
            string timestampColor = type switch
            {
                LogType.Warning => WarningColor,
                LogType.Error or LogType.Exception or LogType.Assert => ErrorColor,
                _ => DefaultColor
            };

            string timestamp = DateTime.Now.ToString(TimestampFormat, CultureInfo.InvariantCulture);
            string line = $"{LogTextFormatter.Colorize($"[{timestamp}]", timestampColor)} "
                + LogTextFormatter.Colorize(message, DefaultColor);

            BufferedLines.Enqueue(line);
            while (BufferedLines.Count > MaxLines)
                BufferedLines.Dequeue();

            LinesChanged?.Invoke();
        }

        private void ClearLog()
        {
            BufferedLines.Clear();

            logText.text = string.Empty;
            scrollRect.verticalNormalizedPosition = 1f;
        }

        private void OnLinesChanged()
        {
            if (IsOpen)
                Redraw();
        }

        private void Redraw()
        {
            logText.text = string.Join("\n", BufferedLines);

            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }
#endif
    }
}