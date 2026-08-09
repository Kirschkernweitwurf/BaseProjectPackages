using Base.ToolPackage.Editor.CodebaseGraph.Model;

namespace Base.ToolPackage.Editor.CodebaseGraph.Analysis
{
    /// <summary>Thresholds that turn raw coupling numbers into findings.</summary>
    public static class CouplingMetrics
    {
        private const int GodClassFanOut = 25;
        private const int GodClassMemberCount = 40;
        private const int InstabilityMinimumFanIn = 3;
        private const float InstabilityThreshold = 0.8f;

        /// <summary>True when the type carries so much that it almost certainly does more than one job.</summary>
        /// <param name="type">Type to test.</param>
        /// <returns>True when the type looks overloaded.</returns>
        public static bool IsGodClass(TypeNodeInfo type)
            => type.Members.Count > GodClassMemberCount || type.FanOut > GodClassFanOut;

        /// <summary>
        /// True when a lot of code depends on this type while it in turn depends on a lot of others.
        /// Changing it then ripples outward, which is exactly what a stable shared type should not do.
        /// </summary>
        /// <param name="type">Type to test.</param>
        /// <returns>True when the type is a risky dependency.</returns>
        public static bool IsUnstableDependency(TypeNodeInfo type)
            => type.FanIn >= InstabilityMinimumFanIn && type.Instability > InstabilityThreshold;
    }
}
