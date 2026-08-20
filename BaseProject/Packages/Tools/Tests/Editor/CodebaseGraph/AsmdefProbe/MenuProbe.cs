using Base.CorePackage.MenuManaging;

namespace AsmdefProbe
{
    /// <summary>
    /// Fixture for <see cref="Base.ToolPackage.Editor.Tests.AssemblyEdgeRollUpTests"/>. This assembly
    /// references Base.CorePackage and nothing else, so it does not reference Base.ServicePackage,
    /// where the IShutdownHandler interface on <see cref="Menu"/> lives.
    /// <para>
    /// That this compiles is the result worth keeping: inheriting a type whose interface lives in a
    /// third assembly does not force a reference to that assembly. The roll-up used to record the
    /// inherited interface as a usage anyway, which produced an assembly edge the asmdef did not
    /// declare and could not have declared. Deleting this file removes the only case in the project
    /// that catches that coming back.
    /// </para>
    /// </summary>
    internal sealed class MenuProbe : Menu
    {
    }
}