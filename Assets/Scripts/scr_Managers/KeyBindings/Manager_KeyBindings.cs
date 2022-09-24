using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

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
        GameObject targetKeyBindingsParent = null;
        List<GameObject> keyParents = null;
        if (targetKeyBindingParentName == "general")
        {
            targetKeyBindingsParent = par_GeneralKeyBindingsParent;
            keyParents = generalKeyParents;
        }
        else if (targetKeyBindingParentName == "movement")
        {
            targetKeyBindingsParent = par_MovementKeyBindingsParent;
            keyParents = movementKeyParents;
        }
        else if (targetKeyBindingParentName == "combat")
        {
            targetKeyBindingsParent = par_CombatKeyBindingsParent;
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
            string info = AssignScript.str_Info;

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
                    string buttonInfo = btn.GetComponentInChildren<UI_AssignKey>().str_Info;
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

        Debug.Log("Sucessfully assigned key " + pressedKey.ToString().Replace("KeyCode.", "") + " to action " + info + "!");
    }
    public void ResetKeyBindings(bool canReset)
    {
        if (!canReset)
        {
            //open the confirmation UI to let the player confirm
            //if they actually want to reset the key bindings or not
            ConfirmationScript.RecieveData(gameObject,
                                           "keyBindingsScript",
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
            KeyBindings["UseWeaponOrShootBow"] = KeyCode.Mouse0;
            KeyBindings["BlockOrAimBow"] = KeyCode.Mouse1;
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
        keyBindingsFile.WriteLine("WARNING: Invalid values will break the game - edit at your own risk!");
        keyBindingsFile.WriteLine("");

        keyBindingsFile.WriteLine("---GENERAL KEYBINDS---");
        keyBindingsFile.WriteLine("gkb_Save: " + KeyBindings["Save"].ToString().Replace("KeyCode.", ""));
        keyBindingsFile.WriteLine("gkb_Load: " + KeyBindings["Load"].ToString().Replace("KeyCode.", ""));
        keyBindingsFile.WriteLine("gkb_TogglePauseMenu: " + KeyBindings["TogglePauseMenu"].ToString().Replace("KeyCode.", ""));
        keyBindingsFile.WriteLine("gkb_ToggleConsole: " + KeyBindings["ToggleConsole"].ToString().Replace("KeyCode.", ""));
        keyBindingsFile.WriteLine("gkb_TogglePlayerMenu: " + KeyBindings["TogglePlayerMenu"].ToString().Replace("KeyCode.", ""));
        keyBindingsFile.WriteLine("gkb_PickUpOrInteract: " + KeyBindings["PickUpOrInteract"].ToString().Replace("KeyCode.", ""));
        keyBindingsFile.WriteLine("");

        keyBindingsFile.WriteLine("---MOVEMENT KEYBINDS---");
        keyBindingsFile.WriteLine("mkb_WalkForwards: " + KeyBindings["WalkForwards"].ToString().Replace("KeyCode.", ""));
        keyBindingsFile.WriteLine("mkb_WalkBackwards: " + KeyBindings["WalkBackwards"].ToString().Replace("KeyCode.", ""));
        keyBindingsFile.WriteLine("mkb_WalkLeft: " + KeyBindings["WalkLeft"].ToString().Replace("KeyCode.", ""));
        keyBindingsFile.WriteLine("mkb_WalkRight: " + KeyBindings["WalkRight"].ToString().Replace("KeyCode.", ""));
        keyBindingsFile.WriteLine("mkb_Jump: " + KeyBindings["Jump"].ToString().Replace("KeyCode.", ""));
        keyBindingsFile.WriteLine("mkb_Sprint: " + KeyBindings["Sprint"].ToString().Replace("KeyCode.", ""));
        keyBindingsFile.WriteLine("mkb_Crouch: " + KeyBindings["Crouch"].ToString().Replace("KeyCode.", ""));
        keyBindingsFile.WriteLine("mkb_ToggleFirstAndThirdPerson: " + KeyBindings["ToggleFirstAndThirdPerson"].ToString().Replace("KeyCode.", ""));
        keyBindingsFile.WriteLine("");

        keyBindingsFile.WriteLine("---COMBAT KEYBINDS---");
        keyBindingsFile.WriteLine("ckb_CastSpell: " + KeyBindings["CastSpell"].ToString().Replace("KeyCode.", ""));
        keyBindingsFile.WriteLine("ckb_UseWeaponOrShootBow: " + KeyBindings["UseWeaponOrShootBow"].ToString().Replace("KeyCode.", ""));
        keyBindingsFile.WriteLine("ckb_BlockOrAimBow: " + KeyBindings["BlockOrAimBow"].ToString().Replace("KeyCode.", ""));
        keyBindingsFile.WriteLine("ckb_DropEquippedWeapon: " + KeyBindings["DropEquippedWeapon"].ToString().Replace("KeyCode.", ""));

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
                if (line.Contains(':')
                    && line.Contains('_'))
                {
                    string[] valueSplit = line.Split(':');
                    string value = valueSplit[1];

                    //split type between _
                    string[] typeSplit = valueSplit[0].Split('_');
                    string type = typeSplit[0];
                    string typeName = typeSplit[1];

                    //load general key bindings
                    if (type == "gkb"
                        && (typeName == "Save"
                        || typeName == "Load"
                        || typeName == "TogglePauseMenu"
                        || typeName == "ToggleConsole"
                        || typeName == "TogglePlayerMenu"
                        || typeName == "PickUpOrInteract"))
                    {
                        KeyBindings[typeName] = (KeyCode)Enum.Parse(typeof(KeyCode), value);
                    }
                    //load movement key bindings
                    else if (type == "mkb"
                             && (typeName == "WalkForwards"
                             || typeName == "WalkBackwards"
                             || typeName == "WalkLeft"
                             || typeName == "WalkRight"
                             || typeName == "Jump"
                             || typeName == "Sprint"
                             || typeName == "Crouch"
                             || typeName == "ToggleFirstAndThirdPerson"))
                    {
                        KeyBindings[typeName] = (KeyCode)Enum.Parse(typeof(KeyCode), value);
                    }
                    //load combat key bindings
                    else if (type == "ckb"
                             && (typeName == "CastSpell"
                             || typeName == "UseWeaponOrShootBow"
                             || typeName == "BlockOrAimBow"
                             || typeName == "DropEquippedWeapon"))
                    {
                        KeyBindings[typeName] = (KeyCode)Enum.Parse(typeof(KeyCode), value);
                    }
                }
            }

            Debug.Log("Successfully loaded " + KeyBindings.Count + " key bindings!");
        }
        else
        {
            ResetKeyBindings(true);
        }
    }

    /*
    this method checks for all keypresses in the game 
    and does the actions that were requested 
    if the pressed key is assigned
    */
    public bool GetButtonDown(string buttonName)
    {
        if (!KeyBindings.ContainsKey(buttonName))
        {
            return false;
        }
        else
        {
            if (buttonName == "Sprint")
            {
                return Input.GetKey(KeyBindings[buttonName]);
            }
            else
            {
                return Input.GetKeyDown(KeyBindings[buttonName]);
            }
        }
    }
}