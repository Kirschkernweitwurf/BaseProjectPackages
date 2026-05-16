using Base.ToolPackage.Editor.AssetZoo.Runtime.Config;
using Base.UtilityPackage.Logging;

namespace Base.ToolPackage.Editor.AssetZoo.Runtime.Layout
{
    /// <summary>
    /// Factory for creating layout strategies based on the type specified in the settings.
    /// </summary>
    public static class LayoutStrategyFactory
    {
        /// <summary>
        /// Creates an instance of <see cref="ILayoutStrategy"/> based on the provided <see cref="ELayoutType"/>.
        /// </summary>
        /// <param name="type">The type of layout strategy to create.</param>
        /// <returns>An instance of <see cref="ILayoutStrategy"/> corresponding to the specified type.</returns>
        public static ILayoutStrategy Create(ELayoutType type)
        {
            switch (type)
            {
                case ELayoutType.Grid:
                {
                    return new GridLayoutStrategy();
                }
                case ELayoutType.Line:
                {
                    return new LineLayoutStrategy();
                }
                case ELayoutType.Circle:
                {
                    return new CircleLayoutStrategy();
                }
                default:
                {
                    CustomLogger.LogWarning($"Unknown layout type {type}, defaulting to Grid.", null);
                    return new GridLayoutStrategy();
                }
            }
        }
    }
}