using System.Globalization;
using Base.ToolsPackage.AssetZoo;
using UnityEngine;

namespace Base.ToolsPackage.Editor.AssetZoo.Overlay
{
    /// <summary>
    /// Puts what was measured into words an artist reads at a glance. Small things in centimeters, because nobody
    /// thinks of a mug as 0.09 meters.
    /// </summary>
    internal static class ZooItemText
    {
        private const string CentimeterFormat = "{0:0} x {1:0} x {2:0} cm";
        private const float CentimetersFrom = 1f;
        private const float CentimetersPerMeter = 100f;
        private const string DetailSeparator = " · ";
        private const string MaterialFormat = "{0} material";
        private const string MaterialsFormat = "{0} materials";
        private const string MeterFormat = "{0:0.00} x {1:0.00} x {2:0.00} m";
        private const float ThousandTriangles = 1000f;
        private const string ThousandTrianglesFormat = "{0:0.#}k tris";
        private const string TrianglesFormat = "{0} tris";

        /// <summary>Writes a size as width x height x depth.</summary>
        /// <param name="size">The size in meters.</param>
        /// <returns>The size in the unit that reads best.</returns>
        public static string FormatSize(Vector3 size)
        {
            if (Mathf.Max(size.x, size.y, size.z) >= CentimetersFrom)
                return string.Format(CultureInfo.InvariantCulture, MeterFormat, size.x, size.y, size.z);

            return string.Format(CultureInfo.InvariantCulture, CentimeterFormat, size.x * CentimetersPerMeter,
                size.y * CentimetersPerMeter, size.z * CentimetersPerMeter);
        }

        /// <summary>Writes a triangle count, short once it runs into the thousands.</summary>
        /// <param name="triangles">The number of triangles.</param>
        /// <returns>The count in words.</returns>
        public static string FormatTriangles(int triangles) => triangles >= ThousandTriangles
            ? string.Format(CultureInfo.InvariantCulture, ThousandTrianglesFormat, triangles / ThousandTriangles)
            : string.Format(CultureInfo.InvariantCulture, TrianglesFormat, triangles);

        /// <summary>Writes the one line that says what an asset is made of.</summary>
        /// <param name="item">The item to describe.</param>
        /// <returns>Its size, and its triangles and materials when it has any.</returns>
        public static string Describe(ZooItem item)
        {
            string size = FormatSize(item.MeasuredBounds.size);

            if (item.TriangleCount == 0)
                return size;

            string materials = string.Format(CultureInfo.InvariantCulture, item.MaterialCount == 1
                ? MaterialFormat
                : MaterialsFormat, item.MaterialCount);

            return size + DetailSeparator + FormatTriangles(item.TriangleCount) + DetailSeparator + materials;
        }
    }
}