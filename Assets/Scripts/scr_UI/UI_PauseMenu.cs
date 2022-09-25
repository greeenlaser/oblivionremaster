using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UI_PauseMenu : MonoBehaviour
{
    [Header("Pause menu UI")]
    [SerializeField] private GameObject par_PauseMenu;
    [SerializeField] private GameObject par_PMContent;
    [SerializeField] private GameObject par_LoadContent;
    [SerializeField] private GameObject par_SettingsContent;
    [SerializeField] private GameObject par_KeyBindingsContent;
    [SerializeField] private Button btn_ReturnToPM;
    [SerializeField] private Button btn_ReturnToGame;
    [SerializeField] private Button btn_Save;
    [SerializeField] private Button btn_Load;
    [SerializeField] private Button btn_Settings;
    [SerializeField] private Button btn_KeyBindings;
    [SerializeField] private Button btn_ReturnToMM;
    [SerializeField] private Button btn_Quit;

    [Header("Scripts")]
    [SerializeField] private GameObject thePlayer;

    //public but hidden variables
    [HideInInspector] public bool canUnpause;
    [HideInInspector] public bool canTogglePMUI;
    [HideInInspector] public bool isPaused;

    //private variables
    private Manager_GameSaving SavingScript;
    private UI_Confirmation ConfirmationScript;
    private Manager_KeyBindings KeyBindingsScript;
    private Manager_Settings SettingsScript;
    private Manager_UIReuse UIReuseScript;
    private Player_Movement PlayerMovementScript;
    private Player_Camera PlayerCameraScript;

    private void Awake()
    {
        SavingScript = GetComponent<Manager_GameSaving>();
        ConfirmationScript = GetComponent<UI_Confirmation>();
        KeyBindingsScript = GetComponent<Manager_KeyBindings>();
        SettingsScript = GetComponent<Manager_Settings>();
        UIReuseScript = GetComponent<Manager_UIReuse>();
        PlayerMovementScript = thePlayer.GetComponent<Player_Movement>();
        PlayerCameraScript = thePlayer.GetComponentInChildren<Player_Camera>();

        btn_ReturnToPM.onClick.AddListener(ShowPMContent);
        btn_ReturnToGame.onClick.AddListener(UnpauseGame);
        btn_Save.onClick.AddListener(delegate { SavingScript.CreateSaveFile(""); });
        btn_Load.onClick.AddListener(ShowLoadContent);
        btn_Settings.onClick.AddListener(ShowSettingsContent);
        btn_KeyBindings.onClick.AddListener(ShowKeyBindingsContent);
        btn_ReturnToMM.onClick.AddListener(delegate { ReturnToMM(false); });
        btn_Quit.onClick.AddListener(delegate { QuitGame(false); });
    }

    private void Update()
    {
        if (KeyBindingsScript.GetButtonDown("TogglePauseMenu"))
        {
            isPaused = !isPaused;
        }

        if (isPaused
            && canTogglePMUI
            && !par_PauseMenu.activeInHierarchy)
        {
            PauseWithUI();
        }
        else if (!isPaused
                 && par_PauseMenu.activeInHierarchy)
        {
            if (canUnpause)
            {
                UnpauseGame();
            }
            else
            {
                ClosePMContent();
            }
        }
    }

    //unpauses the game
    public void UnpauseGame()
    {
        ClosePMContent();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Time.timeScale = 1;

        PlayerMovementScript.canMove = true;
        PlayerCameraScript.isCamEnabled = true;

        //reset or load saved settings
        //if settings were not saved after player exited settings menu
        if (!SettingsScript.savedSettings)
        {
            SettingsScript.LoadSettings();
        }

        isPaused = false;
    }
    //pauses the game with UI
    public void PauseWithUI()
    {
        ShowPMContent();
        PauseWithoutUI();

        isPaused = true;
    }
    //only pauses the game without opening the UI
    public void PauseWithoutUI()
    {
        PlayerMovementScript.canMove = false;
        PlayerCameraScript.isCamEnabled = false;

        Time.timeScale = 0;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    //closes all other pm content and opens main pm content
    public void ShowPMContent()
    {
        par_PauseMenu.SetActive(true);
        par_PMContent.SetActive(true);

        par_LoadContent.SetActive(false);
        par_SettingsContent.SetActive(false);
        par_KeyBindingsContent.SetActive(false);

        //reset or load saved settings
        //if settings were not saved after player exited settings menu
        if (!SettingsScript.savedSettings)
        {
            SettingsScript.LoadSettings();
        }

        btn_ReturnToPM.gameObject.SetActive(false);
    }
    //closes all pm content
    public void ClosePMContent()
    {
        par_LoadContent.SetActive(false);
        par_SettingsContent.SetActive(false);
        par_KeyBindingsContent.SetActive(false);

        btn_ReturnToPM.gameObject.SetActive(false);

        par_PMContent.SetActive(false);
        par_PauseMenu.SetActive(false);
    }
    //shows load UI
    public void ShowLoadContent()
    {
        par_PMContent.SetActive(false);
        par_LoadContent.SetActive(true);

        SavingScript.ShowGameSaves();

        UIReuseScript.ClearSaveData();

        btn_ReturnToPM.gameObject.SetActive(true);
    }
    //shows all game settings
    public void ShowSettingsContent()
    {
        par_PMContent.SetActive(false);
        par_SettingsContent.SetActive(true);

        SettingsScript.RebuildSettingsList("general");

        btn_ReturnToPM.gameObject.SetActive(true);
    }
    //shows all key bindings
    public void ShowKeyBindingsContent()
    {
        par_PMContent.SetActive(false);
        par_KeyBindingsContent.SetActive(true);

        KeyBindingsScript.ShowGeneralKeyBindings();

        btn_ReturnToPM.gameObject.SetActive(true);
    }
    //goes back to main menu
    public void ReturnToMM(bool confirmedAction)
    {
        if (!confirmedAction)
        {
            //open the confirmation UI to let the player confirm
            //if they actually want to go back to the main menu or not
            ConfirmationScript.RecieveData(gameObject,
                                           "pauseMenuScript",
                                           "returnToMM",
                                           "");
        }
        else
        {
            SceneManager.LoadScene(0);
        }
    }
    //quits the game
    public void QuitGame(bool confirmedAction)
    {
        if (!confirmedAction)
        {
            //open the confirmation UI to let the player confirm
            //if they actually want to quit the game or not
            ConfirmationScript.RecieveData(gameObject, 
                                           "pauseMenuScript", 
                                           "quit",
                                           "");
        }
        else
        {
            Application.Quit();
        }
    }
}