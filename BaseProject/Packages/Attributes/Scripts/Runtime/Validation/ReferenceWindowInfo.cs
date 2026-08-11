namespace Base.AttributePackage
{
    /// <summary>Shared menu path and pointer text so logs can direct the user to the overview window.</summary>
    internal static class ReferenceWindowInfo
    {
        /// <summary>Menu path of the overview window. Also used to register the menu entry.</summary>
        public const string MenuPath = "Tools/Base Packages/Unity Editor/References/Required References";

        /// <summary>Title of the overview window.</summary>
        public const string WindowTitle = "Required References";

        private const string PathSeparator = "/";

        private const string ReadableSeparator = " > ";
        /// <summary>Human-readable menu location of the overview window.</summary>
        private static readonly string MenuLocation = MenuPath.Replace(PathSeparator, ReadableSeparator);
        /// <summary>Pointer appended to validation logs, with the location on its own line.</summary>
        public static readonly string LogPointer = $"See {WindowTitle} window\n {MenuLocation} for details.";
    }
}