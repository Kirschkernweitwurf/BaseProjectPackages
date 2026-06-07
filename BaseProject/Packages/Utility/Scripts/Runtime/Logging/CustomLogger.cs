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
        /// <summary>
        /// Logs a message to the Unity Console.
        /// </summary>
        /// <param name="message">The message to log (can include styled text).</param>
        /// <param name="context">Optional Unity object that the log refers to.</param>
        /// <param name="filePath">Automatically filled by compiler to detect the calling class name.</param>
        public static void Log(string message, Object context, [CallerFilePath] string filePath = "")
        {
            string className = Path.GetFileNameWithoutExtension(filePath);
            Debug.Log(FormatMessage(message, className), context);
        }

        /// <summary>
        /// Logs a warning message to the Unity Console.
        /// </summary>
        /// <param name="message">The warning message to log (can include styled text).</param>
        /// <param name="context">Optional Unity object that the log refers to.</param>
        /// <param name="filePath">Automatically filled by compiler to detect the calling class name.</param>
        public static void LogWarning(string message, Object context, [CallerFilePath] string filePath = "")
        {
            string className = Path.GetFileNameWithoutExtension(filePath);
            Debug.LogWarning(FormatMessage(message, className), context);
        }

        /// <summary>
        /// Logs an error message to the Unity Console.
        /// </summary>
        /// <param name="message">The error message to log (can include styled text).</param>
        /// <param name="context">Optional Unity object that the log refers to.</param>
        /// <param name="filePath">Automatically filled by compiler to detect the calling class name.</param>
        public static void LogError(string message, Object context, [CallerFilePath] string filePath = "")
        {
            string className = Path.GetFileNameWithoutExtension(filePath);
            Debug.LogError(FormatMessage(message, className), context);
        }

        private static string FormatMessage(string message, string className)
        {
            string editorMarker = Platform.IsEditorMode() ? LogTextFormatter.EditorMarker : string.Empty;
            string color = CustomLoggingUtils.GetColor(className);
            return $"{editorMarker}<color={color}><b>[{className}]</b></color> {message}";
        }
    }
}