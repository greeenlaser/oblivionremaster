using System.IO;
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
    [SerializeField] private GameObject par_CreditsContent;
    [SerializeField] private Button btn_ReturnToMM;
    [SerializeField] private Button btn_ContinueOrNewGame;
    [SerializeField] private Button btn_Load;
    [SerializeField] private Button btn_Credits;
    [SerializeField] private Button btn_Quit;

    //private variables
    private Manager_GameSaving SavingScript;
    private GameManager GameManagerScript;
    private Manager_UIReuse UIReuseScript;

    private void Awake()
    {
        SavingScript = GetComponent<Manager_GameSaving>();
        GameManagerScript = GetComponent<GameManager>();
        UIReuseScript = GetComponent<Manager_UIReuse>();

        btn_ContinueOrNewGame.onClick.AddListener(StartNewGame);
        btn_Load.onClick.AddListener(ShowLoadContent);
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
        par_CreditsContent.SetActive(false);
        btn_ReturnToMM.gameObject.SetActive(false);

        par_MMContent.SetActive(true);
    }

    //start a new game and override game save loading in gamemanager script
    public void StartNewGame()
    {
        string loadFilePath = GameManagerScript.gamePath + @"\loadfile.txt";

        //using a text editor to write text to the game save file in the saved file path
        using StreamWriter loadFile = File.CreateText(loadFilePath);

        loadFile.WriteLine("restart");

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