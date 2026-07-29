using System;
using System.Diagnostics;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.UtilityPackage.Logging
{
    /// <summary>
    /// Wraps Unity's log handler and prefixes every message with the calling class name, colored for
    /// readability in the Console. Edit mode logs get an extra marker so they can be told apart from
    /// play mode logs.
    /// </summary>
    /// <remarks>
    /// Unlike <see cref="CustomLogger"/> this also covers plain <see cref="UnityEngine.Debug"/> calls and logs
    /// coming from third party code, at the cost of a stack trace lookup per message.
    /// </remarks>
    public sealed class CustomLogHandler : ILogHandler
    {
        // The already formatted message is passed as an argument instead of as the format string,
        // so braces inside a message can never be mistaken for a format placeholder.
        private const string PassthroughFormat = "{0}";
        private const string UnityNamespacePrefix = "UnityEngine";

        /// <summary>The genuine Unity handler this instance forwards to.</summary>
        public ILogHandler DefaultLogHandler { get; }

        /// <summary>
        /// Creates a handler that decorates the given Unity handler.
        /// </summary>
        /// <param name="defaultLogHandler">
        /// The genuine Unity handler to forward to. Must not be another <see cref="CustomLogHandler"/>.
        /// </param>
        public CustomLogHandler(ILogHandler defaultLogHandler) => DefaultLogHandler = defaultLogHandler;

        /// <inheritdoc/>
        public void LogFormat(LogType logType, Object context, string format, params object[] args)
            => DefaultLogHandler.LogFormat(logType, context, PassthroughFormat,
                BuildPrefix() + string.Format(format, args));

        /// <inheritdoc/>
        public void LogException(Exception exception, Object context)
            => DefaultLogHandler.LogException(exception, context);

        private static string BuildPrefix()
        {
            string editorMarker = CustomLoggingUtils.GetEditorMarker();
            string caller = GetCallerClassName();

            // No caller resolved (e.g. release builds): omit the class tag entirely.
            if (caller == null)
                return editorMarker;

            return $"{editorMarker}{CustomLoggingUtils.BuildClassTag(caller)} ";
        }

        /// <summary>
        /// Returns the calling class name, or null when it cannot or should not be resolved.
        /// </summary>
        private static string GetCallerClassName()
        {
            // Stack trace analysis is only worth its cost in editor / dev builds.
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            return ResolveCallerFromStackTrace();
#else
            return null;
#endif
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private static string ResolveCallerFromStackTrace()
        {
            StackTrace stackTrace = new(1, false);

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
            while (type.Name.IndexOf('<') >= 0
                   && type.DeclaringType != null)
                type = type.DeclaringType;

            return type.Name;
        }
#endif
    }
}