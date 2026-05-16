using System;
using UnityEngine;

namespace Base.UtilityPackage.Editor
{
    /// <summary>
    /// Base class for <c>[Serializable]</c> data containers that need C# field
    /// defaults applied to instances Unity allocates without running the constructor
    /// (e.g. when the inspector "+" button adds a new entry to a serialized list).
    /// Inherit and override <see cref="ApplyDefaults"/> to seed your fields.
    /// </summary>
    /// <remarks>
    /// Why this is needed: when Unity adds a new element to a serialized list, it
    /// zero-initializes the element's backing fields instead of calling your C#
    /// constructor. C# property/field initializers like <c>= Color.cyan</c> are
    /// compiled into the constructor, so they're skipped and the inspector shows
    /// zero-valued defaults (e.g. <c>Color.clear</c> instead of <c>Color.cyan</c>).
    /// This class detects that case after deserialization and applies the defaults
    /// exactly once.
    /// </remarks>
    [Serializable]
    public abstract class SerializableDefaults : ISerializationCallbackReceiver
    {
        // False on instances Unity allocates without running our constructor.
        // Detected in OnAfterDeserialize so defaults are applied exactly once,
        // and never overwrite values the user has subsequently authored.
        [SerializeField, HideInInspector] private bool hasDefaults;

        void ISerializationCallbackReceiver.OnBeforeSerialize() { }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            if (hasDefaults)
                return;

            hasDefaults = true;
            ApplyDefaults();
        }

        /// <summary>
        /// Called exactly once per instance, the first time it's deserialized.
        /// Override to set initial values for your fields.
        /// </summary>
        protected abstract void ApplyDefaults();
    }
}