using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using static Manager_Settings;

//check if string only contains upper or lowercase english alphabet letters
//Regex.IsMatch("", @"^[a-zA-Z]+$")
//check if string only contains positive or negative non-decimal numbers
//Regex.IsMatch("", @"-?\d+")

public class Manager_Console : MonoBehaviour
{
    [Header("Scripts")]
    [SerializeField] private GameObject thePlayer;
    [SerializeField] private GameObject par_Managers;

    //public but hidden variables
    [HideInInspector] public bool canToggleConsole;

    //private variables
    private bool isConsoleOpen;
    private bool debugMenuEnabled;
    private bool startedConsoleSetupWait;
    private string input;
    private string output;
    private string lastOutput;
    private int currentSelectedInsertedCommand;
    private int currentScene;
    private readonly List<string> separatedWords = new();
    private readonly List<GameObject> createdTexts = new();
    private readonly List<string> insertedCommands = new();
    private readonly char[] letters = new char[]
    {
        'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 
        'j', 'k' , 'l', 'm', 'n', 'o', 'p', 'q', 'r', 
        's', 't', 'u', 'v', 'w', 'x', 'y', 'z'
    };

    //target selection
    private bool isSelectingTarget;
    private GameObject target;
    private Vector3 consoleHiddenPosition = new(0, -1460, 0);
    private Vector3 consoleVisiblePosition = new(0, 0, 0);

    //scripts
    private Player_Movement PlayerMovementScript;
    private UI_Inventory PlayerInventoryScript;
    private Player_Stats PlayerStatsScript;
    private UI_PlayerMenu PlayerMenuScript;
    private GameManager GameManagerScript;
    private UI_PauseMenu PauseMenuScript;
    private Manager_KeyBindings KeyBindingsScript;
    private Manager_Settings SettingsScript;
    private Manager_GameSaving GameSavingScript;
    private Manager_AllowedEffects AllowedEffectsScript;
    private Manager_DealEffect EffectManagerScript;
    private Manager_UIReuse UIReuseScript;

    //private variables
    public enum MessageType
    {
        CONSOLE_INFO_MESSAGE,
        CONSOLE_SUCCESS_MESSAGE,
        CONSOLE_ERROR_MESSAGE,
        CONSOLE_COMMAND,
        UNITY_LOG_MESSAGE,
        UNITY_LOG_ERROR,
        INVALID_FILE_VARIABLE_VALUE,
        UNAUTHORIZED_PLAYER_ACTION
    }

    private void Awake()
    {
        GameManagerScript = GetComponent<GameManager>();
        PauseMenuScript = GetComponent<UI_PauseMenu>();
        KeyBindingsScript = GetComponent<Manager_KeyBindings>();
        SettingsScript = GetComponent<Manager_Settings>();
        GameSavingScript = GetComponent<Manager_GameSaving>();
        UIReuseScript = GetComponent<Manager_UIReuse>();

        currentScene = SceneManager.GetActiveScene().buildIndex;

        //console is always enabled at the beginning in main menu
        if (currentScene == 0)
        {
            canToggleConsole = true;
        }
        else if (currentScene == 1)
        {
            PlayerMovementScript = thePlayer.GetComponent<Player_Movement>();
            PlayerInventoryScript = thePlayer.GetComponent<UI_Inventory>();
            PlayerStatsScript = thePlayer.GetComponent<Player_Stats>();
            PlayerMenuScript = GetComponent<UI_PlayerMenu>();
            AllowedEffectsScript = GetComponent<Manager_AllowedEffects>();
            EffectManagerScript = GetComponent<Manager_DealEffect>();
        }

        debugMenuEnabled = true;
    }

    private void Start()
    {
        CreateNewConsoleLine("Loaded scene " + SceneManager.GetActiveScene().name + " (" + currentScene + ")", MessageType.CONSOLE_INFO_MESSAGE.ToString());
        CreateNewConsoleLine("Game version " + UIReuseScript.txt_GameVersion.text, MessageType.CONSOLE_INFO_MESSAGE.ToString());
    }

    private void Update()
    {
        if (KeyBindingsScript.GetKeyDown("ToggleConsole")
            && canToggleConsole)
        {
            if (currentScene == 0)
            {
                isConsoleOpen = !isConsoleOpen;
            }
            else if (currentScene == 1
                     && !PauseMenuScript.isKeyAssignUIOpen)
            {
                PauseMenuScript.isConsoleOpen = !PauseMenuScript.isConsoleOpen;
            }
        }

        if (!UIReuseScript.par_Console.activeInHierarchy)
        {
            if ((currentScene == 0
                && isConsoleOpen)
                || currentScene == 1
                && PauseMenuScript.isConsoleOpen)
            {
                if (currentScene == 1)
                {
                    PauseMenuScript.PauseWithoutUI();
                }
                OpenConsole();
            }
        }
        else if (UIReuseScript.par_Console.activeInHierarchy)
        {
            if ((currentScene == 0
                 && !isConsoleOpen)
                 || currentScene == 1
                 && !PauseMenuScript.isConsoleOpen)
            {
                if (currentScene == 1)
                {
                    if (isSelectingTarget)
                    {
                        CreateNewConsoleLine("Cancelled target selection command.", MessageType.CONSOLE_INFO_MESSAGE.ToString());
                        UIReuseScript.par_Console.transform.localPosition = consoleVisiblePosition;
                        isSelectingTarget = false;
                    }

                    PauseMenuScript.UnpauseGame();
                }
                CloseConsole();
            }
        }

        //choose newer and older inserted commands with arrow keys
        if (insertedCommands.Count > 0)
        {
            if ((currentScene == 0
                && isConsoleOpen)
                || (currentScene == 1
                && PauseMenuScript.isConsoleOpen))
            {
                //always picks newest added console command if input field is empty
                if ((Input.GetKeyDown(KeyCode.UpArrow)
                    || Input.GetKeyDown(KeyCode.DownArrow))
                    && UIReuseScript.consoleInputField.text == "")
                {
                    if (isSelectingTarget)
                    {
                        CreateNewConsoleLine("Cancelled target selection command.", MessageType.CONSOLE_INFO_MESSAGE.ToString());
                        UIReuseScript.par_Console.transform.localPosition = consoleVisiblePosition;
                        isSelectingTarget = false;
                    }

                    currentSelectedInsertedCommand = insertedCommands.Count - 1;
                    UIReuseScript.consoleInputField.text = insertedCommands[^1];
                    UIReuseScript.consoleInputField.MoveToEndOfLine(false, false);
                }
                else
                {
                    if (Input.GetKeyDown(KeyCode.UpArrow))
                    {
                        if (isSelectingTarget)
                        {
                            CreateNewConsoleLine("Cancelled target selection command.", MessageType.CONSOLE_INFO_MESSAGE.ToString());
                            UIReuseScript.par_Console.transform.localPosition = consoleVisiblePosition;
                            isSelectingTarget = false;
                        }

                        currentSelectedInsertedCommand--;
                        if (currentSelectedInsertedCommand < 0)
                        {
                            currentSelectedInsertedCommand = insertedCommands.Count - 1;
                        }

                        UIReuseScript.consoleInputField.text = insertedCommands[currentSelectedInsertedCommand];
                        UIReuseScript.consoleInputField.MoveToEndOfLine(false, false);
                    }
                    else if (Input.GetKeyDown(KeyCode.DownArrow))
                    {
                        if (isSelectingTarget)
                        {
                            CreateNewConsoleLine("Cancelled target selection command.", MessageType.CONSOLE_INFO_MESSAGE.ToString());
                            UIReuseScript.par_Console.transform.localPosition = consoleVisiblePosition;
                            isSelectingTarget = false;
                        }

                        currentSelectedInsertedCommand++;
                        if (currentSelectedInsertedCommand == insertedCommands.Count)
                        {
                            currentSelectedInsertedCommand = 0;
                        }

                        UIReuseScript.consoleInputField.text = insertedCommands[currentSelectedInsertedCommand];
                        UIReuseScript.consoleInputField.MoveToEndOfLine(false, false);
                    }
                }
            }
        }

        //if target is being selected
        if (isSelectingTarget)
        {
            if (UIReuseScript.par_Console.transform.localPosition != consoleHiddenPosition)
            {
                UIReuseScript.par_Console.transform.localPosition = consoleHiddenPosition;
            }

            //pressing left mouse button selects a target
            if (Input.GetMouseButtonDown(0))
            {
                bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hitInfo);
                if (hit)
                {
                    if (hitInfo.transform.GetComponent<Env_Item>() != null
                        || hitInfo.transform.GetComponent<UI_Inventory>() != null
                        || hitInfo.transform.GetComponent<Env_Door>() != null)
                    {
                        CreateNewConsoleLine("Hit " + hitInfo.transform.name + ".", MessageType.CONSOLE_INFO_MESSAGE.ToString());
                        target = hitInfo.transform.gameObject;
                    }
                    else
                    {
                        CreateNewConsoleLine("Did not hit anything interactable...", MessageType.CONSOLE_INFO_MESSAGE.ToString());
                    }

                    UIReuseScript.par_Console.transform.localPosition = consoleVisiblePosition;

                    isSelectingTarget = false;
                }
                else
                {
                    CreateNewConsoleLine("Cancelled target selection command.", MessageType.CONSOLE_INFO_MESSAGE.ToString());
                    UIReuseScript.par_Console.transform.localPosition = consoleVisiblePosition;
                    isSelectingTarget = false;
                }
            }
        }
    }

    private void OpenConsole()
    {
        UIReuseScript.par_Console.SetActive(true);
        UIReuseScript.consoleInputField.ActivateInputField();
    }
    private void CloseConsole()
    {
        UIReuseScript.par_Console.SetActive(false);
    }

    private void CheckInsertedText()
    {
        //splits each word as its own and adds to separated words list
        foreach (string word in input.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            separatedWords.Add(word);
        }

        insertedCommands.Add(input);
        currentSelectedInsertedCommand = insertedCommands.Count - 1;
        CreateNewConsoleLine("--" + input + "--", MessageType.CONSOLE_COMMAND.ToString());

        //if inserted text was not empty and player pressed enter
        if (separatedWords.Count >= 1)
        {
            bool isInt = int.TryParse(separatedWords[0], out _);
            if (isInt)
            {
                CreateNewConsoleLine("Error: Console command cannot start with a number!", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
            }
            else
            {
                //clear console log
                if (separatedWords[0] == "clear"
                         && separatedWords.Count == 1)
                {
                    Command_ClearConsole();
                }
                //toggle debug menu
                else if (separatedWords[0] == "tdm"
                         && separatedWords.Count == 1)
                {
                    Command_ToggleDebugMenu();
                }

                //toggle godmode
                else if (separatedWords[0] == "tgm"
                         && separatedWords.Count == 1
                         && currentScene == 1
                         && PlayerStatsScript.currentHealth > 0)
                {
                    Command_ToggleGodMode();
                }
                //toggle noclip
                else if (separatedWords[0] == "tnc"
                         && separatedWords.Count == 1
                         && currentScene == 1)
                {
                    Command_ToggleNoclip();
                }

                //show all saves
                else if (separatedWords[0] == "showsaves"
                         && separatedWords.Count == 1)
                {
                    Command_ShowSaves();
                }
                //delete all saves
                else if (separatedWords[0] == "deletesaves"
                         && separatedWords.Count == 1)
                {
                    Command_DeleteSaves();
                }
                //save with save name
                else if (separatedWords[0] == "save"
                         && separatedWords.Count == 2
                         && currentScene == 1
                         && PlayerStatsScript.currentHealth > 0)
                {
                    Command_SaveWithName();
                }
                //load with save file name
                else if (separatedWords[0] == "load"
                         && separatedWords.Count == 2)
                {
                    Command_LoadWithName();
                }

                //reset all key bindings to default values
                else if (separatedWords[0] == "resetkeybinds"
                         && separatedWords.Count == 1)
                {
                    Command_ResetKeyBinds();
                }
                //list all key bindings and their values
                else if (separatedWords[0] == "showkeybinds"
                         && separatedWords.Count == 1)
                {
                    Command_ShowKeyBinds();
                }
                //set keybindingname to keybindingvalue
                else if (separatedWords[0] == "setkeybind"
                         && separatedWords.Count == 3)
                {
                    Command_SetKeyBind();
                }

                //reset all settings to default values
                else if (separatedWords[0] == "resetsettings"
                         && separatedWords.Count == 1
                         && currentScene == 1)
                {
                    Command_ResetSettings();
                }
                //list all settings and their values
                else if (separatedWords[0] == "showsettings"
                         && separatedWords.Count == 1
                         && currentScene == 1)
                {
                    Command_ShowSettings();
                }
                //set settingsname to settingsvalue
                else if (separatedWords[0] == "setsetting"
                         && separatedWords.Count == 3
                         && currentScene == 1)
                {
                    Command_SetSetting();
                }

                //restart game from the beginning
                else if (separatedWords[0] == "restart" 
                         && separatedWords.Count == 1 
                         && currentScene == 1)
                {
                    Command_Restart();
                }
                //quit game
                else if (separatedWords[0] == "quit"
                         && separatedWords.Count == 1)
                {
                    Command_Quit();
                }

                //player commands
                else if (separatedWords[0] == "player"
                         && currentScene == 1)
                {
                    //teleport player
                    if (separatedWords[1] == "tp"
                        && separatedWords.Count == 5
                        && PlayerStatsScript.currentHealth > 0)
                    {
                        Command_TeleportPlayer();
                    }

                    //show player stats
                    else if (separatedWords[1] == "showstats"
                             && separatedWords.Count == 2)
                    {
                        Command_ShowPlayerStats();
                    }
                    //reset player stats
                    else if (separatedWords[1] == "resetstats"
                             && separatedWords.Count == 2
                             && PlayerStatsScript.currentHealth > 0)
                    {
                        Command_ResetPlayerStats();
                    }
                    //set player stat
                    else if (separatedWords[1] == "setstat"
                             && separatedWords.Count == 4
                             && PlayerStatsScript.currentHealth > 0)
                    {
                        Command_SetPlayerStat();
                    }

                    //list all item types
                    else if (separatedWords[1] == "showitemtypes"
                             && separatedWords.Count == 2)
                    {
                        Command_ShowItemTypes();
                    }
                    //list all items of the selected item type that start with the selected english alphabet letter
                    else if (separatedWords.Count == 3
                             && separatedWords[2].Length == 1
                             && (separatedWords[1] == "shwe"
                             || separatedWords[1] == "shar"
                             || separatedWords[1] == "shsh"
                             || separatedWords[1] == "shco"
                             || separatedWords[1] == "shai"
                             || separatedWords[1] == "shsp"
                             || separatedWords[1] == "sham"
                             || separatedWords[1] == "shmi"))
                    {
                        char letter = separatedWords[2].ToCharArray()[0];
                        bool foundLetter = false;
                        foreach (char c in letters)
                        {
                            if (letter == c)
                            {
                                foundLetter = true;
                                break;
                            }
                        }

                        if (foundLetter)
                        {
                            Command_ShowItemTypeItems(separatedWords[1], letter);
                        }
                        else
                        {
                            CreateNewConsoleLine("Error: Inserted item type first letter is invalid! It must be a lower-case english alphabet letter.", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
                        }
                    }
                    //list all player items, their counts and protected state
                    else if (separatedWords[1] == "showinventoryitems"
                             && separatedWords.Count == 2)
                    {
                        Command_ShowAllPlayerItems();
                    }
                    //add item to player inventory
                    else if (separatedWords[1] == "additem"
                             && separatedWords.Count == 4
                             && PlayerStatsScript.currentHealth > 0)
                    {
                        Command_AddItem();
                    }
                    //remove item from player inventory
                    else if (separatedWords[1] == "removeitem"
                             && separatedWords.Count == 4
                             && PlayerStatsScript.currentHealth > 0)
                    {
                        Command_RemoveItem();
                    }
                    //show all item stats
                    else if (separatedWords[1] == "showitemstats"
                             && separatedWords.Count == 4)
                    {
                        Command_ShowItemStats();
                    }
                    //set item stat
                    else if (separatedWords[1] == "setitemstat"
                             && separatedWords.Count == 6
                             && PlayerStatsScript.currentHealth > 0)
                    {
                        FindItem("setItemStat");
                    }
                    //show all item enchantments
                    else if (separatedWords[1] == "showitemenchantments"
                             && separatedWords.Count == 4)
                    {
                        FindItem("showItemEnchantments");
                    }
                    //add new item enchantment
                    else if (separatedWords[1] == "additemenchantment"
                             && separatedWords.Count == 7
                             && PlayerStatsScript.currentHealth > 0)
                    {
                        FindItem("addItemEnchantment");
                    }
                    //remove existing item enchantment
                    else if (separatedWords[1] == "removeitemenchantment"
                             && separatedWords.Count == 5
                             && PlayerStatsScript.currentHealth > 0)
                    {
                        FindItem("removeItemEnchantment");
                    }

                    //show effects that can be added to plauer
                    else if (separatedWords[1] == "showalleffects"
                             && separatedWords.Count == 2)
                    {
                        Command_ShowAllEffects();
                    }
                    //show effects that are active on player
                    else if (separatedWords[1] == "showactiveeffects"
                             && separatedWords.Count == 2)
                    {
                        Command_ShowActiveEffects();
                    }
                    //add effect with value and duration to player
                    else if (separatedWords[1] == "addeffect"
                             && separatedWords.Count == 5
                             && PlayerStatsScript.currentHealth > 0)
                    {
                        FindEffect("addPlayerEffect", null);
                    }
                    //remove effect from player
                    else if (separatedWords[1] == "removeeffect"
                             && separatedWords.Count == 3
                             && PlayerStatsScript.currentHealth > 0)
                    {
                        FindEffect("removePlayerEffect", null);
                    }

                    else
                    {
                        insertedCommands.Add(input);
                        currentSelectedInsertedCommand = insertedCommands.Count - 1;

                        CreateNewConsoleLine("Error: Unknown or invalid command or this command is not allowed in this scene or while player is dead!", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
                    }
                }

                //select target
                else if (separatedWords[0] == "st"
                         && separatedWords.Count == 1
                         && currentScene == 1)
                {
                    Command_SelectTarget();
                }
                //target commands
                else if (separatedWords[0] == "target"
                         && currentScene == 1
                         && PlayerStatsScript.currentHealth > 0)
                {
                    //unlock target
                    if (separatedWords[1] == "unlock"
                             && separatedWords.Count == 2)
                    {
                        Command_UnlockTarget();
                    }
                    else
                    {
                        insertedCommands.Add(input);
                        currentSelectedInsertedCommand = insertedCommands.Count - 1;

                        CreateNewConsoleLine("Error: Unknown or invalid command or this command is not allowed in this scene or while player is dead!", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
                    }
                }

                else
                {
                    insertedCommands.Add(input);
                    currentSelectedInsertedCommand = insertedCommands.Count - 1;

                    CreateNewConsoleLine("Error: Unknown or invalid command or this command is not allowed in this scene or while player is dead!", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
                }
            }
        }
        else
        {
            insertedCommands.Add(input);
            currentSelectedInsertedCommand = insertedCommands.Count - 1;

            CreateNewConsoleLine("Error: No command was inserted!", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
        }

        separatedWords.Clear();

        input = "";
    }

    //clear console log
    private void Command_ClearConsole()
    {
        separatedWords.Clear();

        lastOutput = "";

        foreach (GameObject createdText in createdTexts)
        {
            Destroy(createdText);
        }
        createdTexts.Clear();
    }
    //toggle debug menu
    public void Command_ToggleDebugMenu()
    {
        if (!debugMenuEnabled)
        {
            UIReuseScript.par_DebugMenu.transform.position -= new Vector3(0, 114, 0);

            CreateNewConsoleLine("Enabled debug menu.", MessageType.CONSOLE_INFO_MESSAGE.ToString());
            debugMenuEnabled = true;
        }
        else
        {
            UIReuseScript.par_DebugMenu.transform.position += new Vector3(0, 114, 0);

            CreateNewConsoleLine("Disabled debug menu.", MessageType.CONSOLE_INFO_MESSAGE.ToString());
            debugMenuEnabled = false;
        }
    }

    //toggle godmode
    private void Command_ToggleGodMode()
    {
        PlayerStatsScript.isGodmodeEnabled = !PlayerStatsScript.isGodmodeEnabled;
        if (!PlayerStatsScript.isGodmodeEnabled)
        {
            CreateNewConsoleLine("Disabled god mode.", MessageType.CONSOLE_INFO_MESSAGE.ToString());
        }
        else
        {
            CreateNewConsoleLine("Enabled god mode.", MessageType.CONSOLE_INFO_MESSAGE.ToString());
        }
    }
    //toggle noclip
    private void Command_ToggleNoclip()
    {
        PlayerMovementScript.isNoclipping = !PlayerMovementScript.isNoclipping;
        if (!PlayerMovementScript.isNoclipping)
        {
            CreateNewConsoleLine("Disabled noclipping.", MessageType.CONSOLE_INFO_MESSAGE.ToString());
        }
        else
        {
            CreateNewConsoleLine("Enabled noclipping.", MessageType.CONSOLE_INFO_MESSAGE.ToString());
        }
    }

    //list all game saves
    private void Command_ShowSaves()
    {
        //default game saves path
        string path = GameManagerScript.savePath;

        //if a directory was found at the path
        if (Directory.Exists(path))
        {
            //save all save files to 
            string[] saves = Directory.GetFiles(path);

            //if any saves exist at path
            if (saves.Length > 0)
            {
                //display all save names without .txt
                foreach (string save in saves)
                {
                    if (save.Contains(".txt"))
                    {
                        string saveName = Path.GetFileName(save);
                        CreateNewConsoleLine(saveName.Replace(".txt", ""), MessageType.CONSOLE_INFO_MESSAGE.ToString());
                    }
                }
            }
            else
            {
                CreateNewConsoleLine("Error: Cannot list any saves because save folder at path " + path + " is empty!", MessageType.CONSOLE_INFO_MESSAGE.ToString());
            }
        }
        else
        {
            CreateNewConsoleLine("Error: Cannot find game saves folder!", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
        }
    }
    //delete all game saves
    private void Command_DeleteSaves()
    {
        string path = GameManagerScript.savePath;
        DirectoryInfo di = new(path);

        //deletes all save files
        if (Directory.GetFiles(path).Length > 0)
        {
            foreach (FileInfo file in di.EnumerateFiles())
            {
                file.Delete();
            }

            CreateNewConsoleLine("Successfully deleted all save files from " + path + "!", MessageType.CONSOLE_SUCCESS_MESSAGE.ToString());
        }
        else
        {
            CreateNewConsoleLine("Error: " + path + " has no save files to delete!", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
        }
    }
    //save game with name
    private void Command_SaveWithName()
    {
        //save the potential save name
        string saveName = separatedWords[1];
        GameSavingScript.CreateSaveFile(saveName);
    }
    //load game save with name
    private void Command_LoadWithName()
    {
        string path = GameManagerScript.savePath;
        if (File.Exists(path + @"\" + separatedWords[1] + ".txt"))
        {
            GameSavingScript.CreateLoadFile(separatedWords[1] + ".txt");
        }
        else
        {
            CreateNewConsoleLine("Error: Save file " + separatedWords[1] + " does not exist!", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
        }
    }

    //reset all key bindings to default values
    private void Command_ResetKeyBinds()
    {
        KeyBindingsScript.ResetKeyBindings(true);
        CreateNewConsoleLine("Successfully reset " + KeyBindingsScript.KeyBindings.Count + " key bindings!", MessageType.CONSOLE_SUCCESS_MESSAGE.ToString());
    }
    //list all key bindings and their values
    private void Command_ShowKeyBinds()
    {
        CreateNewConsoleLine("---KEY BINDINGS---", MessageType.CONSOLE_INFO_MESSAGE.ToString());
        foreach (KeyValuePair<string, KeyCode> dict in KeyBindingsScript.KeyBindings)
        {
            string key = dict.Key;
            KeyCode value = dict.Value;
            CreateNewConsoleLine(key + ": " + value.ToString().Replace("KeyCode.", ""), MessageType.CONSOLE_INFO_MESSAGE.ToString());
        }
    }
    //set keybindingname to keybindingvalue
    private void Command_SetKeyBind()
    {
        string userKey = separatedWords[1];
        KeyCode userValue = (KeyCode)Enum.Parse(typeof(KeyCode), separatedWords[2]);

        bool foundCorrectValue = false;

        foreach (KeyValuePair<string, KeyCode> dict in KeyBindingsScript.KeyBindings)
        {
            string key = dict.Key;
            if (userKey == key)
            {
                userKey = key;
                foundCorrectValue = true;
                break;
            }
        }

        if (foundCorrectValue)
        {
            KeyBindingsScript.KeyBindings[userKey] = userValue;

            List<GameObject> allKeyParents = new();
            foreach (GameObject buttonParent in KeyBindingsScript.generalKeyParents)
            {
                allKeyParents.Add(buttonParent);
            }
            foreach (GameObject buttonParent in KeyBindingsScript.movementKeyParents)
            {
                allKeyParents.Add(buttonParent);
            }
            foreach (GameObject buttonParent in KeyBindingsScript.combatKeyParents)
            {
                allKeyParents.Add(buttonParent);
            }

            foreach (GameObject buttonParent in allKeyParents)
            {
                Button button = buttonParent.GetComponentInChildren<Button>();
                TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
                UI_AssignKey AssignScript = button.GetComponentInChildren<UI_AssignKey>();

                if (AssignScript.info == userKey)
                {
                    buttonText.text = userValue.ToString().Replace("KeyCode.", "");

                    if (KeyBindingsScript.generalKeyParents.Contains(buttonParent))
                    {
                        KeyBindingsScript.RebuildKeyBindingsList("general");
                    }
                    else if (KeyBindingsScript.movementKeyParents.Contains(buttonParent))
                    {
                        KeyBindingsScript.RebuildKeyBindingsList("movement");
                    }
                    else if (KeyBindingsScript.combatKeyParents.Contains(buttonParent))
                    {
                        KeyBindingsScript.RebuildKeyBindingsList("combat");
                    }

                    CreateNewConsoleLine("Successfully set " + userKey + "s keycode to " + userValue + "!", MessageType.CONSOLE_SUCCESS_MESSAGE.ToString());
                    break;
                }
            }
        }
        else
        {
            CreateNewConsoleLine("Error: Inserted key binding name " + userKey + " or key binding value " + userValue.ToString().Replace("KeyCode.", "") + " does not exist! Type showkeybindings to list all key bindings.", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
        }
    }

    //reset all settings to default values
    private void Command_ResetSettings()
    {
        SettingsScript.ResetSettings(true);
        CreateNewConsoleLine("Successfully reset all settings to default values!", MessageType.CONSOLE_SUCCESS_MESSAGE.ToString());
    }
    //list all settings and their values
    private void Command_ShowSettings()
    {
        List<Transform> parents = new();
        foreach (GameObject par in UIReuseScript.generalSettingsParents)
        {
            parents.Add(par.transform);
        }
        foreach (GameObject par in UIReuseScript.graphicsSettingsParents)
        {
            parents.Add(par.transform);
        }
        foreach (GameObject par in UIReuseScript.audioSettingsParents)
        {
            parents.Add(par.transform);
        }

        foreach (Transform par in parents)
        {
            string info = par.GetComponentInChildren<UI_AssignSettings>().info;

            //general settings
            if (info == "Difficulty")
            {
                CreateNewConsoleLine("difficulty: " + SettingsScript.user_Difficulty, MessageType.CONSOLE_INFO_MESSAGE.ToString());
            }
            else if (info == "MouseSpeed")
            {
                CreateNewConsoleLine("mouse_speed: " + SettingsScript.user_MouseSpeed, MessageType.CONSOLE_INFO_MESSAGE.ToString());
            }

            //graphics settings
            else if (info == "Preset")
            {
                CreateNewConsoleLine("preset: " + SettingsScript.user_Preset, MessageType.CONSOLE_INFO_MESSAGE.ToString());
            }
            else if (info == "Resolution")
            {
                CreateNewConsoleLine("resolution: " + SettingsScript.user_Resolution.ToString().Replace("res_", ""), MessageType.CONSOLE_INFO_MESSAGE.ToString());
            }
            else if (info == "FullScreenMode")
            {
                CreateNewConsoleLine("fullscreen_mode: " + SettingsScript.user_FullScreenMode, MessageType.CONSOLE_INFO_MESSAGE.ToString());
            }
            else if (info == "FieldOfView")
            {
                CreateNewConsoleLine("field_of_view: " + SettingsScript.user_FieldOfView, MessageType.CONSOLE_INFO_MESSAGE.ToString());
            }
            else if (info == "VSyncState")
            {
                CreateNewConsoleLine("vsync_state: " + SettingsScript.user_EnableVSync, MessageType.CONSOLE_INFO_MESSAGE.ToString());
            }
            else if (info == "TextureQuality")
            {
                CreateNewConsoleLine("texture_quality: " + SettingsScript.user_TextureQuality, MessageType.CONSOLE_INFO_MESSAGE.ToString());
            }
            else if (info == "LightDistance")
            {
                CreateNewConsoleLine("light_distance: " + SettingsScript.user_LightDistance, MessageType.CONSOLE_INFO_MESSAGE.ToString());
            }
            else if (info == "ShadowDistance")
            {
                CreateNewConsoleLine("shadow_distance: " + SettingsScript.user_ShadowDistance, MessageType.CONSOLE_INFO_MESSAGE.ToString());
            }
            else if (info == "ShadowQuality")
            {
                CreateNewConsoleLine("shadow_quality: " + SettingsScript.user_ShadowDistance, MessageType.CONSOLE_INFO_MESSAGE.ToString());
            }
            else if (info == "TreeDistance")
            {
                CreateNewConsoleLine("tree_distance: " + SettingsScript.user_TreeDistance, MessageType.CONSOLE_INFO_MESSAGE.ToString());
            }
            else if (info == "GrassDistance")
            {
                CreateNewConsoleLine("grass_distance: " + SettingsScript.user_GrassDistance, MessageType.CONSOLE_INFO_MESSAGE.ToString());
            }
            else if (info == "ObjectDistance")
            {
                CreateNewConsoleLine("object_distance: " + SettingsScript.user_ObjectDistance, MessageType.CONSOLE_INFO_MESSAGE.ToString());
            }
            else if (info == "item_Distance")
            {
                CreateNewConsoleLine("item_distance: " + SettingsScript.user_ItemDistance, MessageType.CONSOLE_INFO_MESSAGE.ToString());
            }
            else if (info == "AIDistance")
            {
                CreateNewConsoleLine("ai_distance: " + SettingsScript.user_AIDistance, MessageType.CONSOLE_INFO_MESSAGE.ToString());
            }

            //audio settings
            else if (info == "MasterVolume")
            {
                CreateNewConsoleLine("master_volume: " + SettingsScript.user_MasterVolume, MessageType.CONSOLE_INFO_MESSAGE.ToString());
            }
            else if (info == "MusicVolume")
            {
                CreateNewConsoleLine("music_volume: " + SettingsScript.user_MusicVolume, MessageType.CONSOLE_INFO_MESSAGE.ToString());
            }
            else if (info == "SFXVolume")
            {
                CreateNewConsoleLine("sfx_volume: " + SettingsScript.user_SFXVolume, MessageType.CONSOLE_INFO_MESSAGE.ToString());
            }
            else if (info == "NPCVolume")
            {
                CreateNewConsoleLine("npc_volume: " + SettingsScript.user_NPCVolume, MessageType.CONSOLE_INFO_MESSAGE.ToString());
            }
        }
    }
    //set settingsname to settingsvalue
    private void Command_SetSetting()
    {
        string type = separatedWords[1];
        string value = separatedWords[2];

        if (!Regex.IsMatch(type, @"^[a-zA-Z]+$"))
        {
            CreateNewConsoleLine("Error: Setting name " + type + " cannot contain any numbers or symbols!", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
        }
        else
        {
            if (!Regex.IsMatch(value, @"-?\d+"))
            {
                CreateNewConsoleLine("Error: Setting value must be an integer!", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
            }
            else
            {
                int valueInt = int.Parse(value);

                //general settings
                if (type == "difficulty")
                {
                    if (valueInt < -100
                        || valueInt > 100)
                    {
                        CreateNewConsoleLine("Error: " + type + " is out of range! Must be between -100 and 100.", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
                    }
                    else
                    {
                        SettingsScript.user_Difficulty = valueInt;

                        PauseMenuScript.PauseWithUI();
                        PauseMenuScript.ShowSettingsContent();
                        SettingsScript.RebuildSettingsList("general");
                        PauseMenuScript.UnpauseGame();

                        SettingsScript.SaveSettings();
                        CreateNewConsoleLine("Successfully changed " + type + " to " + value + "!", MessageType.CONSOLE_SUCCESS_MESSAGE.ToString());
                    }
                }
                else if (type == "mouse_speed")
                {
                    if (valueInt < 1
                        || valueInt > 100)
                    {
                        CreateNewConsoleLine("Error: Invalid value for " + type + "! Must be between 1 and 100.", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
                    }
                    else
                    {
                        SettingsScript.user_MouseSpeed = valueInt;

                        PauseMenuScript.PauseWithUI();
                        PauseMenuScript.ShowSettingsContent();
                        SettingsScript.RebuildSettingsList("general");
                        PauseMenuScript.UnpauseGame();

                        SettingsScript.SaveSettings();
                        CreateNewConsoleLine("Successfully changed " + type + " to " + value + "!", MessageType.CONSOLE_SUCCESS_MESSAGE.ToString());
                    }
                }

                //graphics settings
                else if (type == "preset")
                {
                    List<string> values = new(Enum.GetNames(typeof(UserDefined_Preset)));
                    values.Remove("custom");

                    bool foundAvailableValue = false;
                    foreach (string val in values)
                    {
                        if (value == val)
                        {
                            foundAvailableValue = true;
                            break;
                        }
                    }

                    if (!foundAvailableValue)
                    {
                        CreateNewConsoleLine("Error: Invalid value for " + type + "! Must be low, medium, high or ultra.", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
                    }
                    else
                    {
                        SettingsScript.user_Preset = (UserDefined_Preset)Enum.Parse(typeof(UserDefined_Preset), value);

                        PauseMenuScript.PauseWithUI();
                        PauseMenuScript.ShowSettingsContent();
                        SettingsScript.RebuildSettingsList("graphics");
                        PauseMenuScript.UnpauseGame();

                        SettingsScript.SaveSettings();
                        CreateNewConsoleLine("Successfully changed " + type + " to " + value + "!", MessageType.CONSOLE_SUCCESS_MESSAGE.ToString());
                    }
                }
                else if (type == "resolution")
                {
                    List<string> values = new(Enum.GetNames(typeof(UserDefined_Resolution)));
                    foreach (string val in values)
                    {
                        val.Replace("res_", "");
                    }

                    bool foundAvailableValue = false;
                    foreach (string val in values)
                    {
                        if ("res_" + value == val)
                        {
                            foundAvailableValue = true;
                            break;
                        }
                    }

                    if (!foundAvailableValue)
                    {
                        CreateNewConsoleLine("Error: Invalid value for " + type + "!", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
                    }
                    else
                    {
                        SettingsScript.user_Resolution = (UserDefined_Resolution)Enum.Parse(typeof(UserDefined_Resolution), "res_" + value);

                        PauseMenuScript.PauseWithUI();
                        PauseMenuScript.ShowSettingsContent();
                        SettingsScript.RebuildSettingsList("graphics");
                        PauseMenuScript.UnpauseGame();

                        SettingsScript.SaveSettings();
                        CreateNewConsoleLine("Successfully changed " + type + " to " + value + "!", MessageType.CONSOLE_SUCCESS_MESSAGE.ToString());
                    }
                }
                else if (type == "fullscreen_mode")
                {
                    List<string> values = new(Enum.GetNames(typeof(UserDefined_FullScreenMode)));

                    bool foundAvailableValue = false;
                    foreach (string val in values)
                    {
                        if (value == val)
                        {
                            foundAvailableValue = true;
                            break;
                        }
                    }

                    if (!foundAvailableValue)
                    {
                        CreateNewConsoleLine("Error: Invalid value for " + type + "!", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
                    }
                    else
                    {
                        SettingsScript.user_Resolution = (UserDefined_Resolution)Enum.Parse(typeof(UserDefined_Resolution), value);

                        PauseMenuScript.PauseWithUI();
                        PauseMenuScript.ShowSettingsContent();
                        SettingsScript.RebuildSettingsList("graphics");
                        PauseMenuScript.UnpauseGame();

                        SettingsScript.SaveSettings();
                        CreateNewConsoleLine("Successfully changed " + type + " to " + value + "!", MessageType.CONSOLE_SUCCESS_MESSAGE.ToString());
                    }
                }
                else if (type == "field_of_view")
                {
                    if (valueInt < 70
                        || valueInt > 110)
                    {
                        CreateNewConsoleLine("Error: Invalid value for " + type + "! Must be between 70 and 110.", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
                    }
                    else
                    {
                        SettingsScript.user_FieldOfView = valueInt;

                        PauseMenuScript.PauseWithUI();
                        PauseMenuScript.ShowSettingsContent();
                        SettingsScript.RebuildSettingsList("graphics");
                        PauseMenuScript.UnpauseGame();

                        SettingsScript.SaveSettings();
                        CreateNewConsoleLine("Successfully changed " + type + " to " + value + "!", MessageType.CONSOLE_SUCCESS_MESSAGE.ToString());
                    }
                }
                else if (type == "vsync_state")
                {
                    if (!bool.TryParse(value, out _))
                    {
                        CreateNewConsoleLine("Error: Invalid value for " + type + "! Must be true or false.", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
                    }
                    else
                    {
                        SettingsScript.user_EnableVSync = value;

                        PauseMenuScript.PauseWithUI();
                        PauseMenuScript.ShowSettingsContent();
                        SettingsScript.RebuildSettingsList("graphics");
                        PauseMenuScript.UnpauseGame();

                        SettingsScript.SaveSettings();
                        CreateNewConsoleLine("Successfully changed " + type + " to " + value + "!", MessageType.CONSOLE_SUCCESS_MESSAGE.ToString());
                    }
                }
                else if (type == "texture_quality")
                {
                    List<string> values = new(Enum.GetNames(typeof(UserDefined_TextureQuality)));

                    bool foundAvailableValue = false;
                    foreach (string val in values)
                    {
                        if (value == val)
                        {
                            foundAvailableValue = true;
                            break;
                        }
                    }

                    if (!foundAvailableValue)
                    {
                        CreateNewConsoleLine("Error: Invalid value for " + type + "! Must be low, medium, high or ultra.", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
                    }
                    else
                    {
                        SettingsScript.user_TextureQuality = (UserDefined_TextureQuality)Enum.Parse(typeof(UserDefined_TextureQuality), value);

                        PauseMenuScript.PauseWithUI();
                        PauseMenuScript.ShowSettingsContent();
                        SettingsScript.RebuildSettingsList("graphics");
                        PauseMenuScript.UnpauseGame();

                        SettingsScript.SaveSettings();
                        CreateNewConsoleLine("Successfully changed " + type + " to " + value + "!", MessageType.CONSOLE_SUCCESS_MESSAGE.ToString());
                    }
                }
                else if (type == "light_distance")
                {
                    if (valueInt < 15
                        || valueInt > 5000)
                    {
                        CreateNewConsoleLine("Error: Invalid value for " + type + "! Must be between 15 and 5000.", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
                    }
                    else
                    {
                        SettingsScript.user_LightDistance = valueInt;

                        PauseMenuScript.PauseWithUI();
                        PauseMenuScript.ShowSettingsContent();
                        SettingsScript.RebuildSettingsList("graphics");
                        PauseMenuScript.UnpauseGame();

                        SettingsScript.SaveSettings();
                        CreateNewConsoleLine("Successfully changed " + type + " to " + value + "!", MessageType.CONSOLE_SUCCESS_MESSAGE.ToString());
                    }
                }
                else if (type == "shadow_distance")
                {
                    if (valueInt < 15
                        || valueInt > 5000)
                    {
                        CreateNewConsoleLine("Error: Invalid value for " + type + "! Must be between 15 and 5000.", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
                    }
                    else
                    {
                        SettingsScript.user_ShadowDistance = valueInt;

                        PauseMenuScript.PauseWithUI();
                        PauseMenuScript.ShowSettingsContent();
                        SettingsScript.RebuildSettingsList("graphics");
                        PauseMenuScript.UnpauseGame();

                        SettingsScript.SaveSettings();
                        CreateNewConsoleLine("Successfully changed " + type + " to " + value + "!", MessageType.CONSOLE_SUCCESS_MESSAGE.ToString());
                    }
                }
                else if (type == "shadow_quality")
                {
                    List<string> values = new(Enum.GetNames(typeof(UserDefined_ShadowQuality)));

                    bool foundAvailableValue = false;
                    foreach (string val in values)
                    {
                        if (value == val)
                        {
                            foundAvailableValue = true;
                            break;
                        }
                    }

                    if (!foundAvailableValue)
                    {
                        CreateNewConsoleLine("Error: Invalid value for " + type + "! Must be low, medium, high or ultra.", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
                    }
                    else
                    {
                        SettingsScript.user_ShadowQuality = (UserDefined_ShadowQuality)Enum.Parse(typeof(UserDefined_ShadowQuality), value);

                        PauseMenuScript.PauseWithUI();
                        PauseMenuScript.ShowSettingsContent();
                        SettingsScript.RebuildSettingsList("graphics");
                        PauseMenuScript.UnpauseGame();

                        SettingsScript.SaveSettings();
                        CreateNewConsoleLine("Successfully changed " + type + " to " + value + "!", MessageType.CONSOLE_SUCCESS_MESSAGE.ToString());
                    }
                }
                else if (type == "tree_distance")
                {
                    if (valueInt < 15
                        || valueInt > 5000)
                    {
                        CreateNewConsoleLine("Error: Invalid value for " + type + "! Must be between 15 and 5000.", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
                    }
                    else
                    {
                        SettingsScript.user_TreeDistance = valueInt;

                        PauseMenuScript.PauseWithUI();
                        PauseMenuScript.ShowSettingsContent();
                        SettingsScript.RebuildSettingsList("graphics");
                        PauseMenuScript.UnpauseGame();

                        SettingsScript.SaveSettings();
                        CreateNewConsoleLine("Successfully changed " + type + " to " + value + "!", MessageType.CONSOLE_SUCCESS_MESSAGE.ToString());
                    }
                }
                else if (type == "grass_distance")
                {
                    if (valueInt < 15
                        || valueInt > 5000)
                    {
                        CreateNewConsoleLine("Error: Invalid value for " + type + "! Must be between 15 and 5000.", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
                    }
                    else
                    {
                        SettingsScript.user_GrassDistance = valueInt;

                        PauseMenuScript.PauseWithUI();
                        PauseMenuScript.ShowSettingsContent();
                        SettingsScript.RebuildSettingsList("graphics");
                        PauseMenuScript.UnpauseGame();

                        SettingsScript.SaveSettings();
                        CreateNewConsoleLine("Successfully changed " + type + " to " + value + "!", MessageType.CONSOLE_SUCCESS_MESSAGE.ToString());
                    }
                }
                else if (type == "object_distance")
                {
                    if (valueInt < 15
                        || valueInt > 5000)
                    {
                        CreateNewConsoleLine("Error: Invalid value for " + type + "! Must be between 15 and 5000.", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
                    }
                    else
                    {
                        SettingsScript.user_ObjectDistance = valueInt;

                        PauseMenuScript.PauseWithUI();
                        PauseMenuScript.ShowSettingsContent();
                        SettingsScript.RebuildSettingsList("graphics");
                        PauseMenuScript.UnpauseGame();

                        SettingsScript.SaveSettings();
                        CreateNewConsoleLine("Successfully changed " + type + " to " + value + "!", MessageType.CONSOLE_SUCCESS_MESSAGE.ToString());
                    }
                }
                else if (type == "item_distance")
                {
                    if (valueInt < 15
                        || valueInt > 5000)
                    {
                        CreateNewConsoleLine("Error: Invalid value for " + type + "! Must be between 15 and 5000.", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
                    }
                    else
                    {
                        SettingsScript.user_ItemDistance = valueInt;

                        PauseMenuScript.PauseWithUI();
                        PauseMenuScript.ShowSettingsContent();
                        SettingsScript.RebuildSettingsList("graphics");
                        PauseMenuScript.UnpauseGame();

                        SettingsScript.SaveSettings();
                        CreateNewConsoleLine("Successfully changed " + type + " to " + value + "!", MessageType.CONSOLE_SUCCESS_MESSAGE.ToString());
                    }
                }
                else if (type == "ai_distance")
                {
                    if (valueInt < 15
                        || valueInt > 5000)
                    {
                        CreateNewConsoleLine("Error: Invalid value for " + type + "! Must be between 15 and 5000.", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
                    }
                    else
                    {
                        SettingsScript.user_AIDistance = valueInt;

                        PauseMenuScript.PauseWithUI();
                        PauseMenuScript.ShowSettingsContent();
                        SettingsScript.RebuildSettingsList("graphics");
                        PauseMenuScript.UnpauseGame();

                        SettingsScript.SaveSettings();
                        CreateNewConsoleLine("Successfully changed " + type + " to " + value + "!", MessageType.CONSOLE_SUCCESS_MESSAGE.ToString());
                    }
                }

                //audio settings
                else if (type == "master_volume")
                {
                    if (valueInt < 0
                        || valueInt > 100)
                    {
                        CreateNewConsoleLine("Error: Invalid value for " + type + "! Must be between 0 and 100.", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
                    }
                    else
                    {
                        SettingsScript.user_MasterVolume = valueInt;

                        PauseMenuScript.PauseWithUI();
                        PauseMenuScript.ShowSettingsContent();
                        SettingsScript.RebuildSettingsList("audio");
                        PauseMenuScript.UnpauseGame();

                        SettingsScript.SaveSettings();
                        CreateNewConsoleLine("Successfully changed " + type + " to " + value + "!", MessageType.CONSOLE_SUCCESS_MESSAGE.ToString());
                    }
                }
                else if (type == "music_volume")
                {
                    if (valueInt < 0
                        || valueInt > 100)
                    {
                        CreateNewConsoleLine("Error: Invalid value for " + type + "! Must be between 0 and 100.", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
                    }
                    else
                    {
                        SettingsScript.user_MusicVolume = valueInt;

                        PauseMenuScript.PauseWithUI();
                        PauseMenuScript.ShowSettingsContent();
                        SettingsScript.RebuildSettingsList("audio");
                        PauseMenuScript.UnpauseGame();

                        SettingsScript.SaveSettings();
                        CreateNewConsoleLine("Successfully changed " + type + " to " + value + "!", MessageType.CONSOLE_SUCCESS_MESSAGE.ToString());
                    }
                }
                else if (type == "sfx_volume")
                {
                    if (valueInt < 0
                        || valueInt > 100)
                    {
                        CreateNewConsoleLine("Error: Invalid value for " + type + "! Must be between 0 and 100.", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
                    }
                    else
                    {
                        SettingsScript.user_SFXVolume = valueInt;

                        PauseMenuScript.PauseWithUI();
                        PauseMenuScript.ShowSettingsContent();
                        SettingsScript.RebuildSettingsList("audio");
                        PauseMenuScript.UnpauseGame();

                        SettingsScript.SaveSettings();
                        CreateNewConsoleLine("Successfully changed " + type + " to " + value + "!", MessageType.CONSOLE_SUCCESS_MESSAGE.ToString());
                    }
                }
                else if (type == "npc_volume")
                {
                    if (valueInt < 0
                        || valueInt > 100)
                    {
                        CreateNewConsoleLine("Error: Invalid value for " + type + "! Must be between 0 and 100.", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
                    }
                    else
                    {
                        SettingsScript.user_NPCVolume = valueInt;

                        PauseMenuScript.PauseWithUI();
                        PauseMenuScript.ShowSettingsContent();
                        SettingsScript.RebuildSettingsList("audio");
                        PauseMenuScript.UnpauseGame();

                        SettingsScript.SaveSettings();
                        CreateNewConsoleLine("Successfully changed " + type + " to " + value + "!", MessageType.CONSOLE_SUCCESS_MESSAGE.ToString());
                    }
                }

                else
                {
                    CreateNewConsoleLine("Error: Settings name " + type + " was not found! Type showallsettings to list all game settings.", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
                }
            }
        }
    }

    //restart game from beginning
    private void Command_Restart()
    {
        string loadFilePath = GameManagerScript.gamePath + @"\loadfile.txt";

        //using a text editor to write text to the game save file in the saved file path
        using StreamWriter loadFile = File.CreateText(loadFilePath);

        loadFile.WriteLine("restart");

        SceneManager.LoadScene(1);
    }
    //quit game
    private void Command_Quit()
    {
        #if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
        #endif

        #if UNITY_STANDALONE
        Application.Quit();
        #endif
    }

    //teleport player to xyz coordinates
    private void Command_TeleportPlayer()
    {
        string secondWord = separatedWords[2];
        string thirdWord = separatedWords[3];
        string fourthWord = separatedWords[4];

        bool firstVecCorrect = Regex.IsMatch(secondWord, @"-?\d+(\.\d+)?");
        bool secondVecCorrect = Regex.IsMatch(thirdWord, @"-?\d+(\.\d+)?");
        bool thirdVecCorrect = Regex.IsMatch(fourthWord, @"-?\d+(\.\d+)?");
        if (!firstVecCorrect)
        {
            CreateNewConsoleLine("Error: Teleport coordinate first input must be a number!", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
        }
        if (!secondVecCorrect)
        {
            CreateNewConsoleLine("Error: Teleport coordinate second input must be a number!", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
        }
        if (!thirdVecCorrect)
        {
            CreateNewConsoleLine("Error: Teleport coordinate third input must be a number!", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
        }

        //if all 3 are numbers then assign them as
        //teleport position coordinates and move player to target
        if (firstVecCorrect && secondVecCorrect && thirdVecCorrect)
        {
            int firstVec = int.Parse(secondWord);
            int secondVec = int.Parse(thirdWord);
            int thirdVec = int.Parse(fourthWord);

            if (firstVec > 1000000 || firstVec < -1000000)
            {
                CreateNewConsoleLine("Error: Teleport coordinate first input cannot be higher than 1000000 and lower than -1000000!", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
            }
            if (secondVec >= 1000000 || secondVec <= -1000000)
            {
                CreateNewConsoleLine("Error: Teleport coordinate second input cannot be higher than 1000000 and lower than -1000000!", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
            }
            if (thirdVec >= 1000000 || thirdVec <= -1000000)
            {
                CreateNewConsoleLine("Error: Teleport coordinate third input cannot be higher than 1000000 and lower than -1000000!", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
            }

            else
            {
                //set teleport position
                Vector3 teleportPos = new(firstVec, secondVec, thirdVec);

                CreateNewConsoleLine("Successfully teleported player to " + teleportPos + "!", MessageType.CONSOLE_SUCCESS_MESSAGE.ToString());

                thePlayer.transform.position = teleportPos;
            }
        }
    }

    //list all player stats
    private void Command_ShowPlayerStats()
    {
        CreateNewConsoleLine("---PLAYER MOVEMENT---", MessageType.CONSOLE_INFO_MESSAGE.ToString());
        CreateNewConsoleLine("walkspeed: " + PlayerStatsScript.walkSpeed, MessageType.CONSOLE_INFO_MESSAGE.ToString());
        CreateNewConsoleLine("sprintspeed: " + PlayerStatsScript.sprintSpeed, MessageType.CONSOLE_INFO_MESSAGE.ToString());
        CreateNewConsoleLine("crouchspeed: " + PlayerStatsScript.crouchSpeed, MessageType.CONSOLE_INFO_MESSAGE.ToString());
        CreateNewConsoleLine("jumpheight: " + PlayerStatsScript.jumpHeight, MessageType.CONSOLE_INFO_MESSAGE.ToString());
        CreateNewConsoleLine("", MessageType.CONSOLE_INFO_MESSAGE.ToString());

        CreateNewConsoleLine("---PLAYER CAMERA---", MessageType.CONSOLE_INFO_MESSAGE.ToString());
        CreateNewConsoleLine("cameramovespeed: " + SettingsScript.user_MouseSpeed, MessageType.CONSOLE_INFO_MESSAGE.ToString());
        CreateNewConsoleLine("fov: " + SettingsScript.user_FieldOfView, MessageType.CONSOLE_INFO_MESSAGE.ToString());
        CreateNewConsoleLine("", MessageType.CONSOLE_INFO_MESSAGE.ToString());

        CreateNewConsoleLine("---PLAYER MAIN VALUES---", MessageType.CONSOLE_INFO_MESSAGE.ToString());
        CreateNewConsoleLine("Note: only player level is editable, cannot change player required points to level up.", MessageType.CONSOLE_INFO_MESSAGE.ToString());
        CreateNewConsoleLine("level: " + PlayerStatsScript.level + ", " + PlayerStatsScript.level_PointsToNextLevel, MessageType.CONSOLE_INFO_MESSAGE.ToString());
        CreateNewConsoleLine("maxhealth: " + PlayerStatsScript.maxHealth, MessageType.CONSOLE_INFO_MESSAGE.ToString());
        CreateNewConsoleLine("currenthealth: " + PlayerStatsScript.currentHealth, MessageType.CONSOLE_INFO_MESSAGE.ToString());
        CreateNewConsoleLine("maxStamina: " + PlayerStatsScript.maxStamina, MessageType.CONSOLE_INFO_MESSAGE.ToString());
        CreateNewConsoleLine("currentStamina: " + PlayerStatsScript.currentStamina, MessageType.CONSOLE_INFO_MESSAGE.ToString());
        CreateNewConsoleLine("maxmagicka: " + PlayerStatsScript.maxMagicka, MessageType.CONSOLE_INFO_MESSAGE.ToString());
        CreateNewConsoleLine("currentmagicka: " + PlayerStatsScript.currentMagicka, MessageType.CONSOLE_INFO_MESSAGE.ToString());
        CreateNewConsoleLine("", MessageType.CONSOLE_INFO_MESSAGE.ToString());

        CreateNewConsoleLine("---INVENTORY STATS---", MessageType.CONSOLE_INFO_MESSAGE.ToString());
        CreateNewConsoleLine("maxinvspace: " + PlayerStatsScript.maxInvSpace, MessageType.CONSOLE_INFO_MESSAGE.ToString());
        CreateNewConsoleLine("invspace (not editable): " + PlayerStatsScript.invSpace, MessageType.CONSOLE_INFO_MESSAGE.ToString());
        CreateNewConsoleLine("", MessageType.CONSOLE_INFO_MESSAGE.ToString());

        CreateNewConsoleLine("---PLAYER ATTRIBUTES---", MessageType.CONSOLE_INFO_MESSAGE.ToString());
        CreateNewConsoleLine("Strength: " + PlayerStatsScript.Attributes["Strength"], MessageType.CONSOLE_INFO_MESSAGE.ToString());
        CreateNewConsoleLine("Intelligence: " + PlayerStatsScript.Attributes["Intelligence"], MessageType.CONSOLE_INFO_MESSAGE.ToString());
        CreateNewConsoleLine("Willpower: " + PlayerStatsScript.Attributes["Willpower"], MessageType.CONSOLE_INFO_MESSAGE.ToString());
        CreateNewConsoleLine("Agility: " + PlayerStatsScript.Attributes["Agility"], MessageType.CONSOLE_INFO_MESSAGE.ToString());
        CreateNewConsoleLine("Speed: " + PlayerStatsScript.Attributes["Speed"], MessageType.CONSOLE_INFO_MESSAGE.ToString());
        CreateNewConsoleLine("Endurance: " + PlayerStatsScript.Attributes["Endurance"], MessageType.CONSOLE_INFO_MESSAGE.ToString());
        CreateNewConsoleLine("Personality: " + PlayerStatsScript.Attributes["Personality"], MessageType.CONSOLE_INFO_MESSAGE.ToString());
        CreateNewConsoleLine("Luck: " + PlayerStatsScript.Attributes["Luck"], MessageType.CONSOLE_INFO_MESSAGE.ToString());
        CreateNewConsoleLine("", MessageType.CONSOLE_INFO_MESSAGE.ToString());

        CreateNewConsoleLine("---PLAYER SKILLS---", MessageType.CONSOLE_INFO_MESSAGE.ToString());
        CreateNewConsoleLine("Note: only player skill level is editable, cannot change player required points to level up selecte skill.", MessageType.CONSOLE_INFO_MESSAGE.ToString());
        CreateNewConsoleLine("Blade: " + PlayerStatsScript.Skills["Blade"] + ", " + PlayerStatsScript.SkillPoints["Blade"], MessageType.CONSOLE_INFO_MESSAGE.ToString());
        CreateNewConsoleLine("Blunt: " + PlayerStatsScript.Skills["Blunt"] + ", " + PlayerStatsScript.SkillPoints["Blunt"], MessageType.CONSOLE_INFO_MESSAGE.ToString());
        CreateNewConsoleLine("HandToHand: " + PlayerStatsScript.Skills["HandToHand"] + ", " + PlayerStatsScript.SkillPoints["HandToHand"], MessageType.CONSOLE_INFO_MESSAGE.ToString());
        CreateNewConsoleLine("Armorer: " + PlayerStatsScript.Skills["Armorer"] + ", " + PlayerStatsScript.SkillPoints["Armorer"], MessageType.CONSOLE_INFO_MESSAGE.ToString());
        CreateNewConsoleLine("Block: " + PlayerStatsScript.Skills["Block"] + ", " + PlayerStatsScript.SkillPoints["Block"], MessageType.CONSOLE_INFO_MESSAGE.ToString());
        CreateNewConsoleLine("HeavyArmor: " + PlayerStatsScript.Skills["HeavyArmor"] + ", " + PlayerStatsScript.SkillPoints["HeavyArmor"], MessageType.CONSOLE_INFO_MESSAGE.ToString());
        CreateNewConsoleLine("Athletics: " + PlayerStatsScript.Skills["Athletics"] + ", " + PlayerStatsScript.SkillPoints["Athletics"], MessageType.CONSOLE_INFO_MESSAGE.ToString());
        CreateNewConsoleLine("Alteration: " + PlayerStatsScript.Skills["Alteration"] + ", " + PlayerStatsScript.SkillPoints["Alteration"], MessageType.CONSOLE_INFO_MESSAGE.ToString());
        CreateNewConsoleLine("Destruction: " + PlayerStatsScript.Skills["Destruction"] + ", " + PlayerStatsScript.SkillPoints["Destruction"], MessageType.CONSOLE_INFO_MESSAGE.ToString());
        CreateNewConsoleLine("Restoration: " + PlayerStatsScript.Skills["Restoration"] + ", " + PlayerStatsScript.SkillPoints["Restoration"], MessageType.CONSOLE_INFO_MESSAGE.ToString());
        CreateNewConsoleLine("Alchemy: " + PlayerStatsScript.Skills["Alchemy"] + ", " + PlayerStatsScript.SkillPoints["Alchemy"], MessageType.CONSOLE_INFO_MESSAGE.ToString());
        CreateNewConsoleLine("Conjuration: " + PlayerStatsScript.Skills["Conjuration"] + ", " + PlayerStatsScript.SkillPoints["Conjuration"], MessageType.CONSOLE_INFO_MESSAGE.ToString());
        CreateNewConsoleLine("Mysticism: " + PlayerStatsScript.Skills["Mysticism"] + ", " + PlayerStatsScript.SkillPoints["Mysticism"], MessageType.CONSOLE_INFO_MESSAGE.ToString());
        CreateNewConsoleLine("Illusion: " + PlayerStatsScript.Skills["Illusion"] + ", " + PlayerStatsScript.SkillPoints["Illusion"], MessageType.CONSOLE_INFO_MESSAGE.ToString());
        CreateNewConsoleLine("Security: " + PlayerStatsScript.Skills["Security"] + ", " + PlayerStatsScript.SkillPoints["Security"], MessageType.CONSOLE_INFO_MESSAGE.ToString());
        CreateNewConsoleLine("Sneak: " + PlayerStatsScript.Skills["Sneak"] + ", " + PlayerStatsScript.SkillPoints["Sneak"], MessageType.CONSOLE_INFO_MESSAGE.ToString());
        CreateNewConsoleLine("Marksman: " + PlayerStatsScript.Skills["Marksman"] + ", " + PlayerStatsScript.SkillPoints["Marksman"], MessageType.CONSOLE_INFO_MESSAGE.ToString());
        CreateNewConsoleLine("Acrobatics: " + PlayerStatsScript.Skills["Acrobatics"] + ", " + PlayerStatsScript.SkillPoints["Acrobatics"], MessageType.CONSOLE_INFO_MESSAGE.ToString());
        CreateNewConsoleLine("LightArmor: " + PlayerStatsScript.Skills["LightArmor"] + ", " + PlayerStatsScript.SkillPoints["LightArmor"], MessageType.CONSOLE_INFO_MESSAGE.ToString());
        CreateNewConsoleLine("Mercantile: " + PlayerStatsScript.Skills["Mercantile"] + ", " + PlayerStatsScript.SkillPoints["Mercantile"], MessageType.CONSOLE_INFO_MESSAGE.ToString());
        CreateNewConsoleLine("Speechcraft: " + PlayerStatsScript.Skills["Speechcraft"] + ", " + PlayerStatsScript.SkillPoints["Speechcraft"], MessageType.CONSOLE_INFO_MESSAGE.ToString());
    }
    //reset all player stats
    private void Command_ResetPlayerStats()
    {
        PlayerStatsScript.ResetStats();
        CreateNewConsoleLine("Successfully reset all player stats to their default values!", MessageType.CONSOLE_SUCCESS_MESSAGE.ToString());
    }
    //set player stat
    private void Command_SetPlayerStat()
    {
        string statName = separatedWords[2];

        bool isFloat = float.TryParse(separatedWords[3], out _);
        if (!isFloat)
        {
            CreateNewConsoleLine("Error: Inserted stat value must be a number!", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
        }
        else
        {
            float statValue = float.Parse(separatedWords[3]);

            if (statValue < 0
                || statValue > 1000000)
            {
                CreateNewConsoleLine("Error: Stat values must be between 0 and 1000000!", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
            }
            else
            {
                if (statName == "walkspeed")
                {
                    PlayerStatsScript.walkSpeed = (int)statValue;
                    CreateNewConsoleLine("Successfully set player walk speed to " + (int)statValue + "!", MessageType.CONSOLE_SUCCESS_MESSAGE.ToString());
                }
                else if (statName == "sprintspeed")
                {
                    PlayerStatsScript.sprintSpeed = (int)statValue;
                    CreateNewConsoleLine("Successfully set player sprint speed to " + (int)statValue + "!", MessageType.CONSOLE_SUCCESS_MESSAGE.ToString());
                }
                else if (statName == "crouchspeed")
                {
                    PlayerStatsScript.crouchSpeed = (int)statValue;
                    CreateNewConsoleLine("Successfully set player crouch speed to " + (int)statValue + "!", MessageType.CONSOLE_SUCCESS_MESSAGE.ToString());
                }
                else if (statName == "jumpheight")
                {
                    PlayerStatsScript.jumpHeight = (int)statValue;
                    CreateNewConsoleLine("Successfully set player jump height to " + (int)statValue + "!", MessageType.CONSOLE_SUCCESS_MESSAGE.ToString());
                }
                else if (statName == "cameramovespeed")
                {
                    SettingsScript.user_MouseSpeed = (int)statValue;
                    thePlayer.GetComponentInChildren<Player_Camera>().sensX = (int)statValue;
                    thePlayer.GetComponentInChildren<Player_Camera>().sensY = (int)statValue;
                }
                else if (statName == "fov")
                {
                    if (statValue < 70
                        || statValue > 110)
                    {
                        CreateNewConsoleLine("Error: Camera field of view must be between 70 and 110!", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
                    }
                    else
                    {
                        SettingsScript.user_FieldOfView = (int)statValue;
                        thePlayer.GetComponentInChildren<Camera>().fieldOfView = (int)statValue;
                        CreateNewConsoleLine("Successfully set player field of view to " + (int)statValue + "!", MessageType.CONSOLE_SUCCESS_MESSAGE.ToString());
                    }
                }
                else if (statName == "level")
                {
                    if (statValue < 0
                        || statValue > 100)
                    {
                        CreateNewConsoleLine("Error: Player level must be between 1 and 100!", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
                    }
                    else
                    {
                        PlayerStatsScript.level = (int)statValue;
                        PlayerStatsScript.level_PointsToNextLevel = PlayerStatsScript.level * 500;
                        CreateNewConsoleLine("Successfully set player level to " + (int)statValue + "!", MessageType.CONSOLE_SUCCESS_MESSAGE.ToString());
                    }
                }
                else if (statName == "maxhealth")
                {
                    PlayerStatsScript.maxHealth = (int)statValue;
                    if (PlayerStatsScript.currentHealth > PlayerStatsScript.maxHealth)
                    {
                        PlayerStatsScript.currentHealth = PlayerStatsScript.maxHealth;
                    }
                    PlayerStatsScript.UpdateBar(PlayerStatsScript.healthBar);

                    if (PlayerStatsScript.currentHealth == 0
                        && !PlayerStatsScript.isGodmodeEnabled)
                    {
                        PlayerStatsScript.PlayerDeath();
                    }

                    CreateNewConsoleLine("Successfully set player max health to " + (int)statValue + "!", MessageType.CONSOLE_SUCCESS_MESSAGE.ToString());
                }
                else if (statName == "currenthealth")
                {
                    PlayerStatsScript.currentHealth = (int)statValue;
                    if (PlayerStatsScript.currentHealth > PlayerStatsScript.maxHealth)
                    {
                        PlayerStatsScript.maxHealth = PlayerStatsScript.currentHealth;
                    }
                    PlayerStatsScript.UpdateBar(PlayerStatsScript.healthBar);

                    if (PlayerStatsScript.currentHealth == 0
                        && !PlayerStatsScript.isGodmodeEnabled)
                    {
                        PlayerStatsScript.PlayerDeath();
                    }

                    CreateNewConsoleLine("Successfully set player current health to " + (int)statValue + "!", MessageType.CONSOLE_SUCCESS_MESSAGE.ToString());
                }
                else if (statName == "maxstamina")
                {
                    PlayerStatsScript.maxStamina = (int)statValue;
                    if (PlayerStatsScript.currentStamina > PlayerStatsScript.maxStamina)
                    {
                        PlayerStatsScript.currentStamina = PlayerStatsScript.maxStamina;
                    }
                    PlayerStatsScript.UpdateBar(PlayerStatsScript.staminaBar);
                    CreateNewConsoleLine("Successfully set player max stamina to " + (int)statValue + "!", MessageType.CONSOLE_SUCCESS_MESSAGE.ToString());
                }
                else if (statName == "currentstamina")
                {
                    PlayerStatsScript.currentStamina = (int)statValue;
                    if (PlayerStatsScript.currentStamina > PlayerStatsScript.maxStamina)
                    {
                        PlayerStatsScript.maxStamina = PlayerStatsScript.currentStamina;
                    }
                    PlayerStatsScript.UpdateBar(PlayerStatsScript.staminaBar);
                    CreateNewConsoleLine("Successfully set player current stamina to " + (int)statValue + "!", MessageType.CONSOLE_SUCCESS_MESSAGE.ToString());
                }
                else if (statName == "maxmagicka")
                {
                    PlayerStatsScript.maxMagicka = (int)statValue;
                    if (PlayerStatsScript.currentMagicka > PlayerStatsScript.maxMagicka)
                    {
                        PlayerStatsScript.currentMagicka = PlayerStatsScript.maxMagicka;
                    }
                    PlayerStatsScript.UpdateBar(PlayerStatsScript.magickaBar);
                    CreateNewConsoleLine("Successfully set player max magicka to " + (int)statValue + "!", MessageType.CONSOLE_SUCCESS_MESSAGE.ToString());
                }
                else if (statName == "currentmagicka")
                {
                    PlayerStatsScript.currentMagicka = (int)statValue;
                    if (PlayerStatsScript.currentMagicka > PlayerStatsScript.maxMagicka)
                    {
                        PlayerStatsScript.maxMagicka = PlayerStatsScript.currentMagicka;
                    }
                    PlayerStatsScript.UpdateBar(PlayerStatsScript.magickaBar);
                    CreateNewConsoleLine("Successfully set player current magicka to " + (int)statValue + "!", MessageType.CONSOLE_SUCCESS_MESSAGE.ToString());
                }
                else if (statName == "maxinvspace")
                {
                    PlayerStatsScript.maxInvSpace = (int)statValue;
                    CreateNewConsoleLine("Successfully set player max inventory space to " + (int)statValue + "!", MessageType.CONSOLE_SUCCESS_MESSAGE.ToString());
                }

                //set stat value
                else if (statName == "Strength"
                         || statName == "Intelligence"
                         || statName == "Willpower"
                         || statName == "Agility"
                         || statName == "Speed"
                         || statName == "Endurance"
                         || statName == "Personality"
                         || statName == "Luck")
                {
                    if (statValue < 1
                        || statValue > 100)
                    {
                        CreateNewConsoleLine("Error: " + statName + " must be between 1 and 10!", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
                    }
                    else
                    {
                        foreach (KeyValuePair<string, int> attributes in PlayerStatsScript.Attributes)
                        {
                            string attributeName = attributes.Key;

                            if (statName == attributeName)
                            {
                                PlayerStatsScript.Attributes[statName] = (int)statValue;

                                CreateNewConsoleLine("Successfully set " + statName + " to " + (int)statValue + "!", MessageType.CONSOLE_SUCCESS_MESSAGE.ToString());
                                break;
                            }
                        }
                    }
                }

                //set skill level
                else if (statName == "Blade"
                         || statName == "Blunt"
                         || statName == "HandToHand"
                         || statName == "Armorer"
                         || statName == "Block"
                         || statName == "HeavyArmor"
                         || statName == "Athletics"
                         || statName == "Alteration"
                         || statName == "Destruction"
                         || statName == "Restoration"
                         || statName == "Alchemy"
                         || statName == "Conjuration"
                         || statName == "Mysticism"
                         || statName == "Illusion"
                         || statName == "Security"
                         || statName == "Sneak"
                         || statName == "Marksman"
                         || statName == "Acrobatics"
                         || statName == "LightArmor"
                         || statName == "Mercantile"
                         || statName == "Speechcraft")
                {
                    if (statValue < 1
                        || statValue > 100)
                    {
                        CreateNewConsoleLine("Error: " + statName + " must be between 1 and 100!", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
                    }
                    else
                    {
                        foreach (KeyValuePair<string, int> skills in PlayerStatsScript.Skills)
                        {
                            string skillName = skills.Key;

                            if (statName == skillName)
                            {
                                PlayerStatsScript.Skills[statName] = (int)statValue;
                                break;
                            }
                        }
                        foreach (KeyValuePair<string, int> skillPoints in PlayerStatsScript.SkillPoints)
                        {
                            string skillName = skillPoints.Key;

                            if (statName == skillName)
                            {
                                PlayerStatsScript.SkillPoints[statName] = PlayerStatsScript.Skills[statName] * 150;
                                break;
                            }
                        }

                        CreateNewConsoleLine("Successfully set " + statName + " to " + (int)statValue + "!", MessageType.CONSOLE_SUCCESS_MESSAGE.ToString());
                    }
                }

                else
                {
                    CreateNewConsoleLine("Error: Inserted stat name was not found! Type player showstats to list all player stats.", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
                }
            }
        }
    }

    //list all spawnable item types
    private void Command_ShowItemTypes()
    {
        CreateNewConsoleLine("shwe - show all weapons", MessageType.CONSOLE_INFO_MESSAGE.ToString());
        CreateNewConsoleLine("shar - show all armor", MessageType.CONSOLE_INFO_MESSAGE.ToString());
        CreateNewConsoleLine("shsh - show all shields", MessageType.CONSOLE_INFO_MESSAGE.ToString());
        CreateNewConsoleLine("shco - show all consumables", MessageType.CONSOLE_INFO_MESSAGE.ToString());
        CreateNewConsoleLine("shai - show all alchemy ingredients", MessageType.CONSOLE_INFO_MESSAGE.ToString());
        CreateNewConsoleLine("shsp - show all spells", MessageType.CONSOLE_INFO_MESSAGE.ToString());
        CreateNewConsoleLine("sham - show all ammo", MessageType.CONSOLE_INFO_MESSAGE.ToString());
        CreateNewConsoleLine("shmi - show all misc items", MessageType.CONSOLE_INFO_MESSAGE.ToString());
    }
    //list all items of the selected item type that start with the selected english alphabet letter
    private void Command_ShowItemTypeItems(string typeName, char letter)
    {
        int foundCount = 0;

        foreach (GameObject item in PlayerMenuScript.templateItems)
        {
            Env_Item ItemScript = item.GetComponent<Env_Item>();
            char firstLetter = item.name.ToCharArray()[0];
            char lowerCaseLetter = char.ToLower(firstLetter);

            if (letter == lowerCaseLetter)
            {
                if ((typeName == "shwe"
                    && ItemScript.itemType == Env_Item.ItemType.weapon)
                    || (typeName == "shar"
                    && ItemScript.itemType == Env_Item.ItemType.armor)
                    || (typeName == "shsh"
                    && ItemScript.itemType == Env_Item.ItemType.shield)
                    || (typeName == "shco"
                    && ItemScript.itemType == Env_Item.ItemType.consumable)
                    || (typeName == "shai"
                    && ItemScript.itemType == Env_Item.ItemType.alchemyIngredient)
                    || (typeName == "shsp"
                    && ItemScript.itemType == Env_Item.ItemType.spell)
                    || (typeName == "sham"
                    && ItemScript.itemType == Env_Item.ItemType.ammo)
                    || (typeName == "shmi"
                    && ItemScript.itemType == Env_Item.ItemType.misc))
                {
                    CreateNewConsoleLine(item.name, MessageType.CONSOLE_INFO_MESSAGE.ToString());
                    foundCount++;
                }
            }
        }

        if (foundCount == 0)
        {
            string type = "";
            if (typeName == "shwe")
            {
                type = "weapons were";
            }
            else if (typeName == "shar")
            {
                type = "armor was";
            }
            else if (typeName == "shsh")
            {
                type = "shields were";
            }
            else if (typeName == "shco")
            {
                type = "consumables were";
            }
            else if (typeName == "shai")
            {
                type = "alchemy ingredients were";
            }
            else if (typeName == "shsp")
            {
                type = "spells were";
            }
            else if (typeName == "sham")
            {
                type = "ammo were";
            }
            else if (typeName == "shmi")
            {
                type = "misc items were";
            }

            CreateNewConsoleLine("Error: No " + type + " found with the beginning letter " + letter + "!", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
        }
    }
    //list all player inventory items and their count
    private void Command_ShowAllPlayerItems()
    {
        if (PlayerInventoryScript.playerItems.Count == 0)
        {
            CreateNewConsoleLine("Player inventory has no items to display.", MessageType.CONSOLE_INFO_MESSAGE.ToString());
        }
        else
        {
            foreach (GameObject item in PlayerInventoryScript.playerItems)
            {
                Env_Item ItemScript = item.GetComponent<Env_Item>();

                CreateNewConsoleLine(item.name + " x" + ItemScript.itemCount, MessageType.CONSOLE_INFO_MESSAGE.ToString());
            }
        }
    }
    //add items to player inventory
    private void Command_AddItem()
    {
        if (!Regex.IsMatch(separatedWords[3], @"-?\d+"))
        {
            CreateNewConsoleLine("Error: Item count must be an integer!", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
        }
        else
        {
            GameObject targetTemplateItem = null;
            string itemName = separatedWords[2];
            int itemCount = int.Parse(separatedWords[3]);

            foreach (GameObject item in PlayerMenuScript.templateItems)
            {
                if (itemName == item.name)
                {
                    targetTemplateItem = item;
                    break;
                }
            }

            if (targetTemplateItem == null)
            {
                CreateNewConsoleLine("Error: " + itemName + " is an invalid item name! Type player showitemtypes to sort through all spawnable item types.", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
            }
            else
            {
                if (itemCount < 1
                    || itemCount > 1000000)
                {
                    CreateNewConsoleLine("Error: Cannot spawn more than 1000000 items at once!", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
                }
                else
                {
                    GameObject existingItem = null;
                    foreach (GameObject item in PlayerInventoryScript.playerItems)
                    {
                        if (item.name == itemName
                            && item.GetComponent<Env_Item>().isStackable)
                        {
                            existingItem = item;
                            break;
                        }
                    }

                    Env_Item ItemScript = targetTemplateItem.GetComponent<Env_Item>();
                    int remainingSpace = PlayerStatsScript.maxInvSpace;
                    foreach (GameObject item in PlayerInventoryScript.playerItems)
                    {
                        if (item.GetComponent<Env_Item>().itemWeight > 0)
                        {
                            remainingSpace -= item.GetComponent<Env_Item>().itemWeight * item.GetComponent<Env_Item>().itemCount;
                        }
                    }
                    int totalTakenSpace = ItemScript.itemWeight * itemCount;

                    if (!ItemScript.isStackable
                        && itemCount != 1)
                    {
                        CreateNewConsoleLine("Error: Cannot add more than one of " + itemName + " because it is not stackable!", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
                    }
                    else
                    {
                        if (totalTakenSpace > remainingSpace)
                        {
                            CreateNewConsoleLine("Error: Not enough inventory space to add " + itemCount + " " + itemName + "(s)!", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
                        }
                        else
                        {
                            if (existingItem == null)
                            {
                                GameObject newItem = Instantiate(targetTemplateItem,
                                                                 PlayerInventoryScript.par_PlayerItems.transform.position,
                                                                 Quaternion.identity,
                                                                 PlayerInventoryScript.par_PlayerItems.transform);
                                newItem.SetActive(false);
                                newItem.name = targetTemplateItem.name;

                                Env_Item NewItemScript = newItem.GetComponent<Env_Item>();
                                NewItemScript.itemCount = itemCount;
                                if (NewItemScript.itemType == Env_Item.ItemType.weapon
                                    || NewItemScript.itemType == Env_Item.ItemType.armor
                                    || NewItemScript.itemType == Env_Item.ItemType.shield)
                                {
                                    NewItemScript.itemCurrentDurability = NewItemScript.itemMaxDurability;

                                    if (NewItemScript.itemType == Env_Item.ItemType.weapon)
                                    {
                                        newItem.GetComponent<Item_Weapon>().damage_Current = newItem.GetComponent<Item_Weapon>().damage_Default;
                                    }
                                }

                                PlayerInventoryScript.playerItems.Add(newItem);
                                PlayerStatsScript.invSpace += totalTakenSpace;

                                CreateNewConsoleLine("Successfully added " + itemCount + " " + itemName + "(s) to player inventory!", MessageType.CONSOLE_SUCCESS_MESSAGE.ToString());
                            }
                            else
                            {
                                existingItem.GetComponent<Env_Item>().itemCount += itemCount;
                                PlayerStatsScript.invSpace += totalTakenSpace;

                                CreateNewConsoleLine("Successfully added " + itemCount + " " + itemName + "(s) to player inventory!", MessageType.CONSOLE_SUCCESS_MESSAGE.ToString());
                            }
                        }
                    }
                }
            }
        }
    }
    //remove items from player inventory
    private void Command_RemoveItem()
    {
        if (!Regex.IsMatch(separatedWords[3], @"-?\d+"))
        {
            CreateNewConsoleLine("Error: Item count must be an integer!", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
        }
        else
        {
            GameObject targetPlayerItem = null;
            string itemName = separatedWords[2];
            int itemCount = int.Parse(separatedWords[3]);

            foreach (GameObject item in PlayerInventoryScript.playerItems)
            {
                if (itemName == item.name)
                {
                    targetPlayerItem = item;
                    break;
                }
            }

            if (targetPlayerItem == null)
            {
                CreateNewConsoleLine("Error: Did not find " + itemName + "(s) from player inventory! Type player showinventoryitems to list all player items.", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
            }
            else
            {
                if (targetPlayerItem.GetComponent<Env_Item>().isProtected
                    || targetPlayerItem.GetComponent<Env_Item>().isEquipped)
                {
                    CreateNewConsoleLine("Error: Cannot remove protected or equipped items from player inventory!", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
                }
                else
                {
                    if (itemCount < targetPlayerItem.GetComponent<Env_Item>().itemCount)
                    {
                        CreateNewConsoleLine("Error: Cannot remove more than total count of the selected item!", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
                    }
                    else
                    {
                        Env_Item ItemScript = targetPlayerItem.GetComponent<Env_Item>();
                        int totalTakenWeight = ItemScript.itemWeight * ItemScript.itemCount;

                        if (!ItemScript.isStackable
                            && itemCount != 1)
                        {
                            CreateNewConsoleLine("Error: Cannot remove more than one of " + itemName + " because it is not stackable!", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
                        }
                        else
                        {
                            if (itemCount < targetPlayerItem.GetComponent<Env_Item>().itemCount)
                            {
                                targetPlayerItem.GetComponent<Env_Item>().itemCount -= itemCount;
                                PlayerStatsScript.invSpace -= totalTakenWeight;

                                CreateNewConsoleLine("Successfully removed " + itemCount + " " + itemName + "(s) from player inventory!", MessageType.CONSOLE_SUCCESS_MESSAGE.ToString());
                            }
                            else
                            {
                                PlayerStatsScript.invSpace -= totalTakenWeight;
                                PlayerInventoryScript.playerItems.Remove(targetPlayerItem);
                                Destroy(targetPlayerItem);

                                CreateNewConsoleLine("Successfully removed " + itemCount + " " + itemName + "(s) from player inventory!", MessageType.CONSOLE_SUCCESS_MESSAGE.ToString());
                            }
                        }
                    }
                }
            }
        }
    }
    //show item stats
    private void Command_ShowItemStats()
    {
        GameObject targetPlayerItem = null;
        string itemName = separatedWords[2];

        if (Regex.IsMatch(separatedWords[3], @"-?\d+"))
        {
            List<GameObject> sameItems = new();
            foreach (GameObject item in PlayerInventoryScript.playerItems)
            {
                if (itemName == item.name)
                {
                    sameItems.Add(item);
                }
            }
            foreach (GameObject item in sameItems)
            {
                if (int.Parse(separatedWords[3]) == sameItems.IndexOf(item))
                {
                    targetPlayerItem = item;
                    break;
                }
            }
        }

        if (targetPlayerItem == null)
        {
            CreateNewConsoleLine("Error: Item position or name is invalid! Type player showinventoryitems to list all player items.", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
        }
        else
        {
            Env_Item ItemScript = targetPlayerItem.GetComponent<Env_Item>();
            CreateNewConsoleLine("isProtected: " + ItemScript.isProtected, MessageType.CONSOLE_INFO_MESSAGE.ToString());
            CreateNewConsoleLine("isStackable: " + ItemScript.isStackable, MessageType.CONSOLE_INFO_MESSAGE.ToString());
            CreateNewConsoleLine("type: " + ItemScript.itemType.ToString(), MessageType.CONSOLE_INFO_MESSAGE.ToString());
            CreateNewConsoleLine("quality: " + ItemScript.itemQuality.ToString(), MessageType.CONSOLE_INFO_MESSAGE.ToString());
            CreateNewConsoleLine("value: " + ItemScript.itemValue, MessageType.CONSOLE_INFO_MESSAGE.ToString());
            CreateNewConsoleLine("weight: " + ItemScript.itemWeight, MessageType.CONSOLE_INFO_MESSAGE.ToString());
            CreateNewConsoleLine("count: " + ItemScript.itemCount, MessageType.CONSOLE_INFO_MESSAGE.ToString());
            if (ItemScript.itemType == Env_Item.ItemType.weapon
                || ItemScript.itemType == Env_Item.ItemType.armor
                || ItemScript.itemType == Env_Item.ItemType.shield)
            {
                CreateNewConsoleLine("durability: " + ItemScript.itemCurrentDurability + "/" + ItemScript.itemMaxDurability, MessageType.CONSOLE_INFO_MESSAGE.ToString());

                if (ItemScript.itemType == Env_Item.ItemType.weapon)
                {
                    CreateNewConsoleLine("damage: " + targetPlayerItem.GetComponent<Item_Weapon>().damage_Current, MessageType.CONSOLE_INFO_MESSAGE.ToString());
                }
            }
        }
    }
    //set item stat
    private void Command_SetItemStat(GameObject targetItem)
    {
        Env_Item ItemScript = targetItem.GetComponent<Env_Item>();

        if (separatedWords[4] != "value"
            && separatedWords[4] != "weight"
            && separatedWords[4] != "count"
            && (separatedWords[4] != "damage"
            || (separatedWords[4] == "damage"
            && ItemScript.itemType != Env_Item.ItemType.weapon))
            && (separatedWords[4] != "durability"
            || (separatedWords[4] == "durability"
            && ItemScript.itemType != Env_Item.ItemType.weapon
            && ItemScript.itemType != Env_Item.ItemType.armor
            && ItemScript.itemType != Env_Item.ItemType.shield)))
        {
            CreateNewConsoleLine("Error: Stat name " + separatedWords[4] + " is invalid or not editable! Type player showitemstats " + separatedWords[1] + " to list all stats for this item.", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
        }
        else
        {
            if (!Regex.IsMatch(separatedWords[5], @"-?\d+"))
            {
                CreateNewConsoleLine("Error: Stat value " + separatedWords[4] + " must be an integer!", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
            }
            else
            {
                int statValue = int.Parse(separatedWords[5]);
                if (separatedWords[4] == "value")
                {
                    if (statValue < 0
                        || statValue > 25000)
                    {
                        CreateNewConsoleLine("Error: Item value must be between 0 and 25000!", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
                    }
                    else
                    {
                        ItemScript.itemValue = statValue;
                        CreateNewConsoleLine("Successfully set " + separatedWords[2] + " item value to " + statValue + "!", MessageType.CONSOLE_SUCCESS_MESSAGE.ToString());
                    }
                }
                else if (separatedWords[4] == "weight")
                {
                    if (statValue < 0
                        || statValue > 100)
                    {
                        CreateNewConsoleLine("Error: Item weight must be between 0 and 100!", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
                    }
                    else
                    {
                        ItemScript.itemWeight = statValue;
                        CreateNewConsoleLine("Successfully set " + separatedWords[2] + " item weight to " + statValue + "!", MessageType.CONSOLE_SUCCESS_MESSAGE.ToString());
                    }
                }
                else if (separatedWords[4] == "count")
                {
                    if (!ItemScript.isStackable)
                    {
                        CreateNewConsoleLine("Error: Cannot edit non-stackable item count!", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
                    }
                    else
                    {
                        if (statValue < 0
                            || statValue > 1000000)
                        {
                            CreateNewConsoleLine("Error: Item count must be between 1 and 1000000!", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
                        }
                        else
                        {
                            int currentSingleWeight = ItemScript.itemWeight;
                            int currentTotalWeight = ItemScript.itemCount * ItemScript.itemWeight;
                            int currentTakenSpace = PlayerStatsScript.invSpace;
                            int maxSpace = PlayerStatsScript.maxInvSpace;

                            int availableSpaceWithoutItem = maxSpace - currentTakenSpace - currentTotalWeight;
                            int availableSpaceWithNewCount = maxSpace - availableSpaceWithoutItem + currentSingleWeight * statValue;

                            if (availableSpaceWithNewCount < 0)
                            {
                                CreateNewConsoleLine("Error: Cannot set item count to " + statValue + " because it would take more space than available space!", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
                            }
                            else
                            {
                                ItemScript.itemCount = statValue;
                                PlayerStatsScript.invSpace = availableSpaceWithNewCount;
                                CreateNewConsoleLine("Successfully set " + separatedWords[2] + " item count to " + statValue + "!", MessageType.CONSOLE_SUCCESS_MESSAGE.ToString());
                            }
                        }
                    }
                }
                else if (separatedWords[4] == "damage")
                {
                    if (statValue < 0
                        || statValue > 1000000)
                    {
                        CreateNewConsoleLine("Error: Item damage must be between 0 and 1000000!", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
                    }
                    else
                    {
                        targetItem.GetComponent<Item_Weapon>().damage_Current = statValue;
                        CreateNewConsoleLine("Successfully set " + separatedWords[2] + " damage to " + statValue + "!", MessageType.CONSOLE_SUCCESS_MESSAGE.ToString());
                    }
                }
                else if (separatedWords[4] == "durability")
                {
                    if (statValue < 0
                        || statValue > ItemScript.itemMaxDurability)
                    {
                        CreateNewConsoleLine("Error: Item durability must be between 0 and item max durability!", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
                    }
                    else
                    {
                        ItemScript.itemCurrentDurability = statValue;
                        CreateNewConsoleLine("Successfully set " + separatedWords[2] + " duraiblity to " + statValue + "!", MessageType.CONSOLE_SUCCESS_MESSAGE.ToString());
                    }
                }
            }
        }
    }
    //show all item enchantments
    private void Command_ShowItemEnchantments(GameObject targetItem)
    {
        bool foundEnchantments = false;
        foreach (GameObject enchantment in targetItem.GetComponent<Env_Item>().activeEnchantments)
        {
            if (enchantment.GetComponent<Env_Effect>() != null)
            {
                foundEnchantments = true;
            }
        }
        if (!foundEnchantments)
        {
            CreateNewConsoleLine(targetItem.name.Replace("_", " ") + " does not have any enchantments.", MessageType.CONSOLE_INFO_MESSAGE.ToString());
        }
        else
        {
            foreach (GameObject enchantment in targetItem.GetComponent<Env_Item>().activeEnchantments)
            {
                if (enchantment.GetComponent<Env_Effect>() != null)
                {
                    string effectName = enchantment.GetComponent<Env_Effect>().theEffectName;
                    int effectValue = (int)enchantment.GetComponent<Env_Effect>().theEffectValue;
                    CreateNewConsoleLine(effectName + ", " + effectValue, MessageType.CONSOLE_INFO_MESSAGE.ToString());
                }
            }
        }
    }
    //add new enchantment to item
    private void Command_AddItemEnchantment(GameObject targetItem)
    {
        string effectName = separatedWords[4];
        int value = int.Parse(separatedWords[5]);
        int duration = int.Parse(separatedWords[6]);

        //look for an already existing enchantment
        //and update its value and duration
        //if the same effect was added again
        GameObject foundDuplicateEffect = null;
        foreach (GameObject enchantment in targetItem.GetComponent<Env_Item>().activeEnchantments)
        {
            if (enchantment.GetComponent<Env_Effect>().theEffectName == effectName)
            {
                foundDuplicateEffect = enchantment;
                break;
            }
        }
        if (foundDuplicateEffect != null)
        {
            foundDuplicateEffect.GetComponent<Env_Effect>().theEffectValue = value;
            foundDuplicateEffect.GetComponent<Env_Effect>().theEffectDuration = duration;
        }
        //create a new enchantment gameobject onto target item if a duplicate was not found
        else
        {
            EffectManagerScript.DealEffect(thePlayer,
                                           targetItem,
                                           effectName,
                                           value,
                                           duration);
            CreateNewConsoleLine("Successfully added enchantment " + effectName + " with value " + value + " and duration " + duration + " to " + targetItem.name.Replace("_", " ") + ".", MessageType.CONSOLE_INFO_MESSAGE.ToString());
        }
    }
    //remove existing enchantment from item
    private void Command_RemoveItemEnchantment(GameObject targetItem)
    {
        bool foundEnchantments = false;
        foreach (GameObject enchantment in targetItem.GetComponent<Env_Item>().activeEnchantments)
        {
            if (enchantment.GetComponent<Env_Effect>() != null)
            {
                foundEnchantments = true;
            }
        }
        if (!foundEnchantments)
        {
            CreateNewConsoleLine(targetItem.name.Replace("_", " ") + " does not have any enchantments.", MessageType.CONSOLE_INFO_MESSAGE.ToString());
        }
        else
        {
            bool foundEnchantment = false;
            string enchantmentName = separatedWords[4];
            foreach (GameObject enchantment in targetItem.GetComponent<Env_Item>().activeEnchantments)
            {
                if (enchantment.GetComponent<Env_Effect>() != null
                    && enchantment.GetComponent<Env_Effect>().theEffectName 
                    == enchantmentName)
                {
                    foundEnchantment = true;
                    break;
                }
            }
            if (!foundEnchantment)
            {
                CreateNewConsoleLine("Error: Did not find enchantment " + enchantmentName + " from " + targetItem.name.Replace("_", " ") + " !", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
            }
            else
            {
                foreach (GameObject enchantment in targetItem.GetComponent<Env_Item>().activeEnchantments)
                {
                    string activeEffectName = enchantment.GetComponent<Env_Effect>().theEffectName;
                    if (enchantmentName == activeEffectName)
                    {
                        Destroy(enchantment);
                    }
                }
                PlayerStatsScript.activeEffects.RemoveAll(x => x == null);
            }
        }
    }

    //show all effects that can be added to player
    private void Command_ShowAllEffects()
    {
        foreach (string effect in AllowedEffectsScript.allEffects)
        {
            CreateNewConsoleLine(effect, MessageType.CONSOLE_INFO_MESSAGE.ToString());
        }
    }
    //show all effects that are added to player
    private void Command_ShowActiveEffects()
    {
        if (PlayerStatsScript.activeEffects.Count == 0)
        {
            CreateNewConsoleLine("Player has no active effects.", MessageType.CONSOLE_INFO_MESSAGE.ToString());
        }
        else
        {
            foreach (GameObject effect in PlayerStatsScript.activeEffects)
            {
                Env_Effect ActiveEffectScript = effect.GetComponent<Env_Effect>();
                string effectName = ActiveEffectScript.theEffectName;
                float effectValue = ActiveEffectScript.theEffectValue;
                CreateNewConsoleLine(effectName + ", " + effectValue, MessageType.CONSOLE_INFO_MESSAGE.ToString());
            }
        }
    }
    //add effect with value and duration to player
    private void Command_AddEffectToPlayer()
    {
        string effectName = separatedWords[2];
        int value = int.Parse(separatedWords[3]);
        int duration = int.Parse(separatedWords[4]);

        //look for an already existing effect
        //and update its value and duration
        //if the same effect was added again
        GameObject foundDuplicateEffect = null;
        foreach (GameObject effect in PlayerStatsScript.activeEffects)
        {
            if (effect.GetComponent<Env_Effect>().theEffectName == effectName)
            {
                foundDuplicateEffect = effect;
                break;
            }
        }
        if (foundDuplicateEffect != null)
        {
            foundDuplicateEffect.GetComponent<Env_Effect>().theEffectValue = value;
            foundDuplicateEffect.GetComponent<Env_Effect>().theEffectDuration = duration;
        }
        //create a new effect gameobject onto player if a duplicate was not found
        else
        {
            EffectManagerScript.DealEffect(thePlayer,
                                           thePlayer,
                                           effectName,
                                           value,
                                           duration);
            CreateNewConsoleLine("Successfully added effect " + effectName + " with value " + value + " and duration " + duration + " to player.", MessageType.CONSOLE_INFO_MESSAGE.ToString());
        }
    }
    //remove effect from player
    private void Command_RemoveEffectFromPlayer()
    {
        //effect name
        bool isEffectNameFloat = float.TryParse(separatedWords[2], out _);
        if (isEffectNameFloat)
        {
            CreateNewConsoleLine("Error: Effect name cannot be a number!", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
        }
        else
        {
            string effectName = separatedWords[2];
            bool foundEffectName = false;
            foreach (GameObject effect in PlayerStatsScript.activeEffects)
            {
                string activeEffectName = effect.GetComponent<Env_Effect>().theEffectName;
                if (effectName == activeEffectName)
                {
                    foundEffectName = true;
                    break;
                }
            }
            if (!foundEffectName)
            {
                CreateNewConsoleLine("Error: Did not find effect with name " + effectName + "! Use player show_allplayereffects to list all available effects.", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
            }
            else
            {
                //deletes the effect and removes it from active effects list
                foreach (GameObject effect in PlayerStatsScript.activeEffects)
                {
                    string activeEffectName = effect.GetComponent<Env_Effect>().theEffectName;
                    if (effectName == activeEffectName)
                    {
                        Destroy(effect);
                    }
                }
                PlayerStatsScript.activeEffects.RemoveAll(x => x == null);
            }
        }
    }

    //select target
    private void Command_SelectTarget()
    {
        target = null;
        isSelectingTarget = true;
        CreateNewConsoleLine("Selecting target...", MessageType.CONSOLE_INFO_MESSAGE.ToString());
    }
    //unlock selected target
    private void Command_UnlockTarget()
    {
        if (target == null)
        {
            CreateNewConsoleLine("Error: No target selected! Type st to select a target.", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
        }
        else
        {
            //unlock respawnable container
            if (target.GetComponent<UI_Inventory>() 
                && target.GetComponent<UI_Inventory>().containerType
                == UI_Inventory.ContainerType.container)
            {
                if (!target.GetComponent<Env_LockStatus>().isUnlocked)
                {
                    target.GetComponent<Env_LockStatus>().isUnlocked = true;
                    CreateNewConsoleLine("Successfully unlocked container " + target.GetComponent<UI_Inventory>().containerName + "!", MessageType.CONSOLE_SUCCESS_MESSAGE.ToString());
                }
                else
                {
                    CreateNewConsoleLine("Error: Selected container " + target.GetComponent<UI_Inventory>().containerName + " is already unlocked!", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
                }
            }
            //unlock door or open gate
            else if (target.GetComponent<Env_Door>())
            {
                Manager_Door DoorManagerScript = target.GetComponent<Env_Door>().DoorManagerScript;
                if (DoorManagerScript.doorType == Manager_Door.DoorType.door_Single 
                    || DoorManagerScript.doorType == Manager_Door.DoorType.door_Double)
                {
                    if (!DoorManagerScript.GetComponent<Env_LockStatus>().isUnlocked)
                    {
                        DoorManagerScript.GetComponent<Env_LockStatus>().isUnlocked = true;
                        CreateNewConsoleLine("Successfully unlocked " + DoorManagerScript.doorName + "!", MessageType.CONSOLE_SUCCESS_MESSAGE.ToString());
                    }
                    else
                    {
                        CreateNewConsoleLine("Error: Selected door " + DoorManagerScript.doorName + " is already unlocked!", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
                    }
                }
                else if (DoorManagerScript.doorType == Manager_Door.DoorType.gate)
                {
                    DoorManagerScript.OpenDoor();
                    CreateNewConsoleLine("Successfully opened " + DoorManagerScript.doorName + "!", MessageType.CONSOLE_SUCCESS_MESSAGE.ToString());
                }
            }
            else
            {
                CreateNewConsoleLine("Error: Selected target is not an unlockable object!", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
            }
        }
    }

    //checks if item sorted by index exists in player inventory
    private void FindItem(string targetMethod)
    {
        GameObject targetPlayerItem = null;
        string itemName = separatedWords[2];

        if (Regex.IsMatch(separatedWords[3], @"-?\d+"))
        {
            List<GameObject> sameItems = new();
            foreach (GameObject item in PlayerInventoryScript.playerItems)
            {
                if (itemName == item.name)
                {
                    sameItems.Add(item);
                }
            }
            foreach (GameObject item in sameItems)
            {
                if (int.Parse(separatedWords[3]) == sameItems.IndexOf(item))
                {
                    targetPlayerItem = item;
                    break;
                }
            }
        }

        if (targetPlayerItem == null)
        {
            CreateNewConsoleLine("Error: Item position or name is invalid! Type player showinventoryitems to list all player items.", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
        }
        else
        {
            if (targetMethod == "setItemStat")
            {
                Command_SetItemStat(targetPlayerItem);
            }
            else if (targetMethod == "showItemEnchantments")
            {
                Command_ShowItemEnchantments(targetPlayerItem);
            }
            else if (targetMethod == "addItemEnchantment"
                     || targetMethod == "removeItemEnchantment")
            {
                FindEffect(targetMethod, targetPlayerItem);
            }
            else if (targetMethod == "addEffect")
            {
                FindEffect(targetMethod, null);
            }
        }
    }
    //checks if effect/enchantment exists
    private void FindEffect(string targetMethod, GameObject targetPlayerItem)
    {
        bool foundEffect = false;
        string effect = separatedWords[4];
        foreach (string existingEnchantment in AllowedEffectsScript.allEffects)
        {
            if (existingEnchantment == effect)
            {
                foundEffect = true;
                break;
            }
        }

        if (!foundEffect)
        {
            CreateNewConsoleLine("Error: Did not find effect/enchantment " + effect + "! Type player showalleffects to list all game effects.", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
        }
        else
        {
            if (targetMethod == "removePlayerEffect")
            {
                Command_RemoveEffectFromPlayer();
            }
            else if (targetMethod == "removeItemEnchantment")
            {
                Command_RemoveItemEnchantment(targetPlayerItem);
            }
            else
            {
                //effect value
                if (!Regex.IsMatch(separatedWords[5], @"-?\d+"))
                {
                    CreateNewConsoleLine("Error: Effect value must be an integer!", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
                }
                else
                {
                    int value = int.Parse(separatedWords[5]);
                    if (value < 0
                        || value > 1000001)
                    {
                        CreateNewConsoleLine("Error: Effect value is out of range! Must be set between 0 and 1000001.", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
                    }
                    else
                    {
                        //effect duration
                        if (!Regex.IsMatch(separatedWords[6], @"-?\d+"))
                        {
                            CreateNewConsoleLine("Error: Effect duration must be an integer!", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
                        }
                        else
                        {
                            int duration = int.Parse(separatedWords[6]);
                            if (duration < -3
                                || duration == 0
                                || duration > 1000000)
                            {
                                CreateNewConsoleLine("Error: Effect duration is out of range!", MessageType.CONSOLE_ERROR_MESSAGE.ToString());
                            }
                            else
                            {
                                if (targetMethod == "addPlayerEffect")
                                {
                                    Command_AddEffectToPlayer();
                                }
                                else if (targetMethod == "addItemEnchantment")
                                {
                                    Command_AddItemEnchantment(targetPlayerItem);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    //reads all unity debug log messages and creates a new debug file message for each one
    public void HandleLog(string logString, string unusedStackString, LogType type)
    {
        if (par_Managers != null)
        {
            output = logString;

            if (!startedConsoleSetupWait)
            {
                if (lastOutput == output)
                {
                    startedConsoleSetupWait = true;
                    StartCoroutine(ConsoleSetupWait());
                }

                NewUnitylogMessage(unusedStackString, type);
            }
        }
    }

    //reads inserted text from input field in console UI
    public void ReadStringInput(string s)
    {
        input = s;

        if (Input.GetKeyDown(KeyCode.Return))
        {
            //check inserted text
            CheckInsertedText();
            //clear inserted text
            UIReuseScript.consoleInputField.text = "";
            //enable input field
            UIReuseScript.consoleInputField.ActivateInputField();
        }
    }

    //creates a new debug file line with time, info type and message
    private void NewUnitylogMessage(string unusedStackString, LogType type)
    {
        string resultMessage;

        if (output.Contains("Exception")
            || output.Contains("CS")
            || output.Contains("Error"))
        {
            resultMessage = MessageType.UNITY_LOG_ERROR.ToString() + "] [" + unusedStackString + "] [" + type + "]";
        }
        else if (output.Contains("Incorrect value"))
        {
            resultMessage = MessageType.INVALID_FILE_VARIABLE_VALUE.ToString();
        }
        else if (output.Contains("Requirements not met"))
        {
            resultMessage = MessageType.UNAUTHORIZED_PLAYER_ACTION.ToString();
        }
        else
        {
            resultMessage = MessageType.UNITY_LOG_MESSAGE.ToString();
        }

        CreateNewConsoleLine(output, resultMessage);
        lastOutput = output;
    }

    //add a new line to the console
    public void CreateNewConsoleLine(string message, string source)
    {
        if (par_Managers != null)
        {
            //create text object
            GameObject newConsoleText = Instantiate(UIReuseScript.txt_InsertedTextTemplate.gameObject);
            //add created text object to list
            createdTexts.Add(newConsoleText);

            //check if createdTexts list is longer than limit
            //and remove oldest
            if (createdTexts.Count > 200)
            {
                GameObject oldestText = createdTexts[0];
                createdTexts.Remove(oldestText);
                Destroy(oldestText);
            }

            string date = "[" + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + "]";
            string msg = date + " [" + source.Replace("_", " ") + "] - " + message + "\n";

            newConsoleText.transform.SetParent(UIReuseScript.par_ConsoleContent.transform, false);
            newConsoleText.GetComponent<TMP_Text>().text = date + " " + message;

            //using a text editor to write new text to new debug file in the debug file path
            using StreamWriter debugFile = File.AppendText(GameManagerScript.debugFilePath);

            if (message != "")
            {
                debugFile.WriteLine(msg);
            }
        }
    }

    //waits half a second when game starts before allowing console to enable
    private IEnumerator ConsoleSetupWait()
    {
        yield return new WaitForSeconds(0.5f);
        startedConsoleSetupWait = false;
    }
}