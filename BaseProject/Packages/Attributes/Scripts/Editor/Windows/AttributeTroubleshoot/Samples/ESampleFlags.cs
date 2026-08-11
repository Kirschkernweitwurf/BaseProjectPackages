using System;

namespace Base.AttributePackage.Editor.Windows.AttributeTroubleshoot.Samples
{
    /// <summary>Damage types used to demonstrate the multi-select mask attribute in the showcase.</summary>
    [Flags]
    internal enum ESampleFlags : byte
    {
        /// <summary>Nothing selected.</summary>
        None = 0,

        /// <summary>Fire damage.</summary>
        Fire = 1,

        /// <summary>Ice damage.</summary>
        Ice = 2,

        /// <summary>Poison damage.</summary>
        Poison = 4,

        /// <summary>Shock damage.</summary>
        Shock = 8
    }
}