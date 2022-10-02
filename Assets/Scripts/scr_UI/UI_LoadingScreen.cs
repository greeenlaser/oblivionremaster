using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class UI_LoadingScreen : MonoBehaviour
{
    [Header("Loading screen UI")]
    [SerializeField] private GameObject par_LoadingUI;
    [SerializeField] private RawImage loadingImage;
    [SerializeField] private TMP_Text txt_Tip;
    [SerializeField] private Slider loadingSlider; 
    public Button btn_Continue;
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
        currentScene = SceneManager.GetActiveScene().buildIndex;

        if (currentScene == 1)
        {
            PauseMenuScript = GetComponent<UI_PauseMenu>();
            ConsoleScript = GetComponent<Manager_Console>();
            PlayerMovementScript = thePlayer.GetComponent<Player_Movement>();
            PlayerCameraScript = thePlayer.GetComponentInChildren<Player_Camera>();

            btn_Continue.onClick.AddListener(CloseLoadingScreen);
        }
    }

    public void UpdateLoadingScreenBar(int newValue)
    {
        loadingSlider.value = newValue;
    }
    public void OpenLoadingScreen()
    {
        loadingImage.texture = loadingImages[UnityEngine.Random.Range(0, loadingImages.Count)];
        txt_Tip.text = tips[UnityEngine.Random.Range(0, tips.Count)];

        par_LoadingUI.SetActive(true);
        btn_Continue.gameObject.SetActive(false);
    }
    public void CloseLoadingScreen()
    {
        par_LoadingUI.SetActive(false);

        PauseMenuScript.canTogglePMUI = true;
        PauseMenuScript.canUnpause = true;
        PauseMenuScript.UnpauseGame();

        ConsoleScript.canToggleConsole = true;

        PlayerMovementScript.LoadPlayer();
        PlayerCameraScript.isCamEnabled = true;
    }
}