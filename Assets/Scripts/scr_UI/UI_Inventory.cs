using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_Inventory : MonoBehaviour
{
    [Header("General assignables")]
    public GameObject par_PlayerItems;
    [SerializeField] private Transform pos_EquippedWeapon;
    public ContainerType containerType = ContainerType.player;
    public enum ContainerType
    {
        player,
        container,
        altar_of_enchanting,
    }

    [Header("Container assignables")]
    public GameObject par_ContainerItems;
    public string containerName;

    [Header("Scripts")]
    [SerializeField] private GameObject thePlayer;
    [SerializeField] private GameObject par_Managers;

    //public but hidden variables
    [HideInInspector] public bool tumbler1Unlocked;
    [HideInInspector] public bool tumbler2Unlocked;
    [HideInInspector] public bool tumbler3Unlocked;
    [HideInInspector] public bool tumbler4Unlocked;
    [HideInInspector] public bool tumbler5Unlocked;
    [HideInInspector] public string currentlyOpenedInventory;
    [HideInInspector] public GameObject heldObject;
    [HideInInspector] public List<GameObject> playerItems = new();
    [HideInInspector] public List<GameObject> containerItems = new();
    [HideInInspector] public Env_LockStatus LockStatusScript;

    //weapon
    [HideInInspector] public GameObject equippedWeapon;
    [HideInInspector] public GameObject lastEquippedWeapon;
    [HideInInspector] public bool isWeaponUnsheathed;
    [HideInInspector] public bool isSheathingUnsheathingWeapon;

    //scripts
    private UI_Inventory PlayerInventoryScript;
    private UI_Lockpicking LockpickingScript;
    private Player_Stats PlayerStatsScript;
    private UI_PlayerMenu PlayerMenuScript;
    private UI_PauseMenu PauseMenuScript;
    private UI_Confirmation ConfirmationScript;
    private Manager_Announcements AnnouncementScript;
    private Manager_KeyBindings KeyBindingsScript;
    private Manager_UIReuse UIReuseScript;

    //private variables
    private GameObject selectedEnchantableItem;
    private GameObject selectedEnchantment;
    private GameObject selectedSoulGem;

    private void Awake()
    {
        PlayerInventoryScript = thePlayer.GetComponent<UI_Inventory>();
        if (containerType == ContainerType.container)
        {
            LockpickingScript = par_Managers.GetComponent<UI_Lockpicking>();
        }
        if (par_ContainerItems != null)
        {
            LockStatusScript = GetComponent<Env_LockStatus>();
        }
        PlayerStatsScript = GetComponent<Player_Stats>();
        PlayerMenuScript = par_Managers.GetComponent<UI_PlayerMenu>();
        ConfirmationScript = par_Managers.GetComponent<UI_Confirmation>();
        PauseMenuScript = par_Managers.GetComponent<UI_PauseMenu>();
        AnnouncementScript = par_Managers.GetComponent<Manager_Announcements>();
        KeyBindingsScript = par_Managers.GetComponent<Manager_KeyBindings>();
        UIReuseScript = par_Managers.GetComponent<Manager_UIReuse>();
    }

    private void Update()
    {
        if (transform.name == "Player")
        {
            //update last equipped weapon if a new weapon was equipped
            if (equippedWeapon != null
                && lastEquippedWeapon != equippedWeapon)
            {
                lastEquippedWeapon = equippedWeapon;
            }

            //equip/unequip last equipped weapon
            //if any weapons have been held in this game instance
            if (KeyBindingsScript.GetKeyDown("EquipUnequipWeapon")
                && lastEquippedWeapon != null
                && !PauseMenuScript.isConsoleOpen
                && !isSheathingUnsheathingWeapon
                && !equippedWeapon.GetComponent<Item_Weapon>().isCallingMainAttack
                && !equippedWeapon.GetComponent<Item_Weapon>().isUsing
                && !equippedWeapon.GetComponent<Item_Weapon>().isBlocking
                && !equippedWeapon.GetComponent<Item_Weapon>().isAiming
                && !equippedWeapon.GetComponent<Item_Weapon>().isAiming
                && !equippedWeapon.GetComponent<Item_Weapon>().isReloading
                && !equippedWeapon.GetComponent<Item_Weapon>().isCasting)
            {
                if (!isWeaponUnsheathed)
                {
                    equippedWeapon.GetComponent<Item_Weapon>().instantiatedWeapon.GetComponent<Anim_Weapon>().StartAnimation("unsheath", 2);
                }
                else
                {
                    equippedWeapon.GetComponent<Item_Weapon>().instantiatedWeapon.GetComponent<Anim_Weapon>().StartAnimation("sheath", 2);
                }
            }
        }
    }

    //used for only respawnable containers,
    //checks if the target container is locked or not
    public void CheckIfLocked()
    {
        if (LockStatusScript.isUnlocked)
        {
            PlayerMenuScript.targetContainer = gameObject;
            PauseMenuScript.isPlayerMenuOpen = true;
        }
        else if (!LockStatusScript.isUnlocked
                 && containerType == ContainerType.container)
        {
            if (!LockStatusScript.needsKey)
            {
                bool hasLockpicks = false;
                foreach (GameObject item in PlayerInventoryScript.playerItems)
                {
                    if (item.name == "Lockpick")
                    {
                        PauseMenuScript.isPlayerMenuOpen = true;
                        hasLockpicks = true;

                        LockpickingScript.LockStatusScript = LockStatusScript;
                        LockpickingScript.OpenlockpickUI();

                        break;
                    }
                }
                if (!hasLockpicks)
                {
                    AnnouncementScript.CreateAnnouncement("Did not find lockpicks to unlock " + containerName + "!");
                    Debug.LogWarning("Requirements not met: Did not find lockpicks to unlock " + containerName + "!");
                }
            }
            else
            {
                bool foundKey = false;
                foreach (GameObject item in PlayerInventoryScript.playerItems)
                {
                    if (item == LockStatusScript.key)
                    {
                        foundKey = true;
                        break;
                    }
                }

                if (!foundKey)
                {
                    AnnouncementScript.CreateAnnouncement("Did not find right key to unlock " + containerName + "!");
                    Debug.LogWarning("Requirements not met: Did not find right key to unlock " + containerName + "!");
                }
                else
                {
                    PlayerInventoryScript.playerItems.Remove(LockStatusScript.key);
                    foreach (Transform item in PlayerInventoryScript.par_PlayerItems.transform)
                    {
                        if (item.gameObject == LockStatusScript.key)
                        {
                            Destroy(item.gameObject);
                            LockStatusScript.isUnlocked = true;

                            PlayerMenuScript.targetContainer = gameObject;
                            PauseMenuScript.isPlayerMenuOpen = true;

                            break;
                        }
                    }
                }
            }
        }
    }

    public void OpenInventory(string inventoryType)
    {
        currentlyOpenedInventory = inventoryType;

        UIReuseScript.ClearInventory();
        UIReuseScript.CloseSelectedItemInfo();

        PlayerMenuScript.btn_ReusedButton1.onClick.RemoveAllListeners();
        PlayerMenuScript.btn_ReusedButton2.onClick.RemoveAllListeners();
        PlayerMenuScript.btn_ReusedButton3.onClick.RemoveAllListeners();
        PlayerMenuScript.btn_ReusedButton4.onClick.RemoveAllListeners();
        PlayerMenuScript.btn_ReusedButton5.onClick.RemoveAllListeners();

        //player or container inventory UI
        if (inventoryType == "inventory"
            || inventoryType == "container"
            || inventoryType == "altar_of_enchanting")
        {
            RebuildInventory("allItems");
            if (inventoryType == "inventory"
                || inventoryType == "container")
            {
                PlayerMenuScript.btn_ReusedButton1.onClick.AddListener(delegate { RebuildInventory("allItems"); });
                PlayerMenuScript.btn_ReusedButton2.onClick.AddListener(delegate { RebuildInventory("weapon"); });
                PlayerMenuScript.btn_ReusedButton3.onClick.AddListener(delegate { RebuildInventory("armor"); });
                PlayerMenuScript.btn_ReusedButton4.onClick.AddListener(delegate { RebuildInventory("consumable"); });
                PlayerMenuScript.btn_ReusedButton5.onClick.AddListener(delegate { RebuildInventory("misc"); });

                if (inventoryType == "container")
                {
                    PlayerMenuScript.btn_ShowInventoryUI.GetComponentInChildren<TMP_Text>().text = "Container inventory";
                    PlayerMenuScript.btn_ShowInventoryUI.onClick.RemoveAllListeners();
                    PlayerMenuScript.btn_ShowInventoryUI.onClick.AddListener(delegate { SwitchInventoryType("container_ContainerInventory"); });

                    PlayerMenuScript.btn_ShowMagickaUI.GetComponentInChildren<TMP_Text>().text = "Player inventory";
                    PlayerMenuScript.btn_ShowMagickaUI.onClick.RemoveAllListeners();
                    PlayerMenuScript.btn_ShowMagickaUI.onClick.AddListener(delegate { SwitchInventoryType("container_PlayerInventory"); });
                }
            }
            else if (inventoryType == "altar_of_enchanting")
            {
                PlayerMenuScript.btn_ReusedButton1.onClick.AddListener(delegate { RebuildInventory("allItems"); });
                PlayerMenuScript.btn_ReusedButton2.onClick.AddListener(delegate { RebuildInventory("weapon"); });
                PlayerMenuScript.btn_ReusedButton3.onClick.AddListener(delegate { RebuildInventory("armor"); });
            }
        }
        //magic UI
        else if (inventoryType == "magic")
        {
            RebuildInventory("allMagicka");
            PlayerMenuScript.btn_ReusedButton1.onClick.AddListener(delegate { RebuildInventory("allMagicka"); });
            PlayerMenuScript.btn_ReusedButton2.onClick.AddListener(delegate { RebuildInventory("target"); });
            PlayerMenuScript.btn_ReusedButton3.onClick.AddListener(delegate { RebuildInventory("touch"); });
            PlayerMenuScript.btn_ReusedButton4.onClick.AddListener(delegate { RebuildInventory("self"); });
            PlayerMenuScript.btn_ReusedButton5.onClick.AddListener(delegate { RebuildInventory("active"); });
        }
        else if (inventoryType == "allEnchantments")
        {
            RebuildInventory("allEnchantments");
        }
        else if (inventoryType == "allSoulGems")
        {
            RebuildInventory("allSoulGems");
        }
    }
    public void CloseInventory()
    {
        UIReuseScript.par_Inventory.SetActive(false);
        UIReuseScript.ClearInventory();
        UIReuseScript.CloseSelectedItemInfo();
        UIReuseScript.txt_InventoryCount.text = "";
    }

    //switch container inventory type
    public void SwitchInventoryType(string type)
    {
        if (type == "container_ContainerInventory")
        {
            PlayerMenuScript.isContainerOpen = true;
            PlayerMenuScript.isPlayerInventoryOpen = false;
        }
        else if (type == "container_PlayerInventory")
        {
            PlayerMenuScript.isContainerOpen = false;
            PlayerMenuScript.isPlayerInventoryOpen = true;
        }
        RebuildInventory("allItems");
    }
    //rebuilds the entire inventory UI for target inventory
    public void RebuildInventory(string targetInventory)
    {
        //clear any previously assigned inventory buttons
        //if there are any
        UIReuseScript.ClearInventory();
        UIReuseScript.CloseSelectedItemInfo();

        PlayerMenuScript.btn_ReusedButton1.interactable = true;
        PlayerMenuScript.btn_ReusedButton2.interactable = true;
        PlayerMenuScript.btn_ReusedButton3.interactable = true;
        PlayerMenuScript.btn_ReusedButton4.interactable = true;
        PlayerMenuScript.btn_ReusedButton5.interactable = true;

        string inventoryTitle = "";
        //reusable button 1
        if (targetInventory == "allItems")
        {
            inventoryTitle = "All items";
            if (containerType == ContainerType.player
                || containerType == ContainerType.container)
            {
                PlayerMenuScript.btn_ReusedButton1.interactable = false;
            }
            else if (containerType == ContainerType.altar_of_enchanting)
            {
                PlayerMenuScript.btn_ReusedButton2.interactable = false;
            }
        }
        else if (targetInventory == "allMagicka")
        {
            inventoryTitle = "All spells";
            PlayerMenuScript.btn_ReusedButton1.interactable = false;
        }
        else if (targetInventory == "allEnchantments")
        {
            inventoryTitle = "All enchantments";
        }
        else if (targetInventory == "allSoulGems")
        {
            inventoryTitle = "All soul gems";
        }

        //reusable button 2
        else if (targetInventory == "weapon")
        {
            inventoryTitle = "All weapons";
            if (containerType == ContainerType.player
                || containerType == ContainerType.container)
            {
                PlayerMenuScript.btn_ReusedButton2.interactable = false;
            }
            else if (containerType == ContainerType.altar_of_enchanting)
            {
                PlayerMenuScript.btn_ReusedButton3.interactable = false;
            }
        }
        else if (targetInventory == "target")
        {
            inventoryTitle = "All target spells";
            PlayerMenuScript.btn_ReusedButton2.interactable = false;
        }

        //reusable button 3
        else if (targetInventory == "armor")
        {
            inventoryTitle = "All armor";
            if (containerType == ContainerType.player
                || containerType == ContainerType.container)
            {
                PlayerMenuScript.btn_ReusedButton3.interactable = false;
            }
            else if (containerType == ContainerType.altar_of_enchanting)
            {
                PlayerMenuScript.btn_ReusedButton4.interactable = false;
            }
        }
        else if (targetInventory == "touch")
        {
            inventoryTitle = "All touch spells";
            PlayerMenuScript.btn_ReusedButton3.interactable = false;
        }

        //reusable button 4
        else if (targetInventory == "consumable")
        {
            inventoryTitle = "All alchemy items and consumables";
            PlayerMenuScript.btn_ReusedButton4.interactable = false;
        }
        else if (targetInventory == "self")
        {
            inventoryTitle = "All self spells";
            PlayerMenuScript.btn_ReusedButton4.interactable = false;
        }

        //reusable button 5
        else if (targetInventory == "misc")
        {
            inventoryTitle = "All misc items";
            PlayerMenuScript.btn_ReusedButton5.interactable = false;
        }
        else if (targetInventory == "active")
        {
            inventoryTitle = "All active spells, abilities, powers etc";
            PlayerMenuScript.btn_ReusedButton5.interactable = false;
        }

        PlayerMenuScript.txt_PlayerMenuTitle.text = inventoryTitle;

        //list all player inventory items
        if (PlayerMenuScript.targetContainer == null)
        {
            int invSpace = PlayerStatsScript.invSpace;
            int maxInvSpace = PlayerStatsScript.maxInvSpace;
            UIReuseScript.txt_InventoryCount.text = invSpace + "/" + maxInvSpace;

            //create a new inventory button for each inventory item
            //depending on the selected inventory sort type
            foreach (GameObject item in playerItems)
            {
                if (item != null)
                {
                    Env_Item itemScript = item.GetComponent<Env_Item>();

                    //player inventory
                    if (PlayerMenuScript.isPlayerInventoryOpen)
                    {
                        if ((targetInventory == "allItems"                           //list all regular items only
                            && itemScript.itemType != Env_Item.ItemType.spell)
                            || (targetInventory == "allSpells"                       //list all spells only
                            && itemScript.itemType != Env_Item.ItemType.weapon
                            && itemScript.itemType != Env_Item.ItemType.armor
                            && itemScript.itemType != Env_Item.ItemType.shield
                            && itemScript.itemType != Env_Item.ItemType.consumable
                            && itemScript.itemType != Env_Item.ItemType.ammo
                            && itemScript.itemType != Env_Item.ItemType.misc)
                            || itemScript.itemType.ToString() == targetInventory)    //list specific item type
                        {
                            GameObject newButton = Instantiate(UIReuseScript.btn_ItemTemplateButton.gameObject,
                                                               UIReuseScript.inventoryContent.transform.position,
                                                               Quaternion.identity,
                                                               UIReuseScript.inventoryContent.transform);

                            UIReuseScript.inventoryButtons.Add(newButton.GetComponent<Button>());

                            string buttonText = itemScript.itemName.Replace("_", " ");
                            if (itemScript.itemCount > 1)
                            {
                                buttonText += " x" + itemScript.itemCount;
                            }
                            newButton.GetComponentInChildren<TMP_Text>().text = buttonText;

                            newButton.GetComponent<Button>().onClick.AddListener(
                                delegate { ShowSelectedItemInfo(item, newButton.GetComponent<Button>()); });
                        }
                    }
                    //enchantable items in altar of enchanting
                    else if (PlayerMenuScript.isAltarOfEnchantingOpen)
                    {
                        //list all enchantable weapons
                        if ((targetInventory == "allItems"                           //list all weapons and armor only
                            && itemScript.itemType != Env_Item.ItemType.consumable
                            && itemScript.itemType != Env_Item.ItemType.alchemyIngredient
                            && itemScript.itemType != Env_Item.ItemType.ammo
                            && itemScript.itemType != Env_Item.ItemType.misc
                            && itemScript.itemType != Env_Item.ItemType.spell)
                            || targetInventory == "armor"                            //list weapons
                            || targetInventory == "weapon")                          //list armor
                        {
                            GameObject newButton = Instantiate(UIReuseScript.btn_ItemTemplateButton.gameObject,
                                                               UIReuseScript.inventoryContent.transform.position,
                                                               Quaternion.identity,
                                                               UIReuseScript.inventoryContent.transform);

                            UIReuseScript.inventoryButtons.Add(newButton.GetComponent<Button>());

                            string buttonText = itemScript.itemName.Replace("_", " ");
                            if (itemScript.itemCount > 1)
                            {
                                buttonText += " x" + itemScript.itemCount;
                            }
                            newButton.GetComponentInChildren<TMP_Text>().text = buttonText;

                            newButton.GetComponent<Button>().onClick.AddListener(
                                delegate { ShowSelectedItemInfo(item, newButton.GetComponent<Button>()); });
                        }

                        //list all enchantments
                        else if (targetInventory == "allEnchantments"
                                 && itemScript.itemType == Env_Item.ItemType.enchantment)
                        {
                            
                        }

                        //list all filled soul gems
                        else if (targetInventory == "allSoulGems"
                                 && item.name.Contains("soul_gem"))
                        {

                        }
                    }
                }
            }
        }
        //list all container items
        else if (PlayerMenuScript.targetContainer != null
                && (PlayerMenuScript.isContainerOpen
                || PlayerMenuScript.isPlayerInventoryOpen))
        {
            int invSpace = PlayerStatsScript.invSpace;
            int maxInvSpace = PlayerStatsScript.maxInvSpace;
            UIReuseScript.txt_InventoryCount.text = invSpace + "/" + maxInvSpace;

            if (PlayerMenuScript.isContainerOpen)
            {
                PlayerMenuScript.btn_ShowInventoryUI.interactable = false;
                PlayerMenuScript.btn_ShowMagickaUI.interactable = true;

                //create a new inventory button for each inventory item
                //depending on the selected inventory sort type
                foreach (GameObject item in PlayerMenuScript.targetContainer.GetComponent<UI_Inventory>().containerItems)
                {
                    if (item != null)
                    {
                        Env_Item itemScript = item.GetComponent<Env_Item>();

                        if ((targetInventory == "allItems"                           //list all regular items only
                            && itemScript.itemType != Env_Item.ItemType.spell)
                            || itemScript.itemType.ToString() == targetInventory)    //list specific item type
                        {
                            GameObject newButton = Instantiate(UIReuseScript.btn_ItemTemplateButton.gameObject,
                                                               UIReuseScript.inventoryContent.transform.position,
                                                               Quaternion.identity,
                                                               UIReuseScript.inventoryContent.transform);

                            UIReuseScript.inventoryButtons.Add(newButton.GetComponent<Button>());

                            string buttonText = itemScript.itemName;
                            if (itemScript.itemCount > 1)
                            {
                                buttonText += " x" + itemScript.itemCount;
                            }
                            newButton.GetComponentInChildren<TMP_Text>().text = buttonText;

                            newButton.GetComponent<Button>().onClick.AddListener(
                                delegate { ShowSelectedItemInfo(item, newButton.GetComponent<Button>()); });
                        }
                    }
                }
            }
            else if (PlayerMenuScript.isPlayerInventoryOpen)
            {
                PlayerMenuScript.btn_ShowInventoryUI.interactable = true;
                PlayerMenuScript.btn_ShowMagickaUI.interactable = false;

                //create a new inventory button for each inventory item
                //depending on the selected inventory sort type
                foreach (GameObject item in playerItems)
                {
                    if (item != null)
                    {
                        Env_Item itemScript = item.GetComponent<Env_Item>();

                        if ((targetInventory == "allItems"                           //list all regular items only
                            && itemScript.itemType != Env_Item.ItemType.spell)
                            || itemScript.itemType.ToString() == targetInventory)    //list specific item type
                        {
                            GameObject newButton = Instantiate(UIReuseScript.btn_ItemTemplateButton.gameObject,
                                                               UIReuseScript.inventoryContent.transform.position,
                                                               Quaternion.identity,
                                                               UIReuseScript.inventoryContent.transform);

                            UIReuseScript.inventoryButtons.Add(newButton.GetComponent<Button>());

                            string buttonText = itemScript.itemName;
                            if (itemScript.itemCount > 1)
                            {
                                buttonText += " x" + itemScript.itemCount;
                            }
                            newButton.GetComponentInChildren<TMP_Text>().text = buttonText;

                            newButton.GetComponent<Button>().onClick.AddListener(
                                delegate { ShowSelectedItemInfo(item, newButton.GetComponent<Button>()); });
                        }
                    }
                }
            }
        }
    }

    //show selected item details and interactable buttons in target inventory
    public void ShowSelectedItemInfo(GameObject targetItem, Button targetButton)
    {
        targetButton.interactable = false;
        foreach (Button btn in UIReuseScript.inventoryButtons)
        {
            if (btn != targetButton
                && btn.interactable == false)
            {
                btn.interactable = true;
            }
        }

        UIReuseScript.CloseSelectedItemInfo();

        Env_Item itemScript = targetItem.GetComponent<Env_Item>();

        UIReuseScript.par_ItemStats.SetActive(true);
        UIReuseScript.txt_ItemName.text = itemScript.itemName.Replace('_', ' ');
        UIReuseScript.txt_ItemDescription.text = itemScript.itemDescription;
        UIReuseScript.txt_ItemType.text = "Type: " + itemScript.itemType.ToString();
        UIReuseScript.txt_ItemValue.text = "Value: " + itemScript.itemValue.ToString();
        UIReuseScript.txt_ItemWeight.text = "Weight: " + itemScript.itemWeight.ToString();
        UIReuseScript.txt_ItemCount.text = "Count: " + itemScript.itemCount.ToString();
        if (itemScript.itemType != Env_Item.ItemType.weapon)
        {
            UIReuseScript.txt_WeaponDamage.text = "";
        }
        else
        {
            UIReuseScript.txt_WeaponDamage.text = "Damage: " + targetItem.GetComponent<Item_Weapon>().damage_Current;
        }

        if (PlayerMenuScript.targetContainer == null)
        {
            //take method is used when player is taking an item from the world
            if (!PlayerMenuScript.isPlayerInventoryOpen
                && !PlayerMenuScript.isAltarOfEnchantingOpen)
            {
                UIReuseScript.btn_Interact.gameObject.SetActive(true);
                UIReuseScript.btn_Interact.interactable = false;
                UIReuseScript.btn_Interact.GetComponentInChildren<TMP_Text>().text = "Take";
                UIReuseScript.btn_Interact.onClick.AddListener(
                    delegate { TakeItem(targetItem,
                                        null); });
            }

            //use and drop methods are used when player is not in container
            else if (PlayerMenuScript.isPlayerInventoryOpen
                     && !PlayerMenuScript.isAltarOfEnchantingOpen)
            {
                UIReuseScript.btn_Interact.gameObject.SetActive(true);
                UIReuseScript.btn_Interact.interactable = false;
                if (targetItem.GetComponent<Item_Weapon>() != null
                    || targetItem.GetComponent<Item_Armor>() != null
                    || targetItem.GetComponent<Item_Shield>() != null
                    || targetItem.GetComponent<Item_Spell>() != null
                    || targetItem.GetComponent<Item_Ammo>() != null)
                {
                    UIReuseScript.btn_Interact.interactable = true;

                    if (!targetItem.GetComponent<Env_Item>().isEquipped)
                    {
                        UIReuseScript.btn_Interact.GetComponentInChildren<TMP_Text>().text = "Equip";
                    }
                    else
                    {
                        UIReuseScript.btn_Interact.GetComponentInChildren<TMP_Text>().text = "Unequip";
                    }

                    UIReuseScript.btn_Interact.onClick.AddListener(
                        delegate { UseItem(targetItem); });
                }
                else if (targetItem.GetComponent<Item_Consumable>() != null)
                {
                    UIReuseScript.btn_Interact.interactable = true;
                    UIReuseScript.btn_Interact.GetComponentInChildren<TMP_Text>().text = "Consume";
                    UIReuseScript.btn_Interact.onClick.AddListener(
                        delegate { UseItem(targetItem); });
                }
                else if (targetItem.GetComponent<Item_Readable>() != null)
                {
                    UIReuseScript.btn_Interact.interactable = true;
                    UIReuseScript.btn_Interact.GetComponentInChildren<TMP_Text>().text = "Read";
                    UIReuseScript.btn_Interact.onClick.AddListener(
                        delegate { UseItem(targetItem); });
                }
                else if (targetItem.GetComponent<Item_AlchemyTool>() != null)
                {
                    UIReuseScript.btn_Interact.interactable = true;
                    UIReuseScript.btn_Interact.GetComponentInChildren<TMP_Text>().text = "Use";
                    UIReuseScript.btn_Interact.onClick.AddListener(
                        delegate { UseItem(targetItem); });
                }
                else
                {
                    UIReuseScript.btn_Interact.gameObject.SetActive(false);
                }

                UIReuseScript.btn_Drop.gameObject.SetActive(true);
                if (!itemScript.isProtected)
                {
                    UIReuseScript.btn_Drop.interactable = true;
                    UIReuseScript.btn_Drop.onClick.AddListener(
                        delegate { DropItem(targetItem); });
                }
                else
                {
                    UIReuseScript.btn_Drop.interactable = false;
                }

                if (itemScript.itemType == Env_Item.ItemType.weapon
                    || itemScript.itemType == Env_Item.ItemType.armor
                    || itemScript.itemType == Env_Item.ItemType.shield)
                {
                    ShowRepairInfo(targetItem);
                }

                UIReuseScript.btn_ShowExtraStats.onClick.AddListener(ShowExtraStats);
            }

            //enchant method is available if player has any soul gems
            //and if this item is enchantable and is not yet enchanted
            else if (!PlayerMenuScript.isPlayerInventoryOpen
                     && PlayerMenuScript.isAltarOfEnchantingOpen)
            {
                bool hasSoulGems = false;
                foreach (GameObject item in PlayerInventoryScript.playerItems)
                {
                    if (item.GetComponent<Env_Item>().itemName.Contains("soul_gem"))
                    {
                        hasSoulGems = true;
                        break;
                    }
                }

                UIReuseScript.btn_Interact.gameObject.SetActive(true);
                UIReuseScript.btn_Interact.GetComponentInChildren<TMP_Text>().text = "Enchant";
                if (hasSoulGems
                    && targetItem.GetComponent<Env_Item>().isEnchantable)
                {
                    UIReuseScript.btn_Interact.interactable = true;
                    UIReuseScript.btn_Interact.onClick.AddListener(
                        delegate { EnchantItem(targetItem, "", null); } );
                    UIReuseScript.btn_Interact.onClick.AddListener(
                        PlayerMenuScript.ShowEnchantments);
                }
                else
                {
                    UIReuseScript.btn_Interact.interactable = false;
                }

                UIReuseScript.btn_ShowExtraStats.onClick.AddListener(ShowExtraStats);
            }
        }

        //take and place methods are used when player is taking from or placing to container
        else
        {
            UIReuseScript.btn_Interact.gameObject.SetActive(true);
            UIReuseScript.btn_Interact.interactable = false;
            if (PlayerMenuScript.isContainerOpen)
            {
                UIReuseScript.btn_Interact.interactable = true;
                UIReuseScript.btn_Interact.GetComponentInChildren<TMP_Text>().text = "Take";
                UIReuseScript.btn_Interact.onClick.AddListener(
                    delegate { TakeItem(targetItem,
                                        par_ContainerItems); });
            }
            else if (PlayerMenuScript.isPlayerInventoryOpen)
            {
                UIReuseScript.btn_Interact.GetComponentInChildren<TMP_Text>().text = "Place";
                if (!itemScript.isProtected)
                {
                    UIReuseScript.btn_Interact.interactable = true;
                    UIReuseScript.btn_Interact.onClick.AddListener(
                        delegate { TakeItem(targetItem,
                                            par_PlayerItems); });
                }
                else
                {
                    UIReuseScript.btn_Interact.interactable = false;
                }
            }

            UIReuseScript.btn_ShowExtraStats.onClick.AddListener(ShowExtraStats);
        }
    } 

    //enables the repair UI automatically along with the
    //item stat UI if the selected item is repairable
    public void ShowRepairInfo(GameObject targetItem)
    {
        UIReuseScript.par_RepairUI.SetActive(true);
        UIReuseScript.txt_Durability.text = "Durability: " + 
            targetItem.GetComponent<Env_Item>().itemCurrentDurability.ToString() + "/" +
            targetItem.GetComponent<Env_Item>().itemMaxDurability.ToString();
        UIReuseScript.txt_RepairHammerCount.text = "Repair hammers: 0";
        UIReuseScript.btn_Repair.interactable = false;

        foreach (GameObject item in PlayerInventoryScript.playerItems)
        {
            if (item.name == "Repair_hammer")
            {
                if (targetItem.GetComponent<Env_Item>().itemCurrentDurability 
                    < targetItem.GetComponent<Env_Item>().itemMaxDurability)
                {
                    UIReuseScript.txt_RepairHammerCount.text = "Repair hammers: " + item.GetComponent<Env_Item>().itemCount.ToString();

                    UIReuseScript.btn_Repair.interactable = true;
                    UIReuseScript.btn_Repair.onClick.AddListener(delegate { RepairSelectedItem(targetItem, item); });
                }
                break;
            }
        }
    }
    //repair selected item with an existing repair hammer
    public void RepairSelectedItem(GameObject targetItem, GameObject repairHammer)
    {
        int armorerLevel = PlayerStatsScript.Skills["Armorer"];
        int maxChance = 100 / armorerLevel;
        int chanceToBreak = Random.Range(0, maxChance);

        if (chanceToBreak >= maxChance / 2)
        {
            if (repairHammer.GetComponent<Env_Item>().itemCount == 1)
            {
                UIReuseScript.txt_RepairHammerCount.text = "Repair hammers: 0";
                UIReuseScript.btn_Repair.interactable = false;

                Destroy(repairHammer);

                PauseMenuScript.UnpauseGame();
                RemoveDuplicates();
                PauseMenuScript.PauseWithoutUI();
            }
            else
            {
                repairHammer.GetComponent<Env_Item>().itemCount--;
                UIReuseScript.txt_RepairHammerCount.text = "Repair hammers: " + repairHammer.GetComponent<Env_Item>().itemCount.ToString();
            }
        }
        targetItem.GetComponent<Env_Item>().itemCurrentDurability += armorerLevel;
        if (targetItem.GetComponent<Env_Item>().itemCurrentDurability 
            > targetItem.GetComponent<Env_Item>().itemMaxDurability)
        {
            targetItem.GetComponent<Env_Item>().itemCurrentDurability = targetItem.GetComponent<Env_Item>().itemMaxDurability;
        }
        UIReuseScript.txt_Durability.text = targetItem.GetComponent<Env_Item>().itemCurrentDurability.ToString();
    }

    //item is what item will be enchanted
    //enchantment is what enchantment will be applied to enchantable item
    //soulGem is what soul gem will be used to enchant enchantable item
    public void EnchantItem(GameObject item,
                            string enchantment,
                            GameObject soulGem)
    {
        //selected item
        if (item != null)
        {

        }
        //selected enchantment
        if (enchantment != "")
        {

        }
        //selected soul gem
        if (soulGem != null)
        {

        }
    }

    //show the extra stats UI if the
    //arrow pointing right button was pressed
    public void ShowExtraStats()
    {
        UIReuseScript.par_ExtraStats.SetActive(true);
    }

    //equip/use selected item in player inventory
    public void UseItem(GameObject targetItem)
    {
        if (targetItem.GetComponent<Item_Weapon>() != null)
        {
            if (equippedWeapon == null
                || (equippedWeapon != null
                && !equippedWeapon.GetComponent<Item_Weapon>().isUsing
                && !equippedWeapon.GetComponent<Item_Weapon>().isBlocking
                && !equippedWeapon.GetComponent<Item_Weapon>().isAiming
                && !equippedWeapon.GetComponent<Item_Weapon>().isReloading))
            {
                if (!targetItem.GetComponent<Env_Item>().isEquipped)
                {
                    if (equippedWeapon != null)
                    {
                        equippedWeapon.GetComponent<Env_Item>().isEquipped = false;
                        equippedWeapon.SetActive(false);
                        foreach (Transform child in equippedWeapon.transform)
                        {
                            child.GetComponent<MeshRenderer>().enabled = true;
                        }
                        equippedWeapon.GetComponent<Rigidbody>().isKinematic = false;
                        Destroy(equippedWeapon.GetComponent<Item_Weapon>().instantiatedWeapon);
                    }

                    GameObject animatedWeapon = Instantiate(targetItem.GetComponent<Item_Weapon>().templateAnimatedWeapon,
                                                            pos_EquippedWeapon.position,
                                                            Quaternion.identity,
                                                            pos_EquippedWeapon);
                    equippedWeapon = targetItem;
                    equippedWeapon.SetActive(true);
                    foreach (Transform child in equippedWeapon.transform)
                    {
                        child.GetComponent<MeshRenderer>().enabled = false;
                    }
                    equippedWeapon.GetComponent<Rigidbody>().isKinematic = true;
                    equippedWeapon.GetComponent<Item_Weapon>().instantiatedWeapon 
                        = animatedWeapon;
                    equippedWeapon.GetComponent<Item_Weapon>().WeaponAnimationScript 
                        = animatedWeapon.GetComponent<Anim_Weapon>();
                    equippedWeapon.GetComponent<Item_Weapon>().weaponAnimator 
                        = equippedWeapon.GetComponent<Item_Weapon>().instantiatedWeapon.GetComponent<Animator>();

                    targetItem.GetComponent<Env_Item>().isEquipped = true;

                    UIReuseScript.img_EquippedWeapon.texture = targetItem.GetComponent<Item_Weapon>().img_ItemLogo;

                    UIReuseScript.btn_Interact.GetComponentInChildren<TMP_Text>().text = "Unequip";
                }
                else
                {
                    equippedWeapon.GetComponent<Env_Item>().isEquipped = false;
                    equippedWeapon.SetActive(false);
                    foreach (Transform child in equippedWeapon.transform)
                    {
                        child.GetComponent<MeshRenderer>().enabled = true;
                    }
                    equippedWeapon.GetComponent<Rigidbody>().isKinematic = false;
                    Destroy(equippedWeapon.GetComponent<Item_Weapon>().instantiatedWeapon);

                    UIReuseScript.img_EquippedWeapon.texture = null;

                    UIReuseScript.btn_Interact.GetComponentInChildren<TMP_Text>().text = "Equip";
                }
            }
            else
            {
                if (PlayerMenuScript.isPlayerInventoryOpen)
                {
                    UIReuseScript.btn_Interact.interactable = false;
                }
                else
                {
                    Debug.LogWarning("Warning: Cannot unequip " + PlayerInventoryScript.equippedWeapon.GetComponent<Env_Item>().itemName.Replace("_", " ") + " with EquipUnequipWeapon key because it is in use!");
                }
            }
        }
        else
        {
            UIReuseScript.btn_Interact.interactable = false;
        }
    }
    //take an item from a container or from the world
    public void TakeItem(GameObject targetItem, 
                         GameObject originalContainer)
    {
        Env_Item itemScript = targetItem.GetComponent<Env_Item>();

        int maxInventorySpace = PlayerStatsScript.maxInvSpace;
        int currentInventorySpace = PlayerStatsScript.invSpace;
        int singleItemTakenSpace = currentInventorySpace + itemScript.itemWeight;
        int totalTakenSpace = currentInventorySpace + (itemScript.itemWeight * itemScript.itemCount);

        //player takes items from world
        if (originalContainer == null)
        {
            if (targetItem.GetComponent<Env_Item>().itemCount == 1
                && totalTakenSpace <= maxInventorySpace)
            {
                SuccessfulItemMove(1,
                                   PlayerMenuScript.par_DroppedItems,
                                   par_PlayerItems,
                                   targetItem);
            }
            else if (targetItem.GetComponent<Env_Item>().itemCount > 1
                     && singleItemTakenSpace <= maxInventorySpace)
            {
                PauseMenuScript.PauseWithoutUI();
                ConfirmationScript.MoveItem(gameObject,
                                            "takeFromWorld",
                                            PlayerMenuScript.par_DroppedItems,
                                            par_PlayerItems,
                                            targetItem);
            }
            else
            {
                AnnouncementScript.CreateAnnouncement("Not enough space to take " + targetItem.name + "(s)!");
                Debug.LogWarning("Requirements not met: Not enough space to take " + targetItem.name + "(s)!");
            }
        }
        //player takes/places items from/to container
        else
        {
            if (originalContainer == par_ContainerItems)
            {
                if (targetItem.GetComponent<Env_Item>().itemCount == 1)
                {
                    SuccessfulItemMove(1,
                                       par_ContainerItems,
                                       par_PlayerItems,
                                       targetItem);
                }
                else if (targetItem.GetComponent<Env_Item>().itemCount > 1)
                {
                    PauseMenuScript.PauseWithoutUI();
                    ConfirmationScript.MoveItem(gameObject,
                                                "takeFromContainer",
                                                par_ContainerItems,
                                                par_PlayerItems,
                                                targetItem);
                }
            }
            else if (originalContainer == par_PlayerItems)
            {
                if (targetItem.GetComponent<Env_Item>().itemCount == 1
                    && totalTakenSpace <= maxInventorySpace)
                {
                    SuccessfulItemMove(1,
                                       par_PlayerItems,
                                       PlayerMenuScript.targetContainer.GetComponent<UI_Inventory>().par_ContainerItems,
                                       targetItem);
                }
                else if (targetItem.GetComponent<Env_Item>().itemCount > 1
                         && singleItemTakenSpace <= maxInventorySpace)
                {
                    PauseMenuScript.PauseWithoutUI();
                    ConfirmationScript.MoveItem(gameObject,
                                                "placeToContainer",
                                                par_PlayerItems,
                                                PlayerMenuScript.targetContainer.GetComponent<UI_Inventory>().par_ContainerItems,
                                                targetItem);
                }
            }
        }
    }
    //drop an item from player inventory
    public void DropItem(GameObject targetItem)
    {
        Env_Item itemScript = targetItem.GetComponent<Env_Item>();

        if (!itemScript.isProtected)
        {
            UIReuseScript.btn_Drop.interactable = true;
            if (targetItem.GetComponent<Env_Item>().itemCount == 1)
            {
                SuccessfulItemMove(1,
                                   par_PlayerItems,
                                   PlayerMenuScript.par_DroppedItems,
                                   targetItem);
            }
            else if (targetItem.GetComponent<Env_Item>().itemCount > 1)
            {
                PauseMenuScript.PauseWithoutUI();
                ConfirmationScript.MoveItem(gameObject,
                                            "placeToWorld",
                                            par_PlayerItems,
                                            PlayerMenuScript.par_DroppedItems,
                                            targetItem);
            }
        }
        else
        {
            UIReuseScript.btn_Drop.interactable = false;
        }
    }

    //handles item movement when taking a single item
    //or after player has confirmed selected count of stacked item movement
    public void SuccessfulItemMove(int selectedCount,
                                   GameObject originalLocation,
                                   GameObject targetLocation,
                                   GameObject item)
    {
        Env_Item itemScript = item.GetComponent<Env_Item>();
        GameObject targetItem;

        if (originalLocation == PlayerMenuScript.par_DroppedItems
            && targetLocation == par_PlayerItems)
        {
            AnnouncementScript.CreateAnnouncement("Picked up " + selectedCount + " " + item.name + "(s).");
            Debug.Log("Info: Picked up " + selectedCount + " " + item.name + "(s).");
        }

        //if the player isnt moving all of the items
        if (selectedCount < itemScript.itemCount)
        {
            targetItem = Instantiate(item,
                                     item.transform.position,
                                     Quaternion.identity,
                                     item.transform);

            //update original and duplicate item counts
            int itemRemainderCount = itemScript.itemCount - selectedCount;
            itemScript.itemCount = itemRemainderCount;
            targetItem.GetComponent<Env_Item>().itemCount = selectedCount;
        }
        else
        {
            targetItem = item;
        }

        targetItem.name = item.name;

        //taking items from world
        if (originalLocation == PlayerMenuScript.par_DroppedItems
            && targetLocation == par_PlayerItems)
        {
            targetItem.SetActive(false);

            targetItem.transform.parent = par_PlayerItems.transform;
            targetItem.transform.position = par_PlayerItems.transform.position;

            Env_Item targetItemScript = targetItem.GetComponent<Env_Item>();
            int totalCount = targetItemScript.itemWeight * selectedCount;
            PlayerStatsScript.invSpace += totalCount;
        }
        //dropping items to world
        else if (originalLocation == par_PlayerItems
                 && targetLocation == PlayerMenuScript.par_DroppedItems)
        {
            Env_Item targetItemScript = targetItem.GetComponent<Env_Item>();
            int totalCount = targetItemScript.itemWeight * selectedCount;
            PlayerStatsScript.invSpace -= totalCount;

            //get a random direction (360�) in radians
            float angle = Random.Range(0.0f, Mathf.PI * 2);
            //create a vector with length 1.0
            Vector3 dropPos = new(Mathf.Sin(angle), 0, Mathf.Cos(angle));
            //set item drop position
            targetItem.transform.position = thePlayer.transform.position + dropPos;

            targetItem.transform.parent = PlayerMenuScript.par_DroppedItems.transform;

            targetItem.SetActive(true);
        }
        //taking items from container
        else if (originalLocation == par_ContainerItems
                 && targetLocation == par_PlayerItems)
        {
            targetItem.transform.parent = par_PlayerItems.transform;
            targetItem.transform.position = par_PlayerItems.transform.position;

            Env_Item targetItemScript = targetItem.GetComponent<Env_Item>();
            int totalCount = targetItemScript.itemWeight * selectedCount;
            PlayerStatsScript.invSpace -= totalCount;
        }
        //placing items to container
        else if (originalLocation == par_PlayerItems
                 && targetLocation == PlayerMenuScript.targetContainer.GetComponent<UI_Inventory>().par_ContainerItems)
        {
            Transform targetLoc = PlayerMenuScript.targetContainer.GetComponent<UI_Inventory>().par_ContainerItems.transform;
            targetItem.transform.parent = targetLoc;
            targetItem.transform.position = targetLoc.position;

            Env_Item targetItemScript = targetItem.GetComponent<Env_Item>();
            int totalCount = targetItemScript.itemWeight * selectedCount;
            PlayerStatsScript.invSpace += totalCount;
        }

        if (PauseMenuScript.isPaused)
        {
            PauseMenuScript.UnpauseGame();
            RemoveDuplicates();
            PauseMenuScript.PauseWithoutUI();
        }
        else
        {
            RemoveDuplicates();
        }
    }

    //remove all duplicate items from target inventory
    private void RemoveDuplicates()
    {
        List<GameObject> thePlayerInventory = new();
        List<GameObject> theContainerInventory = new();
        GameObject thePlayerItemsParent = null;
        GameObject theContainerItemsParent = null;
        if (PlayerMenuScript.targetContainer == null)
        {
            thePlayerInventory = playerItems;
            thePlayerItemsParent = par_PlayerItems;
        }
        else if (PlayerMenuScript.targetContainer != null)
        {
            thePlayerInventory = PlayerMenuScript.PlayerInventoryScript.playerItems;
            thePlayerItemsParent = PlayerMenuScript.PlayerInventoryScript.par_PlayerItems;
            theContainerInventory = PlayerMenuScript.targetContainer.GetComponent<UI_Inventory>().containerItems;
            theContainerItemsParent = PlayerMenuScript.targetContainer.GetComponent<UI_Inventory>().par_ContainerItems;
        }

        thePlayerInventory.Clear();

        List<GameObject> tempItems = new();
        List<GameObject> destroyedItems = new();
        foreach (Transform item in thePlayerItemsParent.transform)
        {
            tempItems.Add(item.gameObject);
        }
        //get all stackable player items
        for (int a = 0; a < tempItems.Count; a++)
        {
            GameObject item = tempItems[a];
            Env_Item itemScript = item.GetComponent<Env_Item>();
            if (itemScript.isStackable)
            {
                //get all player items and remove found duplicates
                List<GameObject> duplicates = new();
                for (int b = 0; b < tempItems.Count; b++)
                {
                    GameObject tempItem = tempItems[b];
                    if (tempItem != item
                        && tempItem.name == item.name)
                    {
                        duplicates.Add(tempItem);
                        tempItems.Remove(tempItem);
                    }
                }

                //get all duplicates and add their counts to item
                for (int c = 0; c < duplicates.Count; c++)
                {
                    GameObject removedItem = duplicates[c];
                    Env_Item removedItemScript = removedItem.GetComponent<Env_Item>();
                    itemScript.itemCount += removedItemScript.itemCount;

                    duplicates.Remove(removedItem);
                    destroyedItems.Add(removedItem);
                }
            }
        }

        foreach (GameObject destroyedItem in destroyedItems)
        {
            Destroy(destroyedItem);
        }

        thePlayerInventory.Clear();
        foreach (Transform item in thePlayerItemsParent.transform)
        {
            thePlayerInventory.Add(item.gameObject);
        }

        if (theContainerItemsParent != null)
        {
            theContainerInventory.Clear();

            List<GameObject> tempContainerItems = new();
            List<GameObject> destroyedContainerItems = new();
            foreach (Transform item in theContainerItemsParent.transform)
            {
                tempContainerItems.Add(item.gameObject);
            }
            //get all stackable player items
            for (int a = 0; a < tempContainerItems.Count; a++)
            {
                GameObject item = tempContainerItems[a];
                Env_Item itemScript = item.GetComponent<Env_Item>();
                if (itemScript.isStackable)
                {
                    //get all player items and remove found duplicates
                    List<GameObject> duplicates = new();
                    for (int b = 0; b < tempContainerItems.Count; b++)
                    {
                        GameObject tempItem = tempContainerItems[b];
                        if (tempItem != item
                            && tempItem.name == item.name)
                        {
                            duplicates.Add(tempItem);
                            tempContainerItems.Remove(tempItem);
                        }
                    }

                    //get all duplicates and add their counts to item
                    for (int c = 0; c < duplicates.Count; c++)
                    {
                        GameObject removedItem = duplicates[c];
                        Env_Item removedItemScript = removedItem.GetComponent<Env_Item>();
                        itemScript.itemCount += removedItemScript.itemCount;

                        duplicates.Remove(removedItem);
                        destroyedContainerItems.Add(removedItem);
                    }
                }
            }

            foreach (GameObject destroyedItem in destroyedContainerItems)
            {
                Destroy(destroyedItem);
            }

            theContainerInventory.Clear();
            foreach (Transform item in theContainerItemsParent.transform)
            {
                theContainerInventory.Add(item.gameObject);
            }
        }

        //rebuilds the currently opened inventory
        if (UIReuseScript.par_Inventory.activeInHierarchy)
        {
            OpenInventory(currentlyOpenedInventory);
        }
    }
}