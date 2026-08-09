using System.Collections.Generic;
using System.Reflection;

namespace Base.ToolPackage.Editor.CodebaseGraph.Scanning
{
    /// <summary>
    /// Remembers what each metadata token resolved to. Rebuilding a token is the expensive half of
    /// reading IL, and the same members are referenced over and over, so a project with a few hundred
    /// thousand instructions still only names a few tens of thousands of distinct tokens.
    /// <br/><br/>
    /// Caching by token alone is safe here because every result is normalized to a definition: a
    /// constructed generic collapses onto its open form, and a bare generic parameter is dropped, so the
    /// generic context a token was read in cannot change the answer.
    /// </summary>
    public sealed class TokenResolutionCache
    {
        private readonly Dictionary<(Module Module, int Token), TokenResolution> _entries = new();

        /// <summary>Number of distinct tokens resolved so far.</summary>
        public int Count => _entries.Count;

        /// <summary>Looks up a token that has already been resolved.</summary>
        /// <param name="module">Module the token belongs to.</param>
        /// <param name="token">The metadata token.</param>
        /// <param name="resolution">What it resolved to.</param>
        /// <returns>True when the token was already known.</returns>
        public bool TryGet(Module module, int token, out TokenResolution resolution)
            => _entries.TryGetValue((module, token), out resolution);

        /// <summary>Remembers what a token resolved to, including a failure.</summary>
        /// <param name="module">Module the token belongs to.</param>
        /// <param name="token">The metadata token.</param>
        /// <param name="resolution">What it resolved to.</param>
        public void Store(Module module, int token, TokenResolution resolution)
            => _entries[(module, token)] = resolution;
    }
}
