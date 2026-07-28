using UnityEngine;

namespace Base.UtilityPackage
{
    /// <summary>
    /// Provides utility functions for audio-related mathematical operations.
    /// </summary>
    public static class AudioMathUtility
    {
        private const float DecibelScale = 20f;
        private const float LogarithmBase = 10f;
        private const float MinLinearValue = 0.0001f;

        /// <summary>
        /// Converts a linear volume value (0 to 1) to decibels.
        /// </summary>
        /// <param name="linearValue">The linear volume, clamped to a positive minimum before conversion.</param>
        /// <returns>The volume in decibels.</returns>
        public static float ConvertLinearToDecibel(float linearValue)
            => DecibelScale * Mathf.Log10(Mathf.Max(linearValue, MinLinearValue));

        /// <summary>
        /// Converts a decibel value back to a linear scale (0 to 1).
        /// </summary>
        /// <param name="decibelValue">The volume in decibels.</param>
        /// <returns>The linear volume.</returns>
        public static float ConvertDecibelToLinear(float decibelValue)
            => Mathf.Pow(LogarithmBase, decibelValue / DecibelScale);
    }
}
