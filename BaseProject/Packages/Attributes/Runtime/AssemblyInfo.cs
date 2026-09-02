using System.Runtime.CompilerServices;

// The runtime assembly holds the attributes and the validation rules behind them. The rules, the scanner
// and the shared reflection helpers are implementation detail rather than API, so they are internal, and
// the editor assembly is the one place outside that legitimately reads them. The test assembly is the
// other, since a rule that cannot be reached cannot be covered.
[assembly: InternalsVisibleTo("Base.AttributesPackage.Editor")]
[assembly: InternalsVisibleTo("Base.AttributesPackage.Tests")]