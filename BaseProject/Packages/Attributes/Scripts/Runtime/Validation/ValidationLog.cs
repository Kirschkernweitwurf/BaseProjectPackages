namespace Base.AttributePackage
{
    /// <summary>Builds the log message for a validation issue, including the overview-window pointer.</summary>
    public static class ValidationLog
    {
        /// <summary>Composes a single error line plus the window pointer.</summary>
        public static string Build(ReferenceIssue issue)
        {
            string where = issue.Owner != null
                ? issue.Owner.name
                : "<unknown>";

            return $"{where} -> {issue.Path}: {issue.Reason}\n{ReferenceWindowInfo.LogPointer}";
        }
    }
}