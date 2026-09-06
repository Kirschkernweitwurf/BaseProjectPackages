using System.Runtime.CompilerServices;

// Assembly wide, so it lives at the assembly root rather than inside one system's folder. The
// tracking table, the pool wrapper and the source configurator are implementation detail rather than
// API, which is what keeps them internal, and the two test assemblies are the only places outside
// that have a reason to reach them. InternalsVisibleTo opens every internal in Base.AudioPackage to
// them, not only those three, which is what it does and cannot be narrowed.
[assembly: InternalsVisibleTo("Base.AudioPackage.Tests")]
[assembly: InternalsVisibleTo("Base.AudioPackage.PlayTests")]