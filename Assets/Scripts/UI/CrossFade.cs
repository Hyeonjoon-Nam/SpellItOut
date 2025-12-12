/*--------------------------------------------------------------------------------*
  File Name: CrossFade.cs
  Authors: Nathaniel Thoma

  Copyright DigiPen Institute of Technology
 *--------------------------------------------------------------------------------*/

using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CrossFade : SceneTransition
{
    public CanvasGroup crossFade;
    public float duration = 1.0f;

    public override IEnumerator AnimateTransitionIn()
    {
        // Enable this to block further inputs
        crossFade.GetComponent<Image>().raycastTarget = true;

        float t = 0.0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            crossFade.alpha = Mathf.Lerp(0.0f, 1.0f, t / duration);
            yield return null;
        }
        crossFade.alpha = 1.0f;
    }

    public override IEnumerator AnimateTransitionOut()
    {
        float t = 0.0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            crossFade.alpha = Mathf.Lerp(1.0f, 0.0f, t / duration);
            yield return null;
        }
        crossFade.alpha = 0.0f;
    }
}
