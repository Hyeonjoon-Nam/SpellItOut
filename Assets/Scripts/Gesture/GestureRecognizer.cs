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

            // Check all the templates and calculate score
            foreach (var t in templates)
            {
                if (t == null || t.pattern == null || t.pattern.Length == 0)
                    continue;

                float score = GestureRecognitionUtil.DirectionSequenceDistance(dirs, t.pattern);
                if (score < bestScore)
                {
                    bestScore = score;
                    bestKind = t.GetKind();
                }
            }

            const float acceptThreshold = 4.0f;

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

        internal static float DirectionSequenceDistance(List<int> a, int[] b)
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
    }
}
