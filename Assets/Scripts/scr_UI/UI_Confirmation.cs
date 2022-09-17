using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_Confirmation : MonoBehaviour
{
    //private variables
    private UI_MainMenu MainMenuScript;
    private Manager_UIReuse UIReuseScript;

    private void Awake()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        UIReuseScript = GetComponent<Manager_UIReuse>();
        if (currentSceneIndex == 0)
        {
            MainMenuScript = GetComponent<UI_MainMenu>();
        }

        UIReuseScript.btn_Cancel.onClick.AddListener(UIReuseScript.ClearConfirmationUI);
        UIReuseScript.ClearConfirmationUI();
    }

    /*
    recieve data from one of the many UI elements
    which require the user to confirm their choice to proceed
    and assign appropriate events to correct UI elements

    caller object is the actual gameobject calling this event
    caller name is the name of the script calling this event
    action is what the caller script needs confirmation for
    */
    public void RecieveData(GameObject callerObject, string callerName, string action)
    {
        //resets confirmation UI
        UIReuseScript.ClearConfirmationUI();

        UIReuseScript.par_Confirmation.SetActive(true);

        //the manager scripts gameobject
        if (callerObject.name == "par_Managers")
        {
            //the main menu script
            if (callerName == "mainMenuScript")
            {
                //the player wants to quit the game
                if (action == "quit")
                {
                    UIReuseScript.txt_Confirmation.text = "Are you sure you want to quit the game?";

                    UIReuseScript.btn_Confirm1.gameObject.SetActive(true);
                    UIReuseScript.btn_Confirm1.GetComponentInChildren<TMP_Text>().text = "Quit";
                    UIReuseScript.btn_Confirm1.onClick.AddListener(delegate { MainMenuScript.QuitGame(true); });
                }
            }
        }
    }
}