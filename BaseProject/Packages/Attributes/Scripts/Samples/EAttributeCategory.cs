namespace Base.AttributePackage.Samples
{
    /// <summary>
    /// The groups the reference list sorts its attributes into.
    /// </summary>
    /// <remarks>
    /// An enum rather than a free string, so a typo cannot quietly open an eleventh category with one
    /// entry in it. The name is nicified for display, which is why the two-word ones are written as one.
    /// </remarks>
    internal enum EAttributeCategory : byte
    {
        /// <summary>Methods that run when a value changes, and buttons that run on demand.</summary>
        Callbacks = 0,

        /// <summary>Lists, arrays and tables.</summary>
        Collections = 1,

        /// <summary>Showing, hiding and greying a field based on another one.</summary>
        Conditions = 2,

        /// <summary>Headings, boxes, spacing and grouping.</summary>
        Layout = 3,

        /// <summary>Fields that offer a list of valid answers instead of free text.</summary>
        Pickers = 4,

        /// <summary>References that fill or constrain themselves.</summary>
        References = 5,

        /// <summary>The types Unity cannot serialize on its own.</summary>
        Serialization = 6,

        /// <summary>Fields that complain when they are wrong.</summary>
        Validation = 7,

        /// <summary>Controls that replace or extend the plain field.</summary>
        Widgets = 8
    }
}