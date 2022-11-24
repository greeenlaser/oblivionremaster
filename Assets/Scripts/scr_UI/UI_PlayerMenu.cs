using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_PlayerMenu : MonoBehaviour
{
    [Header("Player menu UI")]
    [SerializeField] private GameObject par_PlayerMenuUI;
    public TMP_Text txt_PlayerMenuTitle;
    [SerializeField] private GameObject par_StatsUI;
    [SerializeField] private GameObject par_QuestsAndMapsUI;
    public Button btn_ShowStatsUI;
    public Button btn_ShowInventoryUI;
    public Button btn_ShowMagickaUI;
    public Button btn_ShowQuestsAndMapsUI;
    public Button btn_ReusedButton1;
    public Button btn_ReusedButton2;
    public Button btn_ReusedButton3;
    public Button btn_ReusedButton4;
    public Button btn_ReusedButton5;
    public GameObject par_DroppedItems;
    public GameObject par_TemplateItems;
    public List<GameObject> templateItems = new();

    [Header("Scripts")]
    [SerializeField] private GameObject thePlayer;

    //public but hidden variables
    [HideInInspector] public bool isPlayerInventoryOpen;
    [HideInInspector] public bool isContainerOpen;
    [HideInInspector] public GameObject targetContainer;
    [HideInInspector] public UI_Inventory PlayerInventoryScript;

    //private variables
    private Player_Stats PlayerStatsScript;
    private Manager_KeyBindings KeyBindingsScript;
    private UI_PauseMenu PauseMenuScript;
    private Manager_UIReuse UIReuseScript;

    private void Awake()
    {
        PlayerStatsScript = thePlayer.GetComponent<Player_Stats>();
        KeyBindingsScript = GetComponent<Manager_KeyBindings>();
        PauseMenuScript = GetComponent<UI_PauseMenu>();
        UIReuseScript = GetComponent<Manager_UIReuse>();
        PlayerInventoryScript = thePlayer.GetComponent<UI_Inventory>();

        btn_ShowStatsUI.onClick.AddListener(ShowStatsUI);
        btn_ShowQuestsAndMapsUI.onClick.AddListener(ShowQuestsAndMapsUI);
    }

    private void Update()
    {
        if (KeyBindingsScript.GetKeyDown("TogglePlayerMenu")
            && !PauseMenuScript.isPaused
            && !PauseMenuScript.isConsoleOpen
            && !PauseMenuScript.isConfirmationUIOpen
            && !PauseMenuScript.isKeyAssignUIOpen
            && !PauseMenuScript.isLockpickUIOpen
            && PlayerStatsScript.currentHealth > 0)
        {
            PauseMenuScript.isPlayerMenuOpen = !PauseMenuScript.isPlayerMenuOpen;
        }

        if (PauseMenuScript.isPlayerMenuOpen
            && !par_PlayerMenuUI.activeInHierarchy)
        {
            OpenPlayerMenu();
        }
        else if (!PauseMenuScript.isPlayerMenuOpen
                 && par_PlayerMenuUI.activeInHierarchy)
        {
            ClosePlayerMenu();
        }
    }

    public void OpenPlayerMenu()
    {
        par_PlayerMenuUI.SetActive(true);

        if (targetContainer == null)
        {
            ShowInventoryUI();
        }
        else if (targetContainer != null)
        {
            ShowContainerUI();
        }

        PauseMenuScript.PauseWithoutUI();
    }
    public void ClosePlayerMenu()
    {
        CloseAllUI();
        par_PlayerMenuUI.SetActive(false);

        targetContainer = null;
        isPlayerInventoryOpen = false;
        isContainerOpen = false;

        PauseMenuScript.UnpauseGame();
    }

    public void ShowStatsUI()
    {
        CloseAllUI();

        par_StatsUI.SetActive(true);
        btn_ShowStatsUI.interactable = false;

        txt_PlayerMenuTitle.text = "Stats etc...";

        isPlayerInventoryOpen = false;

        btn_ReusedButton1.GetComponentInChildren<TMP_Text>().text = "Character";
        btn_ReusedButton2.GetComponentInChildren<TMP_Text>().text = "Attributes";
        btn_ReusedButton3.GetComponentInChildren<TMP_Text>().text = "Skills";
        btn_ReusedButton4.GetComponentInChildren<TMP_Text>().text = "Factions";
        btn_ReusedButton5.GetComponentInChildren<TMP_Text>().text = "Accomplishments";
    }
    public void ShowInventoryUI()
    {
        CloseAllUI();

        UIReuseScript.par_Inventory.SetActive(true);
        btn_ShowInventoryUI.interactable = false;

        isContainerOpen = false;
        isPlayerInventoryOpen = true;

        PlayerInventoryScript.OpenInventory("inventory");

        btn_ReusedButton1.GetComponentInChildren<TMP_Text>().text = "All items";
        btn_ReusedButton2.GetComponentInChildren<TMP_Text>().text = "Weapons";
        btn_ReusedButton3.GetComponentInChildren<TMP_Text>().text = "Armor";
        btn_ReusedButton4.GetComponentInChildren<TMP_Text>().text = "Alchemy";
        btn_ReusedButton5.GetComponentInChildren<TMP_Text>().text = "Misc";
    }
    public void ShowContainerUI()
    {
        CloseAllUI();

        UIReuseScript.par_Inventory.SetActive(true);

        btn_ShowStatsUI.gameObject.SetActive(false);
        btn_ShowQuestsAndMapsUI.gameObject.SetActive(false);

        isContainerOpen = true;
        isPlayerInventoryOpen = false;

        PlayerInventoryScript.OpenInventory("container");

        btn_ReusedButton1.GetComponentInChildren<TMP_Text>().text = "All items";
        btn_ReusedButton2.GetComponentInChildren<TMP_Text>().text = "Weapons";
        btn_ReusedButton3.GetComponentInChildren<TMP_Text>().text = "Armor";
        btn_ReusedButton4.GetComponentInChildren<TMP_Text>().text = "Alchemy";
        btn_ReusedButton5.GetComponentInChildren<TMP_Text>().text = "Misc";
    }
    public void ShowMagickaUI()
    {
        CloseAllUI();

        UIReuseScript.par_Inventory.SetActive(true);
        btn_ShowMagickaUI.interactable = false;

        PlayerInventoryScript.OpenInventory("magic");

        isPlayerInventoryOpen = true;

        btn_ReusedButton1.GetComponentInChildren<TMP_Text>().text = "All magic";
        btn_ReusedButton2.GetComponentInChildren<TMP_Text>().text = "Target";
        btn_ReusedButton3.GetComponentInChildren<TMP_Text>().text = "Touch";
        btn_ReusedButton4.GetComponentInChildren<TMP_Text>().text = "Self";
        btn_ReusedButton5.GetComponentInChildren<TMP_Text>().text = "Active";
    }
    public void ShowQuestsAndMapsUI()
    {
        CloseAllUI();

        par_QuestsAndMapsUI.SetActive(true);
        btn_ShowQuestsAndMapsUI.interactable = false;

        txt_PlayerMenuTitle.text = "Quests and maps etc...";

        isPlayerInventoryOpen = false;

        btn_ReusedButton1.GetComponentInChildren<TMP_Text>().text = "Local map";
        btn_ReusedButton2.GetComponentInChildren<TMP_Text>().text = "World map";
        btn_ReusedButton3.GetComponentInChildren<TMP_Text>().text = "Active quest";
        btn_ReusedButton4.GetComponentInChildren<TMP_Text>().text = "Current quests";
        btn_ReusedButton5.GetComponentInChildren<TMP_Text>().text = "Completed quests";
    }

    private void CloseAllUI()
    {
        par_StatsUI.SetActive(false);
        btn_ShowStatsUI.gameObject.SetActive(true);
        btn_ShowStatsUI.interactable = true;

        PlayerInventoryScript.CloseInventory();
        btn_ShowInventoryUI.gameObject.SetActive(true);
        btn_ShowInventoryUI.onClick.RemoveAllListeners();
        btn_ShowInventoryUI.onClick.AddListener(ShowInventoryUI);
        btn_ShowInventoryUI.interactable = true;
        btn_ShowInventoryUI.GetComponentInChildren<TMP_Text>().text = "Inventory";

        btn_ShowMagickaUI.gameObject.SetActive(true);
        btn_ShowMagickaUI.onClick.RemoveAllListeners();
        btn_ShowMagickaUI.onClick.AddListener(ShowMagickaUI);
        btn_ShowMagickaUI.interactable = true;
        btn_ShowMagickaUI.GetComponentInChildren<TMP_Text>().text = "Magicka";

        par_QuestsAndMapsUI.SetActive(false);
        btn_ShowQuestsAndMapsUI.gameObject.SetActive(true);
        btn_ShowQuestsAndMapsUI.interactable = true;

        btn_ReusedButton1.onClick.RemoveAllListeners();
        btn_ReusedButton1.interactable = true;
        btn_ReusedButton2.onClick.RemoveAllListeners();
        btn_ReusedButton2.interactable = true;
        btn_ReusedButton3.onClick.RemoveAllListeners();
        btn_ReusedButton3.interactable = true;
        btn_ReusedButton4.onClick.RemoveAllListeners();
        btn_ReusedButton4.interactable = true;
        btn_ReusedButton5.onClick.RemoveAllListeners();
        btn_ReusedButton5.interactable = true;
    }
}