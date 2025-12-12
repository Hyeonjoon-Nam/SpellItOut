/*--------------------------------------------------------------------------------*
  File Name: Motion_To_2D_Stroke.cs
  Authors: Hyeonjoon (Joon) Nam

  Copyright DigiPen Institute of Technology
 *--------------------------------------------------------------------------------*/

using System.Collections.Generic;
using UnityEngine;
using TMPro;

using GestureNX;

public class Motion_To_2D_Stroke : MonoBehaviour
{
    [Header("Gesture Output")]
    public ShowResult resultHandler;
    public TMP_Text statusText;
    public TMP_Text debugText;

    [Header("Stroke Settings")]
    [Tooltip("Minimum distance between stored stroke points in virtual units.")]
    public float minSampleDistance = 0.02f;

    [Tooltip("Scale factor for how far the virtual pen moves per frame based on acceleration magnitude.")]
    public float stepScale = 1.0f;

    [Tooltip("Ignore projected acceleration below this magnitude (helps filter noise).")]
    public float accelMagnitudeThreshold = 0.05f;

    [Tooltip("Minimum number of points required to treat the motion as a valid stroke.")]
    public int minPointCount = 8;

    [Tooltip("Target point count after resampling.")]
    public int resampleCount = 16;

    [Tooltip("Number of direction bins. 8 means 0~7 for the cardinal and diagonal directions.")]
    public int dirBins = 8;

    public enum ProjectionPlane
    {
        XY,
        XZ,
        YZ
    }

    [Header("Projection Settings")]
    [Tooltip("Which plane to use when projecting 3D acceleration to 2D.")]
    public ProjectionPlane projectionPlane = ProjectionPlane.XY;

    [Tooltip("Invert X axis after projection.")]
    public bool invertX = false;

    [Tooltip("Invert Y axis after projection.")]
    public bool invertY = false;

    [Header("Debug")]
    [Tooltip("If true, prints debug information into debugText.")]
    public bool logDebugInfo = true;

    [Header("Stroke Debug View")]
    public LineRenderer strokeRenderer;
    public float strokeViewScale = 1.0f;
    public Vector2 strokeViewOffset = Vector2.zero;
    public bool useResampledStrokeForView = true;

    [Header("GestureNX Integration")]
    public GestureHolder gestureHolder;
    public bool sendToGestureHolder = true;

    [Header("Combat Integration")]
    public DrawBoardController drawBoardController;


    private MotionStrokeBuilder strokeBuilder;
    private bool isCapturing = false;
    private float captureStartTime = 0f;
    private bool wasGatePressed = false;

    private void Awake()
    {
        strokeBuilder = new MotionStrokeBuilder(minSampleDistance);
        SetStatus("Idle (motion)");
        SetDebug("No gesture yet");
    }

    public void UpdateFromMotionInput(Vector3 accel, bool gatePressed)
    {
        if (!wasGatePressed && gatePressed)
        {
            BeginMotionStroke();
        }

        if (gatePressed)
        {
            FeedAccelerationSample(accel);
        }

        if (wasGatePressed && !gatePressed)
        {
            EndMotionStroke();
        }

        wasGatePressed = gatePressed;
    }

    public void BeginMotionStroke()
    {
        DrawStrokeForDebug(null);

        isCapturing = true;
        captureStartTime = Time.time;
        strokeBuilder.BeginStroke(Vector2.zero);

        if (sendToGestureHolder && gestureHolder != null)
        {
            gestureHolder.RequestStart();
        }

        SetStatus("Capturing motion stroke...");
        if (logDebugInfo)
        {
            SetDebug("Capturing...");
        }
    }


    public void FeedAccelerationSample(Vector3 accel)
    {
        if (!isCapturing)
            return;

        Vector2 projected = ProjectAccelerationTo2D(accel);
        float magnitude = projected.magnitude;

        if (magnitude < accelMagnitudeThreshold)
            return;

        float stepSize = magnitude * stepScale * Time.deltaTime;
        if (stepSize <= 0f)
            return;

        strokeBuilder.Step(projected, stepSize);

        if (sendToGestureHolder)
        {
            gestureHolder.RequestUpdate(strokeBuilder.CurrentPos);
        }

        if (logDebugInfo)
        {
            SetStatus($"Capturing motion... pts: {strokeBuilder.PointCount}");
        }
    }

    public void EndMotionStroke()
    {
        if (!isCapturing)
            return;

        isCapturing = false;

        float duration = Time.time - captureStartTime;
        List<Vector2> rawPoints = strokeBuilder.GetPointsCopy();

        if (sendToGestureHolder && gestureHolder != null)
        {
            GestureKind kind = gestureHolder.RequestEnd();
            Debug.Log($"[Motion_To_2D_Stroke] GestureHolder returned {kind}");

            if (drawBoardController != null && kind != GestureKind.Invalid)
            {
                drawBoardController.ApplyGestureFromJoycon(kind);
            }
        }

        if (rawPoints.Count < minPointCount)
        {
            SetStatus("Motion finished (too few points)");
            if (logDebugInfo)
            {
                SetDebug($"Unknown (pts: {rawPoints.Count}, dur: {duration:F2}s)");
            }

            DrawStrokeForDebug(null);

            return;
        }

        ProcessStroke(rawPoints, duration);
    }

    private Vector2 ProjectAccelerationTo2D(Vector3 accel)
    {
        Vector2 v;
        switch (projectionPlane)
        {
            case ProjectionPlane.XZ:
                v = new Vector2(accel.x, accel.z);
                break;
            case ProjectionPlane.YZ:
                v = new Vector2(accel.y, accel.z);
                break;
            case ProjectionPlane.XY:
            default:
                v = new Vector2(accel.x, accel.y);
                break;
        }

        if (invertX) v.x = -v.x;
        if (invertY) v.y = -v.y;

        return v;
    }

    private void ProcessStroke(List<Vector2> rawPoints, float duration)
    {
        List<Vector2> norm = NormalizePoints(rawPoints);

        float normLength = ComputePathLength(norm);

        List<Vector2> resampled = Resample(norm, resampleCount);

        List<int> dirs = QuantizeDirections(resampled, dirBins);
        string dirString = dirs.Count > 0 ? string.Join(",", dirs) : "(none)";

        DrawStrokeForDebug(useResampledStrokeForView ? resampled : norm);

        SetStatus("Motion finished (processed)");

        if (logDebugInfo)
        {
            string debug =
                $"Stroke: {rawPoints.Count} pts, dur: {duration:F2}s\n" +
                $"NormLen: {normLength:F2}, Resampled: {resampled.Count} pts\n" +
                $"Dirs ({dirs.Count}): [{dirString}]";
            SetDebug(debug);
        }

        if (resultHandler != null)
        {
            resultHandler.ProcessGesture(dirs, normLength);
        }
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

    private static List<Vector2> NormalizePoints(List<Vector2> src)
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

    private void DrawStrokeForDebug(List<Vector2> pts)
    {
        if (strokeRenderer == null)
            return;

        if (pts == null || pts.Count == 0)
        {
            strokeRenderer.positionCount = 0;
            strokeRenderer.enabled = false;
            return;
        }

        int count = pts.Count;
        strokeRenderer.positionCount = count;

        for (int i = 0; i < count; ++i)
        {
            Vector2 p = pts[i];
            Vector3 v = new Vector3(
                p.x * strokeViewScale + strokeViewOffset.x,
                p.y * strokeViewScale + strokeViewOffset.y,
                0f
            );
            strokeRenderer.SetPosition(i, v);
        }

        strokeRenderer.enabled = true;
    }



    private void SetStatus(string msg)
    {
        if (statusText != null)
            statusText.text = msg;
    }

    private void SetDebug(string msg)
    {
        if (debugText != null)
            debugText.text = msg;
    }

    private class MotionStrokeBuilder
    {
        private readonly List<Vector2> points = new List<Vector2>();
        private readonly float minSampleDistance;
        private Vector2 currentPos;

        public int PointCount => points.Count;
        public Vector2 CurrentPos => currentPos;

        public MotionStrokeBuilder(float minSampleDistance)
        {
            this.minSampleDistance = Mathf.Max(1e-4f, minSampleDistance);
        }

        public void BeginStroke(Vector2 startPos)
        {
            points.Clear();
            currentPos = startPos;
            points.Add(currentPos);
        }

        public void Step(Vector2 direction, float distance)
        {
            if (distance <= 0f)
                return;

            if (direction.sqrMagnitude < 1e-6f)
                return;

            Vector2 dirNorm = direction.normalized;
            Vector2 newPos = currentPos + dirNorm * distance;

            if (points.Count == 0 || Vector2.Distance(points[points.Count - 1], newPos) >= minSampleDistance)
            {
                currentPos = newPos;
                points.Add(currentPos);
            }
            else
            {
                currentPos = newPos;
            }
        }

        public List<Vector2> GetPointsCopy()
        {
            return new List<Vector2>(points);
        }
    }
}
