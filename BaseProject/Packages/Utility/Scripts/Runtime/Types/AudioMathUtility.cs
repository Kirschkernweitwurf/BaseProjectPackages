using UnityEngine;

namespace Base.UtilityPackage.Types
{
    /// <summary>
    /// Provides utility functions for audio-related mathematical operations.
    /// </summary>
    public static class AudioMathUtility
    {
        /// <summary>
        /// Converts a linear volume value (0 to 1) to decibels.
        /// </summary>
        public static float ConvertLinearToDecibel(float linearValue)
        {
            return 20f * Mathf.Log10(Mathf.Max(linearValue, 0.0001f));
        }

        /// <summary>
        /// Converts a decibel value back to a linear scale (0 to 1).
        /// </summary>
        public static float ConvertDecibelToLinear(float decibelValue) => Mathf.Pow(10f, decibelValue / 20f);
    }
}