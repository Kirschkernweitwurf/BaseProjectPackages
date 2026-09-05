using System.Runtime.CompilerServices;

// Assembly wide, so it lives at the assembly root rather than beside one window. This package had no
// test assembly at all, which is the only reason its sync guards had nothing on them.
[assembly: InternalsVisibleTo("Base.LocalizationPackage.Tests")]