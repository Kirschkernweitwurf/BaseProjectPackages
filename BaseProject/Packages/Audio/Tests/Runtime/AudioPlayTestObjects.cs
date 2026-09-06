using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Base.AudioPackage.PlayTests
{
    /// <summary>
    /// Builds and wires the objects the play mode audio tests run against.
    /// <para>
    /// Both managers read their dependencies from serialized fields in <c>Awake</c>, so a test has to
    /// fill those in before the object is switched on. Reflection is what reaches a private field from
    /// out here, and the field names come from constants the classes build with <c>nameof</c>, so a
    /// rename moves the tests with it rather than leaving them looking for a field that is gone.
    /// </para>
    /// </summary>
    internal static class AudioPlayTestObjects
    {
        private const string BackingFieldFormat = "<{0}>k__BackingField";
        private const int ClipChannels = 1;
        private const int ClipFrequency = 8000;

        private const BindingFlags FieldFlags = BindingFlags.Instance | BindingFlags.NonPublic;

        /// <summary>
        /// Creates a clip of the given length in samples. At the frequency used here, 400 samples is a
        /// twentieth of a second, which is short enough to wait out inside a test.
        /// </summary>
        /// <param name="name">The clip name, so a failing assertion names the clip it meant.</param>
        /// <param name="samples">How many samples the clip holds.</param>
        /// <returns>A clip that exists only in memory.</returns>
        internal static AudioClip CreateClip(string name, int samples)
            => AudioClip.Create(name, samples, ClipChannels, ClipFrequency, stream: false);

        /// <summary>Creates an unsaved container with every setting left at its default.</summary>
        /// <returns>A container the caller has to destroy again.</returns>
        internal static AudioContainer CreateContainer() => ScriptableObject.CreateInstance<AudioContainer>();

        /// <summary>Creates a source on its own object, to be used as a pool prefab.</summary>
        /// <param name="name">The name of the object carrying the source.</param>
        /// <param name="hosts">The list the object is recorded in, so the teardown finds it.</param>
        /// <returns>The new source.</returns>
        internal static AudioSource CreatePrefab(string name, ICollection<GameObject> hosts)
        {
            GameObject host = new(name);
            hosts.Add(host);

            return host.AddComponent<AudioSource>();
        }

        /// <summary>
        /// Writes a serialized field of a component that has not been switched on yet.
        /// </summary>
        /// <param name="target">The component to write to.</param>
        /// <param name="fieldName">The name of the serialized field.</param>
        /// <param name="value">The value to write.</param>
        internal static void SetField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, FieldFlags);

            if (field == null)
                return;

            field.SetValue(target, value);
        }

        /// <summary>
        /// Writes an auto property of a container through the field the compiler generated for it.
        /// </summary>
        /// <param name="container">The container to write to.</param>
        /// <param name="propertyName">The name of the auto property.</param>
        /// <param name="value">The value to write.</param>
        internal static void SetProperty(AudioContainer container, string propertyName, object value)
            => SetField(container, string.Format(BackingFieldFormat, propertyName), value);

        /// <summary>Counts how many of a transform's children are switched on.</summary>
        /// <param name="parent">The transform whose children are counted.</param>
        /// <returns>The number of active children.</returns>
        internal static int CountActiveChildren(Transform parent)
        {
            int active = 0;

            for (int i = 0; i < parent.childCount; i++)
            {
                if (parent.GetChild(i).gameObject.activeSelf)
                    active++;
            }

            return active;
        }
    }
}