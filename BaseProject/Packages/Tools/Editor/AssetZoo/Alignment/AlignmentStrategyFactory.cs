using Base.ToolPackage.Editor.AssetZoo.Config;
using Base.UtilityPackage.Logging;

namespace Base.ToolPackage.Editor.AssetZoo.Alignment
{
    /// <summary>
    /// A factory class for creating instances of <see cref="IAlignmentStrategy"/>
    /// based on the specified <see cref="EAlignmentMode"/>.
    /// </summary>
    public static class AlignmentStrategyFactory
    {
        /// <summary>
        /// Creates an instance of <see cref="IAlignmentStrategy"/> based on the provided <see cref="EAlignmentMode"/>.
        /// </summary>
        /// <param name="mode">The alignment mode for which to create the strategy.</param>
        /// <returns>>An instance of <see cref="IAlignmentStrategy"/> corresponding to the specified mode.</returns>
        public static IAlignmentStrategy Create(EAlignmentMode mode)
        {
            switch (mode)
            {
                case EAlignmentMode.Ground:
                {
                    return new GroundAlignment();
                }
                case EAlignmentMode.Center:
                {
                    return new CenterAlignment();
                }
                case EAlignmentMode.Pivot:
                {
                    return new PivotAlignment();
                }
                default:
                {
                    CustomLogger.LogWarning($"Unknown alignment mode {mode}, defaulting to Pivot.", null);
                    return new PivotAlignment();
                }
            }
        }
    }
}