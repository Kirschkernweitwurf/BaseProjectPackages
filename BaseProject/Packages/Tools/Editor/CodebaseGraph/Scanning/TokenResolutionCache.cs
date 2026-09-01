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
    /// generic context a token was read in cannot change the answer. Failures are only cached when the
    /// method had no generic context at all, since those genuinely do depend on it and remembering one
    /// would report the token as unresolvable everywhere else in the module.
    /// </summary>
    internal sealed class TokenResolutionCache
    {
        private readonly Dictionary<(Module Module, int Token), TokenResolution> _entries = new();
        private readonly Dictionary<(Module Module, int Token), string> _literals = new();

        /// <summary>Looks up a token that has already been resolved.</summary>
        /// <param name="module">Module the token belongs to.</param>
        /// <param name="token">The metadata token.</param>
        /// <param name="resolution">What it resolved to.</param>
        /// <returns>True when the token was already known.</returns>
        internal bool TryGet(Module module, int token, out TokenResolution resolution)
            => _entries.TryGetValue((module, token), out resolution);

        /// <summary>Remembers what a token resolved to.</summary>
        /// <param name="module">Module the token belongs to.</param>
        /// <param name="token">The metadata token.</param>
        /// <param name="resolution">What it resolved to.</param>
        /// <param name="isContextFree">
        /// Whether the method it was read in had no generic arguments. A failure is only remembered
        /// then, because with a context the same token may resolve perfectly well somewhere else, and
        /// remembering the first failure would report it as unresolvable across the whole module.
        /// </param>
        internal void Store(Module module, int token, TokenResolution resolution, bool isContextFree)
        {
            if (!resolution.IsResolved && !isContextFree)
                return;

            _entries[(module, token)] = resolution;
        }

        /// <summary>Looks up a string literal that has already been read.</summary>
        /// <param name="module">Module the token belongs to.</param>
        /// <param name="token">The metadata token.</param>
        /// <param name="literal">The literal when it was already known.</param>
        /// <returns>True when the token was already read.</returns>
        internal bool TryGetLiteral(Module module, int token, out string literal)
            => _literals.TryGetValue((module, token), out literal);

        /// <summary>Remembers a string literal.</summary>
        /// <param name="module">Module the token belongs to.</param>
        /// <param name="token">The metadata token.</param>
        /// <param name="literal">The literal that was read.</param>
        internal void StoreLiteral(Module module, int token, string literal) => _literals[(module, token)] = literal;
    }
}