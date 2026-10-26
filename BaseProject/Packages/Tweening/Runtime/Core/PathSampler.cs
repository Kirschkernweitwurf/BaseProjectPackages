using System;
using System.Collections.Generic;
using UnityEngine;

namespace Base.TweeningPackage.Core
{
    /// <summary>Walks a path at an even speed, measured once it knows where it starts.</summary>
    internal sealed class PathSampler
    {
        private const int CurveSamplesPerPoint = 16;

        private readonly Vector3[] _path;
        private readonly bool _isCurved;
        private readonly List<Vector3> _points = new();
        private readonly List<float> _distances = new();

        /// <summary>Walks <paramref name="path"/>, as straight lines or rounded into a curve.</summary>
        public PathSampler(Vector3[] path, bool isCurved)
        {
            _path = path ?? Array.Empty<Vector3>();
            _isCurved = isCurved;
        }

        /// <summary>Measures the path from <paramref name="start"/> through every point.</summary>
        public void Begin(Vector3 start)
        {
            List<Vector3> corners = new() { start };
            corners.AddRange(_path);

            _points.Clear();
            _distances.Clear();

            if (_isCurved && corners.Count > 2)
                SampleCurve(corners);
            else
                _points.AddRange(corners);

            float total = 0f;
            _distances.Add(0f);

            for (int i = 1; i < _points.Count; i++)
            {
                total += Vector3.Distance(_points[i - 1], _points[i]);
                _distances.Add(total);
            }
        }

        /// <summary>The point <paramref name="t"/> of the way along the path, by length.</summary>
        public Vector3 At(float t)
        {
            if (_points.Count == 0)
                return Vector3.zero;

            float total = _distances[^1];
            if (_points.Count == 1 || total <= 0f)
                return _points[^1];

            float distance = Mathf.Clamp01(t) * total;

            for (int i = 1; i < _points.Count; i++)
            {
                if (_distances[i] < distance)
                    continue;

                float span = _distances[i] - _distances[i - 1];
                float along = span <= 0f
                    ? 1f
                    : (distance - _distances[i - 1]) / span;

                return Vector3.Lerp(_points[i - 1], _points[i], along);
            }

            return _points[^1];
        }

        private static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;

            return 0.5f * (2f * p1 + (p2 - p0) * t + (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2
                + (3f * p1 - p0 - 3f * p2 + p3) * t3);
        }

        // The curve passes through every corner; the first and last corner double as their own neighbors.
        private void SampleCurve(List<Vector3> corners)
        {
            for (int i = 0; i < corners.Count - 1; i++)
            {
                Vector3 p0 = corners[Mathf.Max(0, i - 1)];
                Vector3 p1 = corners[i];
                Vector3 p2 = corners[i + 1];
                Vector3 p3 = corners[Mathf.Min(corners.Count - 1, i + 2)];

                for (int step = 0; step < CurveSamplesPerPoint; step++)
                    _points.Add(CatmullRom(p0, p1, p2, p3, (float)step / CurveSamplesPerPoint));
            }

            _points.Add(corners[^1]);
        }
    }
}
