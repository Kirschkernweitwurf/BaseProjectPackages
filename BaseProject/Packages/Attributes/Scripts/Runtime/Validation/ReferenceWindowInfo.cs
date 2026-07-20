namespace Base.AttributePackage
{
    /// <summary>Shared pointer text so logs can direct the user to the overview window.</summary>
    public static class ReferenceWindowInfo
    {
        /// <summary>Pointer appended to validation logs, with the location on its own line.</summary>
        public const string LogPointer = "See Required References window\n" + MenuLocation;

        /// <summary>Human-readable menu location of the overview window.</summary>
        public const string MenuLocation = "Tools > Base Packages > Unity Editor > References > Required References";
    }
}