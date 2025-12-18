/*--------------------------------------------------------------------------------*
  File Name: GestureRecognizer.cs
  Authors: Hyeonjoon Nam, Sam Friedman

  Copyright DigiPen Institute of Technology
 *--------------------------------------------------------------------------------*/

using System.Collections.Generic;
using UnityEngine;

namespace GestureNX
{
    public enum GestureKind
    {
        Invalid = -1,
        AttackL,
        AttackR,
        Magic,
        Heal,
        Guard,
        DodgeL,
        DodgeR,
    }

    [System.Serializable]
    public class GestureTemplate
    {
        public string name;
        public int[] pattern;
        public Vector2[] shape;

        public GestureKind GetKind()
        {
            return name switch
            {
                "AttackL" => GestureKind.AttackL,
                "AttackR" => GestureKind.AttackR,
                "Magic" => GestureKind.Magic,
                "Heal" => GestureKind.Heal,
                "Guard" => GestureKind.Guard,
                "DodgeL" => GestureKind.DodgeL,
                "DodgeR" => GestureKind.DodgeR,
                _ => GestureKind.Invalid,
            };
        }
    }

    public class GestureRecognizer : MonoBehaviour
    {
        [Header("Templates")]
        public List<GestureTemplate> templates = new();

        [Header("Matching")]
        [Tooltip("Use weighted edit distance (more tolerant to missing/extra direction tokens).")]
        public bool useWeightedEditDistance = true;

        [Tooltip("Insert/Delete cost for edit distance. Lower = more tolerant.")]
        [Range(0.1f, 2.0f)]
        public float insertDeleteCost = 0.75f;

        [Tooltip("Scale for substitution cost (direction difference). Lower = more tolerant.")]
        [Range(0.25f, 2.0f)]
        public float substitutionScale = 1.0f;

        [Tooltip("Acceptance threshold for the chosen scoring method. Higher = more tolerant.")]
        [Range(0.0f, 20.0f)]
        public float acceptThreshold = 5.0f;

        public GestureKind Process(List<int> dirs, float normLength)
        {
            if (dirs == null || dirs.Count == 0)
            {
                return GestureKind.Invalid;
            }

            if (templates == null || templates.Count == 0)
            {
                return GestureKind.Invalid;
            }

            GestureKind bestKind = GestureKind.Invalid;
            float bestScore = float.MaxValue;

            foreach (var t in templates)
            {
                if (t == null || t.pattern == null || t.pattern.Length == 0)
                    continue;

                float score = useWeightedEditDistance
                    ? GestureRecognitionUtil.WeightedEditDistance(dirs, t.pattern, insertDeleteCost, substitutionScale)
                    : GestureRecognitionUtil.DirectionSequenceDistanceLegacy(dirs, t.pattern);

                if (score < bestScore)
                {
                    bestScore = score;
                    bestKind = t.GetKind();
                }
            }

            if (bestScore > acceptThreshold)
            {
                return GestureKind.Invalid;
            }

            return bestKind;
        }
    }

    static class GestureRecognitionUtil
    {
        private const int DirBins = 8;

        // Legacy scoring (your current approach)
        internal static float DirectionSequenceDistanceLegacy(List<int> a, int[] b)
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

        // Weighted Levenshtein distance for int direction sequences
        // - Substitution cost uses circular direction difference (0..4 for 8 bins)
        // - Insertion/Deletion cost is configurable
        internal static float WeightedEditDistance(List<int> a, int[] b, float insDelCost, float subScale)
        {
            int n = a.Count;
            int m = b.Length;

            if (n == 0) return m * insDelCost;
            if (m == 0) return n * insDelCost;

            // Two-row DP to reduce allocations
            float[] prev = new float[m + 1];
            float[] curr = new float[m + 1];

            prev[0] = 0f;
            for (int j = 1; j <= m; ++j)
                prev[j] = j * insDelCost;

            for (int i = 1; i <= n; ++i)
            {
                curr[0] = i * insDelCost;
                int ai = a[i - 1];

                for (int j = 1; j <= m; ++j)
                {
                    int bj = b[j - 1];

                    int diff = Mathf.Abs(ai - bj);
                    diff = Mathf.Min(diff, DirBins - diff);

                    float subCost = diff * subScale;

                    float del = prev[j] + insDelCost;
                    float ins = curr[j - 1] + insDelCost;
                    float sub = prev[j - 1] + subCost;

                    float best = del;
                    if (ins < best) best = ins;
                    if (sub < best) best = sub;

                    curr[j] = best;
                }

                // Swap rows
                var tmp = prev;
                prev = curr;
                curr = tmp;
            }

            return prev[m];
        }
    }
}
