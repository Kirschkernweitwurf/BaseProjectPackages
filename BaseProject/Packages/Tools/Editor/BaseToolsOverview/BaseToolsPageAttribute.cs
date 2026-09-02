using System;

namespace Base.ToolsPackage.Editor.BaseToolsOverview
{
    /// <summary>
    /// Optional one line description for a project settings page under Base Tools. Put it on the
    /// method that creates the page, or on the type declaring that method, and the overview page
    /// shows the sentence under the page name.
    /// </summary>
    /// <remarks>
    /// Nothing has to be registered anywhere. Pages are found on their own; this only carries the
    /// sentence a settings provider itself has no place for.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
    internal sealed class BaseToolsPageAttribute : Attribute
    {
        /// <summary>The sentence shown under the page name in the overview.</summary>
        public string Description { get; }

        /// <summary>Describes a settings page for the Base Tools overview.</summary>
        /// <param name="description">The sentence shown under the page name.</param>
        public BaseToolsPageAttribute(string description) => Description = description;
    }
}