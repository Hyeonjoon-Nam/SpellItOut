/*--------------------------------------------------------------------------------*
  File Name: ItemPickup.cs
  Authors: Haneul Lee

  Copyright DigiPen Institute of Technology
 *--------------------------------------------------------------------------------*/

using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ItemPickup : MonoBehaviour
{
    [Header("Item Settings")]
    public Item item;
    public int quantity = 1;
    
    [Header("Pickup Settings")]
    public float pickupRadius = 2f;
    public bool autoPickup = false;
    
    [Header("Visual Settings")]
    public bool rotate = true;
    public float rotationSpeed = 90f;
    public bool bobUpDown = true;
    public float bobSpeed = 2f;
    public float bobAmount = 0.2f;
    
    private Vector3 startPosition;
    private Collider pickupCollider;
    
    void Start()
    {
        startPosition = transform.position;
        pickupCollider = GetComponent<Collider>();
        
        if (pickupCollider != null)
        {
            pickupCollider.isTrigger = true;
        }
        
        if (item == null)
        {
            Debug.LogError($"ItemPickup on {gameObject.name} has no item assigned!");
        }
    }
    
    void Update()
    {
        if (rotate)
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
        
        if (bobUpDown)
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobAmount;
            transform.position = new Vector3(startPosition.x, newY, startPosition.z);
        }
    }
    
    public bool TryPickup()
    {
        if (item == null) return false;
        
        if (Inventory.Instance != null && Inventory.Instance.AddItem(item, quantity))
        {
            Debug.Log($"Picked up {quantity}x {item.itemName}!");
            Destroy(gameObject);
            return true;
        }
        else
        {
            Debug.LogWarning("Failed to pick up item - inventory might be full");
            return false;
        }
    }
    
    public bool IsInRange(Vector3 position)
    {
        float distance = Vector3.Distance(transform.position, position);
        return distance <= pickupRadius;
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}


