using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Draws a Vector2 as a two-handled range slider, with bounds that may come from other members
    /// rather than only from constants.
    /// </summary>
    /// <remarks>
    /// X holds the low end and Y the high end. The bounds follow the same shapes as
    /// <see cref="SliderAttribute"/>: two numbers, two member names, one Vector2 member, or a mix.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class MinMaxSliderAttribute : PropertyAttribute
    {
        /// <summary>Lower bound, when given as a constant.</summary>
        public float Min { get; }

        /// <summary>Upper bound, when given as a constant.</summary>
        public float Max { get; }

        /// <summary>Name of the member holding the lower bound, or null.</summary>
        public string MinMember { get; }

        /// <summary>Name of the member holding the upper bound, or null.</summary>
        public string MaxMember { get; }

        /// <summary>Name of a Vector2 member holding both bounds, or null.</summary>
        public string RangeMember { get; }

        /// <summary>Whether the stored range is clamped into the bounds even when set elsewhere.</summary>
        public bool AutoClamp { get; set; }

        /// <summary>Creates a range slider with constant bounds.</summary>
        /// <param name="min">Lower bound.</param>
        /// <param name="max">Upper bound.</param>
        public MinMaxSliderAttribute(float min, float max)
        {
            Min = min;
            Max = max;
        }

        /// <summary>Creates a range slider reading both bounds from a Vector2 member.</summary>
        /// <param name="rangeMember">Name of the Vector2 member holding both bounds.</param>
        public MinMaxSliderAttribute(string rangeMember) => RangeMember = rangeMember;

        /// <summary>Creates a range slider reading both bounds from separate members.</summary>
        /// <param name="minMember">Name of the member holding the lower bound.</param>
        /// <param name="maxMember">Name of the member holding the upper bound.</param>
        public MinMaxSliderAttribute(string minMember, string maxMember)
        {
            MinMember = minMember;
            MaxMember = maxMember;
        }

        /// <summary>Creates a range slider with a constant lower bound and a member-driven upper one.</summary>
        /// <param name="min">Lower bound.</param>
        /// <param name="maxMember">Name of the member holding the upper bound.</param>
        public MinMaxSliderAttribute(float min, string maxMember)
        {
            Min = min;
            MaxMember = maxMember;
        }

        /// <summary>Creates a range slider with a member-driven lower bound and a constant upper one.</summary>
        /// <param name="minMember">Name of the member holding the lower bound.</param>
        /// <param name="max">Upper bound.</param>
        public MinMaxSliderAttribute(string minMember, float max)
        {
            MinMember = minMember;
            Max = max;
        }
    }
}