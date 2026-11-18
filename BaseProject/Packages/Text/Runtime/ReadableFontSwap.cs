using System;
using Base.AttributesPackage;
using TMPro;
using UnityEngine;

namespace Base.TextPackage
{
    /// <summary>
    /// Replaces one designed font with a readable one. The readable font is scaled so its lowercase letters are as
    /// tall as the designed font's and its lines keep the designed line height, so a box holds about as many lines
    /// as before. Only the width of the words changes.
    /// </summary>
    [Serializable]
    public sealed class ReadableFontSwap
    {
        private const float EmToSpacingUnits = 100f;
        private const float Unscaled = 1f;

        [Tooltip("The designed font this swap replaces.")]
        [Required] [SerializeField] private TMP_FontAsset designedFont;

        [Tooltip("Only replaces boxes drawn with this material of the designed font, like a heading material. Empty "
            + "replaces every box with the designed font.")]
        [SerializeField] private Material onlyWithMaterial;

        [Tooltip("The readable font that takes its place, drawn with its own default material.")]
        [Required] [SerializeField] private TMP_FontAsset readableFont;

        /// <summary>The readable font that takes the designed font's place.</summary>
        public TMP_FontAsset ReadableFont => readableFont;

        /// <summary>
        /// The size of the readable font relative to the designed size, so lowercase letters keep their height.
        /// </summary>
        public float SizeFactor
        {
            get
            {
                float readableHeight = GetLowercaseHeight(readableFont);
                return readableHeight > 0f
                    ? GetLowercaseHeight(designedFont) / readableHeight
                    : Unscaled;
            }
        }

        /// <summary>
        /// The line spacing that brings the readable font's lines to the designed line height, at the readable size,
        /// in TextMeshPro units.
        /// </summary>
        public float LineSpacingOffset => (GetLineHeight(designedFont) / SizeFactor - GetLineHeight(readableFont))
            * EmToSpacingUnits;

        /// <summary>Whether this swaps <paramref name="font"/> drawn with <paramref name="material"/>.</summary>
        public bool IsFor(TMP_FontAsset font, Material material) => font == designedFont
            && (onlyWithMaterial == null || material == onlyWithMaterial);

        private static float GetLowercaseHeight(TMP_FontAsset font) =>
            font.faceInfo.meanLine / font.faceInfo.pointSize;

        private static float GetLineHeight(TMP_FontAsset font) => font.faceInfo.lineHeight / font.faceInfo.pointSize;
    }
}