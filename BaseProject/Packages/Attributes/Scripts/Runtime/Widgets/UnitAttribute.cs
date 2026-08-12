using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Shows a unit after a numeric field, from a fixed vocabulary rather than a free string.
    /// </summary>
    /// <remarks>
    /// Functionally a <see cref="SuffixAttribute"/>. The difference is the constants: a unit written as
    /// a literal is one typo away from disagreeing with the identical unit three fields down, and no
    /// tool can tell you that "m/s" and "m/s " are the same thing. Naming them makes the set greppable
    /// and the spelling decided once.
    /// <para>
    /// The string constructor stays for the units nobody standardised. It is the exception, not the
    /// default.
    /// </para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class UnitAttribute : PropertyAttribute
    {
        /// <summary>Amperes.</summary>
        public const string Ampere = "A";

        /// <summary>Candela.</summary>
        public const string Candela = "cd";

        /// <summary>Centimeters.</summary>
        public const string Centimeter = "cm";

        /// <summary>Degrees of arc.</summary>
        public const string Degree = "\u00B0";

        /// <summary>Degrees of arc per second.</summary>
        public const string DegreePerSecond = "\u00B0/s";

        /// <summary>Frames per second.</summary>
        public const string FramesPerSecond = "fps";

        /// <summary>Grams.</summary>
        public const string Gram = "g";

        /// <summary>Hertz.</summary>
        public const string Hertz = "Hz";

        /// <summary>Kelvin.</summary>
        public const string Kelvin = "K";

        /// <summary>Kilograms.</summary>
        public const string Kilogram = "kg";

        /// <summary>Meters.</summary>
        public const string Meter = "m";

        /// <summary>Meters per second.</summary>
        public const string MetersPerSecond = "m/s";

        /// <summary>Meters per second squared.</summary>
        public const string MetersPerSecondSquared = "m/s\u00B2";

        /// <summary>Millimeters.</summary>
        public const string Millimeter = "mm";

        /// <summary>Milliseconds.</summary>
        public const string Millisecond = "ms";

        /// <summary>Moles.</summary>
        public const string Mole = "mol";

        /// <summary>Newtons.</summary>
        public const string Newton = "N";

        /// <summary>Percent.</summary>
        public const string Percent = "%";

        /// <summary>Pixels.</summary>
        public const string Pixel = "px";

        /// <summary>Radians.</summary>
        public const string Radian = "rad";

        /// <summary>Radians per second.</summary>
        public const string RadiansPerSecond = "rad/s";

        /// <summary>Seconds.</summary>
        public const string Second = "s";

        /// <summary>Units of the project's world space.</summary>
        public const string WorldUnit = "u";

        /// <summary>The unit shown after the value.</summary>
        public string Unit { get; }

        /// <summary>Creates the attribute.</summary>
        /// <param name="unit">The unit shown after the value, ideally one of the constants here.</param>
        public UnitAttribute(string unit) => Unit = unit;
    }
}