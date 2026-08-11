using System.Runtime.CompilerServices;

// The runtime assembly holds the attributes and the validation rules behind them. The rules, the scanner
// and the shared reflection helpers are implementation detail rather than API, so they are internal, and
// the editor assembly is the one place outside that legitimately reads them.
[assembly: InternalsVisibleTo("Base.AttributePackage.Editor")]