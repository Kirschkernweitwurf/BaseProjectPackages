using System.Runtime.CompilerServices;

// The samples assembly holds the demonstration objects the reference tab draws, plus the marker and the
// category they are declared with. None of that is API for anyone else, so it stays internal, and the
// editor assembly is the one place outside that legitimately reads it.
[assembly: InternalsVisibleTo("Base.AttributePackage.Editor")]