using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_AssignKey : MonoBehaviour
{
    [Header("Assignables")]
    [SerializeField] private GameObject par_Managers;

    //public but hidden variables
    [HideInInspector] public string info;
    [HideInInspector] public KeyCode pressedKey;

    //private variables
    private bool isAssignUIOpen;
    private float timer;
    private UI_PauseMenu PauseMenuScript;
    private Manager_KeyBindings KeyBindingsScript;
    private Manager_UIReuse UIReuseScript;

    private void Awake()
    {
        PauseMenuScript = par_Managers.GetComponent<UI_PauseMenu>();
        KeyBindingsScript = par_Managers.GetComponent<Manager_KeyBindings>();
        UIReuseScript = par_Managers.GetComponent<Manager_UIReuse>();

        foreach (GameObject item in transform.parent)
        {
            if (item.name == "txt_KeyBinding")
            {
                info = item.GetComponent<TMP_Text>().text;
                break;
            }
        }
    }

    private void Update()
    {
        if (isAssignUIOpen)
        {
            timer -= Time.unscaledDeltaTime;
            UIReuseScript.txt_AssignKey.text = "Assigning key to " + info + ". Press any key to assign it to this function or wait " + (int)timer + " second(s) for the assign UI to close by itself.";

            if (timer <= 0)
            {
                CloseAssigning();
            }
        }
    }

    private void OnGUI()
    {
        if (isAssignUIOpen)
        {
            Event e = Event.current;
            if (e.isKey)
            {
                pressedKey = e.keyCode;
                if (pressedKey != KeyCode.None)
                {
                    KeyBindingsScript.AssignKey(gameObject.GetComponent<Button>(),
                                                pressedKey,
                                                info);
                    CloseAssigning();
                }
            }
        }
    }

    public void OpenAssigning()
    {
        PauseMenuScript.isKeyAssignUIOpen = true;

        UIReuseScript.par_KeyAssign.SetActive(true);

        pressedKey = KeyCode.None;

        timer = 5;
        isAssignUIOpen = true;
    }
    public void CloseAssigning()
    {
        PauseMenuScript.isKeyAssignUIOpen = false;

        UIReuseScript.par_KeyAssign.SetActive(false);

        isAssignUIOpen = false;
    }
}