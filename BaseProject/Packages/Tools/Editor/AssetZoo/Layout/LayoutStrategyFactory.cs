using Base.ToolsPackage.Editor.AssetZoo.Config;

namespace Base.ToolsPackage.Editor.AssetZoo.Layout
{
    /// <summary>
    /// Factory for creating layout strategies based on the type specified in the settings.
    /// </summary>
    internal static class LayoutStrategyFactory
    {
        /// <summary>
        /// Creates an instance of <see cref="ILayoutStrategy"/> based on the provided <see cref="ELayoutType"/>.
        /// </summary>
        /// <param name="type">The type of layout strategy to create.</param>
        /// <returns>An instance of <see cref="ILayoutStrategy"/> corresponding to the specified type.</returns>
        public static ILayoutStrategy Create(ELayoutType type) => type switch
        {
            ELayoutType.Line => new RowLayoutStrategy(true),
            ELayoutType.Circle => new CircleLayoutStrategy(),
            _ => new RowLayoutStrategy(false)
        };
    }
}