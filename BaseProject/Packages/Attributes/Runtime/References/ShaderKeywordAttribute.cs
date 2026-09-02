using System;
using UnityEngine;

namespace Base.AttributesPackage
{
    /// <summary>
    /// Draws a dropdown of the shader keywords of a sibling Material, Renderer or Shader field, for
    /// example <c>[ShaderKeyword(nameof(material))]</c>. Stores the keyword name on a string field.
    /// The keyword sibling of <see cref="ShaderParamAttribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ShaderKeywordAttribute : PropertyAttribute
    {
        /// <summary>Name of the sibling Material, Renderer or Shader field the keywords are read from.</summary>
        public string SourceField { get; }

        /// <summary>Creates the attribute referencing the given source field.</summary>
        /// <param name="sourceField">Name of the sibling Material, Renderer or Shader field.</param>
        public ShaderKeywordAttribute(string sourceField) => SourceField = sourceField;
    }
}