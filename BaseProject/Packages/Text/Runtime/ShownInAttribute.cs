using System;

namespace Base.TextPackage
{
    /// <summary>
    /// Tells the text fit check which text box a localized string ends up in, for a string that code writes into a
    /// label instead of a Localize String Event. Put it on a <see cref="UnityEngine.Localization.LocalizedString"/>
    /// field or on a list of the strings a box shows one at a time and name the <see cref="TMPro.TMP_Text"/> field of
    /// the same component: <c>[ShownIn(nameof(resultLabel))]</c>. The check then renders the string in every language
    /// in that box. Code that swaps the string of a Localize String Event names that field instead; the check follows
    /// it to the label it writes into.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ShownInAttribute : Attribute
    {
        /// <summary>Marks the string as shown in the text box field named <paramref name="textField"/>.</summary>
        /// <param name="textField">
        /// The name of a <see cref="TMPro.TMP_Text"/> or Localize String Event field on the same component.
        /// </param>
        public ShownInAttribute(string textField) => TextField = textField;

        /// <summary>The name of the text box field on the same component.</summary>
        public string TextField { get; }
    }
}