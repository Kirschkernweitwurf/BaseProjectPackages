using System.Collections.Generic;
using Base.UtilityPackage.Logging;
using Object = UnityEngine.Object;

namespace Base.AttributesPackage
{
    /// <summary>
    /// Runs the validation attributes of an object from code and hands back what is wrong, so a tool
    /// outside this package can show the same problems the inspector shows without copying the rules.
    /// </summary>
    public static class ReferenceValidation
    {
        private const string PathReasonSeparator = " ";

        // Reused across calls, because a table validates every row and would otherwise allocate a
        // list per row on every rebuild.
        private static readonly List<ReferenceIssue> Buffer = new();

        /// <summary>
        /// Appends one line per violated validation attribute on <paramref name="target"/>, at any
        /// depth, for example "displayName is required".
        /// </summary>
        /// <param name="target">The component or asset to check.</param>
        /// <param name="problems">The list the problems are appended to.</param>
        public static void Collect(Object target, List<string> problems)
        {
            if (problems == null)
            {
                CustomLogger.LogError($"{nameof(Collect)} needs a list to append the problems to.", target);
                return;
            }

            if (target == null)
            {
                CustomLogger.LogWarning($"{nameof(Collect)} was called without a target.", null);
                return;
            }

            Buffer.Clear();
            ReferenceValidationScanner.Collect(target, Buffer);

            foreach (ReferenceIssue issue in Buffer)
                problems.Add(issue.Path + PathReasonSeparator + issue.Reason);

            Buffer.Clear();
        }
    }
}