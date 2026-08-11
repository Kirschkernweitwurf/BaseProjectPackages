using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Restricts a Color field to a set of swatches read from another member, so a project stays on its
    /// palette instead of picking from the full wheel.
    /// </summary>
    /// <remarks>
    /// The member has to yield an enumerable of Color, the same shape <see cref="DropdownAttribute"/>
    /// accepts. Keeping the palette in code rather than in an asset means it can be a constant, a
    /// computed set or a theme lookup without needing a new asset type.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ColorPaletteAttribute : PropertyAttribute
    {
        /// <summary>Name of the member holding the palette.</summary>
        public string Member { get; }

        /// <summary>Whether the full color picker stays available next to the swatches.</summary>
        public bool AllowCustom { get; set; }

        /// <summary>Creates the attribute.</summary>
        /// <param name="member">Name of the member holding the palette.</param>
        public ColorPaletteAttribute(string member) => Member = member;
    }
}
