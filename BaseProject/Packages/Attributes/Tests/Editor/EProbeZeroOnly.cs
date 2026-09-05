using System;

namespace Base.AttributesPackage.Tests
{
    /// <summary>A flags enum offering nothing but its zero member, so there is no button to draw.</summary>
    [Flags]
    internal enum EProbeZeroOnly : byte
    {
        /// <summary>The zero member, which is not a button.</summary>
        None = 0
    }
}