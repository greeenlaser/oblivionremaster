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
    [SerializeField] private Button btn_ReturnToPM;
    [SerializeField] private Button btn_ReturnToGame;
    [SerializeField] private Button btn_Load;
    [SerializeField] private Button btn_Settings;
    [SerializeField] private Button btn_ReturnToMM;
    [SerializeField] private Button btn_Quit;

    //public but hidden variables
    [HideInInspector] public bool canUnpause;
    [HideInInspector] public bool isPaused;

    //private variables
    private UI_Confirmation ConfirmationScript;

    private void Awake()
    {
        ConfirmationScript = GetComponent<UI_Confirmation>();

        btn_ReturnToPM.onClick.AddListener(ShowPMContent);
        btn_ReturnToGame.onClick.AddListener(UnpauseGame);
        btn_Load.onClick.AddListener(ShowLoadContent);
        btn_Settings.onClick.AddListener(ShowSettingsContent);
        btn_ReturnToMM.onClick.AddListener(delegate { ReturnToMM(false); });
        btn_Quit.onClick.AddListener(delegate { QuitGame(false); });
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isPaused = !isPaused;
        }

        if (isPaused
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
        par_PauseMenu.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Time.timeScale = 1;

        isPaused = false;
    }
    //pauses the game with UI
    public void PauseWithUI()
    {
        ShowPMContent();
        PauseWithoutUI();
    }
    //only pauses the game without opening the UI
    public void PauseWithoutUI()
    {
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

        btn_ReturnToPM.gameObject.SetActive(false);
    }
    //closes all pm content
    public void ClosePMContent()
    {
        par_LoadContent.SetActive(false);
        par_SettingsContent.SetActive(false);

        btn_ReturnToPM.gameObject.SetActive(false);

        par_PMContent.SetActive(false);
        par_PauseMenu.SetActive(false);
    }
    //shows load UI
    public void ShowLoadContent()
    {
        par_PMContent.SetActive(false);
        par_LoadContent.SetActive(true);

        btn_ReturnToPM.gameObject.SetActive(true);
    }
    //shows all game settings
    public void ShowSettingsContent()
    {
        par_PMContent.SetActive(false);
        par_SettingsContent.SetActive(true);

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
                                           "returnToMM");
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
                                           "quit");
        }
        else
        {
            Application.Quit();
        }
    }
}