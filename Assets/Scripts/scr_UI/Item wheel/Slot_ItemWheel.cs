using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Slot_ItemWheel : MonoBehaviour, IPointerClickHandler
{
    [Header("Scripts")]
    [SerializeField] private GameObject thePlayer;
    [SerializeField] private GameObject par_Managers;

    //public but hidden variables
    [HideInInspector] public int slot;
    [HideInInspector] public GameObject assignedItem;
    [HideInInspector] public RawImage img_SlotImage;

    //scripts
    private UI_Inventory PlayerInventoryScript;
    private Manager_ItemWheel ItemWheelScript;

    private void Awake()
    {
        slot = int.Parse(name.Replace("Slot", ""));
        img_SlotImage = GetComponent<RawImage>();

        PlayerInventoryScript = thePlayer.GetComponent<UI_Inventory>();
        ItemWheelScript = par_Managers.GetComponent<Manager_ItemWheel>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        foreach (GameObject item in PlayerInventoryScript.playerItems)
        {
            Debug.Log(item.name + ", " + ItemWheelScript.assignableItemName + "...");

            if (item.name == ItemWheelScript.assignableItemName)
            {
                assignedItem = item;
                ItemWheelScript.AssignToSlot(assignedItem, slot);
                break;
            }
        }
    }
}