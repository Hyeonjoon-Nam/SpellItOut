/*--------------------------------------------------------------------------------*
  File Name: Inventory.cs
  Authors: Haneul Lee

  Copyright DigiPen Institute of Technology
 *--------------------------------------------------------------------------------*/

using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventorySlot
{
    public Item item;
    public int quantity;

    public InventorySlot(Item item, int quantity)
    {
        this.item = item;
        this.quantity = quantity;
    }
}

public class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }

    [Header("Inventory Settings")]
    public int maxSlots = 30;
    
    private List<InventorySlot> items = new List<InventorySlot>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool AddItem(Item item, int quantity = 1)
    {
        if (item == null) return false;

        if (item.maxStack > 1)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].item == item)
                {
                    int canAdd = item.maxStack - items[i].quantity;
                    if (canAdd > 0)
                    {
                        int addAmount = Mathf.Min(canAdd, quantity);
                        items[i].quantity += addAmount;
                        
                        Debug.Log($"Added {addAmount}x {item.itemName} to inventory (Total: {items[i].quantity})");
                        
                        if (quantity > addAmount)
                        {
                            return AddItem(item, quantity - addAmount);
                        }
                        return true;
                    }
                }
            }
        }

        if (items.Count >= maxSlots)
        {
            Debug.LogWarning("Inventory is full!");
            return false;
        }

        items.Add(new InventorySlot(item, quantity));
        Debug.Log($"Added {quantity}x {item.itemName} to inventory");
        return true;
    }

    public bool RemoveItem(Item item, int quantity = 1)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].item == item)
            {
                if (items[i].quantity >= quantity)
                {
                    items[i].quantity -= quantity;
                    
                    if (items[i].quantity <= 0)
                    {
                        items.RemoveAt(i);
                    }
                    
                    Debug.Log($"Removed {quantity}x {item.itemName} from inventory");
                    return true;
                }
            }
        }
        return false;
    }

    public int GetItemCount(Item item)
    {
        int count = 0;
        foreach (var slot in items)
        {
            if (slot.item == item)
            {
                count += slot.quantity;
            }
        }
        return count;
    }

    public bool HasItem(Item item, int quantity = 1)
    {
        return GetItemCount(item) >= quantity;
    }

    public List<InventorySlot> GetAllItems()
    {
        return new List<InventorySlot>(items);
    }

    public void Clear()
    {
        items.Clear();
        Debug.Log("Inventory cleared");
    }
}


