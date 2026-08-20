using System;
using JetBrains.Annotations;

namespace Base.UtilityPackage.Menus
{
    /// <summary>
    /// Marks a static method as a data driven editor menu item. Path and priority are managed in the Menu Manager
    /// window.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    [MeansImplicitUse]
    public sealed class DynamicMenuItemAttribute : Attribute
    {
        /// <summary>Full menu path used until it is changed in the window, for example "Tools/My Tool".</summary>
        public string DefaultPath { get; }

        /// <summary>Optional name of a static bool method in the same type used as the validate function.</summary>
        public string ValidateMethod { get; }

        /// <summary>
        /// Optional name of a static bool method in the same type that reports whether the entry should
        /// show a check mark. The Menu Manager applies it against the path the entry actually sits at,
        /// so the mark keeps working after the entry is moved or renamed.
        /// </summary>
        public string CheckedMethod { get; }

        /// <summary>Creates the attribute.</summary>
        public DynamicMenuItemAttribute(string defaultPath = "", string validateMethod = "",
            string checkedMethod = "")
        {
            DefaultPath = defaultPath;
            ValidateMethod = validateMethod;
            CheckedMethod = checkedMethod;
        }
    }
}