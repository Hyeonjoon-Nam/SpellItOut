/*--------------------------------------------------------------------------------*
  File Name: PathDrawTimeLimit.cs
  Authors: Sam Friedman

  Copyright DigiPen Institute of Technology
 *--------------------------------------------------------------------------------*/

using UnityEngine;

namespace GestureNX
{
    public class PathDrawTimeLimit : MonoBehaviour
    {
        [SerializeField]
        private PathDrawer pathDrawer;

        [SerializeField]
        private Transform pivot;

        /// <summary>
        /// The number of seconds to draw a character before it resets.
        /// </summary>
        [SerializeField, Range(0.0f, 10.0f)]
        private float timeLimit;

        /// <summary>
        /// The number of seconds remaining.
        /// </summary>
        private float timer;

        // Awake is called when the script instance is being loaded
        private void Awake()
        {
            timer = timeLimit;
        }

        // Update is called once per frame
        private void Update()
        {
            timer -= Time.deltaTime;

            if (timer > 0.0f)
            {
                pivot.localScale = new(timer / timeLimit, 1.0f, 1.0f);
            }
            else
            {
                pivot.localScale = Vector3.one;
                
                pathDrawer.Clear();
                
                timer = timeLimit;
            }
        }
    }
}
