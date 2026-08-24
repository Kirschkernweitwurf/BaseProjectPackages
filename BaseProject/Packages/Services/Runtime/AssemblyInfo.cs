using System.Runtime.CompilerServices;

// Assembly wide, so it lives at the assembly root rather than inside one system's folder. The
// locator's registration table is implementation detail rather than API, so it stays internal and the
// editor assembly is the one place outside that legitimately reads it. InternalsVisibleTo opens every
// internal in Base.ServicePackage to it, not only that table, which is what it does and cannot be
// narrowed.
[assembly: InternalsVisibleTo("Base.ServicePackage.Editor")]