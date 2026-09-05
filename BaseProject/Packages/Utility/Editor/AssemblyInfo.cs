using System.Runtime.CompilerServices;

// Assembly wide, so it lives at the assembly root rather than inside one drawer's folder. The runtime
// half of this package is well covered while the editor half had no way of being reached at all: the
// test assembly never referenced it and nothing opened its internals. This closes the second half.
[assembly: InternalsVisibleTo("Base.UtilityPackage.Tests")]