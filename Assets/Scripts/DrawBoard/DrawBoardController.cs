/*--------------------------------------------------------------------------------*
  File Name: DrawBoardController.cs
  Authors: Sam Friedman, Hyeonjoon Nam

  Copyright DigiPen Institute of Technology
 *--------------------------------------------------------------------------------*/

using UnityEngine;

namespace GestureNX
{
    public class DrawBoardController : MonoBehaviour
    {
        enum Status
        {
            Enter,
            Ready,
            Draw,
            Erase,
            Leave,
            Exit,
        }

        [SerializeField, HideInInspector]
        private Collider coll;

        [SerializeField, HideInInspector]
        private PathDrawer pathDrawer;

        [SerializeField]
        private Animator bgAnimator;

        [SerializeField]
        private PlayerCombat playerCombat;

        private Status status = default;

        private bool ApplyGesture(GestureKind kind)
        {
            return kind switch
            {
                GestureKind.AttackL or GestureKind.AttackR => playerCombat.MeleeAttack(),
                GestureKind.Magic => playerCombat.MagicAttack(),
                GestureKind.Heal => playerCombat.Heal(),
                GestureKind.Guard => playerCombat.Guard(),
                GestureKind.DodgeL => playerCombat.DodgeLeft(),
                GestureKind.DodgeR => playerCombat.DodgeRight(),
                _ => false,
            };
        }

        // Update is called once per frame
        private void Update()
        {
            switch (status)
            {
                case Status.Enter:
                    if (bgAnimator.GetCurrentAnimatorStateInfo(0).IsName("Ready"))
                    {
                        coll.enabled = true;
                        status = Status.Ready;
                    }
                    break;
                case Status.Ready:
                    if (PathInputManager.Instance.IsPressed())
                    {
                        bgAnimator.SetBool("IsDraw", true);
                        status = Status.Draw;
                    }
                    break;
                case Status.Draw:
                    if (PathInputManager.Instance.IsReleased())
                    {
                        bgAnimator.SetBool("IsValidGesture", ProcessGesture());
                        bgAnimator.SetBool("IsDraw", false);
                        coll.enabled = false;
                        status = Status.Erase;
                    }
                    break;
                case Status.Erase:
                    if (bgAnimator.GetCurrentAnimatorStateInfo(0).IsName("Ready"))
                    {
                        pathDrawer.Clear();
                        coll.enabled = true;
                        status = Status.Ready;
                    }
                    break;
                case Status.Leave:
                    if (bgAnimator.GetCurrentAnimatorStateInfo(0).IsName("TransitionExit"))
                    {
                        pathDrawer.Clear();
                        coll.enabled = false;
                        status = Status.Exit;
                    }
                    break;
                case Status.Exit:
                    break;
            }
        }

        // OnValidate is called when the script is loaded or a value is changed in the inspector
        private void OnValidate()
        {
            coll = GetComponent<Collider>();
            pathDrawer = GetComponent<PathDrawer>();
        }

        private bool ProcessGesture()
        {
            GestureKind kind = pathDrawer.Evaluate();
            return ApplyGesture(kind);
        }

        public void OnFightFinish()
        {
            bgAnimator.SetTrigger("IsFightFinish");
            status = Status.Leave;
        }
        public void ApplyGestureFromJoycon(GestureKind kind)
        {
            bool isValid = ApplyGesture(kind);

            bgAnimator.SetBool("IsValidGesture", isValid);
            bgAnimator.SetBool("IsDraw", false);
            coll.enabled = false;
            status = Status.Erase;

            Debug.Log($"[DrawBoardController] Joycon gesture: {kind}, valid={isValid}");
        }
    }
}
