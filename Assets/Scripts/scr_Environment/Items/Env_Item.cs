using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Env_Item : MonoBehaviour
{
    public bool isProtected;
    public bool isStackable;
    public string itemName = "REPLACE_MY_NAME";
    public string itemDescription = "REPLACE_MY_DESCRIPTION";
    public int itemValue = 0;
    [Range(0, 100)]
    public int itemWeight = 0;
    [Range(1, 1000000)]
    public int itemCount;

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

    [Header("Special variables")]
    public bool isEnchantable;
    public int soulGemSize;
    public int itemMaxDurability;

    //public but hidden variables
    [HideInInspector] public bool isEquipped;
    [HideInInspector] public bool droppedObject;
    [HideInInspector] public int itemCurrentDurability;
    [HideInInspector] public List<GameObject> activeEnchantments = new();

    private void Awake()
    {
        //forces default name for all items
        if (name != itemName)
        {
            name = itemName;
        }
    }
}