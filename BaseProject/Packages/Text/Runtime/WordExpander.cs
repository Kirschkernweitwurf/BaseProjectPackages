using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Localization.Pseudo;
using UnityEngine.Scripting.APIUpdating;

namespace Base.TextPackage
{
    /// <summary>
    /// Makes a pseudo-localized text longer the way a translation does: with extra words after a space. Unity's
    /// <see cref="Expander"/> glues one block of random characters to the last word instead, which makes a word no
    /// translation has and fails every narrow box for the wrong reason.
    /// </summary>
    /// <remarks>
    /// The filler is the same for the same text, so a check reports the same result every time.
    /// </remarks>
    [Serializable]
    [MovedFrom(true, "Shared.TextStyles", "HandOverHeels.Shared.TextStyles", null)]
    public sealed class WordExpander : IPseudoLocalizationMethod
    {
        private const char Space = ' ';

        private static readonly string[] Words =
        {
            "lorem", "ipsum", "dolor", "sit", "amet", "consectetur", "adipiscing", "elit", "sed", "do", "eiusmod",
            "tempor", "incididunt", "ut", "labore", "et", "dolore", "magna", "aliqua"
        };

        [Tooltip("How much longer a text gets, by its length. 0.5 adds half its length in words.")]
        [SerializeField] private List<Expander.ExpansionRule> expansionRules = new();

        /// <inheritdoc/>
        public void Transform(Message message)
        {
            int length = message.Length;
            int padding = Mathf.CeilToInt(ExpansionFor(length) * length);

            if (padding <= 0)
                return;

            message.Fragments.Add(message.CreateTextFragment(Filler(padding, length)));
        }

        // Starts at a word picked by the length, so different texts do not all end in the same words.
        private static string Filler(int padding, int seed)
        {
            StringBuilder filler = new();

            for (int i = seed; filler.Length < padding; i++)
                filler.Append(Space).Append(Words[i % Words.Length]);

            return filler.ToString();
        }

        private float ExpansionFor(int length)
        {
            foreach (Expander.ExpansionRule rule in expansionRules)
            {
                if (length >= rule.MinCharacters
                    && length < rule.MaxCharacters)
                    return rule.ExpansionAmount;
            }

            return 0f;
        }
    }
}