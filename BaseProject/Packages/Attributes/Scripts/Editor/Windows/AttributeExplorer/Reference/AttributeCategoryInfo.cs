using Base.AttributePackage.Samples;
namespace Base.AttributePackage.Editor.Windows.AttributeExplorer.Reference
{
    /// <summary>
    /// The one-line explanation shown on a category page.
    /// </summary>
    /// <remarks>
    /// Kept here rather than as a doc comment on the enum, since a doc comment is stripped from the
    /// assembly and cannot be read back at runtime.
    /// </remarks>
    internal static class AttributeCategoryInfo
    {
        /// <summary>Describes what belongs in the given category.</summary>
        /// <param name="category">The category to describe.</param>
        /// <returns>One sentence, or an empty string when the category has none.</returns>
        internal static string Describe(EAttributeCategory category) => category switch
        {
            EAttributeCategory.Callbacks => "Methods that run when a value changes, buttons that run on "
                + "demand, and members shown in the inspector that Unity never serializes.",
            EAttributeCategory.Collections => "Lists and arrays drawn as something better than a stack of "
                + "foldouts.",
            EAttributeCategory.Conditions => "Showing, hiding and greying a field based on another one, so "
                + "a component only offers what currently applies.",
            EAttributeCategory.Layout => "Headings, boxes, spacing and grouping. Nothing here changes a "
                + "value; all of it changes how a component reads.",
            EAttributeCategory.Pickers => "Fields that offer a list of valid answers instead of free text, "
                + "so a name cannot be spelled wrong.",
            EAttributeCategory.References => "References that fill themselves from the project rather than "
                + "waiting to be dragged in.",
            EAttributeCategory.Serialization => "The types Unity cannot store on its own.",
            EAttributeCategory.Validation => "Fields that say so when they are wrong, while the object is "
                + "being set up rather than at runtime.",
            EAttributeCategory.Widgets => "Controls that replace or extend the plain field.",
            _ => string.Empty
        };
    }
}