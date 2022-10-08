using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Env_Item : MonoBehaviour
{
    public bool isProtected;
    public bool isStackable;
    public string str_ItemName;
    public string str_ItemDescription;
    public int itemValue;
    public int itemWeight;
    public int itemCount = 1;
    public ItemType itemType;
    public enum ItemType
    {
        weapon,
        armor,
        shield,
        consumable,
        spell,
        ammo,
        misc
    }

    //public but hidden variables
    [HideInInspector] public bool droppedObject;

    //scripts
    private Item_Weapon WeaponScript;
    private Item_Armor ArmorScript;
    private Item_Shield ShieldScript;
    private Item_Consumable ConsumableScript;
    private Item_Spell SpellScript;
    private Item_Ammo AmmoScript;

    private void Awake()
    {
        if (itemType == ItemType.weapon)
        {
            WeaponScript = GetComponent<Item_Weapon>();
        }
        else if (itemType == ItemType.armor)
        {
            ArmorScript = GetComponent<Item_Armor>();
        }
        else if (itemType == ItemType.shield)
        {
            ShieldScript = GetComponent<Item_Shield>();
        }
        else if (itemType == ItemType.consumable)
        {
            ConsumableScript = GetComponent<Item_Consumable>();
        }
        else if (itemType == ItemType.spell)
        {
            SpellScript = GetComponent<Item_Spell>();
        }
        else if (itemType == ItemType.ammo)
        {
            AmmoScript = GetComponent<Item_Ammo>();
        }
    }
}