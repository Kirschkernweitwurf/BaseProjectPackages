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
        public string id;
        public string state;
    }
}