namespace Base.AttributesPackage.Tests
{
    /// <summary>An enum a condition can be pointed at, with more than one value so a change shows.</summary>
    internal enum EProbeMood : byte
    {
        /// <summary>The value a fresh probe starts on.</summary>
        Calm = 0,

        /// <summary>Anything other than the starting value.</summary>
        Angry = 1
    }
}