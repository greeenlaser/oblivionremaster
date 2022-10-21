using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Env_Item : MonoBehaviour
{
    public bool isProtected;
    public bool isStackable;
    public string str_ItemName = "REPLACE_MY_NAME";
    public string str_ItemDescription = "REPLACE_MY_DESCRIPTION";
    [Range(0, 25000)]
    public int itemValue = 0;
    [Range(0, 100)]
    public int itemWeight = 0;
    [Range(1, 999)]
    public int itemCount = 1;
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