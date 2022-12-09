using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class UI_LoadingScreen : MonoBehaviour
{
    [Header("Loading screen UI")]
    public GameObject par_LoadingUI;
    [SerializeField] private RawImage loadingImage;
    [SerializeField] private TMP_Text txt_Tip;
    public TMP_Text txt_PressToContinue;
    [SerializeField] private Slider loadingSlider;
    [SerializeField] private List<Texture> loadingImages = new();
    [SerializeField] private List<string> tips = new();

    [Header("Scripts")]
    [SerializeField] private GameObject thePlayer;

    //private variables
    private int currentScene;

    //scripts
    private UI_PauseMenu PauseMenuScript;
    private Manager_Console ConsoleScript;
    private Player_Movement PlayerMovementScript;
    private Player_Camera PlayerCameraScript;

    private void Awake()
    {
        ConsoleScript = GetComponent<Manager_Console>();

        currentScene = SceneManager.GetActiveScene().buildIndex;

        if (currentScene == 1)
        {
            PauseMenuScript = GetComponent<UI_PauseMenu>();
            PlayerMovementScript = thePlayer.GetComponent<Player_Movement>();
            PlayerCameraScript = thePlayer.GetComponentInChildren<Player_Camera>();
        }
    }

    private void Update()
    {
        //press space to close loading screen
        if (txt_PressToContinue.gameObject.activeInHierarchy
            && Input.GetKeyDown(KeyCode.Space))
        {
            CloseLoadingScreen();
        }
    }

    public void UpdateLoadingScreenBar(int newValue)
    {
        loadingSlider.value = newValue;
    }
    public void OpenLoadingScreen()
    {
        loadingImage.texture = loadingImages[Random.Range(0, loadingImages.Count)];
        txt_Tip.text = tips[Random.Range(0, tips.Count)];

        par_LoadingUI.SetActive(true);
        txt_PressToContinue.gameObject.SetActive(false);
    }
    public void CloseLoadingScreen()
    {
        par_LoadingUI.SetActive(false);

        PauseMenuScript.canTogglePMUI = true;
        PauseMenuScript.UnpauseGame();

        ConsoleScript.canToggleConsole = true;

        PlayerMovementScript.LoadPlayer();
        PlayerCameraScript.isCamEnabled = true;
    }
}