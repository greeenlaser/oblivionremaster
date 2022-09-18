using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_Confirmation : MonoBehaviour
{
    //private variables
    private UI_PauseMenu PauseMenuScript;
    private Manager_UIReuse UIReuseScript;

    private void Awake()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

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

    caller object is the actual gameobject calling this event
    caller name is the name of the script calling this event
    action is what the caller script needs confirmation for
    */
    public void RecieveData(GameObject callerObject, 
                            string callerName, 
                            string action)
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
                if (action == "returnToMM")
                {
                    UIReuseScript.txt_Confirmation.text = "Are you sure you want to go back to the main menu? Unsaved content will be lost.";

                    UIReuseScript.btn_Confirm1.gameObject.SetActive(true);
                    UIReuseScript.btn_Confirm1.GetComponentInChildren<TMP_Text>().text = "Go to main menu";
                    UIReuseScript.btn_Confirm1.onClick.AddListener(delegate { PauseMenuScript.ReturnToMM(true); });
                }
                //the player wants to quit the game
                else if (action == "quit")
                {
                    UIReuseScript.txt_Confirmation.text = "Are you sure you want to quit the game? Unsaved content will be lost.";

                    UIReuseScript.btn_Confirm1.gameObject.SetActive(true);
                    UIReuseScript.btn_Confirm1.GetComponentInChildren<TMP_Text>().text = "Quit";

                    UIReuseScript.btn_Confirm1.onClick.AddListener(delegate { PauseMenuScript.QuitGame(true); });
                }
            }
        }
    }
}