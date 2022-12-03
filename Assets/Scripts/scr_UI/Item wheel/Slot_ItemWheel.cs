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
    [HideInInspector] public Texture defaultTexture;

    //scripts
    private UI_Inventory PlayerInventoryScript;
    private Manager_ItemWheel ItemWheelScript;

    private void Awake()
    {
        slot = int.Parse(name.Replace("Slot", "")) -1;
        img_SlotImage = GetComponent<RawImage>();
        defaultTexture = img_SlotImage.texture;

        PlayerInventoryScript = thePlayer.GetComponent<UI_Inventory>();
        ItemWheelScript = par_Managers.GetComponent<Manager_ItemWheel>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        foreach (GameObject item in PlayerInventoryScript.playerItems)
        {
            if (item.name == ItemWheelScript.assignableItemName.Replace(" ", "_"))
            { 
                ItemWheelScript.AssignToSlot(item, slot);
                break;
            }
        }
    }
}