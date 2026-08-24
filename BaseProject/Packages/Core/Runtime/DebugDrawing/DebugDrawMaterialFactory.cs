using Base.UtilityPackage.Logging;
using UnityEngine;
using UnityEngine.Rendering;

namespace Base.CorePackage.DebugDrawing
{
    /// <summary>
    /// Builds the vertex colored material the line pass draws with, either depth tested or drawn
    /// on top of everything.
    /// </summary>
    /// <remarks>
    /// The properties are set as floats because that is how the built-in colored shader stores
    /// them, even though they are declared as integers in its ShaderLab source.
    /// </remarks>
    internal static class DebugDrawMaterialFactory
    {
        private const string ColoredShaderName = "Hidden/Internal-Colored";
        private const string CullProperty = "_Cull";
        private const string DestinationBlendProperty = "_DstBlend";
        private const string MissingShaderFormat = "The shader \"{0}\" was not found, so debug lines cannot be "
            + "drawn. Add it to the always included shaders under Project Settings, Graphics.";
        private const string SourceBlendProperty = "_SrcBlend";
        private const string ZTestProperty = "_ZTest";
        private const float ZWriteOff = 0f;
        private const string ZWriteProperty = "_ZWrite";

        private static readonly int CullId = Shader.PropertyToID(CullProperty);
        private static readonly int DestinationBlendId = Shader.PropertyToID(DestinationBlendProperty);
        private static readonly int SourceBlendId = Shader.PropertyToID(SourceBlendProperty);
        private static readonly int ZTestId = Shader.PropertyToID(ZTestProperty);
        private static readonly int ZWriteId = Shader.PropertyToID(ZWriteProperty);

        /// <summary>
        /// Creates a material for one of the two line passes.
        /// </summary>
        /// <param name="depthTest">True for lines that geometry in front of them hides.</param>
        /// <param name="material">The created material, or null when this method returns false.</param>
        /// <returns>True if the material was created; otherwise false.</returns>
        internal static bool TryCreate(bool depthTest, out Material material)
        {
            Shader shader = Shader.Find(ColoredShaderName);

            if (shader == null)
            {
                CustomLogger.LogError(string.Format(MissingShaderFormat, ColoredShaderName), null);

                material = null;

                return false;
            }

            material = new Material(shader)
            {
                hideFlags = HideFlags.HideAndDontSave
            };

            float zTest = depthTest
                ? (float)CompareFunction.LessEqual
                : (float)CompareFunction.Always;

            material.SetFloat(SourceBlendId, (float)BlendMode.SrcAlpha);
            material.SetFloat(DestinationBlendId, (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat(CullId, (float)CullMode.Off);
            material.SetFloat(ZWriteId, ZWriteOff);
            material.SetFloat(ZTestId, zTest);

            return true;
        }
    }
}