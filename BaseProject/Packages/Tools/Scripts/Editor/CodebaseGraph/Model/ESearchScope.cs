namespace Base.ToolPackage.Editor.CodebaseGraph.Model
{
    /// <summary>
    /// How far a search reaches. Narrowing the level you are already on and hunting for something you
    /// cannot place are different jobs, and a search box that only does one of them is wrong half
    /// the time.
    /// </summary>
    internal enum ESearchScope : byte
    {
        Everywhere = 0,
        CurrentLevel = 1,
        Types = 2,
        Members = 3
    }
}