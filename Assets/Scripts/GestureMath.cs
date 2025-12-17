using System.Collections.Generic;
using UnityEngine;

public static class GestureMath
{
    // Compute total polyline length.
    public static float ComputePathLength(List<Vector2> pts)
    {
        float len = 0f;
        for (int i = 1; i < pts.Count; ++i)
        {
            len += Vector2.Distance(pts[i - 1], pts[i]);
        }
        return len;
    }

    // Normalize points into a centered, scale-invariant space.
    // This matches Motion_To_2D_Stroke.NormalizePoints behavior. :contentReference[oaicite:13]{index=13}
    public static List<Vector2> NormalizePoints(List<Vector2> src)
    {
        int n = src.Count;
        if (n == 0)
            return new List<Vector2>();

        Vector2 min = src[0];
        Vector2 max = src[0];

        for (int i = 1; i < n; ++i)
        {
            Vector2 p = src[i];
            if (p.x < min.x) min.x = p.x;
            if (p.y < min.y) min.y = p.y;
            if (p.x > max.x) max.x = p.x;
            if (p.y > max.y) max.y = p.y;
        }

        Vector2 size = max - min;
        float scale = Mathf.Max(Mathf.Abs(size.x), Mathf.Abs(size.y));
        if (scale < 1e-5f)
        {
            return new List<Vector2>(src);
        }

        Vector2 center = (min + max) * 0.5f;
        var norm = new List<Vector2>(n);
        for (int i = 0; i < n; ++i)
        {
            Vector2 p = src[i];
            Vector2 np = (p - center) / scale;
            norm.Add(np);
        }

        return norm;
    }

    // Resample polyline to a fixed number of points.
    // This matches Motion_To_2D_Stroke.Resample behavior. :contentReference[oaicite:14]{index=14}
    public static List<Vector2> Resample(List<Vector2> pts, int targetCount)
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
        int currentIndex = 1;

        resampled.Add(pts[0]);
        float distSinceLastSample = 0f;

        var local = new List<Vector2>(pts);

        while (resampled.Count < targetCount && currentIndex < local.Count)
        {
            Vector2 prev = local[currentIndex - 1];
            Vector2 curr = local[currentIndex];
            float segLen = Vector2.Distance(prev, curr);

            if (distSinceLastSample + segLen >= step)
            {
                float remaining = step - distSinceLastSample;
                float t = segLen > 0f ? remaining / segLen : 0f;
                Vector2 newPoint = Vector2.Lerp(prev, curr, t);

                resampled.Add(newPoint);
                local[currentIndex - 1] = newPoint;
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
            resampled.Add(local[local.Count - 1]);
        }

        return resampled;
    }

    // Quantize direction changes into N bins (e.g., 8-dir).
    // This matches Motion_To_2D_Stroke.QuantizeDirections behavior. :contentReference[oaicite:15]{index=15}
    public static List<int> QuantizeDirections(List<Vector2> pts, int bins)
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

    // Direction-sequence distance (used by ShowResult). :contentReference[oaicite:16]{index=16}
    public static float DirectionSequenceDistance(List<int> a, int[] b, int dirBins)
    {
        int maxLen = Mathf.Max(a.Count, b.Length);
        float sum = 0f;

        for (int i = 0; i < maxLen; ++i)
        {
            int da = i < a.Count ? a[i] : a[a.Count - 1];
            int db = i < b.Length ? b[i] : b[b.Length - 1];

            int diff = Mathf.Abs(da - db);
            diff = Mathf.Min(diff, dirBins - diff);
            sum += diff;
        }

        sum += Mathf.Abs(a.Count - b.Length) * 0.5f;
        return sum;
    }
}
