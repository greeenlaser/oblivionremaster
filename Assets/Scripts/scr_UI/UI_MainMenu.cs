using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UI_MainMenu : MonoBehaviour
{
    [Header("Main menu UI")]
    [SerializeField] private GameObject par_MMContent;
    [SerializeField] private GameObject par_LoadContent;
    [SerializeField] private GameObject par_SettingsContent;
    [SerializeField] private GameObject par_CreditsContent;
    [SerializeField] private Button btn_ReturnToMM;
    [SerializeField] private Button btn_ContinueOrNewGame;
    [SerializeField] private Button btn_Load;
    [SerializeField] private Button btn_Settings;
    [SerializeField] private Button btn_Credits;
    [SerializeField] private Button btn_Quit;

    //private variables
    private Manager_GameSaving SavingScript;
    private Manager_UIReuse UIReuseScript;

    private void Awake()
    {
        SavingScript = GetComponent<Manager_GameSaving>();
        UIReuseScript = GetComponent<Manager_UIReuse>();

        btn_ContinueOrNewGame.onClick.AddListener(ContinueOrStartNewGame);
        btn_Load.onClick.AddListener(ShowLoadContent);
        btn_Settings.onClick.AddListener(ShowSettings);
        btn_Credits.onClick.AddListener(ShowCredits);
        btn_Quit.onClick.AddListener(QuitGame);
        btn_ReturnToMM.onClick.AddListener(ShowMMContent);

        //main menu is always displayed at start
        ShowMMContent();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 1;
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
    //if save exists or start new game if none exist
    public void ContinueOrStartNewGame()
    {
        SceneManager.LoadScene(1);
    }
    //shows game saves
    public void ShowLoadContent()
    {
        par_MMContent.SetActive(false);
        par_LoadContent.SetActive(true);

        SavingScript.ShowGameSaves();

        UIReuseScript.ClearSaveData();

        btn_ReturnToMM.gameObject.SetActive(true);
    }
    //shows only a few settings available through the main menu
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
    public void QuitGame()
    {
        Application.Quit();
    }
}