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
    [Min(1)] public int itemCount;
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
    [HideInInspector] public bool droppedObject;

    private void Awake()
    {
        //forces default layer for all items
        if (gameObject.layer != LayerMask.NameToLayer("LimitedCollision"))
        {
            gameObject.layer = LayerMask.NameToLayer("LimitedCollision");
        }
    }
}