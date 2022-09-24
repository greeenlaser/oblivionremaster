using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Manager_UIReuse : MonoBehaviour
{
    [Header("Key assign UI")]
    public GameObject par_KeyAssign;
    public TMP_Text txt_AssignKey;

    [Header("Load UI")]
    public TMP_Text txt_SaveName;
    public TMP_Text txt_SaveDate;
    public TMP_Text txt_SaveCoordinates;
    public Button btn_LoadGame;
    public Button btn_SaveButtonTemplate;
    public Transform par_SaveButtons;

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

        btn_LoadGame.onClick.RemoveAllListeners();
        btn_LoadGame.interactable = false;
    }
}