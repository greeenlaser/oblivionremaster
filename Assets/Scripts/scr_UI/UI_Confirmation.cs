using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_Confirmation : MonoBehaviour
{
    //private variables
    private UI_PauseMenu PauseMenuScript;
    private Manager_GameSaving SavingScript;
    private Manager_KeyBindings KeyBindingsScript;
    private Manager_Settings SettingsScript;
    private Manager_UIReuse UIReuseScript;

    private void Awake()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        SavingScript = GetComponent<Manager_GameSaving>();
        SettingsScript = GetComponent<Manager_Settings>();
        UIReuseScript = GetComponent<Manager_UIReuse>();
        if (currentSceneIndex == 1)
        {
            PauseMenuScript = GetComponent<UI_PauseMenu>();
            KeyBindingsScript = GetComponent<Manager_KeyBindings>();
        }

        UIReuseScript.btn_Cancel.onClick.AddListener(UIReuseScript.ClearConfirmationUI);
        UIReuseScript.ClearConfirmationUI();
    }

    /*
    recieve data from one of the many UI elements
    which require the user to confirm their choice to proceed
    and assign appropriate events to correct UI elements

    caller object is the actual gameobject calling this method
    caller name is the name of the script calling this method
    key is the action or thing that needs to be accessed/edited etc
    */
    public void RecieveData(GameObject callerObject, 
                            string callerName, 
                            string key,
                            string optionalAction)
    {
        //resets confirmation UI
        UIReuseScript.ClearConfirmationUI();

        UIReuseScript.par_Confirmation.SetActive(true);

        //the manager scripts gameobject
        if (callerObject.name == "par_Managers")
        {
            //the pause menu script
            if (callerName == "pauseMenuScript")
            {
                //the player wants to go back to
                //the main menu from the pause menu
                if (key == "returnToMM")
                {
                    UIReuseScript.txt_Confirmation.text = "Are you sure you want to go back to the main menu? Unsaved content will be lost.";

                    UIReuseScript.btn_Confirm1.gameObject.SetActive(true);
                    UIReuseScript.btn_Confirm1.transform.localPosition = new(-150, -79, 0);
                    UIReuseScript.btn_Confirm1.GetComponentInChildren<TMP_Text>().text = "Save and go";
                    UIReuseScript.btn_Confirm1.onClick.AddListener(delegate { SavingScript.CreateSaveFile(""); });
                    UIReuseScript.btn_Confirm1.onClick.AddListener(delegate { PauseMenuScript.ReturnToMM(true); });

                    UIReuseScript.btn_Confirm2.gameObject.SetActive(true);
                    UIReuseScript.btn_Confirm2.GetComponentInChildren<TMP_Text>().text = "Go without saving";
                    UIReuseScript.btn_Confirm2.onClick.AddListener(delegate { PauseMenuScript.ReturnToMM(true); });
                }
                //the player wants to quit the game
                else if (key == "quit")
                {
                    UIReuseScript.txt_Confirmation.text = "Are you sure you want to quit the game? Unsaved content will be lost.";

                    UIReuseScript.btn_Confirm1.gameObject.SetActive(true);
                    UIReuseScript.btn_Confirm1.transform.localPosition = new(-150, -79, 0);
                    UIReuseScript.btn_Confirm1.GetComponentInChildren<TMP_Text>().text = "Save game and quit";
                    UIReuseScript.btn_Confirm1.onClick.AddListener(delegate { SavingScript.CreateSaveFile(""); });
                    UIReuseScript.btn_Confirm1.onClick.AddListener(delegate { PauseMenuScript.QuitGame(true); });

                    UIReuseScript.btn_Confirm2.gameObject.SetActive(true);
                    UIReuseScript.btn_Confirm2.GetComponentInChildren<TMP_Text>().text = "Quit without saving";

                    UIReuseScript.btn_Confirm1.onClick.AddListener(delegate { PauseMenuScript.QuitGame(true); });
                }
            }
            //the save script
            else if (callerName == "saveScript")
            {
                if (optionalAction == "load")
                {
                    UIReuseScript.txt_Confirmation.text = "Are you sure you want to load this save? Unsaved content will be lost.";

                    UIReuseScript.btn_Confirm1.gameObject.SetActive(true);
                    UIReuseScript.btn_Confirm1.transform.localPosition = new(-150, -79, 0);
                    UIReuseScript.btn_Confirm1.GetComponentInChildren<TMP_Text>().text = "Save game and load";
                    UIReuseScript.btn_Confirm1.onClick.AddListener(delegate { SavingScript.CreateSaveFile(""); });
                    UIReuseScript.btn_Confirm1.onClick.AddListener(delegate { SavingScript.CreateLoadFile(key); });

                    UIReuseScript.btn_Confirm2.gameObject.SetActive(true);
                    UIReuseScript.btn_Confirm2.GetComponentInChildren<TMP_Text>().text = "Load without saving";

                    UIReuseScript.btn_Confirm2.onClick.AddListener(delegate { SavingScript.CreateLoadFile(key); });
                }
                else if (optionalAction == "delete")
                {
                    UIReuseScript.txt_Confirmation.text = "Are you sure you want to delete this save?";

                    UIReuseScript.btn_Confirm1.gameObject.SetActive(true);
                    UIReuseScript.btn_Confirm1.transform.localPosition = new(0, -79, 0);
                    UIReuseScript.btn_Confirm1.GetComponentInChildren<TMP_Text>().text = "Delete";
                    UIReuseScript.btn_Confirm1.onClick.AddListener(delegate { SavingScript.DeleteSave(key); });
                    UIReuseScript.btn_Confirm1.onClick.AddListener(UIReuseScript.ClearConfirmationUI);
                }
            }
            //the key bindings script
            else if (callerName == "keyBindingsScript")
            {
                UIReuseScript.txt_Confirmation.text = "Are you sure you want to reset the key bindings?";

                UIReuseScript.btn_Confirm1.gameObject.SetActive(true);
                UIReuseScript.btn_Confirm1.transform.localPosition = new(0, -79, 0);
                UIReuseScript.btn_Confirm1.GetComponentInChildren<TMP_Text>().text = "Reset";

                UIReuseScript.btn_Confirm1.onClick.AddListener(delegate { KeyBindingsScript.ResetKeyBindings(true); });
                UIReuseScript.btn_Confirm1.onClick.AddListener(UIReuseScript.ClearConfirmationUI);
            }
            //the settings script
            else if (callerName == "settingsScript")
            {
                UIReuseScript.txt_Confirmation.text = "Are you sure you want to reset the settings?";

                UIReuseScript.btn_Confirm1.gameObject.SetActive(true);
                UIReuseScript.btn_Confirm1.transform.localPosition = new(0, -79, 0);
                UIReuseScript.btn_Confirm1.GetComponentInChildren<TMP_Text>().text = "Reset";

                UIReuseScript.btn_Confirm1.onClick.AddListener(delegate { SettingsScript.ResetSettings(true); });
                UIReuseScript.btn_Confirm1.onClick.AddListener(UIReuseScript.ClearConfirmationUI);
            }
        }
    }
}