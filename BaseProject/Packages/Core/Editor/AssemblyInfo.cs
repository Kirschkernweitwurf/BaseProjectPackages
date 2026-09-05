using System.Runtime.CompilerServices;

// Assembly wide, so it lives at the assembly root rather than inside one window's folder. The runtime
// half of this package already opens itself to the tests; the editor half held 28 files that no test
// could name, which is why the windows and the layout behind them had nothing on them.
[assembly: InternalsVisibleTo("Base.CorePackage.Tests")]