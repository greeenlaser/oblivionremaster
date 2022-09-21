using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_Confirmation : MonoBehaviour
{
    //private variables
    private UI_PauseMenu PauseMenuScript;
    private Manager_GameSaving SavingScript;
    private Manager_UIReuse UIReuseScript;

    private void Awake()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        SavingScript = GetComponent<Manager_GameSaving>();
        UIReuseScript = GetComponent<Manager_UIReuse>();
        if (currentSceneIndex == 1)
        {
            PauseMenuScript = GetComponent<UI_PauseMenu>();
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
                            string key)
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
                    UIReuseScript.btn_Confirm1.GetComponentInChildren<TMP_Text>().text = "Save game and quit";

                    UIReuseScript.btn_Confirm1.onClick.AddListener(delegate { SavingScript.CreateSaveFile(""); });
                    UIReuseScript.btn_Confirm1.onClick.AddListener(delegate { PauseMenuScript.QuitGame(true); });

                    UIReuseScript.btn_Confirm2.gameObject.SetActive(true);
                    UIReuseScript.btn_Confirm2.GetComponentInChildren<TMP_Text>().text = "Quit without saving";

                    UIReuseScript.btn_Confirm1.onClick.AddListener(delegate { PauseMenuScript.QuitGame(true); });
                }
            }
            //the save script
            else if (key == "saveScript")
            {
                UIReuseScript.txt_Confirmation.text = "Are you sure you want to load this save? Unsaved content will be lost.";

                UIReuseScript.btn_Confirm1.gameObject.SetActive(true);
                UIReuseScript.btn_Confirm1.GetComponentInChildren<TMP_Text>().text = "Save game and load";

                UIReuseScript.btn_Confirm1.onClick.AddListener(delegate { SavingScript.CreateSaveFile(""); } );
                UIReuseScript.btn_Confirm1.onClick.AddListener(delegate { SavingScript.CreateLoadFile(key); });

                UIReuseScript.btn_Confirm2.gameObject.SetActive(true);
                UIReuseScript.btn_Confirm2.GetComponentInChildren<TMP_Text>().text = "Load without saving";

                UIReuseScript.btn_Confirm2.onClick.AddListener(delegate { SavingScript.CreateLoadFile(key); });
            }
        }
    }
}