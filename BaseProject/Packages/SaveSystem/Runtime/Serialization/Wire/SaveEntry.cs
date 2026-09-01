using System;

namespace Base.SaveSystemPackage.Serialization.Wire
{
    /// <summary>
    /// A single id and state pair collected from a savable. Plain serializable type with public fields
    /// so JsonUtility can handle it.
    /// </summary>
    [Serializable]
    internal sealed class SaveEntry
    {
        /// <summary>Identifies the savable this state belongs to.</summary>
        public string id;

        /// <summary>The savable's serialized state, opaque to everything but that savable.</summary>
        public string state;
    }
}