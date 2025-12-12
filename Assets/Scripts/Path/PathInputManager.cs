/*--------------------------------------------------------------------------------*
  File Name: PathInputManager.cs
  Authors: Sam Friedman

  Copyright DigiPen Institute of Technology
 *--------------------------------------------------------------------------------*/

using UnityEngine;


namespace GestureNX
{
    class PathInputManager : MonoBehaviour
    {
        public static PathInputManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }
        }

        /// <summary>
        /// Determines if the primary button of the working input device was just pressed.
        /// </summary>
        /// <returns></returns>
        public bool IsPressed()
        {
            return Input.GetMouseButtonDown(0);
        }

        /// <summary>
        /// Determines if the primary button of the working input device is pressed.
        /// </summary>
        public bool IsDown()
        {
            return Input.GetMouseButton(0);
        }

        /// <summary>
        /// Determines if the primary button of the working input device was just released.
        /// </summary>
        public bool IsReleased()
        {
            return Input.GetMouseButtonUp(0);
        }
    }
}
