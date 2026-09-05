using System;

namespace Base.AttributesPackage.Tests
{
    /// <summary>
    /// A flags enum with a zero member and three bits, plus one name of two words so the nicifying can
    /// be told apart from the raw name.
    /// </summary>
    [Flags]
    internal enum EProbeFlags : byte
    {
        /// <summary>The zero member, which is not a button.</summary>
        None = 0,

        /// <summary>The first bit.</summary>
        First = 1,

        /// <summary>The second bit, named in two words.</summary>
        SecondThing = 2,

        /// <summary>The third bit.</summary>
        Third = 4
    }
}