using System;

namespace Base.SaveSystemPackage
{
    /// <summary>
    /// A single (id, state) pair collected from a savable. Plain [Serializable] type so JsonUtility can handle it.
    /// </summary>
    [Serializable]
    public sealed class SaveEntry
    {
        public string id;
        public string state;
    }
}