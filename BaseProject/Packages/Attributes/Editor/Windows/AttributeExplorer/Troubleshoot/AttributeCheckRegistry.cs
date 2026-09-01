using System;
using System.Collections.Generic;
using UnityEditor;

namespace Base.AttributePackage.Editor.Windows.AttributeExplorer.Troubleshoot
{
    /// <summary>
    /// Discovers and instantiates every <see cref="IAttributeCheck"/> via <see cref="TypeCache"/>, so a
    /// new check is a new file and nothing else.
    /// </summary>
    internal static class AttributeCheckRegistry
    {
        /// <summary>All discovered checks.</summary>
        internal static IAttributeCheck[] Checks => _checks ??= Create();

        private static IAttributeCheck[] _checks;

        private static IAttributeCheck[] Create()
        {
            List<IAttributeCheck> checks = new();

            foreach (Type type in TypeCache.GetTypesDerivedFrom<IAttributeCheck>())
            {
                if (!type.IsAbstract && !type.IsInterface)
                    checks.Add((IAttributeCheck)Activator.CreateInstance(type));
            }

            return checks.ToArray();
        }
    }
}