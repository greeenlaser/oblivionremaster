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
    private UI_LoadingScreen LoadingScreenScript;
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
        LoadingScreenScript = GetComponent<UI_LoadingScreen>();
        PauseMenuScript = GetComponent<UI_PauseMenu>();
    }

    private void Update()
    {
        //open/close item wheel
        if (PauseMenuScript.isPlayerMenuOpen
            && PlayerMenuScipt.isPlayerInventoryOpen)
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
        }
        //switch item wheel assigned items
        else if (!LoadingScreenScript.par_LoadingUI.activeInHierarchy
                 && !PauseMenuScript.isPaused
                 && !PauseMenuScript.isConsoleOpen)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                EquipFromSlot(0);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                EquipFromSlot(1);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                EquipFromSlot(2);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                EquipFromSlot(3);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                EquipFromSlot(4);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                EquipFromSlot(5);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                EquipFromSlot(6);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha8))
            {
                EquipFromSlot(7);
            }
        }
        //force-closes item wheel if player inventory is not open
        else if (!LoadingScreenScript.par_LoadingUI.activeInHierarchy
                 && par_ItemWheel.activeInHierarchy
                 && !PauseMenuScript.isPaused
                 && !PlayerMenuScipt.isPlayerInventoryOpen)
        {
            if (!PauseMenuScript.isPlayerMenuOpen)
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
            && !item.GetComponent<Item_Weapon>().isUsing
            && !item.GetComponent<Item_Weapon>().isBlocking
            && !item.GetComponent<Item_Weapon>().isAiming
            && !item.GetComponent<Item_Weapon>().isReloading)
        {
            //remove this weapon from any previously assigned slots
            foreach (Slot_ItemWheel slotScript in slotScripts)
            {
                if (slotScript.assignedItem == item)
                {
                    slotScript.img_SlotImage.texture = slotScript.defaultTexture;
                    slotScript.assignedItem = null;
                }
            }
            slotScripts[slot].img_SlotImage.texture = item.GetComponent<Item_Weapon>().img_ItemLogo;
            slotScripts[slot].assignedItem = item;

            Debug.Log("Info: Assigned " + item.name.Replace("_", " ") + " to slot " + (slot + 1) + ".");
        }
        else
        {
            Debug.LogWarning("Warning: Cannot assign " + item.name.Replace("_", " ") + " to another slot because it is in use!");
        }
    }

    //equip an item from a slot
    private void EquipFromSlot(int slot)
    {
        if (slotScripts[slot].assignedItem != null)
        {
            //switch to weapon
            if (slotScripts[slot].assignedItem.GetComponent<Item_Weapon>() != null)
            {
                if (PlayerInventoryScript.equippedWeapon == null
                    || (PlayerInventoryScript.equippedWeapon != null
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
    }
}