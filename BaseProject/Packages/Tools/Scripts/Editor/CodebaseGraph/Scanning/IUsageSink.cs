using Base.ToolPackage.Editor.CodebaseGraph.Model;

namespace Base.ToolPackage.Editor.CodebaseGraph.Scanning
{
    /// <summary>Receives the raw usages the scanners find and folds them into the graph.</summary>
    public interface IUsageSink
    {
        /// <summary>Records that one member uses another.</summary>
        /// <param name="sourceKey">Member the usage starts at.</param>
        /// <param name="targetKey">Member being used.</param>
        /// <param name="kind">What kind of usage this is.</param>
        void AddMemberUsage(MemberKey sourceKey, MemberKey targetKey, EUsageKind kind);

        /// <summary>Records that one member references a type without touching a specific member.</summary>
        /// <param name="sourceKey">Member the usage starts at.</param>
        /// <param name="targetKey">Type being referenced.</param>
        void AddTypeUsage(MemberKey sourceKey, TypeKey targetKey);

        /// <summary>Records that one type references another directly, for example by deriving from it.</summary>
        /// <param name="sourceKey">Type the usage starts at.</param>
        /// <param name="targetKey">Type being referenced.</param>
        void AddTypeRelation(TypeKey sourceKey, TypeKey targetKey);

        /// <summary>Adds the size of a compiled body to the member it belongs to.</summary>
        /// <param name="sourceKey">Member the body belongs to.</param>
        /// <param name="size">Size of the body in bytes.</param>
        void AddIlSize(MemberKey sourceKey, int size);

        /// <summary>Records that a metadata token could not be resolved and was skipped.</summary>
        void ReportUnresolvedToken();
    }
}
