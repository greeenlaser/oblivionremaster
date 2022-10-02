using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class Manager_UIReuse : MonoBehaviour
{
    [Header("Key assign UI")]
    public GameObject par_KeyAssign;
    public TMP_Text txt_AssignKey;

    [Header("Load UI")]
    public TMP_Text txt_SaveName;
    public TMP_Text txt_SaveDate;
    public TMP_Text txt_SaveCoordinates;
    public RawImage saveImage;
    public Button btn_LoadSave;
    public Button btn_DeleteSave;
    public Button btn_SaveButtonTemplate;
    public Transform par_SaveButtons;

    [Header("Settings UI")]
    public Button btn_SaveSettings;
    public Button btn_ResetSettings;
    public Button btn_ShowGeneralSettings;
    public Button btn_ShowGraphicsSettings;
    public Button btn_ShowAudioSettings;
    [HideInInspector] public GameObject par_MainGeneralSettingsParent;
    public GameObject par_GeneralSettingsParent;
    [HideInInspector] public GameObject par_MainGraphicsSettingsParent;
    public GameObject par_GraphicsSettingsParent;
    [HideInInspector] public GameObject par_MainAudioSettingsParent;
    public GameObject par_AudioSettingsParent;
    public List<GameObject> generalSettingsParents = new();
    public List<GameObject> graphicsSettingsParents = new();
    public List<GameObject> audioSettingsParents = new();

    [Header("Console UI")]
    public GameObject par_Console;
    public TMP_InputField consoleInputField;
    public TMP_Text txt_InsertedTextTemplate;
    public GameObject par_ConsoleContent;

    [Header("Confirmation UI")]
    public GameObject par_Confirmation;
    public TMP_Text txt_Confirmation;
    public Button btn_Confirm1;
    public Button btn_Confirm2;
    public Button btn_Cancel;

    [Header("Debug menu UI")]
    public GameObject par_DebugMenu;
    public TMP_Text txt_GameVersion;
    public TMP_Text txt_FPS;

    //private variables
    private int currentScene;
    private Manager_Settings SettingsScript;

    private void Awake()
    {
        currentScene = SceneManager.GetActiveScene().buildIndex;

        if (currentScene == 1)
        {
            SettingsScript = GetComponent<Manager_Settings>();

            par_MainGeneralSettingsParent = par_GeneralSettingsParent.transform.parent.parent.parent.gameObject;
            par_MainGraphicsSettingsParent = par_GraphicsSettingsParent.transform.parent.parent.parent.gameObject;
            par_MainAudioSettingsParent = par_AudioSettingsParent.transform.parent.parent.parent.gameObject;

            btn_SaveSettings.onClick.AddListener(SettingsScript.SaveSettings);
            btn_ResetSettings.onClick.AddListener(delegate { SettingsScript.ResetSettings(false); });
            btn_ShowGeneralSettings.onClick.AddListener(delegate { SettingsScript.RebuildSettingsList("general"); });
            btn_ShowGraphicsSettings.onClick.AddListener(delegate { SettingsScript.RebuildSettingsList("graphics"); });
            btn_ShowAudioSettings.onClick.AddListener(delegate { SettingsScript.RebuildSettingsList("audio"); });
        }
    }

    //reset all confirmation UI elements
    public void ClearConfirmationUI()
    {
        btn_Confirm1.onClick.RemoveAllListeners();
        btn_Confirm1.gameObject.SetActive(false);

        btn_Confirm2.onClick.RemoveAllListeners();
        btn_Confirm2.gameObject.SetActive(false);

        par_Confirmation.SetActive(false);
    }

    //clears selected save UI
    public void ClearSaveData()
    {
        txt_SaveName.text = "";
        txt_SaveDate.text = "";
        txt_SaveCoordinates.text = "";
        saveImage.texture = null;
        saveImage.gameObject.SetActive(false);

        btn_LoadSave.onClick.RemoveAllListeners();
        btn_LoadSave.interactable = false;

        btn_DeleteSave.onClick.RemoveAllListeners();
        btn_DeleteSave.interactable = false;
    }

    public void ClearSettingsUI()
    {
        par_MainGeneralSettingsParent.SetActive(false);
        par_MainGraphicsSettingsParent.SetActive(false);
        par_MainAudioSettingsParent.SetActive(false);

        btn_ShowGeneralSettings.interactable = true;
        btn_ShowGraphicsSettings.interactable = true;
        btn_ShowAudioSettings.interactable = true;
    }
}