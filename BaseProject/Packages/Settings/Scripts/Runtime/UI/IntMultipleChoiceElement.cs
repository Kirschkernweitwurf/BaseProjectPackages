using Base.SettingsPackage.Core;
using UnityEngine;

namespace Base.SettingsPackage.UI
{
    /// <summary>
    /// Multiple choice element whose selected index is stored directly in an <see cref="IntSetting"/>.
    /// </summary>
    public sealed class IntMultipleChoiceElement : MultipleChoiceElement<int, IntSetting>
    {
        /// <inheritdoc/>
        protected override int IndexOf(int value) => Mathf.Clamp(value, 0, Mathf.Max(0, Options.Count - 1));

        /// <inheritdoc/>
        protected override int ValueAt(int index) => index;
    }
}