using Base.ToolPackage.Editor.CodebaseGraph.Model;

namespace Base.ToolPackage.Editor.CodebaseGraph.Analysis
{
    /// <summary>
    /// Decides what counts as the published surface of a distributable package. Public is the obvious
    /// part, but protected on a type someone can derive from is a contract too: the subclasses live in
    /// the projects that install the package, which a scan of this project can never see.
    /// </summary>
    internal static class PackageApi
    {
        /// <summary>True when the member is part of what consumers of the package are meant to use.</summary>
        /// <param name="member">Member to test.</param>
        /// <param name="declaring">Type the member is declared on.</param>
        /// <returns>True when nothing here using it proves nothing.</returns>
        internal static bool IsSurface(MemberNodeInfo member, TypeNodeInfo declaring)
        {
            if (declaring == null || !declaring.IsPackageAssembly)
                return false;

            // A type that shares a namespace with an editor window is part of that window, not part of
            // anything a consumer calls. Treating it as published surface hides real dead code among
            // the genuine extension points.
            if (declaring.IsWindowOwned)
                return false;

            if (member.Access == EAccessLevel.Public)
                return true;

            // Protected on a sealed type really is unreachable, so that one still deserves reporting.
            if (declaring.IsSealed)
                return false;

            return member.Access == EAccessLevel.Protected
                || member.Access == EAccessLevel.ProtectedInternal;
        }

        /// <summary>True when the type itself is part of the published surface.</summary>
        /// <param name="type">Type to test.</param>
        /// <returns>True when nothing here using it proves nothing.</returns>
        internal static bool IsSurface(TypeNodeInfo type) => type.IsPackageAssembly
            && type.Access == EAccessLevel.Public;
    }
}