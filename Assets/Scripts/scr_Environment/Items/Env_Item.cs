using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Env_Item : MonoBehaviour
{
    [Tooltip("Checks whether this item can be sold, dropped and destroyed through console.")]
    public bool isProtected;
    [Tooltip("Checks whether there can be more than one of this item.")]
    public bool isStackable;
    [Tooltip("How this item will be referenced as and displayed in the game.")]
    public string itemName = "REPLACE_MY_NAME";
    [Tooltip("A longer description of this item.")]
    public string itemDescription = "REPLACE_MY_DESCRIPTION";
    [Tooltip("Base value of this item.")]
    public int itemValue = 0;
    [Tooltip("How much space will this item take in player inventory.")]
    [Range(0, 100)]
    public int itemWeight = 0;

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
    public int itemMaxDurability;
    public int soulGemSize;

    //public but hidden variables
    [HideInInspector] public bool isEquipped;
    [HideInInspector] public bool droppedObject;
    [HideInInspector] public int itemCount = 1;
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