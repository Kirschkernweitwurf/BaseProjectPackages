using System;
using System.Collections.Generic;
using System.Reflection;
using Base.DataHubPackage.Editor.Table;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.DataHubPackage.Editor.Lists
{
    /// <summary>
    /// A serialized list of asset references on a type, like the cards of a deck. These are the lists a
    /// designer fills by hand, so the hub edits them as counts instead of as slots.
    /// </summary>
    internal sealed class DataListField
    {
        private static readonly Dictionary<Type, List<DataListField>> Cache = new();

        private DataListField(string propertyPath, string label, Type elementType)
        {
            PropertyPath = propertyPath;
            Label = label;
            ElementType = elementType;
        }

        /// <summary>The serialized path of the list.</summary>
        internal string PropertyPath { get; }

        /// <summary>The readable name of the list.</summary>
        internal string Label { get; }

        /// <summary>The asset type the list holds.</summary>
        internal Type ElementType { get; }

        /// <summary>The asset lists declared on <paramref name="ownerType"/>, in inspector order.</summary>
        /// <param name="ownerType">The type that holds the lists.</param>
        /// <returns>The lists. Cached, do not change the list.</returns>
        internal static IReadOnlyList<DataListField> Of(Type ownerType)
        {
            if (Cache.TryGetValue(ownerType, out List<DataListField> cached))
                return cached;

            List<DataListField> lists = new();

            foreach (FieldInfo field in DataSerializedFields.Of(ownerType))
            {
                Type element = DataSerializedFields.ElementType(field.FieldType);

                // A list of plain ScriptableObjects or Objects accepts anything and is never a list a
                // designer fills with game data, like the template lists of the Addressables settings.
                if (element == null
                    || !typeof(Object).IsAssignableFrom(element)
                    || DataSerializedFields.IsUnityType(element)
                    || Attribute.IsDefined(field, typeof(SerializeReference)))
                    continue;

                lists.Add(new DataListField(field.Name, DataColumnLabel.Clean(field.Name), element));
            }

            Cache.Add(ownerType, lists);

            return lists;
        }

        /// <summary>True when every asset in <paramref name="assets"/> fits into this list.</summary>
        /// <param name="assets">The assets to test.</param>
        /// <returns>True when all of them can be added.</returns>
        internal bool Accepts(IReadOnlyList<Object> assets)
        {
            if (assets.Count == 0)
                return false;

            foreach (Object asset in assets)
            {
                if (asset == null
                    || !ElementType.IsInstanceOfType(asset))
                    return false;
            }

            return true;
        }
    }
}