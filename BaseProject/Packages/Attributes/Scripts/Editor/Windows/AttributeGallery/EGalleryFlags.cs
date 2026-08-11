using System;

namespace Base.AttributePackage.Editor.Windows.AttributeGallery
{
    /// <summary>Demo flags enum used by the gallery to show the mask widget.</summary>
    [Flags]
    internal enum EGalleryFlags : byte
    {
        /// <summary>Nothing selected.</summary>
        None = 0,

        /// <summary>First option.</summary>
        Fire = 1,

        /// <summary>Second option.</summary>
        Frost = 2,

        /// <summary>Third option.</summary>
        Shock = 4
    }
}