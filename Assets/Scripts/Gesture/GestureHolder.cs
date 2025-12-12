/*--------------------------------------------------------------------------------*
  File Name: GestureHolder.cs
  Authors: Hyeonjoon Nam, Sam Friedman

  Copyright DigiPen Institute of Technology
 *--------------------------------------------------------------------------------*/

using System.Collections.Generic;
using UnityEngine;

namespace GestureNX
{
    public struct StrokePoint
    {
        public Vector2 pos;
        public float t;

        public StrokePoint(Vector2 p, float time)
        {
            pos = p;
            t = time;
        }
    }

    public class GestureHolder : MonoBehaviour
    {
        [SerializeField]
        private GestureRecognizer gestureRecognizer;

        private const float MinSampleDistance = 2.0f;
        private const int ResampleCount = 16;
        private const int DirBins = 8;

        private List<StrokePoint> points = new();
        private float captureStartTime = 0.0f;

        public void RequestStart()
        {
            points.Clear();

            captureStartTime = Time.time;
        }

        public void RequestUpdate(Vector2 point)
        {
            float t = Time.time - captureStartTime;

            /*
            if (points.Count > 0)
            {
                Vector2 last = points[^1].pos;

                if (Vector2.Distance(last, point) < MinSampleDistance)
                {
                    return;
                }
            }
            */

            points.Add(new(point, t));
        }

        public GestureKind RequestEnd()
        {
            if (points.Count < 8)
            {
                return GestureKind.Invalid;
            }

            List<Vector2> norm = GestureProcessUtil.NormalizePoints(points);
            float normLength = GestureProcessUtil.ComputePathLength(norm);
            List<Vector2> resampled = GestureProcessUtil.ResamplePoints(norm, ResampleCount);
            List<int> dirs = GestureProcessUtil.QuantizeDirections(resampled, DirBins);
            GestureKind kind = gestureRecognizer.Process(dirs, normLength);
            
            points.Clear();

            return kind;
        }
    }

    static class GestureProcessUtil
    {
        internal static float ComputePathLengthPixels(List<StrokePoint> points)
        {
            float len = 0.0f;

            for (int i = 1; i < points.Count; ++i)
            {
                len += Vector2.Distance(points[i - 1].pos, points[i].pos);
            }

            return len;
        }

        internal static float ComputePathLength(List<Vector2> pts)
        {
            float len = 0f;
            for (int i = 1; i < pts.Count; ++i)
            {
                len += Vector2.Distance(pts[i - 1], pts[i]);
            }
            return len;
        }

        internal static List<Vector2> NormalizePoints(List<StrokePoint> src)
        {
            int n = src.Count;
            var pts = new List<Vector2>(n);
            for (int i = 0; i < n; ++i)
            {
                pts.Add(src[i].pos);
            }

            Vector2 min = pts[0];
            Vector2 max = pts[0];
            for (int i = 1; i < n; ++i)
            {
                Vector2 p = pts[i];
                if (p.x < min.x) min.x = p.x;
                if (p.y < min.y) min.y = p.y;
                if (p.x > max.x) max.x = p.x;
                if (p.y > max.y) max.y = p.y;
            }

            Vector2 size = max - min;
            float scale = Mathf.Max(size.x, size.y);
            if (scale < 1e-5f)
            {
                return new List<Vector2>(pts);
            }

            Vector2 center = (min + max) * 0.5f;
            var norm = new List<Vector2>(n);
            for (int i = 0; i < n; ++i)
            {
                Vector2 p = pts[i];
                Vector2 np = (p - center) / scale;
                norm.Add(np);
            }

            return norm;
        }

        internal static List<Vector2> ResamplePoints(List<Vector2> pts, int targetCount)
        {
            var resampled = new List<Vector2>();

            if (pts.Count == 0)
                return resampled;

            if (pts.Count == 1 || targetCount <= 1)
            {
                resampled.Add(pts[0]);
                return resampled;
            }

            float totalLen = 0f;
            for (int i = 1; i < pts.Count; ++i)
            {
                totalLen += Vector2.Distance(pts[i - 1], pts[i]);
            }

            if (totalLen < 1e-5f)
            {
                for (int i = 0; i < targetCount; ++i)
                    resampled.Add(pts[0]);

                return resampled;
            }

            float step = totalLen / (targetCount - 1);
            //float accumulated = 0f;
            int currentIndex = 1;

            resampled.Add(pts[0]);
            float distSinceLastSample = 0f;

            while (resampled.Count < targetCount && currentIndex < pts.Count)
            {
                Vector2 prev = pts[currentIndex - 1];
                Vector2 curr = pts[currentIndex];
                float segLen = Vector2.Distance(prev, curr);

                if (distSinceLastSample + segLen >= step)
                {
                    float remaining = step - distSinceLastSample;
                    float t = segLen > 0f ? remaining / segLen : 0f;
                    Vector2 newPoint = Vector2.Lerp(prev, curr, t);

                    resampled.Add(newPoint);
                    pts[currentIndex - 1] = newPoint;
                    distSinceLastSample = 0f;
                }
                else
                {
                    distSinceLastSample += segLen;
                    currentIndex++;
                }
            }

            while (resampled.Count < targetCount)
            {
                resampled.Add(pts[pts.Count - 1]);
            }

            return resampled;
        }

        internal static List<int> QuantizeDirections(List<Vector2> pts, int bins)
        {
            var dirs = new List<int>();
            if (pts.Count < 2)
                return dirs;

            for (int i = 1; i < pts.Count; ++i)
            {
                Vector2 v = pts[i] - pts[i - 1];
                if (v.sqrMagnitude < 1e-6f)
                    continue;

                float angle = Mathf.Atan2(v.y, v.x);

                if (angle < 0f)
                    angle += Mathf.PI * 2f;

                float normalized = angle / (Mathf.PI * 2f);
                int idx = Mathf.FloorToInt(normalized * bins + 0.5f) % bins;

                if (dirs.Count == 0 || dirs[dirs.Count - 1] != idx)
                {
                    dirs.Add(idx);
                }
            }

            return dirs;
        }
    }
}
