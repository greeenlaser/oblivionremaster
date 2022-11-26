using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Palmmedia.ReportGenerator.Core.Reporting.Builders;

public class Manager_KeyBindings : MonoBehaviour
{
    [Header("Key bindings UI")]
    [SerializeField] private Button btn_SaveKeyBindings;
    [SerializeField] private Button btn_ResetKeyBindings;

    [Header("General key UI")]
    [SerializeField] private GameObject par_GeneralKeyBindingsParent;
    [SerializeField] private Button btn_ShowGeneralKeyBindings;
    public List<GameObject> generalKeyParents = new();

    [Header("Movement key UI")]
    [SerializeField] private GameObject par_MovementKeyBindingsParent;
    [SerializeField] private Button btn_ShowMovementKeyBindings;
    public List<GameObject> movementKeyParents = new();

    [Header("Combat key UI")]
    [SerializeField] private GameObject par_CombatKeyBindingsParent;
    [SerializeField] private Button btn_ShowCombatKeyBindings;
    public List<GameObject> combatKeyParents = new();

    //public but hidden variables
    [HideInInspector] public Dictionary<string, KeyCode> KeyBindings = new();

    //private variables
    private string settingsPath;
    private int currentScene;
    private readonly string[] keycodes = new string[]
    {
        "None", "Backspace", "Tab", "Clear", "Return", "Pause", "Escape", "Space", "Exclaim", "DoubleQuote", "Hash", "Dollar", "Ampersand",
        "Quote", "LeftParen", "RightParen", "Asterisk", "Plus", "Comma", "Minus", "Period", "Slash",
        "Alpha0", "Alpha1", "Alpha2", "Alpha3", "Alpha4", "Alpha5", "Alpha6", "Alpha7", "Alpha8", "Alpha9",
        "Colon", "Semicolon", "Less", "Equals", "Greater", "Question", "At", "LeftBracket", "Backslash", "RightBracket", "Caret", "Underscore", "BackQuote",
        "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z",
        "Delete", "Keypad0", "Keypad1", "Keypad2", "Keypad3", "Keypad4", "Keypad5", "Keypad6", "Keypad7", "Keypad8", "Keypad9",
        "KeypadPeriod", "KeypadDivide", "KeypadMultiply", "KeypadMinus",  "KeypadPlus", "KeypadEnter", "KeypadEquals",
        "UpArrow", "DownArrow", "RightArrow", "LeftArrow", "Insert", "Home", "End", "PageUp", "PageDown",
        "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11", "F12", "F13", "F14", "F15",
        "Numlock", "CapsLock", "ScrollLock", "RightShift", "LeftShift", "RightControl", "LeftControl", "RightAlt", "LeftAlt",
        "RightCommand", "LeftCommand", "LeftWindows", "RightWindows", "AltGr", "Help", "Print", "SysReq", "Break", "Menu",
        "Mouse0", "Mouse1", "Mouse2", "Mouse3", "Mouse4", "Mouse5", "Mouse6"
    };

    //scripts
    private UI_Confirmation ConfirmationScript;
    private GameManager GameManagerScript;
    private Manager_UIReuse UIReuseScript;

    private void Awake()
    {
        ConfirmationScript = GetComponent<UI_Confirmation>();
        GameManagerScript = GetComponent<GameManager>();
        UIReuseScript = GetComponent<Manager_UIReuse>();

        settingsPath = GameManagerScript.settingsPath;

        currentScene = SceneManager.GetActiveScene().buildIndex;

        if (currentScene == 1)
        {
            btn_ShowGeneralKeyBindings.onClick.AddListener(ShowGeneralKeyBindings);
            btn_ShowMovementKeyBindings.onClick.AddListener(ShowMovementKeyBindings);
            btn_ShowCombatKeyBindings.onClick.AddListener(ShowCombatKeyBindings);

            btn_SaveKeyBindings.onClick.AddListener(SaveKeyBindings);
            btn_ResetKeyBindings.onClick.AddListener(delegate { ResetKeyBindings(false); });
        }
    }

    public void ShowGeneralKeyBindings()
    {
        par_GeneralKeyBindingsParent.SetActive(true);
        par_MovementKeyBindingsParent.SetActive(false);
        par_CombatKeyBindingsParent.SetActive(false);

        btn_ShowGeneralKeyBindings.interactable = false;
        btn_ShowMovementKeyBindings.interactable = true;
        btn_ShowCombatKeyBindings.interactable = true;

        RebuildKeyBindingsList("general");
    }
    public void ShowMovementKeyBindings()
    {
        par_GeneralKeyBindingsParent.SetActive(false);
        par_MovementKeyBindingsParent.SetActive(true);
        par_CombatKeyBindingsParent.SetActive(false);

        btn_ShowGeneralKeyBindings.interactable = true;
        btn_ShowMovementKeyBindings.interactable = false;
        btn_ShowCombatKeyBindings.interactable = true;

        RebuildKeyBindingsList("movement");
    }
    public void ShowCombatKeyBindings()
    {
        par_GeneralKeyBindingsParent.SetActive(false);
        par_MovementKeyBindingsParent.SetActive(false);
        par_CombatKeyBindingsParent.SetActive(true);

        btn_ShowGeneralKeyBindings.interactable = true;
        btn_ShowMovementKeyBindings.interactable = true;
        btn_ShowCombatKeyBindings.interactable = false;

        RebuildKeyBindingsList("combat");
    }
    public void RebuildKeyBindingsList(string targetKeyBindingParentName)
    {
        //only lists all key bindings from selected key bindings parent
        List<GameObject> keyParents = null;
        if (targetKeyBindingParentName == "general")
        {
            keyParents = generalKeyParents;
        }
        else if (targetKeyBindingParentName == "movement")
        {
            keyParents = movementKeyParents;
        }
        else if (targetKeyBindingParentName == "combat")
        {
            keyParents = combatKeyParents;
        }

        foreach (GameObject buttonUIParent in keyParents)
        {
            Button keyButton = buttonUIParent.GetComponentInChildren<Button>();

            //remove old button event
            keyButton.onClick.RemoveAllListeners();

            //get the assign script from the button
            UI_AssignKey AssignScript = keyButton.GetComponent<UI_AssignKey>();
            //get the info from the script - what will the pressed button do
            string info = AssignScript.info;

            //get all keys from key bindings keycodes and convert to string array
            string[] values = new string[KeyBindings.Keys.Count];
            //copy all key binding keys to keys array
            KeyBindings.Keys.CopyTo(values, 0);
            //assign text and new event to button
            foreach (KeyValuePair<string, KeyCode> dict in KeyBindings)
            {
                string key = dict.Key;
                if (info == key)
                {
                    //set button text
                    string value = dict.Value.ToString().Replace("KeyCode.", "");
                    TMP_Text input = keyButton.GetComponentInChildren<TMP_Text>();
                    input.text = value;

                    //add event
                    keyButton.onClick.AddListener(AssignScript.OpenAssigning);
                }
            }
        }
    }

    //once a key has been pressed
    //it is assigned to the selected key
    public void AssignKey(Button targetButton, KeyCode pressedKey, string info)
    {
        //create temporary list and add all existing
        //list key parents to the temporary list
        List<GameObject> allKeyParents = new();
        foreach (GameObject ui in generalKeyParents)
        {
            allKeyParents.Add(ui);
        }
        foreach (GameObject ui in movementKeyParents)
        {
            allKeyParents.Add(ui);
        }
        foreach (GameObject ui in combatKeyParents)
        {
            allKeyParents.Add(ui);
        }
        //find existing duplicate KeyCode from 
        //temporary list if it exists
        //and replace it with KeyCode.None
        foreach (KeyValuePair<string, KeyCode> dict in KeyBindings)
        {
            string key = dict.Key;
            KeyCode value = dict.Value;
            if (pressedKey == value)
            {
                KeyBindings[key] = KeyCode.None;

                foreach (GameObject buttonParent in allKeyParents)
                {
                    Button btn = buttonParent.GetComponentInChildren<Button>();
                    string buttonText = btn.GetComponentInChildren<TMP_Text>().text;
                    string buttonInfo = btn.GetComponentInChildren<UI_AssignKey>().info;
                    if (buttonInfo == key)
                    {
                        buttonText = "None";
                        break;
                    }
                }
                break;
            }
        }

        //change dictionary KeyCode
        KeyBindings[info] = pressedKey;
        //change button text
        targetButton.GetComponentInChildren<TMP_Text>().text = pressedKey.ToString().Replace("KeyCode.", "");

        if (par_GeneralKeyBindingsParent.activeInHierarchy)
        {
            RebuildKeyBindingsList("general");
        }
        else if (par_MovementKeyBindingsParent.activeInHierarchy)
        {
            RebuildKeyBindingsList("movement");
        }
        else if (par_CombatKeyBindingsParent.activeInHierarchy)
        {
            RebuildKeyBindingsList("combat");
        }

        Debug.Log("Successfully assigned key " + pressedKey.ToString().Replace("KeyCode.", "") + " to action " + info + "!");
    }
    public void ResetKeyBindings(bool canReset)
    {
        if (!canReset)
        {
            //open the confirmation UI to let the player confirm
            //if they actually want to reset the key bindings or not
            ConfirmationScript.RecieveData(gameObject,
                                           "keyBindingsScript",
                                           "",
                                           "");
        }
        else
        {
            //clears the key bindings dictionary
            //if there are any key bindings
            if (KeyBindings.Count > 0)
            {
                KeyBindings.Clear();
            }

            //assigns default general key bindings
            KeyBindings["Save"] = KeyCode.F5;
            KeyBindings["Load"] = KeyCode.F9;
            KeyBindings["TogglePauseMenu"] = KeyCode.Escape;
            KeyBindings["ToggleConsole"] = KeyCode.PageUp;
            KeyBindings["TogglePlayerMenu"] = KeyCode.Tab;
            KeyBindings["PickUpOrInteract"] = KeyCode.E;

            //assigns default movement key bindings
            KeyBindings["WalkForwards"] = KeyCode.W;
            KeyBindings["WalkBackwards"] = KeyCode.S;
            KeyBindings["WalkLeft"] = KeyCode.A;
            KeyBindings["WalkRight"] = KeyCode.D;
            KeyBindings["Jump"] = KeyCode.Space;
            KeyBindings["Sprint"] = KeyCode.LeftShift;
            KeyBindings["Crouch"] = KeyCode.LeftControl;
            KeyBindings["ToggleFirstAndThirdPerson"] = KeyCode.F;

            //assigns default combat key bindings
            KeyBindings["CastSpell"] = KeyCode.C;
            KeyBindings["MainAttack"] = KeyCode.Mouse0;
            KeyBindings["SideAttack"] = KeyCode.Mouse1;
            KeyBindings["DropEquippedWeapon"] = KeyCode.R;

            if (currentScene == 1)
            {
                if (par_GeneralKeyBindingsParent.activeInHierarchy)
                {
                    RebuildKeyBindingsList("general");
                }
                else if (par_MovementKeyBindingsParent.activeInHierarchy)
                {
                    RebuildKeyBindingsList("movement");
                }
                else if (par_CombatKeyBindingsParent.activeInHierarchy)
                {
                    RebuildKeyBindingsList("combat");
                }
            }
        }
    }
    //save all key bindings to KeyBindings.txt file,
    //delete the old file if it already exists
    public void SaveKeyBindings()
    {
        string filePath = settingsPath + @"\KeyBindings.txt";

        //delete the old key bindings file if it exists
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        //create a new settings file
        using StreamWriter keyBindingsFile = File.CreateText(filePath);

        keyBindingsFile.WriteLine("Save file for " + UIReuseScript.txt_GameVersion.text + ".");
        keyBindingsFile.WriteLine("");

        keyBindingsFile.WriteLine("---GENERAL KEYBINDS---");
        keyBindingsFile.WriteLine("Save: " + KeyBindings["Save"].ToString().Replace("KeyCode.", ""));
        keyBindingsFile.WriteLine("Load: " + KeyBindings["Load"].ToString().Replace("KeyCode.", ""));
        keyBindingsFile.WriteLine("TogglePauseMenu: " + KeyBindings["TogglePauseMenu"].ToString().Replace("KeyCode.", ""));
        keyBindingsFile.WriteLine("ToggleConsole: " + KeyBindings["ToggleConsole"].ToString().Replace("KeyCode.", ""));
        keyBindingsFile.WriteLine("TogglePlayerMenu: " + KeyBindings["TogglePlayerMenu"].ToString().Replace("KeyCode.", ""));
        keyBindingsFile.WriteLine("PickUpOrInteract: " + KeyBindings["PickUpOrInteract"].ToString().Replace("KeyCode.", ""));
        keyBindingsFile.WriteLine("");

        keyBindingsFile.WriteLine("---MOVEMENT KEYBINDS---");
        keyBindingsFile.WriteLine("WalkForwards: " + KeyBindings["WalkForwards"].ToString().Replace("KeyCode.", ""));
        keyBindingsFile.WriteLine("WalkBackwards: " + KeyBindings["WalkBackwards"].ToString().Replace("KeyCode.", ""));
        keyBindingsFile.WriteLine("WalkLeft: " + KeyBindings["WalkLeft"].ToString().Replace("KeyCode.", ""));
        keyBindingsFile.WriteLine("WalkRight: " + KeyBindings["WalkRight"].ToString().Replace("KeyCode.", ""));
        keyBindingsFile.WriteLine("Jump: " + KeyBindings["Jump"].ToString().Replace("KeyCode.", ""));
        keyBindingsFile.WriteLine("Sprint: " + KeyBindings["Sprint"].ToString().Replace("KeyCode.", ""));
        keyBindingsFile.WriteLine("Crouch: " + KeyBindings["Crouch"].ToString().Replace("KeyCode.", ""));
        keyBindingsFile.WriteLine("ToggleFirstAndThirdPerson: " + KeyBindings["ToggleFirstAndThirdPerson"].ToString().Replace("KeyCode.", ""));
        keyBindingsFile.WriteLine("");

        keyBindingsFile.WriteLine("---COMBAT KEYBINDS---");
        keyBindingsFile.WriteLine("CastSpell: " + KeyBindings["CastSpell"].ToString().Replace("KeyCode.", ""));
        keyBindingsFile.WriteLine("MainAttack: " + KeyBindings["MainAttack"].ToString().Replace("KeyCode.", ""));
        keyBindingsFile.WriteLine("SideAttack: " + KeyBindings["SideAttack"].ToString().Replace("KeyCode.", ""));
        keyBindingsFile.WriteLine("DropEquippedWeapon: " + KeyBindings["DropEquippedWeapon"].ToString().Replace("KeyCode.", ""));

        Debug.Log("Successfully saved " + KeyBindings.Count + " key bindings!");
    }
    //load all key bindings from KeyBindings.txt if it exists,
    //otherwise load default values
    public void LoadKeyBindings()
    {
        string filePath = settingsPath + @"\KeyBindings.txt";

        //load keybindings from KeyBindings.txt
        if (File.Exists(filePath))
        {
            foreach (string line in File.ReadLines(filePath))
            {
                //split full line between :
                if (line.Contains(':'))
                {
                    string[] valueSplit = line.Split(':');
                    string type = valueSplit[0];
                    string value = valueSplit[1].Replace(" ", "");

                    bool foundCorrectKeycode = false;
                    foreach (string keycode in keycodes)
                    {
                        if (keycode == value)
                        {
                            foundCorrectKeycode = true;
                            break;
                        }
                    }
                    if (foundCorrectKeycode)
                    {
                        //load general key bindings
                        if (type == "Save"
                            || type == "Load"
                            || type == "TogglePauseMenu"
                            || type == "ToggleConsole"
                            || type == "TogglePlayerMenu"
                            || type == "PickUpOrInteract")
                        {
                            KeyBindings[type] = (KeyCode)Enum.Parse(typeof(KeyCode), value);
                        }
                        //load movement key bindings
                        else if (type == "WalkForwards"
                                 || type == "WalkBackwards"
                                 || type == "WalkLeft"
                                 || type == "WalkRight"
                                 || type == "Jump"
                                 || type == "Sprint"
                                 || type == "Crouch"
                                 || type == "ToggleFirstAndThirdPerson")
                        {
                            KeyBindings[type] = (KeyCode)Enum.Parse(typeof(KeyCode), value);
                        }
                        //load combat key bindings
                        else if (type == "CastSpell"
                                 || type == "MainAttack"
                                 || type == "SideAttack"
                                 || type == "DropEquippedWeapon")
                        {
                            KeyBindings[type] = (KeyCode)Enum.Parse(typeof(KeyCode), value);
                        }
                    }
                    else
                    {
                        Debug.LogError("Error: " + type + " name in key bindings file is invalid! Resetting to default value.");
                    }
                }
            }
            Debug.Log("Successfully loaded key bindings from " + GameManagerScript.settingsPath + @"\KeyBindings.txt" + "!");
        }
        else
        {
            ResetKeyBindings(true);
            Debug.Log("Loaded default key bindings.");
        }
    }

    //checks if key was pressed once
    public bool GetKeyDown(string buttonName)
    {
        if (!KeyBindings.ContainsKey(buttonName))
        {
            return false;
        }
        else
        {
            return Input.GetKeyDown(KeyBindings[buttonName]);
        }
    }
    //checks if key was held down
    public bool GetKey(string buttonName)
    {
        if (!KeyBindings.ContainsKey(buttonName))
        {
            return false;
        }
        else
        {
            return Input.GetKey(KeyBindings[buttonName]);
        }
    }
}