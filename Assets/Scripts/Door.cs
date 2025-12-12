/*--------------------------------------------------------------------------------*
  File Name: Door.cs
  Authors: Nathaniel Thoma

  Copyright DigiPen Institute of Technology
 *--------------------------------------------------------------------------------*/

using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Door : MonoBehaviour
{
    public Transform leftDoor;
    public Transform rightDoor;
    public int level = 0;

    private BoxCollider bc;

    public float rotateSpeed = 2f;

    private Quaternion leftStartRot;
    private Quaternion leftEndRot;
    private Quaternion rightStartRot;
    private Quaternion rightEndRot;
    private bool openDoor = false;
    public bool alreadyOpen = false;

    private GameManager gameManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        bc = GetComponent<BoxCollider>();
        gameManager = FindFirstObjectByType<GameManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (alreadyOpen) return;

        if (other.GetComponent<PlayerController>() && gameManager.currentLevel >= level)
        {
            if (leftDoor && rightDoor)
            {
                // Left Door
                leftStartRot = leftDoor.rotation;
                leftEndRot = Quaternion.Euler(
                    leftStartRot.eulerAngles.x,
                    leftStartRot.eulerAngles.y - 90f,
                    leftStartRot.eulerAngles.z
                );

                // Right Door
                rightStartRot = rightDoor.rotation;
                rightEndRot = Quaternion.Euler(
                    rightStartRot.eulerAngles.x,
                    rightStartRot.eulerAngles.y + 90f,
                    rightStartRot.eulerAngles.z
                );

                // Flags
                openDoor = true;
                alreadyOpen = true;
            }
        }
    }

    private void Update()
    {
        if (openDoor)
        {
            // Update left door
            leftDoor.rotation = Quaternion.Lerp(leftDoor.rotation, leftEndRot, Time.deltaTime * rotateSpeed);
            if (Quaternion.Angle(leftDoor.rotation, leftEndRot) < 0.1f)
            {
                leftDoor.rotation = leftEndRot;
                openDoor = false;
            }

            // Update right door
            rightDoor.rotation = Quaternion.Lerp(rightDoor.rotation, rightEndRot, Time.deltaTime * rotateSpeed);
            if (Quaternion.Angle(rightDoor.rotation, rightEndRot) < 0.1f)
            {
                rightDoor.rotation = rightEndRot;
                openDoor = false;
            }
        }
    }
}
