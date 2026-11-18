using System;
using UnityEngine;

namespace Base.TextPackage
{
    /// <summary>
    /// Extra room between letters, words and lines, in TextMeshPro units (hundredths of the font size), added on top
    /// of the spacing a text box was designed with. Wider spacing eases crowding in running text, the reason it helps
    /// readers with dyslexia (Zorzi et al. 2012); the Xbox Accessibility Guidelines name the minimums for text blocks.
    /// </summary>
    [Serializable]
    public struct TextSpacing : IEquatable<TextSpacing>
    {
        [Tooltip("Extra space between letters, in hundredths of the font size.")]
        [SerializeField] private float characters;

        [Tooltip("Extra space between words, in hundredths of the font size.")]
        [SerializeField] private float words;

        [Tooltip("Extra space between lines, in hundredths of the font size.")]
        [SerializeField] private float lines;

        [Tooltip("Extra space after a paragraph, in hundredths of the font size.")]
        [SerializeField] private float paragraphs;

        /// <summary>Creates a spacing with the given extra room, in hundredths of the font size.</summary>
        public TextSpacing(float characters, float words, float lines, float paragraphs = 0f)
        {
            this.characters = characters;
            this.words = words;
            this.lines = lines;
            this.paragraphs = paragraphs;
        }

        /// <summary>No extra room: every text keeps its designed spacing.</summary>
        public static TextSpacing None => default;

        /// <summary>Extra space between letters, in hundredths of the font size.</summary>
        public float Characters => characters;

        /// <summary>Extra space between words, in hundredths of the font size.</summary>
        public float Words => words;

        /// <summary>Extra space between lines, in hundredths of the font size.</summary>
        public float Lines => lines;

        /// <summary>Extra space after a paragraph, in hundredths of the font size.</summary>
        public float Paragraphs => paragraphs;

        /// <inheritdoc/>
        public bool Equals(TextSpacing other) => characters.Equals(other.characters)
            && words.Equals(other.words)
            && lines.Equals(other.lines)
            && paragraphs.Equals(other.paragraphs);

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is TextSpacing other && Equals(other);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(characters, words, lines, paragraphs);
    }
}