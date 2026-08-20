using System;
using System.Collections.Generic;
using Base.ToolPackage.Editor.CodebaseGraph.Model;

namespace Base.ToolPackage.Editor.CodebaseGraph.Scanning
{
    /// <summary>
    /// Holds every member node and the redirects that fold machinery onto the code that was actually
    /// written. Property accessors point at their property, auto property backing fields point at their
    /// property, event adders point at their event, and lambda bodies point at their owning method.
    /// </summary>
    internal sealed class MemberRegistry
    {
        private const int MaxRedirectDepth = 8;

        private readonly Dictionary<MemberKey, MemberNodeInfo> _members;
        private readonly Dictionary<MemberKey, MemberKey> _redirects = new();
        private readonly Dictionary<TypeKey, Dictionary<string, MemberKey>> _byName = new();

        /// <summary>Creates a registry that fills the given member dictionary.</summary>
        /// <param name="members">Dictionary the graph exposes, filled as nodes are registered.</param>
        public MemberRegistry(Dictionary<MemberKey, MemberNodeInfo> members) => _members = members;

        /// <summary>Adds a member node and makes it findable by name on its declaring type.</summary>
        /// <param name="node">The node to register.</param>
        public void Register(MemberNodeInfo node)
        {
            _members[node.Key] = node;

            if (!_byName.TryGetValue(node.DeclaringTypeKey, out Dictionary<string, MemberKey> names))
            {
                names = new Dictionary<string, MemberKey>(StringComparer.Ordinal);
                _byName[node.DeclaringTypeKey] = names;
            }

            // Overloads share a name. The first one wins, which is all the compiler generated
            // name lookup needs, since it only has a name to go on anyway.
            names.TryAdd(node.Name, node.Key);
        }

        /// <summary>Points one key at another, so usages of the source land on the target node.</summary>
        /// <param name="from">Key that should be folded away.</param>
        /// <param name="to">Key that should receive the usages.</param>
        public void Redirect(MemberKey from, MemberKey to)
        {
            if (!from.IsValid || !to.IsValid || from.Equals(to))
                return;

            _redirects[from] = to;
        }

        /// <summary>Follows the redirect chain to the node a key really belongs to.</summary>
        /// <param name="key">Key to resolve.</param>
        /// <returns>The final key, or the input when there is no redirect.</returns>
        public MemberKey Resolve(MemberKey key)
        {
            MemberKey current = key;

            for (int depth = 0; depth < MaxRedirectDepth; depth++)
            {
                if (!_redirects.TryGetValue(current, out MemberKey next))
                    return current;

                current = next;
            }

            return current;
        }

        /// <summary>Looks up a member of a type by its plain name.</summary>
        /// <param name="typeKey">Type to search on.</param>
        /// <param name="name">Plain member name.</param>
        /// <param name="key">The member key when one was found.</param>
        /// <returns>True when the type declares a member with that name.</returns>
        public bool TryFindByName(TypeKey typeKey, string name, out MemberKey key)
        {
            key = default(MemberKey);
            if (string.IsNullOrEmpty(name))
                return false;

            return _byName.TryGetValue(typeKey, out Dictionary<string, MemberKey> names)
                && names.TryGetValue(name, out key);
        }

        /// <summary>Returns the node for a key, or null when the key is outside the scanned scope.</summary>
        /// <param name="key">Key to look up.</param>
        /// <returns>The member node, or null.</returns>
        public MemberNodeInfo Find(MemberKey key) => _members.GetValueOrDefault(key);
    }
}