using Base.ToolPackage.Editor.CodebaseGraph.Model;
using UnityEditor;

namespace Base.ToolPackage.Editor.CodebaseGraph.Editing
{
    /// <summary>
    /// Confirms a quick fix and applies it. The wording matters as much as the edit here, because the
    /// person agreeing to it is agreeing to have their source rewritten, so the dialog says exactly what
    /// will change and on what terms the edit will refuse itself.
    /// </summary>
    internal static class CodebaseGraphQuickFix
    {
        private const string ApplyLabel = "Apply";
        private const string CancelLabel = "Cancel";
        private const string DemoteTitle = "Make member internal";
        private const string InternalChange = "changing public to internal";
        private const string PrivateChange = "lowering it to private";
        private const string PrivateTitle = "Make member private";
        private const string ReadOnlyChange = "adding readonly";
        private const string ReadOnlyTitle = "Make field readonly";

        private const string WarningFormat = "This edits your source file: {0} in {1}, {2}.\n\nIt finds "
            + "the line by name and refuses if anything looks unclear, so it will do nothing rather than "
            + "the wrong thing. Commit your work first, then check the console after Unity recompiles.";

        /// <summary>Asks for confirmation and rewrites the source when it is given.</summary>
        /// <param name="entry">Entry the finding sits on.</param>
        /// <param name="finding">Finding being acted on.</param>
        /// <returns>True when a file was changed.</returns>
        public static bool Apply(GraphEntry entry, EFinding finding)
        {
            if (entry?.Member == null || entry.Type == null)
                return false;

            bool confirmed = EditorUtility.DisplayDialog(ReadTitle(finding),
                string.Format(WarningFormat, entry.Member.Name, entry.Type.ShortName, ReadChange(finding)),
                ApplyLabel,
                CancelLabel);

            if (!confirmed)
                return false;

            return Rewrite(entry, finding);
        }

        private static bool Rewrite(GraphEntry entry, EFinding finding)
        {
            switch (finding)
            {
                case EFinding.PrivateCandidate:
                    return MemberSourceEditor.DemoteToPrivate(entry.Type, entry.Member);

                case EFinding.PublicButInternalOnly:
                    return MemberSourceEditor.DemoteToInternal(entry.Type, entry.Member);

                default:
                    return MemberSourceEditor.AddReadOnly(entry.Type, entry.Member);
            }
        }

        private static string ReadTitle(EFinding finding)
        {
            switch (finding)
            {
                case EFinding.PrivateCandidate:
                    return PrivateTitle;

                case EFinding.PublicButInternalOnly:
                    return DemoteTitle;

                default:
                    return ReadOnlyTitle;
            }
        }

        private static string ReadChange(EFinding finding)
        {
            switch (finding)
            {
                case EFinding.PrivateCandidate:
                    return PrivateChange;

                case EFinding.PublicButInternalOnly:
                    return InternalChange;

                default:
                    return ReadOnlyChange;
            }
        }
    }
}