/*--------------------------------------------------------------------------------*
  File Name: TopDownCamera.cs
  Authors: Nathaniel Thoma

  Copyright DigiPen Institute of Technology
 *--------------------------------------------------------------------------------*/

using UnityEngine;

public class TopDownCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Camera Settings")]
    public float distance = 10f;      // How far behind the target
    public float height = 10f;        // How high above the target
    public float followSpeed = 5f;    // Smoothness of following
    public float tiltAngle = 40f;     // Tilt angle downward

    private void LateUpdate()
    {
        if (target == null) return;

        // Camera stays at a fixed rotation
        transform.rotation = Quaternion.Euler(tiltAngle, 0f, 0f);

        // Follow target at a fixed offset
        Vector3 desiredPos = target.position + new Vector3(0f, height, -distance);

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPos,
            followSpeed * Time.deltaTime
        );
    }
}
