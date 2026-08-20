using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Draws a dropdown of the shader property names of a sibling Material, Renderer or Shader field,
    /// for example <c>[ShaderParam(nameof(material))]</c>. On a string field the property name is
    /// stored, on an int field the id returned by <c>Shader.PropertyToID</c>. Optionally filtered to a
    /// single property kind, so a color parameter cannot be pointed at a texture.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ShaderParamAttribute : PropertyAttribute
    {
        /// <summary>Name of the sibling Material, Renderer or Shader field the properties are read from.</summary>
        public string SourceField { get; }

        /// <summary>Which property kinds are offered. Defaults to <see cref="EShaderParamType.Any"/>.</summary>
        public EShaderParamType Type { get; }

        /// <summary>Whether the dropdown is restricted to a single property kind.</summary>
        public bool HasFilter => Type != EShaderParamType.Any;

        /// <summary>Creates the attribute referencing the given source field.</summary>
        /// <param name="sourceField">Name of the sibling Material, Renderer or Shader field.</param>
        /// <param name="type">Which property kinds are offered.</param>
        public ShaderParamAttribute(string sourceField, EShaderParamType type = EShaderParamType.Any)
        {
            SourceField = sourceField;
            Type = type;
        }
    }
}