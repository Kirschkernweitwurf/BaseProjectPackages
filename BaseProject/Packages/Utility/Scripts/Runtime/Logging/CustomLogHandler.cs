using System;
using System.Diagnostics;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.UtilityPackage.Logging
{
    /// <summary>
    /// A custom log handler that prefixes log messages with the calling
    /// class name and colors them for better readability in the Unity Console.
    /// Edit-mode logs get an extra "edit" marker to distinguish them from play-mode logs.
    /// </summary>
    public class CustomLogHandler : ILogHandler
    {
        private const string UnityNamespacePrefix = "UnityEngine";

        /// <summary>The genuine Unity handler this instance forwards to.</summary>
        public ILogHandler DefaultLogHandler { get; }

        /// <summary>
        /// Caller-name resolution relies on stack trace analysis, which is expensive, so it can be turned off.
        /// </summary>
        /// <param name="defaultLogHandler">The genuine Unity handler to forward to. Must not be a CustomLogHandler.</param>
        public CustomLogHandler(ILogHandler defaultLogHandler) => DefaultLogHandler = defaultLogHandler;

        /// <inheritdoc/>
        public void LogFormat(LogType logType, Object context, string format, params object[] args)
        {
            string prefix = BuildPrefix();
            string message = string.Format(format, args);
            DefaultLogHandler.LogFormat(logType, context, prefix + message);
        }

        /// <inheritdoc/>
        public void LogException(Exception exception, Object context) => DefaultLogHandler.LogException(exception, context);

        private static string BuildPrefix()
        {
            string editorMarker = Platform.IsEditorMode() ? LogTextFormatter.EditorMarker : string.Empty;
            string caller = GetCallerClassName();

            // No caller resolved (e.g. release builds): omit the class prefix entirely.
            if (caller == null)
                return editorMarker;

            string color = GetColor(caller);
            return $"{editorMarker}<color={color}><b>[{caller}]</b></color> ";
        }

        private static string GetColor(string name)
        {
            float hue = (name.GetHashCode() & int.MaxValue) % 360 / 360f;
            Color color = Color.HSVToRGB(hue, 0.5f, 0.9f);
            return $"#{ColorUtility.ToHtmlStringRGB(color)}";
        }

        /// <summary>
        /// Returns the calling class name, or null if it cannot/should not be resolved.
        /// Pass an explicit name to skip the costly stack trace analysis.
        /// </summary>
        private static string GetCallerClassName(string explicitName = null)
        {
            if (explicitName != null)
                return explicitName;

            // Stack trace analysis is only worth its cost in editor / dev builds.
            return Platform.IsInDevelopmentEnvironment
                ? ResolveCallerFromStackTrace()
                : null;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private static string ResolveCallerFromStackTrace()
        {
            StackTrace stackTrace = new(skipFrames: 1, fNeedFileInfo: false);
            for (int i = 0; i < stackTrace.FrameCount; i++)
            {
                Type type = stackTrace.GetFrame(i)?.GetMethod()?.DeclaringType;
                if (type == null)
                    continue;

                // Skip our own frames and any Unity-internal logging frames.
                if (type == typeof(CustomLogHandler))
                    continue;

                if (type.Namespace?.StartsWith(UnityNamespacePrefix) == true)
                    continue;

                return GetCleanTypeName(type);
            }

            return null;
        }

        /// <summary>
        /// Unwraps compiler-generated types (async/iterator state machines, lambda closures)
        /// whose names contain '&lt;', returning the enclosing user-declared type name.
        /// </summary>
        private static string GetCleanTypeName(Type type)
        {
            while (type.Name.IndexOf('<') >= 0 && type.DeclaringType != null)
                type = type.DeclaringType;

            return type.Name;
        }
#endif
    }
}