using System.Runtime.CompilerServices;

// Assembly wide, so it lives at the assembly root rather than inside one drawer's folder. Almost the
// whole editor half of this package is internal, which until now put it out of reach of any test at
// all. Opening it to the test assembly is what makes the drawers and their helpers coverable.
[assembly: InternalsVisibleTo("Base.AttributesPackage.Tests")]