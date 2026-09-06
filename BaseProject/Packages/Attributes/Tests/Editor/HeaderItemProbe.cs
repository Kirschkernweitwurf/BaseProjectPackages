using UnityEngine;

namespace Base.AttributesPackage.Tests
{
    /// <summary>
    /// Declares one of every shape of header control, including the ones that are supposed to be
    /// skipped. Reflected over rather than instantiated, so it needs to be nothing but a type.
    /// </summary>
    internal sealed class HeaderItemProbe
    {
        /// <summary>The label the named button is declared with.</summary>
        internal const string GivenLabel = "Named Button";

        /// <summary>The label a property control reads.</summary>
        [HeaderLabel] internal string ReadableLabel => "read";

        /// <summary>A property nothing can read, so no control is made for it.</summary>
        [HeaderLabel] internal string WriteOnlyLabel
        {
            set => _ = value;
        }

        /// <summary>A property with no attribute at all.</summary>
        internal string Plain => "plain";

        /// <summary>A button with the label left out, which falls back to the method name.</summary>
        [HeaderButton]
        internal void DoTheThing()
        {
        }

        /// <summary>A button with a label of its own.</summary>
        [HeaderButton(GivenLabel)]
        internal void Named()
        {
        }

        /// <summary>A button whose label is already the method name, so the tooltip adds nothing.</summary>
        [HeaderButton(nameof(SameName))]
        internal void SameName()
        {
        }

        /// <summary>A button that only runs while the game does.</summary>
        [HeaderButton(Mode = EButtonMode.PlayMode)]
        internal void PlayOnly()
        {
        }

        /// <summary>A button that takes an argument, which the header cannot supply.</summary>
        /// <param name="ignored">Never passed, because this is never made into a control.</param>
        [HeaderButton]
        internal void TakesAnArgument(int ignored) => _ = ignored;

        /// <summary>A label method that returns what it shows.</summary>
        /// <returns>The text the header displays.</returns>
        [HeaderLabel]
        internal string ReadState() => "state";

        /// <summary>A label method returning nothing, so there is nothing to show.</summary>
        [HeaderLabel]
        internal void ReturnsNothing()
        {
        }

        /// <summary>A draw method with the rect it is given.</summary>
        /// <param name="rect">The area handed to it by the header.</param>
        [HeaderDraw]
        internal void DrawInto(Rect rect) => _ = rect;

        /// <summary>A draw method taking the wrong argument, so the header skips it.</summary>
        /// <param name="wrong">Never passed.</param>
        [HeaderDraw]
        internal void DrawWrongArgument(int wrong) => _ = wrong;
    }
}