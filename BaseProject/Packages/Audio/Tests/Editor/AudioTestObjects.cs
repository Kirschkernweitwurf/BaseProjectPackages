using UnityEditor;
using UnityEngine;

namespace Base.AudioPackage.Tests
{
    /// <summary>
    /// Builds the objects the audio tests run against. Every playback setting on
    /// <see cref="AudioContainer"/> is an auto property behind a serialized backing field, so the only
    /// way to set one from outside is through <see cref="SerializedObject"/>. The backing field name is
    /// derived from the property name rather than written out, so renaming a property moves the tests
    /// with it instead of leaving them looking for a field that no longer exists.
    /// </summary>
    internal static class AudioTestObjects
    {
        private const string BackingFieldFormat = "<{0}>k__BackingField";
        private const int ClipChannels = 1;
        private const int ClipFrequency = 8000;
        private const int ClipSamples = 8000;
        private const string SourceName = "PooledAudioSource";

        /// <summary>
        /// Creates a one second mono clip. Nothing plays it, it only has to be a distinct asset that a
        /// container can hold and a source can be pointed at.
        /// </summary>
        /// <param name="name">The clip name, so a failing assertion names the clip it meant.</param>
        /// <returns>A clip that exists only in memory.</returns>
        internal static AudioClip CreateClip(string name)
            => AudioClip.Create(name, ClipSamples, ClipChannels, ClipFrequency, stream: false);

        /// <summary>
        /// Creates an unsaved container with every setting left at its default.
        /// </summary>
        /// <returns>A container the caller has to destroy again.</returns>
        internal static AudioContainer CreateContainer() => ScriptableObject.CreateInstance<AudioContainer>();

        /// <summary>
        /// Creates an audio source on its own GameObject under the given parent, so one call to destroy
        /// the parent cleans up everything a test made.
        /// </summary>
        /// <param name="parent">The transform the source is parented to.</param>
        /// <returns>The new source.</returns>
        internal static AudioSource CreateSource(Transform parent)
        {
            GameObject host = new(SourceName);
            host.transform.SetParent(parent);

            return host.AddComponent<AudioSource>();
        }

        /// <summary>
        /// Replaces the clips a container can pick from.
        /// </summary>
        /// <param name="container">The container to edit.</param>
        /// <param name="clips">The clips to assign. Pass none to empty the array.</param>
        internal static void SetClips(AudioContainer container, params AudioClip[] clips)
        {
            SerializedObject serialized = new(container);
            SerializedProperty property = Find(serialized, nameof(AudioContainer.Clips));

            property.arraySize = clips.Length;

            for (int i = 0; i < clips.Length; i++)
                property.GetArrayElementAtIndex(i).objectReferenceValue = clips[i];

            serialized.ApplyModifiedProperties();
        }

        /// <summary>
        /// Sets whether playback continues while the audio listener is paused.
        /// </summary>
        /// <param name="container">The container to edit.</param>
        /// <param name="ignorePause">The value to write.</param>
        internal static void SetIgnorePause(AudioContainer container, bool ignorePause)
            => SetBool(container, nameof(AudioContainer.IgnorePause), ignorePause);

        /// <summary>
        /// Sets whether the container loops.
        /// </summary>
        /// <param name="container">The container to edit.</param>
        /// <param name="loop">The value to write.</param>
        internal static void SetLoop(AudioContainer container, bool loop)
            => SetBool(container, nameof(AudioContainer.Loop), loop);

        /// <summary>
        /// Sets how many clips of this container may play at the same time.
        /// </summary>
        /// <param name="container">The container to edit.</param>
        /// <param name="maxClipsPlaying">The value to write. Negative means unlimited.</param>
        internal static void SetMaxClipsPlaying(AudioContainer container, int maxClipsPlaying)
            => SetInt(container, nameof(AudioContainer.MaxClipsPlaying), maxClipsPlaying);

        /// <summary>
        /// Sets whether the pitch is randomized on every play.
        /// </summary>
        /// <param name="container">The container to edit.</param>
        /// <param name="randomizePitch">The value to write.</param>
        internal static void SetRandomizePitch(AudioContainer container, bool randomizePitch)
            => SetBool(container, nameof(AudioContainer.RandomizePitch), randomizePitch);

        /// <summary>
        /// Sets the volume multiplier of the container.
        /// </summary>
        /// <param name="container">The container to edit.</param>
        /// <param name="volume">The value to write.</param>
        internal static void SetVolume(AudioContainer container, float volume)
            => SetFloat(container, nameof(AudioContainer.Volume), volume);

        /// <summary>
        /// Writes a boolean property through its backing field.
        /// </summary>
        /// <param name="container">The container to edit.</param>
        /// <param name="propertyName">The name of the auto property.</param>
        /// <param name="value">The value to write.</param>
        private static void SetBool(AudioContainer container, string propertyName, bool value)
        {
            SerializedObject serialized = new(container);

            Find(serialized, propertyName).boolValue = value;
            serialized.ApplyModifiedProperties();
        }

        /// <summary>
        /// Writes a float property through its backing field.
        /// </summary>
        /// <param name="container">The container to edit.</param>
        /// <param name="propertyName">The name of the auto property.</param>
        /// <param name="value">The value to write.</param>
        private static void SetFloat(AudioContainer container, string propertyName, float value)
        {
            SerializedObject serialized = new(container);

            Find(serialized, propertyName).floatValue = value;
            serialized.ApplyModifiedProperties();
        }

        /// <summary>
        /// Writes an integer property through its backing field.
        /// </summary>
        /// <param name="container">The container to edit.</param>
        /// <param name="propertyName">The name of the auto property.</param>
        /// <param name="value">The value to write.</param>
        private static void SetInt(AudioContainer container, string propertyName, int value)
        {
            SerializedObject serialized = new(container);

            Find(serialized, propertyName).intValue = value;
            serialized.ApplyModifiedProperties();
        }

        /// <summary>
        /// Finds the serialized backing field of an auto property.
        /// </summary>
        /// <param name="serialized">The serialized container.</param>
        /// <param name="propertyName">The name of the auto property.</param>
        /// <returns>The backing field property.</returns>
        private static SerializedProperty Find(SerializedObject serialized, string propertyName)
            => serialized.FindProperty(string.Format(BackingFieldFormat, propertyName));
    }
}