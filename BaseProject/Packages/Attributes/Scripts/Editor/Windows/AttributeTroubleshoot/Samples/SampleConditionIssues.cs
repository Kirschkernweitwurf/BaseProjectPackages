namespace Base.AttributePackage.Editor.Windows.AttributeTroubleshoot.Samples
{
    /// <summary>
    /// Deliberately broken conditional attributes, so the samples tab can show what an unresolvable
    /// condition looks like. An unresolved condition evaluates to true, which is why these never look
    /// wrong in the inspector.
    /// </summary>
    /// <remarks>
    /// The broken member names are string literals on purpose. That is exactly the state a field ends up
    /// in after the member it pointed at was renamed, which no <c>nameof</c> could reproduce.
    /// </remarks>
    [TroubleshootSample]
    public sealed class SampleConditionIssues
    {
        /// <summary>A valid bool the working conditions point at.</summary>
        public bool isEnabled;

        /// <summary>A float, used to show a condition pointing at the wrong type.</summary>
        public float speed;

        /// <summary>An enum, used by the enum condition samples.</summary>
        public ESampleMode mode;

        /// <summary>Points at a member that no longer exists.</summary>
        [ShowIf("wasRenamedAway")] public int renamedMember;

        /// <summary>Points at a member that exists but is not a bool.</summary>
        [ShowIf(nameof(speed))] public int notABool;

        /// <summary>Lists no members at all.</summary>
        [ShowIf] public int noMembers;

        /// <summary>One of two members is gone, so the whole condition is broken.</summary>
        [EnableIf(nameof(isEnabled), "alsoRenamedAway")] public int partlyMissing;

        /// <summary>Compares an enum member against a value of a different type.</summary>
        [ShowIfEnum(nameof(mode), "Fast")] public int wrongEnumValue;

        /// <summary>Points at a float as if it were an enum.</summary>
        [ShowIfEnum(nameof(speed), ESampleMode.Fast)] public int notAnEnum;
    }
}