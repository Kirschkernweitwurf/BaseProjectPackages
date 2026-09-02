using System;

namespace Base.SaveSystemPackage.Tests
{
    /// <summary>
    /// The payload the codec tests encode and decode. Public fields, because that is what Unity's
    /// serializer writes.
    /// </summary>
    [Serializable]
    public sealed class SaveProbePayload
    {
        /// <summary>A text field, so the round trip covers more than numbers.</summary>
        public string label;

        /// <summary>A numeric field.</summary>
        public int score;
    }
}