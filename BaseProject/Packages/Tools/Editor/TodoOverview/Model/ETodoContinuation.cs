namespace Base.ToolPackage.Editor.TodoOverview.Model
{
    /// <summary>
    /// How far an item reaches past the line its keyword sits on. IDEs disagree here, so the rule is a
    /// setting: Visual Studio and Eclipse stop at the end of the line, while Rider and the other
    /// JetBrains IDEs keep reading as long as the following comment lines are indented deeper than the
    /// keyword itself.
    /// </summary>
    internal enum ETodoContinuation : byte
    {
        /// <summary>Only the line the keyword sits on belongs to the item.</summary>
        SingleLine = 0,

        /// <summary>Following comment lines continue the item while they are indented deeper.</summary>
        Indented = 1,

        /// <summary>Every following line of the same comment block continues the item.</summary>
        WholeBlock = 2
    }
}