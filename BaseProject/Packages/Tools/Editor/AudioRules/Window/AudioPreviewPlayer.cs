using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Base.ToolsPackage.Editor.AudioRules.Window
{
    /// <summary>
    /// Plays a clip in the editor. A designer will not trust a change to a clip they cannot hear,
    /// so this is not optional, but Unity only exposes preview playback through an internal type.
    /// The reflection is resolved once and fails quietly into a no-op, so a future rename costs
    /// the preview button and nothing else.
    /// </summary>
    internal static class AudioPreviewPlayer
    {
        private const string PlayMethodName = "PlayPreviewClip";
        private const string StopMethodName = "StopAllPreviewClips";
        private const string UtilTypeName = "UnityEditor.AudioUtil,UnityEditor";

        private static MethodInfo PlayMethod => _playMethod ??= FindMethod(PlayMethodName);

        private static MethodInfo StopMethod => _stopMethod ??= FindMethod(StopMethodName);

        private static MethodInfo _playMethod;
        private static MethodInfo _stopMethod;

        /// <summary>Plays the clip at the given path.</summary>
        /// <param name="assetPath">Project relative path of the clip.</param>
        internal static void Play(string assetPath) => Play(AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath));

        /// <summary>Stops every preview the editor is playing.</summary>
        internal static void Stop() => StopMethod?.Invoke(null, Array.Empty<object>());

        // Stopping first is what makes a second click restart rather than layer. The method lookup can
        // come back empty on a future editor, which costs the preview and nothing else.
        private static void Play(AudioClip clip)
        {
            if (clip == null)
                return;

            Stop();

            if (PlayMethod == null)
                return;

            PlayMethod.Invoke(null, new object[]
            {
                clip,
                0,
                false
            });
        }

        private static MethodInfo FindMethod(string name)
        {
            Type type = Type.GetType(UtilTypeName);

            return type?.GetMethod(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        }
    }
}