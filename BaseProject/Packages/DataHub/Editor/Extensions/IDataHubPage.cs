namespace Base.DataHubPackage.Editor.Extensions
{
    /// <summary>
    /// A whole custom page of the Data Hub, like a balancing overview. It sits in the type tree next to
    /// the tables and takes the space of the table and the inspector when selected.
    /// </summary>
    public interface IDataHubPage
    {
        /// <summary>Where the page sits in the tree, with '/' between levels.</summary>
        string Path { get; }

        /// <summary>Position among its siblings in the tree. Lower comes first.</summary>
        int Order { get; }

        /// <summary>
        /// What the page is for, shown when hovering the tree entry and the title. Null shows a generic note.
        /// </summary>
        string Description => null;

        /// <summary>
        /// Reads the data again. Called when the page is opened and whenever assets in the project change.
        /// </summary>
        void Refresh();

        /// <summary>Draws the page with GUILayout into the area the hub gives it.</summary>
        void Draw();
    }
}