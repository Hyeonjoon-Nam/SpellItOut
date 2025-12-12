/*--------------------------------------------------------------------------------*
  File Name: BattleCooldownBar.cs
  Authors: Sam Friedman

  Copyright DigiPen Institute of Technology
 *--------------------------------------------------------------------------------*/

using UnityEngine;
using UnityEngine.UI;

namespace GestureNX
{
    public class BattleCooldownBar : MonoBehaviour
    {
        [SerializeField]
        private Transform pivot;

        [SerializeField]
        private Image image;

        /// <summary>
        /// The color of the bar with a cooldown.
        /// </summary>
        [SerializeField]
        private Color colorMin;

        /// <summary>
        /// The color of the bar with no cooldown.
        /// </summary>
        [SerializeField]
        private Color colorMax;

        /// <summary>
        /// The number of seconds remaining.
        /// </summary>
        private float timer = default;

        /// <summary>
        /// The number of seconds for the cooldown.
        /// </summary>
        private float timeLimit = default;

        /// <summary>
        /// The number of seconds to draw a character before it resets.
        /// </summary>
        public float TimeLimit
        {
            private get
            {
                return timeLimit;
            }

            set
            {
                timer = value;
                timeLimit = value;
            }
        }
        
        /// <summary>
        /// The scale of the pivot GameObject along the x-axis.
        /// </summary>
        private float PivotScaleX
        {
            set
            {
                pivot.localScale = new(value, 1.0f, 1.0f);
            }
        }

        /// <summary>
        /// The minimum scale for the pivot GameObject along the x-axis.
        /// </summary>
        private readonly float scaleMinX = 0.0125f;

        /// <summary>
        /// The color of the bar.
        /// </summary>
        private float ColorInterpolant
        {
            set
            {
                image.color = Color.Lerp(colorMin, colorMax, value);
            }
        }

        // Awake is called when the script instance is being loaded
        private void Awake()
        {
            PivotScaleX = scaleMinX;
        }

        // Update is called once per frame
        private void Update()
        {
            if (timer <= 0.0f)
            {
                return;
            }

            timer -= Time.deltaTime;

            if (timer > 0.0f)
            {
                float interpolant = Mathf.Max(TimeLimit - timer, scaleMinX) / TimeLimit;

                PivotScaleX = interpolant;
                ColorInterpolant = interpolant;
            }
            else
            {
                float interpolant = 1.0f;

                PivotScaleX = interpolant;
                ColorInterpolant = interpolant;
            }
        }

        // OnValidate is called when the script is loaded or a value is changed in the inspector
        private void OnValidate()
        {
            image = pivot.transform.GetChild(0).GetComponent<Image>();
        }
    }
}
