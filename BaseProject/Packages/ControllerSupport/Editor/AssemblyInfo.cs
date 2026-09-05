using System.Runtime.CompilerServices;

// Assembly wide, so it lives at the assembly root rather than inside one window's folder. The runtime
// half of this package is covered while the editor half could not be named by any test, which left the
// validator that decides what joins gamepad navigation with nothing on it.
[assembly: InternalsVisibleTo("Base.ControllerSupportPackage.Tests")]