/*--------------------------------------------------------------------------------*
  File Name: ControllerSystem.cs
  Authors: Haneul Lee

  Copyright DigiPen Institute of Technology
*--------------------------------------------------------------------------------*/

using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Status")]
    public bool isAlive = true;
    public int life = 5;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float gravity = -9.81f;

    [Header("Footstep SFX")]
    public AudioClip footstepClip;
    public float footstepInterval = 0.4f;

    private float footstepTimer = 0f;
    private CharacterController controller;
    private Vector3 velocity;
    private ItemPickupDetector itemPickupDetector;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        itemPickupDetector = GetComponent<ItemPickupDetector>();
        if (itemPickupDetector == null)
            itemPickupDetector = gameObject.AddComponent<ItemPickupDetector>();
    }

    void Update()
    {
        if (!isAlive)
            return;

        float x = 0f, z = 0f;

        x = Input.GetAxis("Horizontal");
        z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * moveSpeed * Time.deltaTime);

        // Apply gravity
        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        HandleFootsteps(new Vector2(x, z));
    }

    private void HandleFootsteps(Vector2 move)
    {
        if (move.sqrMagnitude > 0.01f)
        {
            footstepTimer -= Time.deltaTime;
            if (footstepTimer <= 0f)
            {
                footstepTimer = footstepInterval;
                PlayFootstep();
            }
        }
        else
        {
            footstepTimer = 0f;
        }
    }

    private void PlayFootstep()
    {
        if (footstepClip != null)
            AudioSource.PlayClipAtPoint(footstepClip, transform.position);
    }

    public void SetIsAlive(bool value)
    {
        isAlive = value;
    }

    public void Move(Vector2 input)
    {
        Vector3 moveDir = new Vector3(input.x, 0f, input.y);
        if (moveDir.sqrMagnitude < 0.0001f) return;

        moveDir.Normalize();
        transform.position += moveDir * moveSpeed * Time.deltaTime;
        transform.forward = moveDir;
    }
}