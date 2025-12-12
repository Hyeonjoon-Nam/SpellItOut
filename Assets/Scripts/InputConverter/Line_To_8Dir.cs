/*--------------------------------------------------------------------------------*
  File Name: Line_To_8Dir.cs
  Authors: Hyeonjoon (Joon) Nam

  Copyright DigiPen Institute of Technology
 *--------------------------------------------------------------------------------*/


using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Line_To_8Dir : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text statusText;
    public TMP_Text resultText;

    [Header("Result Processor")]
    public ShowResult resultHandler;

    // Stroke capture state
    private struct StrokePoint
    {
        public Vector2 pos;
        public float t;

        public StrokePoint(Vector2 p, float time)
        {
            pos = p;
            t = time;
        }
    }

    private bool isCapturing = false;
    private float captureStartTime = 0f;
    private readonly List<StrokePoint> points = new List<StrokePoint>();

    private const float MinSampleDistance = 2.0f;

    // Processing parameters
    private const int ResampleCount = 16;
    private const int DirBins = 8;

    private void Start()
    {
        SetStatus("Idle");
        SetResult("No gesture yet");
    }

    private void Update()
    {
        HandleMouseInput();
    }

    // Core input handling

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            BeginCapture();
        }

        if (Input.GetMouseButton(0) && isCapturing)
        {
            UpdateCapture();
        }

        if (Input.GetMouseButtonUp(0) && isCapturing)
        {
            EndCapture();
        }
    }

    private void BeginCapture()
    {
        isCapturing = true;
        points.Clear();
        captureStartTime = Time.time;

        Vector2 p = Input.mousePosition;
        points.Add(new StrokePoint(p, 0f));

        SetStatus("Capturing...");
        SetResult("Capturing stroke...");
    }

    private void UpdateCapture()
    {
        Vector2 p = Input.mousePosition;
        float t = Time.time - captureStartTime;

        if (points.Count > 0)
        {
            Vector2 last = points[points.Count - 1].pos;
            if (Vector2.Distance(last, p) < MinSampleDistance)
            {
                return;
            }
        }

        points.Add(new StrokePoint(p, t));

        SetStatus($"Capturing... samples: {points.Count}");
    }

    private void EndCapture()
    {
        isCapturing = false;

        float duration = Time.time - captureStartTime;
        float pathLengthPixels = ComputePathLengthPixels();

        if (points.Count < 8)
        {
            SetStatus("Capture finished (too few points)");
            SetResult($"Unknown (pts: {points.Count}, lenPx: {pathLengthPixels:F2}, dur: {duration:F2}s)");
            return;
        }

        List<Vector2> norm = NormalizePoints(points);
        float normLength = ComputePathLength(norm);
        List<Vector2> resampled = Resample(norm, ResampleCount);
        List<int> dirs = QuantizeDirections(resampled, DirBins);

        string dirString = dirs.Count > 0 ? string.Join(",", dirs) : "(none)";

        SetStatus("Capture finished (processed)");
        SetResult(
            $"Stroke: {points.Count} pts, lenPx: {pathLengthPixels:F2}, dur: {duration:F2}s\n" +
            $"NormLen: {normLength:F2}, Resampled: {resampled.Count} pts\n" +
            $"Dirs ({dirs.Count}): [{dirString}]"
        );

        // Output the result shape
        if (resultHandler != null)
        {
            resultHandler.ProcessGesture(dirs, normLength);
        }
    }

    // Stroke utilities

    private float ComputePathLengthPixels()
    {
        float len = 0f;
        for (int i = 1; i < points.Count; ++i)
        {
            len += Vector2.Distance(points[i - 1].pos, points[i].pos);
        }
        return len;
    }

    private static float ComputePathLength(List<Vector2> pts)
    {
        float len = 0f;
        for (int i = 1; i < pts.Count; ++i)
        {
            len += Vector2.Distance(pts[i - 1], pts[i]);
        }
        return len;
    }

    private static List<Vector2> NormalizePoints(List<StrokePoint> src)
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

    private static List<Vector2> Resample(List<Vector2> pts, int targetCount)
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

    private static List<int> QuantizeDirections(List<Vector2> pts, int bins)
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

    // UI helper functions

    public void SetStatus(string msg)
    {
        if (statusText != null)
            statusText.text = msg;
    }

    public void SetResult(string msg)
    {
        if (resultText != null)
            resultText.text = msg;
    }
}