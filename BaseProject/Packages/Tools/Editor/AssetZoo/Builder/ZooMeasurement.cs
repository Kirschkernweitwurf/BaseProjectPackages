using UnityEngine;

namespace Base.ToolsPackage.Editor.AssetZoo.Builder
{
    /// <summary>
    /// What was measured on one placed asset.
    /// </summary>
    internal readonly struct ZooMeasurement
    {
        /// <summary>The box around what you see, relative to the object the asset was measured under.</summary>
        public Bounds Bounds { get; }

        /// <summary>How many triangles the highest level of detail has.</summary>
        public int Triangles { get; }

        /// <summary>How many different materials the asset uses.</summary>
        public int Materials { get; }

        /// <summary>What an artist should look at. Empty when nothing stands out.</summary>
        public string Problem { get; }

        /// <summary>Creates a measurement.</summary>
        /// <param name="bounds">The box around what you see.</param>
        /// <param name="triangles">How many triangles the highest level of detail has.</param>
        /// <param name="materials">How many different materials the asset uses.</param>
        /// <param name="problem">What an artist should look at, or empty.</param>
        public ZooMeasurement(Bounds bounds, int triangles, int materials, string problem)
        {
            Bounds = bounds;
            Triangles = triangles;
            Materials = materials;
            Problem = problem;
        }
    }
}