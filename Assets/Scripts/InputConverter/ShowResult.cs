/*--------------------------------------------------------------------------------*
  File Name: ShowResult.cs
  Authors: Hyeonjoon (Joon) Nam

  Copyright DigiPen Institute of Technology
 *--------------------------------------------------------------------------------*/

using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShowResult : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text gestureNameText;
    public TMP_Text debugText;

    [Header("Shape Rendering")]
    public LineRenderer shapeRenderer;

    private const int DirBins = 8;

    [System.Serializable]
    public class GestureTemplate
    {
        public string name;
        public int[] pattern;
        public Vector2[] shape;
    }

    [Header("Templates")]
    public List<GestureTemplate> templates = new List<GestureTemplate>();


    // Called by Line_To_8Dir when a stroke is finished
    public void ProcessGesture(List<int> dirs, float normLength)
    {
        if (dirs == null || dirs.Count == 0)
        {
            SetTexts("Unknown", "No direction data.");
            return;
        }

        if (templates == null || templates.Count == 0)
        {
            SetTexts("Unknown", $"No templates set.\nDirs: [{string.Join(",", dirs)}]");
            return;
        }

        string bestName = "Unknown";
        float bestScore = float.MaxValue;
        GestureTemplate bestTemplate = null;

        // Check all the templates and calculate score
        foreach (var t in templates)
        {
            if (t == null || t.pattern == null || t.pattern.Length == 0)
                continue;

            float score = DirectionSequenceDistance(dirs, t.pattern);
            if (score < bestScore)
            {
                bestScore = score;
                bestName = t.name;
                bestTemplate = t;
            }
        }

        const float acceptThreshold = 4.0f;
        if (bestScore > acceptThreshold)
        {
            bestName = "Unknown";
            ClearShape();
        }
        else
        {
            DrawShape(bestTemplate);
        }

        string debug =
            $"Dirs: [{string.Join(",", dirs)}]\n" +
            $"Best: {bestName}, Score: {bestScore:F2}, NormLen: {normLength:F2}";

        SetTexts(bestName, debug);
    }

    // Calculate distance
    private float DirectionSequenceDistance(List<int> a, int[] b)
    {
        int maxLen = Mathf.Max(a.Count, b.Length);
        float sum = 0f;

        for (int i = 0; i < maxLen; ++i)
        {
            int da = i < a.Count ? a[i] : a[a.Count - 1];
            int db = i < b.Length ? b[i] : b[b.Length - 1];

            int diff = Mathf.Abs(da - db);
            diff = Mathf.Min(diff, DirBins - diff);
            sum += diff;
        }

        sum += Mathf.Abs(a.Count - b.Length) * 0.5f;

        return sum;
    }

    private void DrawShape(GestureTemplate template)
    {
        if (shapeRenderer == null || template.shape == null || template.shape.Length == 0)
            return;

        shapeRenderer.positionCount = template.shape.Length;

        for (int i = 0; i < template.shape.Length; ++i)
        {
            Vector2 p = template.shape[i];
            shapeRenderer.SetPosition(i, new Vector3(p.x, p.y, 0f));
        }

        shapeRenderer.enabled = true;
    }

    private void ClearShape()
    {
        if (shapeRenderer != null)
        {
            shapeRenderer.positionCount = 0;
            shapeRenderer.enabled = false;
        }
    }

    // UI function
    private void SetTexts(string gestureName, string debug)
    {
        if (gestureNameText != null)
            gestureNameText.text = gestureName;

        if (debugText != null)
            debugText.text = debug;
    }
}
