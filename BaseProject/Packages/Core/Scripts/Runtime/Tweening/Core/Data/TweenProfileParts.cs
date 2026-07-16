using System;

namespace Base.CorePackage.Tweening.Core.Data
{
    /// <summary>
    /// Defines which parts of a tween profile are applied to the tween that uses it.
    /// </summary>
    [Flags]
    public enum ETweenProfileParts : byte
    {
        /// <summary>
        /// The profile is ignored completely.
        /// </summary>
        None = 0,

        /// <summary>
        /// The profile drives duration, delay and easing.
        /// </summary>
        Settings = 1,

        /// <summary>
        /// The profile drives loop count and loop type.
        /// </summary>
        Loop = 2,

        /// <summary>
        /// The profile drives the start and target values of the tween.
        /// </summary>
        Values = 4,

        /// <summary>
        /// The profile drives everything it can.
        /// </summary>
        All = 7
    }
}