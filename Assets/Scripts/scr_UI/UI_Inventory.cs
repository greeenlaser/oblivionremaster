using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_Inventory : MonoBehaviour
{
    [Header("General assignables")]
    public GameObject par_PlayerItems;
    [SerializeField] private GameObject thePlayer;
    [SerializeField] private GameObject par_Managers;

    [Header("Container assignables")]
    public bool isLocked;
    public LockDifficulty lockDifficulty = LockDifficulty.Apprentice;
    public enum LockDifficulty
    {
        Novice,
        Apprentice,
        Journeyman,
        Expert,
        Master
    }
    public string containerName;
    public ContainerType containerType = ContainerType.player;
    public enum ContainerType
    {
        respawnable,
        store,
        player
    }

    //public but hidden variables
    [HideInInspector] public string currentlyOpenedInventory;
    [HideInInspector] public GameObject heldObject;
    [HideInInspector] public GameObject equippedWeapon;
    [HideInInspector] public List<GameObject> playerItems = new();
    [HideInInspector] public List<GameObject> containerItems = new();

    //scripts
    private UI_PlayerMenu PlayerMenuScript;
    private UI_PauseMenu PauseMenuScript;
    private UI_Confirmation ConfirmationScript;
    private UI_Lockpicking LockpickingScript;
    private Manager_UIReuse UIReuseScript;
    private UI_Inventory PlayerInventoryScript;
    private Player_Stats PlayerStatsScript;

    private void Awake()
    {
        if (gameObject.GetComponent<Player_Movement>() == null)
        {
            PlayerInventoryScript = thePlayer.GetComponent<UI_Inventory>();
        }
        if (containerType == ContainerType.respawnable)
        {
            LockpickingScript = par_Managers.GetComponent<UI_Lockpicking>();
        }
        PlayerStatsScript = GetComponent<Player_Stats>();
        PlayerMenuScript = par_Managers.GetComponent<UI_PlayerMenu>();
        ConfirmationScript = par_Managers.GetComponent<UI_Confirmation>();
        PauseMenuScript = par_Managers.GetComponent<UI_PauseMenu>();
        UIReuseScript = par_Managers.GetComponent<Manager_UIReuse>();
    }

    //used for only respawnable containers,
    //checks if the target container is locked or not
    public void CheckIfLocked()
    {
        foreach (GameObject item in PlayerInventoryScript.playerItems)
        {
            if (item.name == "Lockpick")
            {
                PauseMenuScript.isPlayerMenuOpen = true;
                if (isLocked)
                {
                    LockpickingScript.TargetContainerScript = GetComponent<UI_Inventory>();
                    LockpickingScript.OpenLockpickUI(containerName,
                                                     lockDifficulty.ToString());
                }
                else
                {
                    OpenInventory("container");
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

        //player inventory UI
        if (inventoryType == "inventory"
            || inventoryType == "container")
        {
            RebuildInventory("allItems");
            PlayerMenuScript.btn_ReusedButton1.onClick.AddListener(delegate { RebuildInventory("allItems"); });
            PlayerMenuScript.btn_ReusedButton2.onClick.AddListener(delegate { RebuildInventory("weapon"); });
            PlayerMenuScript.btn_ReusedButton3.onClick.AddListener(delegate { RebuildInventory("armor"); });
            PlayerMenuScript.btn_ReusedButton4.onClick.AddListener(delegate { RebuildInventory("consumable"); });
            PlayerMenuScript.btn_ReusedButton5.onClick.AddListener(delegate { RebuildInventory("misc"); });
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
    }
    public void CloseInventory()
    {
        UIReuseScript.par_Inventory.SetActive(false);
        UIReuseScript.ClearInventory();
        UIReuseScript.CloseSelectedItemInfo();
        UIReuseScript.txt_InventoryCount.text = "";
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
            PlayerMenuScript.btn_ReusedButton1.interactable = false;
        }
        else if (targetInventory == "allMagicka")
        {
            inventoryTitle = "All spells";
            PlayerMenuScript.btn_ReusedButton1.interactable = false;
        }

        //reusable button 2
        else if (targetInventory == "weapon")
        {
            inventoryTitle = "All weapons";
            PlayerMenuScript.btn_ReusedButton2.interactable = false;
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
            PlayerMenuScript.btn_ReusedButton3.interactable = false;
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

        if (PlayerMenuScript.targetContainer == null
            || (PlayerMenuScript.targetContainer != null
            && PlayerMenuScript.isPlayerInventoryOpen))
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

                        string buttonText = itemScript.str_ItemName;
                        if (itemScript.itemCount > 1)
                        {
                            buttonText += " x" + itemScript.itemCount;
                        }
                        newButton.GetComponentInChildren<TMP_Text>().text = buttonText;

                        newButton.GetComponent<Button>().onClick.AddListener(
                            delegate { ShowSelectedItemInfo(item); });
                    }
                }
            }
        }
        else if (PlayerMenuScript.targetContainer != null)
        {
            int invSpace = PlayerStatsScript.invSpace;
            int maxInvSpace = PlayerStatsScript.maxInvSpace;
            UIReuseScript.txt_InventoryCount.text = invSpace + "/" + maxInvSpace;

            if (PlayerMenuScript.isContainerOpen)
            {
                //create a new inventory button for each inventory item
                //depending on the selected inventory sort type
                foreach (GameObject item in containerItems)
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

                            string buttonText = itemScript.str_ItemName;
                            if (itemScript.itemCount > 1)
                            {
                                buttonText += " x" + itemScript.itemCount;
                            }
                            newButton.GetComponentInChildren<TMP_Text>().text = buttonText;

                            newButton.GetComponent<Button>().onClick.AddListener(
                                delegate { ShowSelectedItemInfo(item); });
                        }
                    }
                }
            }
            else if (PlayerMenuScript.isPlayerInventoryOpen)
            {
                //create a new inventory button for each inventory item
                //depending on the selected inventory sort type
                foreach (GameObject item in playerItems)
                {
                    if (item != null)
                    {
                        Env_Item itemScript = item.GetComponent<Env_Item>();

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

                            string buttonText = itemScript.str_ItemName;
                            if (itemScript.itemCount > 1)
                            {
                                buttonText += " x" + itemScript.itemCount;
                            }
                            newButton.GetComponentInChildren<TMP_Text>().text = buttonText;

                            newButton.GetComponent<Button>().onClick.AddListener(
                                delegate { ShowSelectedItemInfo(item); });
                        }
                    }
                }
            }
        }
    }

    //show selected item details and interactable buttons in target inventory
    public void ShowSelectedItemInfo(GameObject targetItem)
    {
        UIReuseScript.CloseSelectedItemInfo();

        Env_Item itemScript = targetItem.GetComponent<Env_Item>();

        UIReuseScript.par_ItemStats.SetActive(true);
        UIReuseScript.txt_ItemName.text = itemScript.str_ItemName.Replace('_', ' ');
        UIReuseScript.txt_ItemDescription.text = itemScript.str_ItemDescription;
        UIReuseScript.txt_ItemType.text = "Type: " + itemScript.itemType.ToString();
        UIReuseScript.txt_ItemValue.text = "Value: " + itemScript.itemValue.ToString();
        UIReuseScript.txt_ItemWeight.text = "Weight: " + itemScript.itemWeight.ToString();
        UIReuseScript.txt_ItemCount.text = "Count: " + itemScript.itemCount.ToString();

        //use and drop methods are used when player is not in container
        if (PlayerMenuScript.targetContainer == null)
        {
            UIReuseScript.btn_Use_Take_Place.gameObject.SetActive(true);
            UIReuseScript.btn_Use_Take_Place.interactable = false;
            if (targetItem.GetComponent<Item_Weapon>() != null
                || targetItem.GetComponent<Item_Armor>() != null
                || targetItem.GetComponent<Item_Shield>() != null
                || targetItem.GetComponent<Item_Spell>() != null
                || targetItem.GetComponent<Item_Ammo>() != null)
            {
                UIReuseScript.btn_Use_Take_Place.interactable = true;
                UIReuseScript.btn_Use_Take_Place.GetComponentInChildren<TMP_Text>().text = "Equip";
                UIReuseScript.btn_Use_Take_Place.onClick.AddListener(
                    delegate { UseItem(targetItem); });
            }
            else if (targetItem.GetComponent<Item_Consumable>() != null)
            {
                UIReuseScript.btn_Use_Take_Place.interactable = true;
                UIReuseScript.btn_Use_Take_Place.GetComponentInChildren<TMP_Text>().text = "Consume";
                UIReuseScript.btn_Use_Take_Place.onClick.AddListener(
                    delegate { UseItem(targetItem); });
            }
            else if (targetItem.GetComponent<Item_Readable>() != null)
            {
                UIReuseScript.btn_Use_Take_Place.interactable = true;
                UIReuseScript.btn_Use_Take_Place.GetComponentInChildren<TMP_Text>().text = "Read";
                UIReuseScript.btn_Use_Take_Place.onClick.AddListener(
                    delegate { UseItem(targetItem); });
            }
            else if (targetItem.GetComponent<Item_AlchemyTool>() != null)
            {
                UIReuseScript.btn_Use_Take_Place.interactable = true;
                UIReuseScript.btn_Use_Take_Place.GetComponentInChildren<TMP_Text>().text = "Use";
                UIReuseScript.btn_Use_Take_Place.onClick.AddListener(
                    delegate { UseItem(targetItem); });
            }
            else
            {
                UIReuseScript.btn_Use_Take_Place.GetComponentInChildren<TMP_Text>().text = "";
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
        }
        //take and place methods are used when player is taking from and placing to container
        else if (PlayerMenuScript.targetContainer != null)
        {

        }
        //take method is used when player is in container or taking an item from the world
        else
        {
            UIReuseScript.btn_Use_Take_Place.gameObject.SetActive(true);
            UIReuseScript.btn_Use_Take_Place.interactable = false;
            UIReuseScript.btn_Use_Take_Place.GetComponentInChildren<TMP_Text>().text = "Take";
            UIReuseScript.btn_Use_Take_Place.onClick.AddListener(
                delegate { TakeItem(targetItem,
                                    null); });
        }
    }

    //equip/use selected item in player inventory
    public void UseItem(GameObject targetItem)
    {
        Debug.Log("using/consuming " + targetItem.GetComponent<Env_Item>().str_ItemName.Replace("_", " ") + "...");
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
        }
        //player takes items from container
        else
        {
            //TODO: add container-specific actions
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

            //get a random direction (360°) in radians
            float angle = Random.Range(0.0f, Mathf.PI * 2);
            //create a vector with length 1.0
            Vector3 dropPos = new(Mathf.Sin(angle), 0, Mathf.Cos(angle));
            //set item drop position
            targetItem.transform.position = thePlayer.transform.position + dropPos;

            targetItem.transform.parent = PlayerMenuScript.par_DroppedItems.transform;

            targetItem.SetActive(true);
        }

        PauseMenuScript.PauseWithoutUI();
        RemoveDuplicates();
        PauseMenuScript.UnpauseGame();
    }

    //remove all duplicate items from target inventory
    private void RemoveDuplicates()
    {
        playerItems.Clear();

        List<GameObject> tempItems = new();
        List<GameObject> destroyedItems = new();
        foreach (Transform item in par_PlayerItems.transform)
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

        playerItems.Clear();
        foreach (Transform item in par_PlayerItems.transform)
        {
            playerItems.Add(item.gameObject);
        }

        //rebuilds the currently opened inventory
        if (UIReuseScript.par_Inventory.activeInHierarchy)
        {
            OpenInventory(currentlyOpenedInventory);
        }
    }
}