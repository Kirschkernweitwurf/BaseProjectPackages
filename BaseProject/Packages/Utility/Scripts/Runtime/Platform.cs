// ReSharper disable ConvertToConstant.Global
// ReSharper disable MemberCanBePrivate.Global

using UnityEngine;

namespace Base.UtilityPackage
{
    /// <summary>
    /// Provides runtime flags for platform and build-specific conditions.
    /// <para></para>
    /// The main advantage of this class is that it allows you to write code that depends on platform or build
    /// conditions without using preprocessor directives that would cause certain compiler warnings or stripping.
    /// <para></para>
    /// <b>Warning:</b> Do <b>not</b> change these fields to <c>const</c> even if your IDE suggests it.
    /// Using <c>const</c> will make the compiler treat branches as unreachable, causing warnings
    /// or stripping valid runtime code. The fields are <c>static readonly</c> to allow runtime
    /// evaluation while keeping compile-time platform assignments.
    /// <example>
    /// Here's how you might use the <see cref="Platform"/> class in your code:
    /// <code>
    /// if (Platform.IsUnityEditor)
    /// {
    ///     // Code that should only run in the Unity Editor
    ///     Debug.Log("Running in the Unity Editor");
    /// }
    /// </code>
    /// </example>
    /// </summary>
    public static class Platform
    {
        /// <summary>
        /// <c>true</c> if running inside the Unity Editor.
        /// </summary>
#if UNITY_EDITOR
        public static readonly bool IsUnityEditor = true;
#else
        public static readonly bool IsUnityEditor = false;
#endif

        /// <summary>
        /// <c>true</c> if running on Windows standalone.
        /// </summary>
#if UNITY_STANDALONE_WIN
        public static readonly bool IsWindows = true;
#else
        public static readonly bool IsWindows = false;
#endif

        /// <summary>
        /// <c>true</c> if running on macOS standalone.
        /// </summary>
#if UNITY_STANDALONE_OSX
        public static readonly bool IsMac = true;
#else
        public static readonly bool IsMac = false;
#endif

        /// <summary>
        /// <c>true</c> if running on Linux standalone.
        /// </summary>
#if UNITY_STANDALONE_LINUX
        public static readonly bool IsLinux = true;
#else
        public static readonly bool IsLinux = false;
#endif

        /// <summary>
        /// <c>true</c> if running on Android.
        /// </summary>
#if UNITY_ANDROID
        public static readonly bool IsAndroid = true;
#else
        public static readonly bool IsAndroid = false;
#endif

        /// <summary>
        /// <c>true</c> if running on iOS.
        /// </summary>
#if UNITY_IOS
        public static readonly bool IsIOS = true;
#else
        public static readonly bool IsIOS = false;
#endif

        /// <summary>
        /// <c>true</c> if running in WebGL.
        /// </summary>
#if UNITY_WEBGL
        public static readonly bool IsWebGL = true;
#else
        public static readonly bool IsWebGL = false;
#endif

        /// <summary>
        /// <c>true</c> if this is a development build.
        /// </summary>
#if DEVELOPMENT_BUILD
        public static readonly bool IsDevelopmentBuild = true;
#else
        public static readonly bool IsDevelopmentBuild = false;
#endif

        /// <summary>
        /// <c>true</c> if this is a debug build (scripting define DEBUG).
        /// </summary>
#if DEBUG
        public static readonly bool IsScriptDebuggingEnabled = true;
#else
        public static readonly bool IsScriptDebuggingEnabled = false;
#endif

        /// <summary>
        /// <c>true</c> if this is any standalone platform (Windows, macOS, Linux).
        /// </summary>
        public static readonly bool IsStandalone = IsWindows || IsMac || IsLinux;

        /// <summary>
        /// <c>true</c> if this is a mobile platform (Android or iOS).
        /// </summary>
        public static readonly bool IsMobile = IsAndroid || IsIOS;

        /// <summary>
        /// <c>true</c> if this is a release build (not editor, not development).
        /// </summary>
        public static readonly bool IsRelease = !IsUnityEditor && !IsDevelopmentBuild;

        /// <summary>
        /// <c>true</c> if this is a development environment (editor or development build).
        /// </summary>
        public static readonly bool IsInDevelopmentEnvironment = IsUnityEditor || IsDevelopmentBuild;

        /// <summary>
        /// This method returns <c>true</c> if running in edit mode (not playing) inside the Unity Editor.
        /// </summary>
        /// <returns><c>true</c> if running in edit mode; otherwise, <c>false</c>.</returns>
        public static bool IsEditorMode() => Application.isEditor && !Application.isPlaying;
    }
}