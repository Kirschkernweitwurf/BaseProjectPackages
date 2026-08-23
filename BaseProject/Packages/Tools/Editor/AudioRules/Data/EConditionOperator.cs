namespace Base.ToolPackage.Editor.AudioRules.Data
{
    /// <summary>How a condition compares the clip against its value.</summary>
    public enum EConditionOperator : byte
    {
        /// <summary>The text contains the value.</summary>
        Contains = 0,

        /// <summary>The value matches exactly. Numbers compare with a small tolerance.</summary>
        Equals = 1,

        /// <summary>The number is at least the value.</summary>
        GreaterOrEqual = 2,

        /// <summary>The number is above the value.</summary>
        GreaterThan = 3,

        /// <summary>The number is at most the value.</summary>
        LessOrEqual = 4,

        /// <summary>The number is below the value.</summary>
        LessThan = 5,

        /// <summary>The text matches a pattern where * stands for any run of characters.</summary>
        Matches = 6,

        /// <summary>The text does not contain the value.</summary>
        NotContains = 7,

        /// <summary>The value does not match.</summary>
        NotEquals = 8
    }
}