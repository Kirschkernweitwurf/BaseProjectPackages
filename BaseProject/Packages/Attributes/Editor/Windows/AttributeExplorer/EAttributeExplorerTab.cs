namespace Base.AttributePackage.Editor.Windows.AttributeExplorer
{
    /// <summary>
    /// Which view the attribute window is showing.
    /// </summary>
    internal enum EAttributeExplorerTab : byte
    {
        /// <summary>One page per attribute, with a live sample and the source behind it.</summary>
        Reference = 0,

        /// <summary>Every attribute at once on one object, drawn through the real inspector.</summary>
        Showcase = 1,

        /// <summary>Attribute usages that cannot work as written.</summary>
        Troubleshoot = 2
    }
}