using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Base.UtilityPackage.Logging
{
    /// <summary>
    /// A custom logger for Unity that prefixes logs with the calling class name.
    /// </summary>
    /// <remarks>
    /// Can be used with <see cref="LogTextFormatter"/> for styled log messages.
    /// </remarks>
    public static class CustomLogger
    {
        // Caller file paths are compile-time constants per call site, so the styled prefix for each
        // class is built once and reused, avoiding per-log path parsing and color hashing.
        private static readonly Dictionary<string, string> PrefixCache = new();

        /// <summary>
        /// Logs a message to the Unity Console.
        /// </summary>
        /// <param name="message">The message to log (can include styled text).</param>
        /// <param name="context">Optional Unity object that the log refers to.</param>
        /// <param name="filePath">Automatically filled by compiler to detect the calling class name.</param>
        public static void Log(string message, Object context, [CallerFilePath] string filePath = "")
            => Debug.Log(FormatMessage(message, filePath), context);

        /// <summary>
        /// Logs a warning message to the Unity Console.
        /// </summary>
        /// <param name="message">The warning message to log (can include styled text).</param>
        /// <param name="context">Optional Unity object that the log refers to.</param>
        /// <param name="filePath">Automatically filled by compiler to detect the calling class name.</param>
        public static void LogWarning(string message, Object context, [CallerFilePath] string filePath = "")
            => Debug.LogWarning(FormatMessage(message, filePath), context);

        /// <summary>
        /// Logs an error message to the Unity Console.
        /// </summary>
        /// <param name="message">The error message to log (can include styled text).</param>
        /// <param name="context">Optional Unity object that the log refers to.</param>
        /// <param name="filePath">Automatically filled by compiler to detect the calling class name.</param>
        public static void LogError(string message, Object context, [CallerFilePath] string filePath = "")
            => Debug.LogError(FormatMessage(message, filePath), context);

        private static string FormatMessage(string message, string filePath)
        {
            string editorMarker = Platform.IsEditorMode()
                ? LogTextFormatter.EditorMarker
                : string.Empty;

            return $"{editorMarker}{GetPrefix(filePath)} {message}";
        }

        private static string GetPrefix(string filePath)
        {
            if (PrefixCache.TryGetValue(filePath, out string prefix))
                return prefix;

            string className = Path.GetFileNameWithoutExtension(filePath);
            string color = CustomLoggingUtils.GetColor(className);
            prefix = $"<color={color}><b>[{className}]</b></color>";
            PrefixCache[filePath] = prefix;
            return prefix;
        }
    }
}
