using System;

namespace Base.ToolPackage.MenuManagerWindow
{
    /// <summary>
    /// Marks a static method as a data driven editor menu item. Path and priority are managed in the Menu Manager
    /// window.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public sealed class DynamicMenuItemAttribute : Attribute
    {
        /// <summary>Full menu path used until it is changed in the window, for example "Tools/My Tool".</summary>
        public string DefaultPath { get; }

        /// <summary>Optional name of a static bool method in the same type used as the validate function.</summary>
        public string ValidateMethod { get; }

        /// <summary>Creates the attribute.</summary>
        public DynamicMenuItemAttribute(string defaultPath = "", string validateMethod = "")
        {
            DefaultPath = defaultPath;
            ValidateMethod = validateMethod;
        }
    }
}