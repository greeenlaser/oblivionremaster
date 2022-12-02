using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager_ItemWheel : MonoBehaviour
{
    [Header("Assignables")]
    [SerializeField] private GameObject par_ItemWheel;
    public List<Slot_ItemWheel> slotScripts = new();

    [Header("Scripts")]
    [SerializeField] private GameObject thePlayer;

    //public but hidden variables
    [HideInInspector] public string assignableItemName;

    //scripts
    private Player_Movement PlayerMovementScript;
    private Player_Camera PlayerCameraScript;
    private UI_Inventory PlayerInventoryScript;
    private UI_PlayerMenu PlayerMenuScipt;
    private Manager_KeyBindings KeyBindingsScript;
    private UI_PauseMenu PauseMenuScript;

    //private variables
    private bool calledItemWheelShowOnce;
    private bool calledItemWheelHideOnce;

    private void Awake()
    {
        PlayerMovementScript = thePlayer.GetComponent<Player_Movement>();
        PlayerCameraScript = thePlayer.GetComponentInChildren<Player_Camera>();
        PlayerInventoryScript = thePlayer.GetComponent<UI_Inventory>();
        PlayerMenuScipt = GetComponent<UI_PlayerMenu>();
        KeyBindingsScript = GetComponent<Manager_KeyBindings>();
        PauseMenuScript = GetComponent<UI_PauseMenu>();
    }

    private void Update()
    {
        //can only open item wheel if game isnt paused
        //or if game is paused and player menu is open 
        if (!PauseMenuScript.isPaused
            || (PauseMenuScript.isPaused
            && PlayerMenuScipt.isPlayerInventoryOpen))
        {
            PauseMenuScript.isItemWheelOpen = KeyBindingsScript.GetKey("ToggleItemWheel");

            if (PauseMenuScript.isItemWheelOpen
                && !calledItemWheelShowOnce) 
            {
                if (!PauseMenuScript.isPaused)
                {
                    PauseMenuScript.PauseWithoutUI();
                }

                ShowItemWheel();
            }
            else if (!PauseMenuScript.isItemWheelOpen
                     && !calledItemWheelHideOnce)
            {
                if (PauseMenuScript.isPaused
                    && !PauseMenuScript.isPlayerMenuOpen)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;

                    Time.timeScale = 1;

                    PlayerMovementScript.canMove = true;
                    PlayerCameraScript.isCamEnabled = true;

                    PauseMenuScript.isPaused = false;
                }

                HideItemWheel();
            }

            //switch to item in slot
            if (!PauseMenuScript.isItemWheelOpen
                && !PauseMenuScript.isPaused)
            {
                if (KeyBindingsScript.GetKey("Alpha1")
                    && slotScripts[0].assignedItem != null)
                {
                    EquipFromSlot(1);
                }
                else if (KeyBindingsScript.GetKey("Alpha2")
                         && slotScripts[1].assignedItem != null)
                {
                    EquipFromSlot(2);
                }
                else if (KeyBindingsScript.GetKey("Alpha3")
                         && slotScripts[2].assignedItem != null)
                {
                    EquipFromSlot(3);
                }
                else if (KeyBindingsScript.GetKey("Alpha4")
                         && slotScripts[3].assignedItem != null)
                {
                    EquipFromSlot(4);
                }
                else if (KeyBindingsScript.GetKey("Alpha5")
                         && slotScripts[4].assignedItem != null)
                {
                    EquipFromSlot(5);
                }
                else if (KeyBindingsScript.GetKey("Alpha6")
                         && slotScripts[5].assignedItem != null)
                {
                    EquipFromSlot(6);
                }
                else if (KeyBindingsScript.GetKey("Alpha7")
                         && slotScripts[6].assignedItem != null)
                {
                    EquipFromSlot(7);
                }
                else if (KeyBindingsScript.GetKey("Alpha8")
                         && slotScripts[7].assignedItem != null)
                {
                    EquipFromSlot(8);
                }
            }
        }
    }

    public void ShowItemWheel()
    {
        par_ItemWheel.SetActive(true);

        calledItemWheelHideOnce = false;
        calledItemWheelShowOnce = true;
    }
    public void HideItemWheel()
    {
        par_ItemWheel.SetActive(false);

        calledItemWheelShowOnce = false;
        calledItemWheelHideOnce = true;
    }

    //assign item to slot
    public void AssignToSlot(GameObject item, int slot)
    {
        //can only switch weapon to other slot if it is not in use
        if (item.GetComponent<Item_Weapon>() != null
            && !item.GetComponent<Item_Weapon>().isCallingMainAttack
            && !item.GetComponent<Item_Weapon>().isUsing
            && !item.GetComponent<Item_Weapon>().isBlocking
            && !item.GetComponent<Item_Weapon>().isAiming
            && !item.GetComponent<Item_Weapon>().isReloading)
        {
            foreach (Slot_ItemWheel slotScript in slotScripts)
            {
                if (slotScript.assignedItem == item)
                {
                    slotScript.assignedItem = null;
                }
                break;
            }
            slotScripts[slot].assignedItem = item;
            slotScripts[slot].img_SlotImage.texture = item.GetComponent<Item_Weapon>().img_ItemLogo;

            Debug.Log("Info: Assigned " + item.name.Replace("_", " ") + " to slot " + slot + ".");
        }
        else
        {
            Debug.LogWarning("Warning: Cannot assign " + item.name.Replace("_", " ") + " to another slot because it is in use!");
        }
    }

    //equip an item from a slot
    private void EquipFromSlot(int slot)
    {
        Debug.Log("equipping item at slot " + slot + "...");

        if (slotScripts[slot].assignedItem != null)
        {
            //switch to weapon
            if (slotScripts[slot].assignedItem.GetComponent<Item_Weapon>() != null)
            {
                if (PlayerInventoryScript.equippedWeapon == null
                    || (PlayerInventoryScript.equippedWeapon != null
                    && !PlayerInventoryScript.equippedWeapon.GetComponent<Item_Weapon>().isCallingMainAttack
                    && !PlayerInventoryScript.equippedWeapon.GetComponent<Item_Weapon>().isUsing
                    && !PlayerInventoryScript.equippedWeapon.GetComponent<Item_Weapon>().isBlocking
                    && !PlayerInventoryScript.equippedWeapon.GetComponent<Item_Weapon>().isAiming
                    && !PlayerInventoryScript.equippedWeapon.GetComponent<Item_Weapon>().isReloading))
                {
                    PlayerInventoryScript.UseItem(slotScripts[slot].assignedItem);
                }
                else
                {
                    Debug.LogWarning("Warning: Cannot unequip " + PlayerInventoryScript.equippedWeapon.GetComponent<Env_Item>().itemName.Replace("_", " ") + " through item wheel because it is in use!");
                }
            }
        }
        else
        {
            Debug.LogWarning("Warning: No item assigned to slot " + slot + 1 + "!");
        }
    }
}