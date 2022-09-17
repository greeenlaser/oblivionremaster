using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class UI_MainMenu : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject par_MMContent;
    [SerializeField] private GameObject par_LoadContent;
    [SerializeField] private GameObject par_SettingsContent;
    [SerializeField] private GameObject par_CreditsContent;
    [SerializeField] private Button btn_ContinueOrNewGame;
    [SerializeField] private Button btn_Load;
    [SerializeField] private Button btn_Settings;
    [SerializeField] private Button btn_Credits;
    [SerializeField] private Button btn_Quit;
    [SerializeField] private Button btn_ReturnToMM;

    //private variables
    private UI_Confirmation ConfirmationScript;

    private void Awake()
    {
        ConfirmationScript = GetComponent<UI_Confirmation>();

        btn_ContinueOrNewGame.onClick.AddListener(ContinueOrStartNewGame);
        btn_Load.onClick.AddListener(ShowLoadContent);
        btn_Settings.onClick.AddListener(ShowSettings);
        btn_Credits.onClick.AddListener(ShowCredits);
        btn_Quit.onClick.AddListener(delegate { QuitGame(false); });
        btn_ReturnToMM.onClick.AddListener(ShowMMContent);

        //temporarily always naming the continue or new game button as "Start new game"
        btn_ContinueOrNewGame.GetComponentInChildren<TMP_Text>().text = "Start new game";

        //main menu is always displayed at start
        ShowMMContent();
    }

    //closes all other UI that may be active and enables main menu UI
    public void ShowMMContent()
    {
        par_LoadContent.SetActive(false);
        par_SettingsContent.SetActive(false);
        par_CreditsContent.SetActive(false);
        btn_ReturnToMM.gameObject.SetActive(false);

        par_MMContent.SetActive(true);
    }
    //loads latest save or starts a new game if none exist,
    //button text is also changed accordingly
    //TODO: add save system to change button text and load latest save
    //if save exists or start new game if none exist
    public void ContinueOrStartNewGame()
    {
        SceneManager.LoadScene(1);
    }
    //shows game saves
    //TODO: create save and load system to list all game saves
    public void ShowLoadContent()
    {
        par_MMContent.SetActive(false);

        par_LoadContent.SetActive(true);
        btn_ReturnToMM.gameObject.SetActive(true);
    }
    //shows only a few settings available through the main menu
    //TODO: create game settings system to list available settings
    public void ShowSettings()
    {
        par_MMContent.SetActive(false);

        par_SettingsContent.SetActive(true);
        btn_ReturnToMM.gameObject.SetActive(true);
    }
    //shows game credits
    public void ShowCredits()
    {
        par_MMContent.SetActive(false);

        par_CreditsContent.SetActive(true);
        btn_ReturnToMM.gameObject.SetActive(true);
    }
    //quits the game
    public void QuitGame(bool confirmedAction)
    {
        if (!confirmedAction)
        {
            //open the confirmation UI to let the player confirm
            //if they actually want to quit the game or not
            ConfirmationScript.RecieveData(gameObject, "mainMenuScript", "quit");
        }
        else
        {
            Application.Quit();
        }
    }
}