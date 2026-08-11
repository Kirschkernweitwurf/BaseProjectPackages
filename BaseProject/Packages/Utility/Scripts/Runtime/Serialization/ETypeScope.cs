namespace Base.UtilityPackage.Serialization
{
    /// <summary>Which assemblies a <see cref="TypeReference"/> picker offers types from.</summary>
    public enum ETypeScope : byte
    {
        /// <summary>
        /// Only types declared in project code. This is the default, because an unconstrained picker over
        /// every loaded assembly is tens of thousands of entries, almost none of which anyone wants.
        /// </summary>
        Project = 0,

        /// <summary>Project code plus the Unity and .NET assemblies.</summary>
        Everything = 1
    }
}
