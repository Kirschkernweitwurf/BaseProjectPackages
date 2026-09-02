using Base.UtilityPackage.Menus;
using UnityEditorInternal;

namespace Base.AttributesPackage.Editor.Core
{
    /// <summary>
    /// Puts <see cref="AttributeInspectorSwitch"/> on a menu item, so the package can be switched off
    /// from inside the editor rather than by editing a preference by hand. The check mark reports the
    /// current state, and the Menu Manager applies it against wherever the entry actually sits.
    /// </summary>
    internal static class AttributeInspectorSwitchMenu
    {
        private const string MenuPath = "Tools/Base Packages/Attributes/Disable Attribute Inspector";

        /// <summary>Flips the switch and repaints so every open inspector picks the new state up.</summary>
        [DynamicMenuItem(MenuPath, checkedMethod: nameof(IsDisabled))]
        private static void Toggle()
        {
            AttributeInspectorSwitch.IsDisabled = !AttributeInspectorSwitch.IsDisabled;

            InternalEditorUtility.RepaintAllViews();
        }

        private static bool IsDisabled() => AttributeInspectorSwitch.IsDisabled;
    }
}