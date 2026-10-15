using Base.DataHubPackage.Editor.Extensions;

namespace Base.DataHubPackage.Editor
{
    /// <summary>One line of the type tree: a table, a page, or a group that only holds other lines.</summary>
    internal sealed class DataHubTreeNode
    {
        /// <summary>Creates a node.</summary>
        /// <param name="path">Full path in the tree, with '/' between levels.</param>
        /// <param name="order">Position among its siblings.</param>
        /// <param name="table">The table shown when selected, or null.</param>
        /// <param name="page">The page shown when selected, or null.</param>
        internal DataHubTreeNode(string path, int order, IDataTable table, IDataHubPage page)
        {
            Path = path;
            Order = order;
            Table = table;
            Page = page;

            int separator = path.LastIndexOf(DataHubTree.PathSeparator);

            Label = separator < 0
                ? path
                : path.Substring(separator + 1);

            ParentPath = separator < 0
                ? null
                : path.Substring(0, separator);
        }

        /// <summary>Full path in the tree.</summary>
        internal string Path { get; }

        /// <summary>The last segment of the path, shown in the tree.</summary>
        internal string Label { get; }

        /// <summary>Path of the parent node, or null at the top level.</summary>
        internal string ParentPath { get; }

        /// <summary>Position among its siblings. A group takes the lowest order of its children.</summary>
        internal int Order { get; set; }

        /// <summary>How deep the node sits, zero at the top level.</summary>
        internal int Depth { get; set; }

        /// <summary>Number of assets the table lists, shown next to the label.</summary>
        internal int Count { get; set; }

        /// <summary>The table shown when the node is selected, or null.</summary>
        internal IDataTable Table { get; }

        /// <summary>The page shown when the node is selected, or null.</summary>
        internal IDataHubPage Page { get; }

        /// <summary>True for a node that only groups others and cannot be selected.</summary>
        internal bool IsGroup => Table == null
            && Page == null;
    }
}