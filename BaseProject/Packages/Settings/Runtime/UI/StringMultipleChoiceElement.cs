using Base.SettingsPackage.Core;

namespace Base.SettingsPackage.UI
{
    /// <summary>
    /// Multiple choice element whose selected option label is stored in a <see cref="StringSetting"/>.
    /// Subclasses may override <see cref="MultipleChoiceElement{TValue,TSetting}.ResolveOptions"/>
    /// to supply options at runtime.
    /// </summary>
    public class StringMultipleChoiceElement : MultipleChoiceElement<string, StringSetting>
    {
        /// <inheritdoc/>
        protected override int IndexOf(string value) => IndexOfOption(value);

        /// <inheritdoc/>
        protected override string ValueAt(int index) => Options[index];
    }
}