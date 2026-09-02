using System;
using UnityEngine;

namespace Base.AttributesPackage
{
    /// <summary>
    /// A slider whose bounds may come from other members rather than only from constants, for the ranges
    /// that depend on something else on the object.
    /// </summary>
    /// <remarks>
    /// Unity's own range attribute takes two literals, which is enough until the maximum is a stat, a
    /// difficulty setting or another field. Each end here is either a number or the name of a member
    /// holding one, and a single Vector2 member can supply both at once.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class SliderAttribute : PropertyAttribute
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

        /// <summary>Whether the stored value is clamped into the range even when it was set elsewhere.</summary>
        public bool AutoClamp { get; set; }

        /// <summary>Creates a slider with constant bounds.</summary>
        /// <param name="min">Lower bound.</param>
        /// <param name="max">Upper bound.</param>
        public SliderAttribute(float min, float max)
        {
            Min = min;
            Max = max;
        }

        /// <summary>Creates a slider reading both bounds from a Vector2 member.</summary>
        /// <param name="rangeMember">Name of the Vector2 member holding both bounds.</param>
        public SliderAttribute(string rangeMember) => RangeMember = rangeMember;

        /// <summary>Creates a slider reading both bounds from separate members.</summary>
        /// <param name="minMember">Name of the member holding the lower bound.</param>
        /// <param name="maxMember">Name of the member holding the upper bound.</param>
        public SliderAttribute(string minMember, string maxMember)
        {
            MinMember = minMember;
            MaxMember = maxMember;
        }

        /// <summary>Creates a slider with a constant lower bound and a member-driven upper one.</summary>
        /// <param name="min">Lower bound.</param>
        /// <param name="maxMember">Name of the member holding the upper bound.</param>
        public SliderAttribute(float min, string maxMember)
        {
            Min = min;
            MaxMember = maxMember;
        }

        /// <summary>Creates a slider with a member-driven lower bound and a constant upper one.</summary>
        /// <param name="minMember">Name of the member holding the lower bound.</param>
        /// <param name="max">Upper bound.</param>
        public SliderAttribute(string minMember, float max)
        {
            MinMember = minMember;
            Max = max;
        }
    }
}