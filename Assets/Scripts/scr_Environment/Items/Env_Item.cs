using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Env_Item : MonoBehaviour
{
    public bool isProtected;
    public bool isStackable;
    public bool isEnchantable;
    public string itemName = "REPLACE_MY_NAME";
    public string itemDescription = "REPLACE_MY_DESCRIPTION";
    public int itemValue = 0;
    [Range(0, 100)]
    public int itemWeight = 0;
    [Range(1, 1000000)]
    public int itemCount;
    public int itemMaxDurability;
    public ItemType itemType = ItemType.misc;
    public enum ItemType
    {
        weapon,
        armor,
        shield,
        consumable,
        alchemyIngredient,
        spell,
        ammo,
        enchantment,
        misc
    }
    public ItemQuality itemQuality = ItemQuality.common;
    public enum ItemQuality
    {
        trash,
        common,
        rare,
        legendary
    }

    //public but hidden variables
    [HideInInspector] public bool isEquipped;
    [HideInInspector] public bool droppedObject;
    [HideInInspector] public int itemCurrentDurability;

    private void Awake()
    {
        //forces default name for all items
        if (name != itemName)
        {
            name = itemName;
        }
    }
}