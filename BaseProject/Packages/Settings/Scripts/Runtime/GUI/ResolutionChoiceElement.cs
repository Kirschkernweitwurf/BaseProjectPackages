using System.Collections.Generic;
using Base.SettingsPackage.Display;

namespace Base.SettingsPackage.GUI
{
    /// <summary>
    /// Resolution picker that populates its options from the available display resolutions at bind time,
    /// storing the chosen "{width}x{height}" label in its string setting.
    /// </summary>
    public sealed class ResolutionChoiceElement : StringMultipleChoiceElement
    {
        /// <inheritdoc/>
        protected override List<string> ResolveOptions() => new(ResolutionProvider.GetAvailableResolutions());
    }
}