/*--------------------------------------------------------------------------------*
  File Name: ItemPickupDetector.cs
  Authors: Haneul Lee

  Copyright DigiPen Institute of Technology
 *--------------------------------------------------------------------------------*/

using System.Collections.Generic;
using UnityEngine;

public class ItemPickupDetector : MonoBehaviour
{
    [Header("Pickup Settings")]
    public float detectionRadius = 3f;
    public KeyCode pickupKey = KeyCode.E;
    
    [Header("Debug")]
    public bool showDebugGizmos = true;
    
    private List<ItemPickup> nearbyItems = new List<ItemPickup>();
    private ItemPickup closestItem;
    
    void Update()
    {
        DetectNearbyItems();
        FindClosestItem();
        
        if (Input.GetKeyDown(pickupKey))
        {
            TryPickupClosestItem();
        }
    }
    
    void DetectNearbyItems()
    {
        nearbyItems.Clear();
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);
        
        foreach (Collider col in colliders)
        {
            ItemPickup itemPickup = col.GetComponent<ItemPickup>();
            if (itemPickup != null && itemPickup.IsInRange(transform.position))
            {
                nearbyItems.Add(itemPickup);
            }
        }
    }
    
    void FindClosestItem()
    {
        closestItem = null;
        float closestDistance = float.MaxValue;
        
        foreach (ItemPickup item in nearbyItems)
        {
            float distance = Vector3.Distance(transform.position, item.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestItem = item;
            }
        }
    }
    
    void TryPickupClosestItem()
    {
        if (closestItem != null)
        {
            closestItem.TryPickup();
        }
        else if (nearbyItems.Count > 0)
        {
            nearbyItems[0].TryPickup();
        }
    }
    
    void OnDrawGizmos()
    {
        if (showDebugGizmos)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
        }
    }
}


