/*--------------------------------------------------------------------------------*
  File Name: Item.cs
  Authors: Haneul Lee

  Copyright DigiPen Institute of Technology
 *--------------------------------------------------------------------------------*/

using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    [Header("Item Info")]
    public string itemName = "New Item";
    public string description = "Item description";
    public Sprite icon;
    public int maxStack = 1;
    
    [Header("Item Type")]
    public ItemType itemType = ItemType.Consumable;
    
    public enum ItemType
    {
        Consumable,
        Equipment,
        Material,
        Key
    }
    
    public virtual void Use()
    {
        Debug.Log($"Using item: {itemName}");
    }
}


