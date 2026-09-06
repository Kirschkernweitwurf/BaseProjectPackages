using System.Runtime.CompilerServices;

// Assembly wide, so it lives at the assembly root rather than inside one system's folder. The two
// serialized field names are implementation detail rather than API, which is what keeps them
// internal, and the test assembly is the one place outside that has a reason to read them.
// InternalsVisibleTo opens every internal in this assembly to it, not only those two, which is what
// it does and cannot be narrowed.
[assembly: InternalsVisibleTo("Base.LocalizationPackage.Tests")]