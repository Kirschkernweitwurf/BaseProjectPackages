namespace Base.AttributePackage.Editor.Windows.AttributeTroubleshoot.Samples
{
    /// <summary>
    /// Attributes pointing at methods whose signature no longer matches, so the samples tab can show the
    /// case where a button never appears or a callback quietly stops firing.
    /// </summary>
    /// <remarks>
    /// The broken method names are string literals on purpose, matching the state a field ends up in
    /// after the method it pointed at was renamed.
    /// </remarks>
    [TroubleshootSample]
    internal sealed class SampleMethodIssues
    {
        /// <summary>Points at a callback that no longer exists.</summary>
        [OnValueChanged("RenamedCallback")] public int missingCallback;

        /// <summary>An int has no element count, so the size callback can never fire.</summary>
        [OnArraySizeChanged(nameof(OnResized))] public int notACollection;

        /// <summary>Points at a button method that no longer exists.</summary>
        [InlineButton("RenamedRoll")] public int missingInlineMethod;

        /// <summary>Points at a validator that does not return bool.</summary>
        [ValidateInput(nameof(Describe))] public int wrongValidator;

        /// <summary>A button method may not take parameters, so no button is drawn.</summary>
        [Button]
        private void TakesParameter(int passes) { }

        /// <summary>A header button method may not take parameters either.</summary>
        [HeaderButton]
        private void AlsoTakesParameter(float weight) { }

        /// <summary>A valid size callback, pointed at by a field that is not a collection.</summary>
        private void OnResized(int size) { }

        /// <summary>Returns a string, so it cannot answer whether the value is valid.</summary>
        private string Describe(int value) => value.ToString();
    }
}