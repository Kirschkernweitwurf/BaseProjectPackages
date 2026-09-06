using System.Runtime.CompilerServices;

// The bus keeps its handler table internal and the inspector window is the one place outside this
// assembly that legitimately reads it. InternalsVisibleTo opens every internal here to that window,
// not only the table, which is what it does and cannot be narrowed.
[assembly: InternalsVisibleTo("Base.CorePackage.Editor.EventBusInspector")]