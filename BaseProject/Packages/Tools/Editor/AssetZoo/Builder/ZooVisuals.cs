using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Base.ToolsPackage.Editor.AssetZoo.Builder
{
    /// <summary>
    /// Knows what of a prefab can be seen. This is what keeps UI screens and manager prefabs out of a zoo, and what
    /// makes its sizes true: they are read from real renderers on a real instance, so the prefab's own rotation and
    /// scale, skinned meshes and switched off children all count the way they count in the game.
    /// </summary>
    internal static class ZooVisuals
    {
        private const string BrokenShaderProblem = "Broken shader";
        private const string BuiltInAssetPrefix = "Resources/unity_builtin";
        private const string BuiltInLibraryPrefix = "Library/unity";

        // Decals come from a render pipeline package this package does not depend on, so they are told apart by
        // name, and these two are the serialized names of the same fields in URP and in HDRP.
        private const string DecalMaterialField = "m_Material";
        private const string DecalOffsetField = "m_Offset";
        private const string DecalSizeField = "m_Size";
        private const string DecalTypeName = "DecalProjector";
        private const string DecalUvBiasField = "m_UVBias";
        private const string DecalUvScaleField = "m_UVScale";
        private const string DefaultMaterialProblem = "Still on the default material";
        private const string EmptySlotProblem = "Empty material slot";
        private const string ScaleFormat = "0.##";
        private const string ScaleProblemFormat = "Root scale is {0}, not 1";
        private const float ScaleTolerance = 0.005f;

        private static readonly string[] BaseColorWords =
        {
            "Base",
            "Main",
            "Albedo",
            "Diffuse"
        };

        private static readonly List<Renderer> RendererBuffer = new();

        /// <summary>True when placing the asset in a scene would show anything at all.</summary>
        /// <param name="asset">A prefab or model.</param>
        public static bool HasVisuals(GameObject asset)
        {
            // A UI screen has renderers in its children but only makes sense on a canvas.
            if (asset == null
                || asset.GetComponent<RectTransform>() != null)
                return false;

            foreach (Renderer renderer in asset.GetComponentsInChildren<Renderer>())
            {
                if (IsArt(renderer))
                    return true;
            }

            return TryGetDecal(asset, out _);
        }

        /// <summary>Finds a decal projector on the asset, if it has one.</summary>
        /// <param name="asset">A prefab or instance.</param>
        /// <param name="decal">The projector.</param>
        /// <returns>True when the asset has one.</returns>
        public static bool TryGetDecal(GameObject asset, out Behaviour decal)
        {
            foreach (Behaviour behaviour in asset.GetComponentsInChildren<Behaviour>())
            {
                if (behaviour == null
                    || behaviour.GetType().Name != DecalTypeName)
                    continue;

                decal = behaviour;
                return true;
            }

            decal = null;
            return false;
        }

        /// <summary>Reads the box a decal projects into.</summary>
        /// <param name="decal">The projector.</param>
        /// <param name="size">The size of the box.</param>
        /// <param name="offset">The middle of the box, relative to the projector.</param>
        public static void GetDecalBox(Behaviour decal, out Vector3 size, out Vector3 offset)
        {
            using SerializedObject serialized = new(decal);

            size = serialized.FindProperty(DecalSizeField).vector3Value;
            offset = serialized.FindProperty(DecalOffsetField).vector3Value;
        }

        /// <summary>
        /// Finds the part of a texture a decal shows. Decals often share one atlas and differ only in the part they
        /// cut out of it, so the texture alone would make all of them look the same.
        /// </summary>
        /// <param name="decal">The projector.</param>
        /// <param name="uv">The part of the texture it shows.</param>
        /// <returns>The texture, or null when its material has none.</returns>
        public static Texture FindDecalPicture(Behaviour decal, out Rect uv)
        {
            using SerializedObject serialized = new(decal);

            Vector2 scale = serialized.FindProperty(DecalUvScaleField).vector2Value;
            Vector2 bias = serialized.FindProperty(DecalUvBiasField).vector2Value;

            uv = new Rect(Mathf.Repeat(bias.x, 1f), Mathf.Repeat(bias.y, 1f), scale.x, scale.y);

            return FindDecalTexture(serialized);
        }

        private static Texture FindDecalTexture(SerializedObject serialized)
        {
            Material material = serialized.FindProperty(DecalMaterialField).objectReferenceValue as Material;

            if (material == null)
                return null;

            Texture picture = null;

            foreach (string property in material.GetTexturePropertyNames())
            {
                Texture texture = material.GetTexture(property);

                if (texture == null)
                    continue;

                // The order of the properties is the shader's, and a noise or normal map can come first.
                if (IsBaseColorProperty(property))
                    return texture;

                if (picture == null)
                    picture = texture;
            }

            return picture;
        }

        /// <summary>
        /// Measures a placed asset. The instance has to stand under an object that is not rotated or scaled.
        /// </summary>
        /// <param name="instance">The instance in the scene.</param>
        /// <param name="space">The object the bounds are relative to.</param>
        /// <returns>What was measured.</returns>
        public static ZooMeasurement Measure(GameObject instance, Transform space)
        {
            CollectRenderers(instance);

            bool hasBounds = false;
            Bounds bounds = new(space.position, Vector3.zero);
            int triangles = 0;
            HashSet<Material> materials = new();
            string problem = string.Empty;

            foreach (Renderer renderer in RendererBuffer)
            {
                bounds = hasBounds
                    ? Encapsulated(bounds, renderer.bounds)
                    : renderer.bounds;

                hasBounds = true;
                triangles += CountTriangles(renderer);

                string found = InspectMaterials(renderer, materials);

                if (problem.Length == 0)
                    problem = found;
            }

            if (problem.Length == 0)
                problem = InspectScale(instance.transform);

            bounds.center -= space.position;

            return new ZooMeasurement(bounds, triangles, materials.Count, problem);
        }

        private static bool IsBaseColorProperty(string property)
        {
            foreach (string word in BaseColorWords)
            {
                if (property.IndexOf(word, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }

            return false;
        }

        private static bool IsArt(Renderer renderer)
        {
            if (renderer is SkinnedMeshRenderer skinned)
                return skinned.sharedMesh != null;

            if (renderer is SpriteRenderer sprite)
                return sprite.sprite != null;

            return renderer is MeshRenderer
                && renderer.TryGetComponent(out MeshFilter filter)
                && filter.sharedMesh != null;
        }

        private static Bounds Encapsulated(Bounds bounds, Bounds other)
        {
            bounds.Encapsulate(other);
            return bounds;
        }

        // Only what the game would draw up close: nothing switched off, and only the first level of detail, or
        // an asset would count its triangles once per level.
        private static void CollectRenderers(GameObject instance)
        {
            RendererBuffer.Clear();

            HashSet<Renderer> lowerLevels = new();

            foreach (LODGroup group in instance.GetComponentsInChildren<LODGroup>())
            {
                LOD[] levels = group.GetLODs();

                for (int i = 1; i < levels.Length; i++)
                    lowerLevels.UnionWith(levels[i].renderers);
            }

            foreach (Renderer renderer in instance.GetComponentsInChildren<Renderer>())
            {
                if (renderer.enabled
                    && IsArt(renderer)
                    && !lowerLevels.Contains(renderer))
                    RendererBuffer.Add(renderer);
            }
        }

        private static int CountTriangles(Renderer renderer)
        {
            Mesh mesh = renderer switch
            {
                SkinnedMeshRenderer skinned => skinned.sharedMesh,
                SpriteRenderer => null,
                _ => renderer.GetComponent<MeshFilter>().sharedMesh
            };

            if (mesh == null)
                return 0;

            uint indices = 0;

            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                if (mesh.GetTopology(i) == MeshTopology.Triangles)
                    indices += mesh.GetIndexCount(i);
            }

            return (int)(indices / 3);
        }

        private static string InspectMaterials(Renderer renderer, HashSet<Material> materials)
        {
            string problem = string.Empty;

            foreach (Material material in renderer.sharedMaterials)
            {
                if (material == null)
                {
                    problem = EmptySlotProblem;
                    continue;
                }

                materials.Add(material);

                if (problem.Length > 0)
                    continue;

                if (material.shader == null
                    || !material.shader.isSupported
                    || ShaderUtil.ShaderHasError(material.shader))
                    problem = BrokenShaderProblem;
                else if (IsDefaultMaterial(material))
                    problem = DefaultMaterialProblem;
            }

            return problem;
        }

        private static bool IsDefaultMaterial(Material material)
        {
            RenderPipelineAsset pipeline = GraphicsSettings.currentRenderPipeline;

            if (pipeline != null
                && material == pipeline.defaultMaterial)
                return true;

            string path = AssetDatabase.GetAssetPath(material);

            return path.StartsWith(BuiltInAssetPrefix, System.StringComparison.Ordinal)
                || path.StartsWith(BuiltInLibraryPrefix, System.StringComparison.Ordinal);
        }

        private static string InspectScale(Transform root)
        {
            Vector3 scale = root.localScale;
            float largest = Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));

            return Mathf.Abs(largest - 1f) > ScaleTolerance
                ? string.Format(ScaleProblemFormat, largest.ToString(ScaleFormat))
                : string.Empty;
        }
    }
}