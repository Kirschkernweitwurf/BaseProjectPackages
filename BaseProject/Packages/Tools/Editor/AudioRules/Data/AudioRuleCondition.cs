using System;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Base.ToolsPackage.Editor.AudioRules.Data
{
    /// <summary>
    /// One test a clip has to pass for its rule to apply, for example "duration below 2 seconds"
    /// or "path contains /Music/". Which of the two values is used depends on the field: the
    /// numeric fields read <see cref="Number"/>, the text fields read <see cref="Text"/>.
    /// </summary>
    [Serializable]
    internal sealed class AudioRuleCondition
    {
        private const string AnyPattern = ".*";
        private const string PatternEnd = "$";
        private const string PatternStart = "^";
        private const float Tolerance = 0.0001f;
        private const char Wildcard = '*';

        [field: Tooltip("The fact about the clip this condition looks at.")]
        [field: SerializeField] public EConditionField Field { get; set; } = EConditionField.DurationSeconds;

        [field: Tooltip("How the clip is compared against the value.")]
        [field: SerializeField] public EConditionOperator Operator { get; set; } = EConditionOperator.LessThan;

        [field: Tooltip("Value the numeric fields are compared against.")]
        [field: SerializeField] public float Number { get; set; }

        [field: Tooltip("Value the text fields are compared against. Supports * as a wildcard with Matches.")]
        [field: SerializeField] public string Text { get; set; } = string.Empty;

        /// <summary>True when the field holds a number rather than text or a flag.</summary>
        public bool IsNumeric => Field is EConditionField.Channels
            or EConditionField.DurationSeconds
            or EConditionField.FileSizeKilobytes
            or EConditionField.SampleRate;

        /// <summary>True when the field holds a flag that is only ever on or off.</summary>
        public bool IsFlag => Field == EConditionField.IsLooping;

        /// <summary>Creates an empty condition. Needed by the serializer.</summary>
        public AudioRuleCondition() { }

        /// <summary>Creates a condition on a numeric field.</summary>
        /// <param name="field">The fact to look at.</param>
        /// <param name="conditionOperator">How to compare.</param>
        /// <param name="number">The value to compare against.</param>
        public AudioRuleCondition(EConditionField field, EConditionOperator conditionOperator, float number)
        {
            Field = field;
            Operator = conditionOperator;
            Number = number;
        }

        /// <summary>Creates a condition on a text field.</summary>
        /// <param name="field">The fact to look at.</param>
        /// <param name="conditionOperator">How to compare.</param>
        /// <param name="text">The value to compare against.</param>
        public AudioRuleCondition(EConditionField field, EConditionOperator conditionOperator, string text)
        {
            Field = field;
            Operator = conditionOperator;
            Text = text;
        }

        /// <summary>Compares a numeric fact against the condition.</summary>
        /// <param name="value">The value taken from the clip.</param>
        /// <returns>True when the condition holds.</returns>
        public bool MatchesNumber(float value) => Operator switch
        {
            EConditionOperator.Equals => Mathf.Abs(value - Number) <= Tolerance,
            EConditionOperator.GreaterOrEqual => value >= Number - Tolerance,
            EConditionOperator.GreaterThan => value > Number,
            EConditionOperator.LessOrEqual => value <= Number + Tolerance,
            EConditionOperator.LessThan => value < Number,
            EConditionOperator.NotEquals => Mathf.Abs(value - Number) > Tolerance,
            _ => false
        };

        /// <summary>Compares a text fact against the condition.</summary>
        /// <param name="value">The value taken from the clip.</param>
        /// <returns>True when the condition holds.</returns>
        public bool MatchesText(string value)
        {
            string actual = value ?? string.Empty;
            string expected = Text ?? string.Empty;

            return Operator switch
            {
                EConditionOperator.Contains => Contains(actual, expected),
                EConditionOperator.Equals => string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase),
                EConditionOperator.Matches => MatchesPattern(actual, expected),
                EConditionOperator.NotContains => !Contains(actual, expected),
                EConditionOperator.NotEquals => !string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase),
                _ => false
            };
        }

        /// <summary>Compares a flag fact against the condition.</summary>
        /// <param name="value">The value taken from the clip.</param>
        /// <returns>True when the condition holds.</returns>
        public bool MatchesFlag(bool value)
        {
            bool expected = Number > 0.5f;

            return Operator switch
            {
                EConditionOperator.Equals => value == expected,
                EConditionOperator.NotEquals => value != expected,
                _ => false
            };
        }

        /// <summary>Short human readable form, shown in the rule list and the clip details.</summary>
        /// <returns>The condition as one line of text.</returns>
        public override string ToString()
        {
            string value = IsNumeric
                ? Number.ToString("0.##")
                : Describe();

            return $"{ObjectNamesLite(Field)} {ObjectNamesLite(Operator)} {value}";
        }

        // Spaces the enum entry out without pulling UnityEditor into a data type.
        private static string ObjectNamesLite(Enum value)
        {
            string raw = value.ToString();
            StringBuilder builder = new();

            for (int index = 0; index < raw.Length; index++)
            {
                if (index > 0
                    && char.IsUpper(raw[index])
                    && !char.IsUpper(raw[index - 1]))
                    builder.Append(' ');

                builder.Append(raw[index]);
            }

            return builder.ToString().ToLowerInvariant();
        }

        private static bool Contains(string actual, string expected) => expected.Length > 0
            && actual.Contains(expected, StringComparison.OrdinalIgnoreCase);

        private static bool MatchesPattern(string actual, string pattern)
        {
            if (pattern.Length == 0)
                return false;

            if (pattern.IndexOf(Wildcard) < 0)
                return string.Equals(actual, pattern, StringComparison.OrdinalIgnoreCase);

            string[] parts = pattern.Split(Wildcard);
            StringBuilder builder = new(PatternStart);

            for (int index = 0; index < parts.Length; index++)
            {
                if (index > 0)
                    builder.Append(AnyPattern);

                builder.Append(Regex.Escape(parts[index]));
            }

            builder.Append(PatternEnd);

            return Regex.IsMatch(actual, builder.ToString(), RegexOptions.IgnoreCase);
        }

        private string Describe() => IsFlag
            ? (Number > 0.5f).ToString()
            : Text;
    }
}