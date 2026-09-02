using Base.ToolsPackage.Editor.CodebaseGraph.Model;

namespace Base.ToolsPackage.Editor.CodebaseGraph.Analysis
{
    /// <summary>
    /// Ranks a finding by how likely it is to be worth acting on. A scan of a real project produces
    /// thousands of statements that are all true, and without a ranking the handful that matter are
    /// invisible among them. The rule of thumb is simple: the narrower the visibility and the more the
    /// code looks like something a person wrote and owns, the more a finding on it means.
    /// </summary>
    internal static class FindingSeverity
    {
        /// <summary>Ranks a finding on a member.</summary>
        /// <param name="finding">The finding being reported.</param>
        /// <param name="member">Member it was reported on.</param>
        /// <param name="declaring">Type the member is declared on.</param>
        /// <returns>How much attention it deserves.</returns>
        internal static ESeverity Resolve(EFinding finding, MemberNodeInfo member, TypeNodeInfo declaring)
        {
            if (IsAlwaysLow(finding))
                return ESeverity.Low;

            // The published surface of a distributable package exists to be called from elsewhere.
            if (PackageApi.IsSurface(member, declaring))
                return ESeverity.Low;

            if (finding == EFinding.SerializedNeverRead)
                return member.AssetUsageCount == 0
                    ? ESeverity.High
                    : ESeverity.Medium;

            if (member.Kind == EMemberKind.SerializedField || member.Kind == EMemberKind.EnumMember)
                return ESeverity.Low;

            if (member.Access == EAccessLevel.Private || member.Access == EAccessLevel.Internal)
                return ESeverity.High;

            return ESeverity.Medium;
        }

        /// <summary>Ranks a finding on a type.</summary>
        /// <param name="finding">The finding being reported.</param>
        /// <param name="type">Type it was reported on.</param>
        /// <returns>How much attention it deserves.</returns>
        internal static ESeverity Resolve(EFinding finding, TypeNodeInfo type)
        {
            if (IsAlwaysLow(finding))
                return ESeverity.Low;

            if (PackageApi.IsSurface(type))
                return ESeverity.Low;

            if (finding == EFinding.DeadType)
                return ESeverity.High;

            return ESeverity.Medium;
        }

        private static bool IsAlwaysLow(EFinding finding) => finding == EFinding.UnusedPublicApi
            || finding == EFinding.UnusedInterfaceMember
            || finding == EFinding.HighInstability
            || finding == EFinding.GodClass;
    }
}