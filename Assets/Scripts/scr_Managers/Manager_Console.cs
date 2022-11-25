using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using static Manager_Settings;

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
        'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k' , 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z'
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
    private Manager_UIReuse UIReuseScript;

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
        }

        debugMenuEnabled = true;
    }

    private void Start()
    {
        CreateNewConsoleLine("Loaded scene " + SceneManager.GetActiveScene().name + " (" + currentScene + ")", "CONSOLE_INFO_MESSAGE");
        CreateNewConsoleLine("Game version " + UIReuseScript.txt_GameVersion.text, "CONSOLE_INFO_MESSAGE");
        CreateNewConsoleLine("Type help to list all commands ", "CONSOLE_INFO_MESSAGE");
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

        //open console
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
        //close console
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
                        CreateNewConsoleLine("Cancelled target selection command.", "CONSOLE_INFO_MESSAGE");
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
                        CreateNewConsoleLine("Cancelled target selection command.", "CONSOLE_INFO_MESSAGE");
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
                            CreateNewConsoleLine("Cancelled target selection command.", "CONSOLE_INFO_MESSAGE");
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
                            CreateNewConsoleLine("Cancelled target selection command.", "CONSOLE_INFO_MESSAGE");
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
                        CreateNewConsoleLine("Hit " + hitInfo.transform.name + ".", "CONSOLE_INFO_MESSAGE");
                        target = hitInfo.transform.gameObject;
                    }
                    else
                    {
                        CreateNewConsoleLine("Did not hit anything interactable...", "CONSOLE_INFO_MESSAGE");
                    }

                    UIReuseScript.par_Console.transform.localPosition = consoleVisiblePosition;

                    isSelectingTarget = false;
                }
                else
                {
                    CreateNewConsoleLine("Cancelled target selection command.", "CONSOLE_INFO_MESSAGE");
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
        CreateNewConsoleLine("--" + input + "--", "CONSOLE_COMMAND");

        //if inserted text was not empty and player pressed enter
        if (separatedWords.Count >= 1)
        {
            bool isInt = int.TryParse(separatedWords[0], out _);
            if (isInt)
            {
                CreateNewConsoleLine("Error: Console command cannot start with a number!", "CONSOLE_ERROR_MESSAGE");
            }
            else
            {
                //list all global commands
                if (separatedWords[0] == "help"
                    && separatedWords.Count == 1)
                {
                    Command_Help();
                }
                //clear console log
                else if (separatedWords[0] == "clear"
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
                         && PlayerStatsScript.currentHealth > 0)
                {
                    Command_ToggleGodMode();
                }
                //toggle noclip
                else if (separatedWords[0] == "tnc"
                         && separatedWords.Count == 1)
                {
                    Command_ToggleNoclip();
                }

                //show all saves
                else if (separatedWords[0] == "shsa"
                         && separatedWords.Count == 1)
                {
                    Command_ShowAllSaves();
                }
                //delete all saves
                else if (separatedWords[0] == "desa"
                         && separatedWords.Count == 1)
                {
                    Command_DeleteAllSaves();
                }
                //save with save name
                else if (separatedWords[0] == "save"
                         && separatedWords.Count == 2
                         && currentScene == 1
                         && PlayerStatsScript.currentHealth > 0)
                //&& PlayerHealthScript.isPlayerAlive)
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
                else if (separatedWords[0] == "reke"
                         && separatedWords.Count == 1)
                {
                    Command_ResetAllKeyBindings();
                }
                //list all key bindings and their values
                else if (separatedWords[0] == "shke"
                         && separatedWords.Count == 1)
                {
                    Command_ShowAllKeyBindings();
                }
                //set keybindingname to keybindingvalue
                else if (separatedWords[0] == "seke"
                         && separatedWords.Count == 3)
                {
                    Command_SetKeyBinding();
                }

                //reset all settings to default values
                else if (separatedWords[0] == "rese"
                         && separatedWords.Count == 1
                         && currentScene == 1)
                {
                    Command_ResetAllSettings();
                }
                //list all settings and their values
                else if (separatedWords[0] == "shse"
                         && separatedWords.Count == 1
                         && currentScene == 1)
                {
                    Command_ShowAllSettings();
                }
                //set settingsname to settingsvalue
                else if (separatedWords[0] == "sese"
                         && separatedWords.Count == 3
                         && currentScene == 1)
                {
                    Command_SetSettings();
                }

                //restart game from the beginning
                else if (separatedWords[0] == "restart"
                         && separatedWords.Count == 1)
                         //&& PlayerHealthScript.isPlayerAlive)
                {
                    Command_Restart();
                }
                //quit game
                else if (separatedWords[0] == "quit"
                         && separatedWords.Count == 1)
                {
                    Command_Quit();
                }

                //list all player commands
                else if (separatedWords[0] == "help"
                         && separatedWords[1] == "player"
                         && separatedWords.Count == 2
                         && currentScene == 1)
                {
                    Command_Help();
                }
                //player commands
                else if (separatedWords[0] == "player"
                         && currentScene == 1)
                {
                    //teleport player
                    if (separatedWords[1] == "tp"
                        && separatedWords.Count == 5)
                    {
                        Command_TeleportPlayer();
                    }

                    //show player stats
                    else if (separatedWords[1] == "shst"
                             && separatedWords.Count == 2)
                    {
                        Command_ShowPlayerStats();
                    }
                    //reset player stats
                    else if (separatedWords[1] == "rest"
                             && separatedWords.Count == 2
                             && PlayerStatsScript.currentHealth > 0)
                    {
                        Command_ResetPlayerStats();
                    }
                    //set player stat
                    else if (separatedWords[1] == "sest"
                             && separatedWords.Count == 4
                             && PlayerStatsScript.currentHealth > 0)
                    {
                        Command_SetPlayerStat();
                    }

                    //list all item types
                    else if (separatedWords[1] == "sait"
                             && separatedWords.Count == 2)
                    {
                        Command_ShowAllItemTypes();
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
                            CreateNewConsoleLine("Error: Inserted item type first letter is invalid! It must be a lower-case english alphabet letter.", "CONSOLE_ERROR_MESSAGE");
                        }
                    }
                    //list all player items, their counts and protected state
                    else if (separatedWords[1] == "sapi"
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
                    else if (separatedWords[1] == "shis"
                             && separatedWords.Count == 4
                             && PlayerStatsScript.currentHealth > 0)
                    {
                        Command_ShowItemStats();
                    }
                    //set item stat
                    else if (separatedWords[1] == "sest"
                             && separatedWords.Count == 6
                             && PlayerStatsScript.currentHealth > 0)
                    {
                        Command_SetItemStat();
                    }

                    else
                    {
                        insertedCommands.Add(input);
                        currentSelectedInsertedCommand = insertedCommands.Count - 1;

                        CreateNewConsoleLine("Error: Unknown or incorrect command or command is disabled (either player is dead or this command is not allowed in this scene)! Type help to list all commands.", "CONSOLE_ERROR_MESSAGE");
                    }
                }

                //select target
                else if (separatedWords[0] == "st"
                         && separatedWords.Count == 1
                         && currentScene == 1)
                {
                    Command_SelectTarget();
                }
                //list all target commands
                else if (separatedWords[0] == "help"
                         && separatedWords[1] == "target"
                         && separatedWords.Count == 2
                         && currentScene == 1)
                {
                    Command_Help();
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

                        CreateNewConsoleLine("Error: Unknown or incorrect command or command is disabled (either player is dead or this command is not allowed in this scene)! Type help to list all commands.", "CONSOLE_ERROR_MESSAGE");
                    }
                }

                else
                {
                    insertedCommands.Add(input);
                    currentSelectedInsertedCommand = insertedCommands.Count - 1;

                    CreateNewConsoleLine("Error: Unknown or incorrect command or command is disabled (either player is dead or this command is not allowed in this scene)! Type help to list all commands.", "CONSOLE_ERROR_MESSAGE");
                }
            }
        }
        else
        {
            insertedCommands.Add(input);
            currentSelectedInsertedCommand = insertedCommands.Count - 1;

            CreateNewConsoleLine("Error: No command was inserted! Type help to list all commands.", "CONSOLE_ERROR_MESSAGE");
        }

        separatedWords.Clear();

        input = "";
    }

    //help command
    private void Command_Help()
    {
        if (separatedWords.Count == 1)
        {
            CreateNewConsoleLine("---GLOBAL COMMANDS---.", "CONSOLE_INFO_MESSAGE");
            CreateNewConsoleLine("clear - clear console log.", "CONSOLE_INFO_MESSAGE");
            CreateNewConsoleLine("tdm - toggle debug menu.", "CONSOLE_INFO_MESSAGE");

            CreateNewConsoleLine("tgm - toggle godmode.", "CONSOLE_INFO_MESSAGE");
            CreateNewConsoleLine("tnc - toggle noclip", "CONSOLE_INFO_MESSAGE");

            CreateNewConsoleLine("shsa - show all game saves.", "CONSOLE_INFO_MESSAGE");
            CreateNewConsoleLine("desa - delete all game saves [WARNING: THIS CANNOT BE UNDONE]", "CONSOLE_INFO_MESSAGE");
            CreateNewConsoleLine("save string - save game with selected save name.", "CONSOLE_INFO_MESSAGE");
            CreateNewConsoleLine("load string - load game with selected save name.", "CONSOLE_INFO_MESSAGE");

            CreateNewConsoleLine("reke - reset all key bindings to default values.", "CONSOLE_INFO_MESSAGE");
            CreateNewConsoleLine("shke - list all key bindings and their current values.", "CONSOLE_INFO_MESSAGE");
            CreateNewConsoleLine("seke string string - set selected key binding to value.", "CONSOLE_INFO_MESSAGE");

            CreateNewConsoleLine("rese - reset all settings to default values.", "CONSOLE_INFO_MESSAGE");
            CreateNewConsoleLine("shse - list all settings and their current values.", "CONSOLE_INFO_MESSAGE");
            CreateNewConsoleLine("sese string string - set selected setting to value.", "CONSOLE_INFO_MESSAGE");

            CreateNewConsoleLine("restart - restart the game from the beginning.", "CONSOLE_INFO_MESSAGE");
            CreateNewConsoleLine("quit - quit game.", "CONSOLE_INFO_MESSAGE");

            CreateNewConsoleLine("help player - list all player commands.", "CONSOLE_INFO_MESSAGE");
            CreateNewConsoleLine("help target - list all target commands.", "CONSOLE_INFO_MESSAGE");
        }
        else if (separatedWords.Count > 1)
        {
            if (separatedWords[1] == "player")
            {
                CreateNewConsoleLine("---PLAYER COMMANDS---.", "CONSOLE_INFO_MESSAGE");
                CreateNewConsoleLine("player tp int int int - teleport player to xyz coordinates.", "CONSOLE_INFO_MESSAGE");
                CreateNewConsoleLine("player shst - list all player stats.", "CONSOLE_INFO_MESSAGE");
                CreateNewConsoleLine("player rest - reset all player stats.", "CONSOLE_INFO_MESSAGE");
                CreateNewConsoleLine("player sest string string - set selected player stat to value.", "CONSOLE_INFO_MESSAGE");
                CreateNewConsoleLine("player sait - list all spawnable item types.", "CONSOLE_INFO_MESSAGE");
                CreateNewConsoleLine("player shwe/shar/shsh/shco/shai/shsp/sham/shmi char - list all items of the selected item type that start with the selected english alphabet letter.", "CONSOLE_INFO_MESSAGE");
                CreateNewConsoleLine("player sapi - list all player inventory items, their counts and protected state.", "CONSOLE_INFO_MESSAGE");
                CreateNewConsoleLine("player additem string int - add selected count of items to player inventory.", "CONSOLE_INFO_MESSAGE");
                CreateNewConsoleLine("player removeitem string int - remove selected count of items from players inventory.", "CONSOLE_INFO_MESSAGE");
                CreateNewConsoleLine("player shis string int - list all selected item stats.", "CONSOLE_INFO_MESSAGE");
                CreateNewConsoleLine("player sest string int - set selected item stat to value.", "CONSOLE_INFO_MESSAGE");
            }
            else if (separatedWords[1] == "target")
            {
                CreateNewConsoleLine("---TARGET COMMANDS---.", "CONSOLE_INFO_MESSAGE");
                CreateNewConsoleLine("st - select target with left mouse button (key binding not editable).", "CONSOLE_INFO_MESSAGE");
                CreateNewConsoleLine("target unlock - unlock selected target.", "CONSOLE_INFO_MESSAGE");
            }
        }
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

            CreateNewConsoleLine("Enabled debug menu.", "CONSOLE_INFO_MESSAGE");
            debugMenuEnabled = true;
        }
        else
        {
            UIReuseScript.par_DebugMenu.transform.position += new Vector3(0, 114, 0);

            CreateNewConsoleLine("Disabled debug menu.", "CONSOLE_INFO_MESSAGE");
            debugMenuEnabled = false;
        }
    }

    //toggle godmode
    private void Command_ToggleGodMode()
    {
        PlayerStatsScript.isGodmodeEnabled = !PlayerStatsScript.isGodmodeEnabled;
        if (!PlayerStatsScript.isGodmodeEnabled)
        {
            CreateNewConsoleLine("Disabled god mode.", "CONSOLE_INFO_MESSAGE");
        }
        else
        {
            CreateNewConsoleLine("Enabled god mode.", "CONSOLE_INFO_MESSAGE");
        }
    }
    //toggle noclip
    private void Command_ToggleNoclip()
    {
        PlayerMovementScript.isNoclipping = !PlayerMovementScript.isNoclipping;
        if (!PlayerMovementScript.isNoclipping)
        {
            CreateNewConsoleLine("Disabled noclipping.", "CONSOLE_INFO_MESSAGE");
        }
        else
        {
            CreateNewConsoleLine("Enabled noclipping.", "CONSOLE_INFO_MESSAGE");
        }
    }

    //list all game saves
    private void Command_ShowAllSaves()
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
                        CreateNewConsoleLine(saveName.Replace(".txt", ""), "CONSOLE_INFO_MESSAGE");
                    }
                }
            }
            else
            {
                CreateNewConsoleLine("Error: Cannot list any saves because save folder at path " + path + " is empty!", "CONSOLE_INFO_MESSAGE");
            }
        }
        else
        {
            CreateNewConsoleLine("Error: Cannot find game saves folder!", "CONSOLE_ERROR_MESSAGE");
        }
    }
    //delete all game saves
    private void Command_DeleteAllSaves()
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

            CreateNewConsoleLine("Successfully deleted all save files from " + path + "!", "CONSOLE_SUCCESS_MESSAGE");
        }
        else
        {
            CreateNewConsoleLine("Error: " + path + " has no save files to delete!", "CONSOLE_ERROR_MESSAGE");
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
            CreateNewConsoleLine("Error: Save file " + separatedWords[1] + " does not exist!", "CONSOLE_ERROR_MESSAGE");
        }
    }

    //reset all key bindings to default values
    private void Command_ResetAllKeyBindings()
    {
        KeyBindingsScript.ResetKeyBindings(true);
        CreateNewConsoleLine("Successfully reset " + KeyBindingsScript.KeyBindings.Count + " key bindings!", "CONSOLE_SUCCESS_MESSAGE");
    }
    //list all key bindings and their values
    private void Command_ShowAllKeyBindings()
    {
        CreateNewConsoleLine("---KEY BINDINGS---", "CONSOLE_INFO_MESSAGE");
        foreach (KeyValuePair<string, KeyCode> dict in KeyBindingsScript.KeyBindings)
        {
            string key = dict.Key;
            KeyCode value = dict.Value;
            CreateNewConsoleLine(key + ": " + value.ToString().Replace("KeyCode.", ""), "CONSOLE_INFO_MESSAGE");
        }
    }
    //set keybindingname to keybindingvalue
    private void Command_SetKeyBinding()
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

                    CreateNewConsoleLine("Successfully set " + userKey + "s keycode to " + userValue + "!", "CONSOLE_SUCCESS_MESSAGE");
                    break;
                }
            }
        }
        else
        {
            CreateNewConsoleLine("Error: Inserted key binding name " + userKey + " or key binding value " + userValue.ToString().Replace("KeyCode.", "") + " does not exist! Type showkeybindings to list all key bindings.", "CONSOLE_ERROR_MESSAGE");
        }
    }

    //reset all settings to default values
    private void Command_ResetAllSettings()
    {
        SettingsScript.ResetSettings(true);
        CreateNewConsoleLine("Successfully reset all settings to default values!", "CONSOLE_SUCCESS_MESSAGE");
    }
    //list all settings and their values
    private void Command_ShowAllSettings()
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
                CreateNewConsoleLine("difficulty: " + SettingsScript.user_Difficulty, "CONSOLE_INFO_MESSAGE");
            }
            else if (info == "MouseSpeed")
            {
                CreateNewConsoleLine("mouse_speed: " + SettingsScript.user_MouseSpeed, "CONSOLE_INFO_MESSAGE");
            }

            //graphics settings
            else if (info == "Preset")
            {
                CreateNewConsoleLine("preset: " + SettingsScript.user_Preset, "CONSOLE_INFO_MESSAGE");
            }
            else if (info == "Resolution")
            {
                CreateNewConsoleLine("resolution: " + SettingsScript.user_Resolution.ToString().Replace("res_", ""), "CONSOLE_INFO_MESSAGE");
            }
            else if (info == "FullScreenMode")
            {
                CreateNewConsoleLine("fullscreen_mode: " + SettingsScript.user_FullScreenMode, "CONSOLE_INFO_MESSAGE");
            }
            else if (info == "FieldOfView")
            {
                CreateNewConsoleLine("field_of_view: " + SettingsScript.user_FieldOfView, "CONSOLE_INFO_MESSAGE");
            }
            else if (info == "VSyncState")
            {
                CreateNewConsoleLine("vsync_state: " + SettingsScript.user_EnableVSync, "CONSOLE_INFO_MESSAGE");
            }
            else if (info == "TextureQuality")
            {
                CreateNewConsoleLine("texture_quality: " + SettingsScript.user_TextureQuality, "CONSOLE_INFO_MESSAGE");
            }
            else if (info == "LightDistance")
            {
                CreateNewConsoleLine("light_distance: " + SettingsScript.user_LightDistance, "CONSOLE_INFO_MESSAGE");
            }
            else if (info == "ShadowDistance")
            {
                CreateNewConsoleLine("shadow_distance: " + SettingsScript.user_ShadowDistance, "CONSOLE_INFO_MESSAGE");
            }
            else if (info == "ShadowQuality")
            {
                CreateNewConsoleLine("shadow_quality: " + SettingsScript.user_ShadowDistance, "CONSOLE_INFO_MESSAGE");
            }
            else if (info == "TreeDistance")
            {
                CreateNewConsoleLine("tree_distance: " + SettingsScript.user_TreeDistance, "CONSOLE_INFO_MESSAGE");
            }
            else if (info == "GrassDistance")
            {
                CreateNewConsoleLine("grass_distance: " + SettingsScript.user_GrassDistance, "CONSOLE_INFO_MESSAGE");
            }
            else if (info == "ObjectDistance")
            {
                CreateNewConsoleLine("object_distance: " + SettingsScript.user_ObjectDistance, "CONSOLE_INFO_MESSAGE");
            }
            else if (info == "item_Distance")
            {
                CreateNewConsoleLine("item_distance: " + SettingsScript.user_ItemDistance, "CONSOLE_INFO_MESSAGE");
            }
            else if (info == "AIDistance")
            {
                CreateNewConsoleLine("ai_distance: " + SettingsScript.user_AIDistance, "CONSOLE_INFO_MESSAGE");
            }

            //audio settings
            else if (info == "MasterVolume")
            {
                CreateNewConsoleLine("master_volume: " + SettingsScript.user_MasterVolume, "CONSOLE_INFO_MESSAGE");
            }
            else if (info == "MusicVolume")
            {
                CreateNewConsoleLine("music_volume: " + SettingsScript.user_MusicVolume, "CONSOLE_INFO_MESSAGE");
            }
            else if (info == "SFXVolume")
            {
                CreateNewConsoleLine("sfx_volume: " + SettingsScript.user_SFXVolume, "CONSOLE_INFO_MESSAGE");
            }
            else if (info == "NPCVolume")
            {
                CreateNewConsoleLine("npc_volume: " + SettingsScript.user_NPCVolume, "CONSOLE_INFO_MESSAGE");
            }
        }
    }
    //set settingsname to settingsvalue
    private void Command_SetSettings()
    {
        string type = separatedWords[1];
        string value = separatedWords[2];

        bool isTypeFloat = float.TryParse(type, out _);
        bool isTypeInt = int.TryParse(type, out _);

        if (!isTypeFloat
            && !isTypeInt)
        {
            //general settings
            if (type == "difficulty")
            {
                int val = int.Parse(value);
                if (val >= -100
                    && val <= 100)
                {
                    SettingsScript.user_Difficulty = val;

                    PauseMenuScript.PauseWithUI();
                    PauseMenuScript.ShowSettingsContent();
                    SettingsScript.RebuildSettingsList("general");
                    PauseMenuScript.UnpauseGame();

                    SettingsScript.SaveSettings();
                    CreateNewConsoleLine("Successfully changed " + type + " to " + value + "!", "CONSOLE_SUCCESS_MESSAGE");
                }
                else
                {
                    CreateNewConsoleLine("Error: Invalid value for " + type + "! Must be between -100 and 100.", "CONSOLE_ERROR_MESSAGE");
                }
            }
            else if (type == "mouse_speed")
            {
                int val = int.Parse(value);
                if (val >= 1
                    && val <= 100)
                {
                    SettingsScript.user_MouseSpeed = val;

                    PauseMenuScript.PauseWithUI();
                    PauseMenuScript.ShowSettingsContent();
                    SettingsScript.RebuildSettingsList("general");
                    PauseMenuScript.UnpauseGame();

                    SettingsScript.SaveSettings();
                    CreateNewConsoleLine("Successfully changed " + type + " to " + value + "!", "CONSOLE_SUCCESS_MESSAGE");
                }
                else
                {
                    CreateNewConsoleLine("Error: Invalid value for " + type + "! Must be between 1 and 100.", "CONSOLE_ERROR_MESSAGE");
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

                if (foundAvailableValue)
                {
                    SettingsScript.user_Preset = (UserDefined_Preset)Enum.Parse(typeof(UserDefined_Preset), value);

                    PauseMenuScript.PauseWithUI();
                    PauseMenuScript.ShowSettingsContent();
                    SettingsScript.RebuildSettingsList("graphics");
                    PauseMenuScript.UnpauseGame();

                    SettingsScript.SaveSettings();
                    CreateNewConsoleLine("Successfully changed " + type + " to " + value + "!", "CONSOLE_SUCCESS_MESSAGE");
                }
                else
                {
                    CreateNewConsoleLine("Error: Invalid value for " + type + "! Must be low, medium, high or ultra.", "CONSOLE_ERROR_MESSAGE");
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

                if (foundAvailableValue)
                {
                    SettingsScript.user_Resolution = (UserDefined_Resolution)Enum.Parse(typeof(UserDefined_Resolution), "res_" + value);

                    PauseMenuScript.PauseWithUI();
                    PauseMenuScript.ShowSettingsContent();
                    SettingsScript.RebuildSettingsList("graphics");
                    PauseMenuScript.UnpauseGame();

                    SettingsScript.SaveSettings();
                    CreateNewConsoleLine("Successfully changed " + type + " to " + value + "!", "CONSOLE_SUCCESS_MESSAGE");
                }
                else
                {
                    CreateNewConsoleLine("Error: Invalid value for " + type + "! Must be 1280x720, 1366x768, 1920x1080, 2160x1440, 2560x1080, 3440x1440, 3840x1080, 3840x2160 or 5120x1440.", "CONSOLE_ERROR_MESSAGE");
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

                if (foundAvailableValue)
                {
                    SettingsScript.user_Resolution = (UserDefined_Resolution)Enum.Parse(typeof(UserDefined_Resolution), value);

                    PauseMenuScript.PauseWithUI();
                    PauseMenuScript.ShowSettingsContent();
                    SettingsScript.RebuildSettingsList("graphics");
                    PauseMenuScript.UnpauseGame();

                    SettingsScript.SaveSettings();
                    CreateNewConsoleLine("Successfully changed " + type + " to " + value + "!", "CONSOLE_SUCCESS_MESSAGE");
                }
                else
                {
                    CreateNewConsoleLine("Error: Invalid value for " + type + "! Must be MaximizedWindow, ExclusiveFullScreen, Windowed or FullScreenWindow.", "CONSOLE_ERROR_MESSAGE");
                }
            }
            else if (type == "field_of_view")
            {
                int val = int.Parse(value);
                if (val >= 70
                    && val <= 110)
                {
                    SettingsScript.user_FieldOfView = val;

                    PauseMenuScript.PauseWithUI();
                    PauseMenuScript.ShowSettingsContent();
                    SettingsScript.RebuildSettingsList("graphics");
                    PauseMenuScript.UnpauseGame();

                    SettingsScript.SaveSettings();
                    CreateNewConsoleLine("Successfully changed " + type + " to " + value + "!", "CONSOLE_SUCCESS_MESSAGE");
                }
                else
                {
                    CreateNewConsoleLine("Error: Invalid value for " + type + "! Must be between 70 and 110.", "CONSOLE_ERROR_MESSAGE");
                }
            }
            else if (type == "vsync_state")
            {
                if (value == "true"
                    || value == "false")
                {
                    SettingsScript.user_EnableVSync = value;

                    PauseMenuScript.PauseWithUI();
                    PauseMenuScript.ShowSettingsContent();
                    SettingsScript.RebuildSettingsList("graphics");
                    PauseMenuScript.UnpauseGame();

                    SettingsScript.SaveSettings();
                    CreateNewConsoleLine("Successfully changed " + type + " to " + value + "!", "CONSOLE_SUCCESS_MESSAGE");
                }
                else
                {
                    CreateNewConsoleLine("Error: Invalid value for " + type + "! Must be true or false.", "CONSOLE_ERROR_MESSAGE");
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

                if (foundAvailableValue)
                {
                    SettingsScript.user_TextureQuality = (UserDefined_TextureQuality)Enum.Parse(typeof(UserDefined_TextureQuality), value);

                    PauseMenuScript.PauseWithUI();
                    PauseMenuScript.ShowSettingsContent();
                    SettingsScript.RebuildSettingsList("graphics");
                    PauseMenuScript.UnpauseGame();

                    SettingsScript.SaveSettings();
                    CreateNewConsoleLine("Successfully changed " + type + " to " + value + "!", "CONSOLE_SUCCESS_MESSAGE");
                }
                else
                {
                    CreateNewConsoleLine("Error: Invalid value for " + type + "! Must be low, medium, high or ultra.", "CONSOLE_ERROR_MESSAGE");
                }
            }
            else if (type == "light_distance")
            {
                int val = int.Parse(value);
                if (val >= 15
                    && val <= 5000)
                {
                    SettingsScript.user_LightDistance = val;

                    PauseMenuScript.PauseWithUI();
                    PauseMenuScript.ShowSettingsContent();
                    SettingsScript.RebuildSettingsList("graphics");
                    PauseMenuScript.UnpauseGame();

                    SettingsScript.SaveSettings();
                    CreateNewConsoleLine("Successfully changed " + type + " to " + value + "!", "CONSOLE_SUCCESS_MESSAGE");
                }
                else
                {
                    CreateNewConsoleLine("Error: Invalid value for " + type + "! Must be between 15 and 5000.", "CONSOLE_ERROR_MESSAGE");
                }
            }
            else if (type == "shadow_distance")
            {
                int val = int.Parse(value);
                if (val >= 15
                    && val <= 5000)
                {
                    SettingsScript.user_ShadowDistance = val;

                    PauseMenuScript.PauseWithUI();
                    PauseMenuScript.ShowSettingsContent();
                    SettingsScript.RebuildSettingsList("graphics");
                    PauseMenuScript.UnpauseGame();

                    SettingsScript.SaveSettings();
                    CreateNewConsoleLine("Successfully changed " + type + " to " + value + "!", "CONSOLE_SUCCESS_MESSAGE");
                }
                else
                {
                    CreateNewConsoleLine("Error: Invalid value for " + type + "! Must be between 15 and 5000.", "CONSOLE_ERROR_MESSAGE");
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

                if (foundAvailableValue)
                {
                    SettingsScript.user_ShadowQuality = (UserDefined_ShadowQuality)Enum.Parse(typeof(UserDefined_ShadowQuality), value);

                    PauseMenuScript.PauseWithUI();
                    PauseMenuScript.ShowSettingsContent();
                    SettingsScript.RebuildSettingsList("graphics");
                    PauseMenuScript.UnpauseGame();

                    SettingsScript.SaveSettings();
                    CreateNewConsoleLine("Successfully changed " + type + " to " + value + "!", "CONSOLE_SUCCESS_MESSAGE");
                }
                else
                {
                    CreateNewConsoleLine("Error: Invalid value for " + type + "! Must be low, medium, high or ultra.", "CONSOLE_ERROR_MESSAGE");
                }
            }
            else if (type == "tree_distance")
            {
                int val = int.Parse(value);
                if (val >= 15
                    && val <= 5000)
                {
                    SettingsScript.user_TreeDistance = val;

                    PauseMenuScript.PauseWithUI();
                    PauseMenuScript.ShowSettingsContent();
                    SettingsScript.RebuildSettingsList("graphics");
                    PauseMenuScript.UnpauseGame();

                    SettingsScript.SaveSettings();
                    CreateNewConsoleLine("Successfully changed " + type + " to " + value + "!", "CONSOLE_SUCCESS_MESSAGE");
                }
                else
                {
                    CreateNewConsoleLine("Error: Invalid value for " + type + "! Must be between 15 and 5000.", "CONSOLE_ERROR_MESSAGE");
                }
            }
            else if (type == "grass_distance")
            {
                int val = int.Parse(value);
                if (val >= 15
                    && val <= 5000)
                {
                    SettingsScript.user_GrassDistance = val;

                    PauseMenuScript.PauseWithUI();
                    PauseMenuScript.ShowSettingsContent();
                    SettingsScript.RebuildSettingsList("graphics");
                    PauseMenuScript.UnpauseGame();

                    SettingsScript.SaveSettings();
                    CreateNewConsoleLine("Successfully changed " + type + " to " + value + "!", "CONSOLE_SUCCESS_MESSAGE");
                }
                else
                {
                    CreateNewConsoleLine("Error: Invalid value for " + type + "! Must be between 15 and 5000.", "CONSOLE_ERROR_MESSAGE");
                }
            }
            else if (type == "object_distance")
            {
                int val = int.Parse(value);
                if (val >= 15
                    && val <= 5000)
                {
                    SettingsScript.user_ObjectDistance = val;

                    PauseMenuScript.PauseWithUI();
                    PauseMenuScript.ShowSettingsContent();
                    SettingsScript.RebuildSettingsList("graphics");
                    PauseMenuScript.UnpauseGame();

                    SettingsScript.SaveSettings();
                    CreateNewConsoleLine("Successfully changed " + type + " to " + value + "!", "CONSOLE_SUCCESS_MESSAGE");
                }
                else
                {
                    CreateNewConsoleLine("Error: Invalid value for " + type + "! Must be between 15 and 5000.", "CONSOLE_ERROR_MESSAGE");
                }
            }
            else if (type == "item_distance")
            {
                int val = int.Parse(value);
                if (val >= 15
                    && val <= 5000)
                {
                    SettingsScript.user_ItemDistance = val;

                    PauseMenuScript.PauseWithUI();
                    PauseMenuScript.ShowSettingsContent();
                    SettingsScript.RebuildSettingsList("graphics");
                    PauseMenuScript.UnpauseGame();

                    SettingsScript.SaveSettings();
                    CreateNewConsoleLine("Successfully changed " + type + " to " + value + "!", "CONSOLE_SUCCESS_MESSAGE");
                }
                else
                {
                    CreateNewConsoleLine("Error: Invalid value for " + type + "! Must be between 15 and 5000.", "CONSOLE_ERROR_MESSAGE");
                }
            }
            else if (type == "ai_distance")
            {
                int val = int.Parse(value);
                if (val >= 15
                    && val <= 5000)
                {
                    SettingsScript.user_AIDistance = val;

                    PauseMenuScript.PauseWithUI();
                    PauseMenuScript.ShowSettingsContent();
                    SettingsScript.RebuildSettingsList("graphics");
                    PauseMenuScript.UnpauseGame();

                    SettingsScript.SaveSettings();
                    CreateNewConsoleLine("Successfully changed " + type + " to " + value + "!", "CONSOLE_SUCCESS_MESSAGE");
                }
                else
                {
                    CreateNewConsoleLine("Error: Invalid value for " + type + "! Must be between 15 and 5000.", "CONSOLE_ERROR_MESSAGE");
                }
            }

            //audio settings
            else if (type == "master_volume")
            {
                int val = int.Parse(value);
                if (val >= 15
                    && val <= 5000)
                {
                    SettingsScript.user_MasterVolume = val;

                    PauseMenuScript.PauseWithUI();
                    PauseMenuScript.ShowSettingsContent();
                    SettingsScript.RebuildSettingsList("audio");
                    PauseMenuScript.UnpauseGame();

                    SettingsScript.SaveSettings();
                    CreateNewConsoleLine("Successfully changed " + type + " to " + value + "!", "CONSOLE_SUCCESS_MESSAGE");
                }
                else
                {
                    CreateNewConsoleLine("Error: Invalid value for " + type + "! Must be between 15 and 5000.", "CONSOLE_ERROR_MESSAGE");
                }
            }
            else if (type == "music_volume")
            {
                int val = int.Parse(value);
                if (val >= 15
                    && val <= 5000)
                {
                    SettingsScript.user_MusicVolume = val;

                    PauseMenuScript.PauseWithUI();
                    PauseMenuScript.ShowSettingsContent();
                    SettingsScript.RebuildSettingsList("audio");
                    PauseMenuScript.UnpauseGame();

                    SettingsScript.SaveSettings();
                    CreateNewConsoleLine("Successfully changed " + type + " to " + value + "!", "CONSOLE_SUCCESS_MESSAGE");
                }
                else
                {
                    CreateNewConsoleLine("Error: Invalid value for " + type + "! Must be between 15 and 5000.", "CONSOLE_ERROR_MESSAGE");
                }
            }
            else if (type == "sfx_volume")
            {
                int val = int.Parse(value);
                if (val >= 15
                    && val <= 5000)
                {
                    SettingsScript.user_SFXVolume = val;

                    PauseMenuScript.PauseWithUI();
                    PauseMenuScript.ShowSettingsContent();
                    SettingsScript.RebuildSettingsList("audio");
                    PauseMenuScript.UnpauseGame();

                    SettingsScript.SaveSettings();
                    CreateNewConsoleLine("Successfully changed " + type + " to " + value + "!", "CONSOLE_SUCCESS_MESSAGE");
                }
                else
                {
                    CreateNewConsoleLine("Error: Invalid value for " + type + "! Must be between 15 and 5000.", "CONSOLE_ERROR_MESSAGE");
                }
            }
            else if (type == "npc_volume")
            {
                int val = int.Parse(value);
                if (val >= 15
                    && val <= 5000)
                {
                    SettingsScript.user_NPCVolume = val;

                    PauseMenuScript.PauseWithUI();
                    PauseMenuScript.ShowSettingsContent();
                    SettingsScript.RebuildSettingsList("audio");
                    PauseMenuScript.UnpauseGame();

                    SettingsScript.SaveSettings();
                    CreateNewConsoleLine("Successfully changed " + type + " to " + value + "!", "CONSOLE_SUCCESS_MESSAGE");
                }
                else
                {
                    CreateNewConsoleLine("Error: Invalid value for " + type + "! Must be between 15 and 5000.", "CONSOLE_ERROR_MESSAGE");
                }
            }

            else
            {
                CreateNewConsoleLine("Error: Settings name " + type + " was not found! Type showallsettings to list all game settings.", "CONSOLE_ERROR_MESSAGE");
            }
        }
        else
        {
            CreateNewConsoleLine("Error: Settings name " + type + " cannot be a number!", "CONSOLE_ERROR_MESSAGE");
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

        bool firstFloatCorrect = float.TryParse(secondWord, out float firstVec);
        bool secondFloatCorrect = float.TryParse(thirdWord, out float secondVec);
        bool thirdFloatCorrect = float.TryParse(fourthWord, out float thirdVec);
        if (!firstFloatCorrect)
        {
            CreateNewConsoleLine("Error: Teleport coordinate first input must be a number!", "CONSOLE_ERROR_MESSAGE");
        }
        if (!secondFloatCorrect)
        {
            CreateNewConsoleLine("Error: Teleport coordinate second input must be a number!", "CONSOLE_ERROR_MESSAGE");
        }
        if (!thirdFloatCorrect)
        {
            CreateNewConsoleLine("Error: Teleport coordinate third input must be a number!", "CONSOLE_ERROR_MESSAGE");
        }

        //if all 3 are numbers then assign them as
        //teleport position coordinates and move player to target
        if (firstFloatCorrect && secondFloatCorrect && thirdFloatCorrect)
        {
            if (firstVec >= 1000001 || firstVec <= -1000001)
            {
                CreateNewConsoleLine("Error: Teleport coordinate first input cannot be higher than 1000000 and lower than -1000000!", "CONSOLE_ERROR_MESSAGE");
            }
            if (secondVec >= 1000001 || secondVec <= -1000001)
            {
                CreateNewConsoleLine("Error: Teleport coordinate second input cannot be higher than 1000000 and lower than -1000000!", "CONSOLE_ERROR_MESSAGE");
            }
            if (thirdVec >= 1000001 || thirdVec <= -1000001)
            {
                CreateNewConsoleLine("Error: Teleport coordinate third input cannot be higher than 1000000 and lower than -1000000!", "CONSOLE_ERROR_MESSAGE");
            }

            else
            {
                //set teleport position
                Vector3 teleportPos = new(firstVec, secondVec, thirdVec);

                CreateNewConsoleLine("Successfully teleported player to " + teleportPos + "!", "CONSOLE_SUCCESS_MESSAGE");

                thePlayer.transform.position = teleportPos;
            }
        }
    }

    //list all player stats
    private void Command_ShowPlayerStats()
    {
        CreateNewConsoleLine("---PLAYER MOVEMENT---", "CONSOLE_INFO_MESSAGE");
        CreateNewConsoleLine("walkspeed: " + PlayerStatsScript.walkSpeed, "CONSOLE_INFO_MESSAGE");
        CreateNewConsoleLine("sprintspeed: " + PlayerStatsScript.sprintSpeed, "CONSOLE_INFO_MESSAGE");
        CreateNewConsoleLine("crouchspeed: " + PlayerStatsScript.crouchSpeed, "CONSOLE_INFO_MESSAGE");
        CreateNewConsoleLine("jumpheight: " + PlayerStatsScript.jumpHeight, "CONSOLE_INFO_MESSAGE");
        CreateNewConsoleLine("", "CONSOLE_INFO_MESSAGE");

        CreateNewConsoleLine("---PLAYER CAMERA---", "CONSOLE_INFO_MESSAGE");
        CreateNewConsoleLine("cameramovespeed: " + SettingsScript.user_MouseSpeed, "CONSOLE_INFO_MESSAGE");
        CreateNewConsoleLine("fov: " + SettingsScript.user_FieldOfView, "CONSOLE_INFO_MESSAGE");
        CreateNewConsoleLine("", "CONSOLE_INFO_MESSAGE");

        CreateNewConsoleLine("---PLAYER MAIN VALUES---", "CONSOLE_INFO_MESSAGE");
        CreateNewConsoleLine("Note: only player level is editable, cannot change player required points to level up.", "CONSOLE_INFO_MESSAGE");
        CreateNewConsoleLine("level: " + PlayerStatsScript.level + ", " + PlayerStatsScript.level_PointsToNextLevel, "CONSOLE_INFO_MESSAGE");
        CreateNewConsoleLine("maxhealth: " + PlayerStatsScript.maxHealth, "CONSOLE_INFO_MESSAGE");
        CreateNewConsoleLine("currenthealth: " + PlayerStatsScript.currentHealth, "CONSOLE_INFO_MESSAGE");
        CreateNewConsoleLine("maxStamina: " + PlayerStatsScript.maxStamina, "CONSOLE_INFO_MESSAGE");
        CreateNewConsoleLine("currentStamina: " + PlayerStatsScript.currentStamina, "CONSOLE_INFO_MESSAGE");
        CreateNewConsoleLine("maxmagicka: " + PlayerStatsScript.maxMagicka, "CONSOLE_INFO_MESSAGE");
        CreateNewConsoleLine("currentmagicka: " + PlayerStatsScript.currentMagicka, "CONSOLE_INFO_MESSAGE");
        CreateNewConsoleLine("", "CONSOLE_INFO_MESSAGE");

        CreateNewConsoleLine("---INVENTORY STATS---", "CONSOLE_INFO_MESSAGE");
        CreateNewConsoleLine("maxinvspace: " + PlayerStatsScript.maxInvSpace, "CONSOLE_INFO_MESSAGE");
        CreateNewConsoleLine("invspace (not editable): " + PlayerStatsScript.invSpace, "CONSOLE_INFO_MESSAGE");
        CreateNewConsoleLine("", "CONSOLE_INFO_MESSAGE");

        CreateNewConsoleLine("---PLAYER ATTRIBUTES---", "CONSOLE_INFO_MESSAGE");
        CreateNewConsoleLine("Strength: " + PlayerStatsScript.Attributes["Strength"], "CONSOLE_INFO_MESSAGE");
        CreateNewConsoleLine("Intelligence: " + PlayerStatsScript.Attributes["Intelligence"], "CONSOLE_INFO_MESSAGE");
        CreateNewConsoleLine("Willpower: " + PlayerStatsScript.Attributes["Willpower"], "CONSOLE_INFO_MESSAGE");
        CreateNewConsoleLine("Agility: " + PlayerStatsScript.Attributes["Agility"], "CONSOLE_INFO_MESSAGE");
        CreateNewConsoleLine("Speed: " + PlayerStatsScript.Attributes["Speed"], "CONSOLE_INFO_MESSAGE");
        CreateNewConsoleLine("Endurance: " + PlayerStatsScript.Attributes["Endurance"], "CONSOLE_INFO_MESSAGE");
        CreateNewConsoleLine("Personality: " + PlayerStatsScript.Attributes["Personality"], "CONSOLE_INFO_MESSAGE");
        CreateNewConsoleLine("Luck: " + PlayerStatsScript.Attributes["Luck"], "CONSOLE_INFO_MESSAGE");
        CreateNewConsoleLine("", "CONSOLE_INFO_MESSAGE");

        CreateNewConsoleLine("---PLAYER SKILLS---", "CONSOLE_INFO_MESSAGE");
        CreateNewConsoleLine("Note: only player skill level is editable, cannot change player required points to level up selecte skill.", "CONSOLE_INFO_MESSAGE");
        CreateNewConsoleLine("Blade: " + PlayerStatsScript.Skills["Blade"] + ", " + PlayerStatsScript.SkillPoints["Blade"], "CONSOLE_INFO_MESSAGE");
        CreateNewConsoleLine("Blunt: " + PlayerStatsScript.Skills["Blunt"] + ", " + PlayerStatsScript.SkillPoints["Blunt"], "CONSOLE_INFO_MESSAGE");
        CreateNewConsoleLine("HandToHand: " + PlayerStatsScript.Skills["HandToHand"] + ", " + PlayerStatsScript.SkillPoints["HandToHand"], "CONSOLE_INFO_MESSAGE");
        CreateNewConsoleLine("Armorer: " + PlayerStatsScript.Skills["Armorer"] + ", " + PlayerStatsScript.SkillPoints["Armorer"], "CONSOLE_INFO_MESSAGE");
        CreateNewConsoleLine("Block: " + PlayerStatsScript.Skills["Block"] + ", " + PlayerStatsScript.SkillPoints["Block"], "CONSOLE_INFO_MESSAGE");
        CreateNewConsoleLine("HeavyArmor: " + PlayerStatsScript.Skills["HeavyArmor"] + ", " + PlayerStatsScript.SkillPoints["HeavyArmor"], "CONSOLE_INFO_MESSAGE");
        CreateNewConsoleLine("Athletics: " + PlayerStatsScript.Skills["Athletics"] + ", " + PlayerStatsScript.SkillPoints["Athletics"], "CONSOLE_INFO_MESSAGE");
        CreateNewConsoleLine("Alteration: " + PlayerStatsScript.Skills["Alteration"] + ", " + PlayerStatsScript.SkillPoints["Alteration"], "CONSOLE_INFO_MESSAGE");
        CreateNewConsoleLine("Destruction: " + PlayerStatsScript.Skills["Destruction"] + ", " + PlayerStatsScript.SkillPoints["Destruction"], "CONSOLE_INFO_MESSAGE");
        CreateNewConsoleLine("Restoration: " + PlayerStatsScript.Skills["Restoration"] + ", " + PlayerStatsScript.SkillPoints["Restoration"], "CONSOLE_INFO_MESSAGE");
        CreateNewConsoleLine("Alchemy: " + PlayerStatsScript.Skills["Alchemy"] + ", " + PlayerStatsScript.SkillPoints["Alchemy"], "CONSOLE_INFO_MESSAGE");
        CreateNewConsoleLine("Conjuration: " + PlayerStatsScript.Skills["Conjuration"] + ", " + PlayerStatsScript.SkillPoints["Conjuration"], "CONSOLE_INFO_MESSAGE");
        CreateNewConsoleLine("Mysticism: " + PlayerStatsScript.Skills["Mysticism"] + ", " + PlayerStatsScript.SkillPoints["Mysticism"], "CONSOLE_INFO_MESSAGE");
        CreateNewConsoleLine("Illusion: " + PlayerStatsScript.Skills["Illusion"] + ", " + PlayerStatsScript.SkillPoints["Illusion"], "CONSOLE_INFO_MESSAGE");
        CreateNewConsoleLine("Security: " + PlayerStatsScript.Skills["Security"] + ", " + PlayerStatsScript.SkillPoints["Security"], "CONSOLE_INFO_MESSAGE");
        CreateNewConsoleLine("Sneak: " + PlayerStatsScript.Skills["Sneak"] + ", " + PlayerStatsScript.SkillPoints["Sneak"], "CONSOLE_INFO_MESSAGE");
        CreateNewConsoleLine("Marksman: " + PlayerStatsScript.Skills["Marksman"] + ", " + PlayerStatsScript.SkillPoints["Marksman"], "CONSOLE_INFO_MESSAGE");
        CreateNewConsoleLine("Acrobatics: " + PlayerStatsScript.Skills["Acrobatics"] + ", " + PlayerStatsScript.SkillPoints["Acrobatics"], "CONSOLE_INFO_MESSAGE");
        CreateNewConsoleLine("LightArmor: " + PlayerStatsScript.Skills["LightArmor"] + ", " + PlayerStatsScript.SkillPoints["LightArmor"], "CONSOLE_INFO_MESSAGE");
        CreateNewConsoleLine("Mercantile: " + PlayerStatsScript.Skills["Mercantile"] + ", " + PlayerStatsScript.SkillPoints["Mercantile"], "CONSOLE_INFO_MESSAGE");
        CreateNewConsoleLine("Speechcraft: " + PlayerStatsScript.Skills["Speechcraft"] + ", " + PlayerStatsScript.SkillPoints["Speechcraft"], "CONSOLE_INFO_MESSAGE");
    }
    //reset all player stats
    private void Command_ResetPlayerStats()
    {
        PlayerStatsScript.ResetStats();
        CreateNewConsoleLine("Successfully reset all player stats to their default values!", "CONSOLE_SUCCESS_MESSAGE");
    }
    //set player stat
    private void Command_SetPlayerStat()
    {
        string statName = separatedWords[2];

        bool isFloat = float.TryParse(separatedWords[3], out _);
        if (!isFloat)
        {
            CreateNewConsoleLine("Error: Inserted stat value must be a number!", "CONSOLE_ERROR_MESSAGE");
        }
        else
        {
            float statValue = float.Parse(separatedWords[3]);

            if (statValue >= 0
                && statValue <= 1000000)
            {
                if (statName == "walkspeed")
                {
                    PlayerStatsScript.walkSpeed = (int)statValue;
                    CreateNewConsoleLine("Successfully set player walk speed to " + (int)statValue + "!", "CONSOLE_SUCCESS_MESSAGE");
                }
                else if (statName == "sprintspeed")
                {
                    PlayerStatsScript.sprintSpeed = (int)statValue;
                    CreateNewConsoleLine("Successfully set player sprint speed to " + (int)statValue + "!", "CONSOLE_SUCCESS_MESSAGE");
                }
                else if (statName == "crouchspeed")
                {
                    PlayerStatsScript.crouchSpeed = (int)statValue;
                    CreateNewConsoleLine("Successfully set player crouch speed to " + (int)statValue + "!", "CONSOLE_SUCCESS_MESSAGE");
                }
                else if (statName == "jumpheight")
                {
                    PlayerStatsScript.jumpHeight = (int)statValue;
                    CreateNewConsoleLine("Successfully set player jump height to " + (int)statValue + "!", "CONSOLE_SUCCESS_MESSAGE");
                }
                else if (statName == "cameramovespeed")
                {
                    SettingsScript.user_MouseSpeed = (int)statValue;
                    thePlayer.GetComponentInChildren<Player_Camera>().sensX = (int)statValue;
                    thePlayer.GetComponentInChildren<Player_Camera>().sensY = (int)statValue;
                }
                else if (statName == "fov")
                {
                    if (statValue >= 70
                        && statValue <= 110)
                    {
                        SettingsScript.user_FieldOfView = (int)statValue;
                        thePlayer.GetComponentInChildren<Camera>().fieldOfView = (int)statValue;
                        CreateNewConsoleLine("Successfully set player field of view to " + (int)statValue + "!", "CONSOLE_SUCCESS_MESSAGE");
                    }
                    else
                    {
                        CreateNewConsoleLine("Error: Camera field of view must be between 70 and 110!", "CONSOLE_ERROR_MESSAGE");
                    }
                }
                else if (statName == "level")
                {
                    if (statValue >= 1
                        && statValue <= 100)
                    {
                        PlayerStatsScript.level = (int)statValue;
                        PlayerStatsScript.level_PointsToNextLevel = PlayerStatsScript.level * 500;
                        CreateNewConsoleLine("Successfully set player level to " + (int)statValue + "!", "CONSOLE_SUCCESS_MESSAGE");
                    }
                    else
                    {
                        CreateNewConsoleLine("Error: Player level must be between 1 and 100!", "CONSOLE_ERROR_MESSAGE");
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

                    CreateNewConsoleLine("Successfully set player max health to " + (int)statValue + "!", "CONSOLE_SUCCESS_MESSAGE");
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

                    CreateNewConsoleLine("Successfully set player current health to " + (int)statValue + "!", "CONSOLE_SUCCESS_MESSAGE");
                }
                else if (statName == "maxstamina")
                {
                    PlayerStatsScript.maxStamina = (int)statValue;
                    if (PlayerStatsScript.currentStamina > PlayerStatsScript.maxStamina)
                    {
                        PlayerStatsScript.currentStamina = PlayerStatsScript.maxStamina;
                    }
                    PlayerStatsScript.UpdateBar(PlayerStatsScript.staminaBar);
                    CreateNewConsoleLine("Successfully set player max stamina to " + (int)statValue + "!", "CONSOLE_SUCCESS_MESSAGE");
                }
                else if (statName == "currentstamina")
                {
                    PlayerStatsScript.currentStamina = (int)statValue;
                    if (PlayerStatsScript.currentStamina > PlayerStatsScript.maxStamina)
                    {
                        PlayerStatsScript.maxStamina = PlayerStatsScript.currentStamina;
                    }
                    PlayerStatsScript.UpdateBar(PlayerStatsScript.staminaBar);
                    CreateNewConsoleLine("Successfully set player current stamina to " + (int)statValue + "!", "CONSOLE_SUCCESS_MESSAGE");
                }
                else if (statName == "maxmagicka")
                {
                    PlayerStatsScript.maxMagicka = (int)statValue;
                    if (PlayerStatsScript.currentMagicka > PlayerStatsScript.maxMagicka)
                    {
                        PlayerStatsScript.currentMagicka = PlayerStatsScript.maxMagicka;
                    }
                    PlayerStatsScript.UpdateBar(PlayerStatsScript.magickaBar);
                    CreateNewConsoleLine("Successfully set player max magicka to " + (int)statValue + "!", "CONSOLE_SUCCESS_MESSAGE");
                }
                else if (statName == "currentmagicka")
                {
                    PlayerStatsScript.currentMagicka = (int)statValue;
                    if (PlayerStatsScript.currentMagicka > PlayerStatsScript.maxMagicka)
                    {
                        PlayerStatsScript.maxMagicka = PlayerStatsScript.currentMagicka;
                    }
                    PlayerStatsScript.UpdateBar(PlayerStatsScript.magickaBar);
                    CreateNewConsoleLine("Successfully set player current magicka to " + (int)statValue + "!", "CONSOLE_SUCCESS_MESSAGE");
                }
                else if (statName == "maxinvspace")
                {
                    PlayerStatsScript.maxInvSpace = (int)statValue;
                    CreateNewConsoleLine("Successfully set player max inventory space to " + (int)statValue + "!", "CONSOLE_SUCCESS_MESSAGE");
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
                    if (statValue >= 1
                        && statValue <= 10)
                    {
                        foreach (KeyValuePair<string, int> attributes in PlayerStatsScript.Attributes)
                        {
                            string attributeName = attributes.Key;

                            if (statName == attributeName)
                            {
                                PlayerStatsScript.Attributes[statName] = (int)statValue;

                                CreateNewConsoleLine("Successfully set " + statName + " to " + (int)statValue + "!", "CONSOLE_SUCCESS_MESSAGE");
                                break;
                            }
                        }
                    }
                    else
                    {
                        CreateNewConsoleLine("Error: " + statName + " must be between 1 and 10!", "CONSOLE_ERROR_MESSAGE");
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
                    if (statValue >= 1
                        && statValue <= 100)
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

                        CreateNewConsoleLine("Successfully set " + statName + " to " + (int)statValue + "!", "CONSOLE_SUCCESS_MESSAGE");
                    }
                    else
                    {
                        CreateNewConsoleLine("Error: " + statName + " must be between 1 and 100!", "CONSOLE_ERROR_MESSAGE");
                    }
                }

                else
                {
                    CreateNewConsoleLine("Error: Inserted stat name was not found! Type player showstats to list all player stats.", "CONSOLE_ERROR_MESSAGE");
                }
            }
            else if (statValue < 0
                     || statValue >= 1000001)
            {
                CreateNewConsoleLine("Error: Stat values must be between 0 and 1000000!", "CONSOLE_ERROR_MESSAGE");
            }
        }
    }

    //list all spawnable item types
    private void Command_ShowAllItemTypes()
    {
        CreateNewConsoleLine("shwe - show all weapons", "CONSOLE_INFO_MESSAGE");
        CreateNewConsoleLine("shar - show all armor", "CONSOLE_INFO_MESSAGE");
        CreateNewConsoleLine("shsh - show all shields", "CONSOLE_INFO_MESSAGE");
        CreateNewConsoleLine("shco - show all consumables", "CONSOLE_INFO_MESSAGE");
        CreateNewConsoleLine("shai - show all alchemy ingredients", "CONSOLE_INFO_MESSAGE");
        CreateNewConsoleLine("shsp - show all spells", "CONSOLE_INFO_MESSAGE");
        CreateNewConsoleLine("sham - show all ammo", "CONSOLE_INFO_MESSAGE");
        CreateNewConsoleLine("shmi - show all misc items", "CONSOLE_INFO_MESSAGE");
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
                    CreateNewConsoleLine(item.name, "CONSOLE_INFO_MESSAGE");
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

            CreateNewConsoleLine("Error: No " + type + " found with the beginning letter " + letter + "!", "CONSOLE_ERROR_MESSAGE");
        }
    }
    //list all player inventory items and their count
    private void Command_ShowAllPlayerItems()
    {
        if (PlayerInventoryScript.playerItems.Count > 0)
        {
            foreach (GameObject item in PlayerInventoryScript.playerItems)
            {
                Env_Item ItemScript = item.GetComponent<Env_Item>();

                CreateNewConsoleLine(item.name + " x" + ItemScript.itemCount, "CONSOLE_INFO_MESSAGE");
            }
        }
        else
        {
            CreateNewConsoleLine("Player inventory has no items to display.", "CONSOLE_INFO_MESSAGE");
        }
    }
    //add items to player inventory
    private void Command_AddItem()
    {
        var count = separatedWords[3];
        bool isCountNumber = true;
        foreach (char c in count)
        {
            if (!char.IsDigit(c))
            {
                isCountNumber = false;
                break;
            }
        }

        if (isCountNumber)
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

            if (targetTemplateItem != null)
            {
                if (itemCount >= 1
                    && itemCount <= 1000000)
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
                    int maxInventorySpace = PlayerStatsScript.maxInvSpace;
                    int currentInventorySpace = PlayerStatsScript.invSpace;
                    int totalTakenSpace = ItemScript.itemWeight * itemCount;

                    if ((!ItemScript.isStackable
                        && itemCount == 1)
                        || ItemScript.isStackable)
                    {
                        if (totalTakenSpace <= maxInventorySpace)
                        {
                            if (existingItem != null)
                            {
                                existingItem.GetComponent<Env_Item>().itemCount += itemCount;
                                PlayerStatsScript.invSpace += totalTakenSpace;

                                CreateNewConsoleLine("Successfully added " + itemCount + " " + itemName + "(s) to player inventory!", "CONSOLE_SUCCESS_MESSAGE");
                            }
                            else
                            {
                                GameObject newItem = Instantiate(targetTemplateItem,
                                                                 PlayerInventoryScript.par_PlayerItems.transform.position,
                                                                 Quaternion.identity,
                                                                 PlayerInventoryScript.par_PlayerItems.transform);
                                newItem.SetActive(false);
                                newItem.name = targetTemplateItem.name;
                                newItem.GetComponent<Env_Item>().itemCount = itemCount;

                                PlayerInventoryScript.playerItems.Add(newItem);
                                PlayerStatsScript.invSpace += totalTakenSpace;

                                CreateNewConsoleLine("Successfully added " + itemCount + " " + itemName + "(s) to player inventory!", "CONSOLE_SUCCESS_MESSAGE");
                            }
                        }
                        else
                        {
                            CreateNewConsoleLine("Error: Not enough inventory space to add " + itemCount + " " + itemName + "(s)!", "CONSOLE_ERROR_MESSAGE");
                        }
                    }
                    else
                    {
                        CreateNewConsoleLine("Error: Cannot add more than one of " + itemName + " because it is not stackable!", "CONSOLE_ERROR_MESSAGE");
                    }
                }
                else
                {
                    CreateNewConsoleLine("Error: Cannot spawn more than 1000000 items at once!", "CONSOLE_ERROR_MESSAGE");
                }
            }
            else
            {
                CreateNewConsoleLine("Error: " + itemName + " is an invalid item name! Type player sait to sort through all spawnable item types.", "CONSOLE_ERROR_MESSAGE");
            }
        }
        else
        {
            CreateNewConsoleLine("Error: Item count must be an integer!", "CONSOLE_ERROR_MESSAGE");
        }
    }
    //remove items from player inventory
    private void Command_RemoveItem()
    {
        var count = separatedWords[3];
        bool isCountNumber = true;
        foreach (char c in count)
        {
            if (!char.IsDigit(c))
            {
                isCountNumber = false;
                break;
            }
        }

        if (isCountNumber)
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

            if (targetPlayerItem != null)
            {
                if (!targetPlayerItem.GetComponent<Env_Item>().isProtected)
                {
                    if (itemCount >= 1
                        && itemCount <= targetPlayerItem.GetComponent<Env_Item>().itemCount)
                    {
                        Env_Item ItemScript = targetPlayerItem.GetComponent<Env_Item>();
                        int totalTakenWeight = ItemScript.itemWeight * ItemScript.itemCount;

                        if ((!ItemScript.isStackable
                            && itemCount == 1)
                            || ItemScript.isStackable)
                        {
                            if (itemCount < targetPlayerItem.GetComponent<Env_Item>().itemCount)
                            {
                                targetPlayerItem.GetComponent<Env_Item>().itemCount -= itemCount;
                                PlayerStatsScript.invSpace -= totalTakenWeight;

                                CreateNewConsoleLine("Successfully removed " + itemCount + " " + itemName + "(s) from player inventory!", "CONSOLE_SUCCESS_MESSAGE");
                            }
                            else
                            {
                                PlayerStatsScript.invSpace -= totalTakenWeight;
                                PlayerInventoryScript.playerItems.Remove(targetPlayerItem);
                                Destroy(targetPlayerItem);

                                CreateNewConsoleLine("Successfully removed " + itemCount + " " + itemName + "(s) from player inventory!", "CONSOLE_SUCCESS_MESSAGE");
                            }
                        }
                        else
                        {
                            CreateNewConsoleLine("Error: Cannot remove more than one of " + itemName + " because it is not stackable!", "CONSOLE_ERROR_MESSAGE");
                        }
                    }
                    else
                    {
                        CreateNewConsoleLine("Error: Cannot remove less than 1 and more than total count of the selected item!", "CONSOLE_ERROR_MESSAGE");
                    }
                }
                else
                {
                    CreateNewConsoleLine("Error: Cannot remove protected items from player inventory!", "CONSOLE_ERROR_MESSAGE");
                }
            }
            else
            {
                CreateNewConsoleLine("Error: Did not find " + itemName + "(s) from player inventory! Type player sapi to list all player items.", "CONSOLE_ERROR_MESSAGE");
            }
        }
        else
        {
            CreateNewConsoleLine("Error: Item count must be an integer!", "CONSOLE_ERROR_MESSAGE");
        }
    }
    //show item stats
    private void Command_ShowItemStats()
    {
        GameObject targetPlayerItem = null;
        string itemName = separatedWords[2];

        bool isItemIndexInt = int.TryParse(separatedWords[3], out _);
        if (isItemIndexInt)
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
            CreateNewConsoleLine("Error: Item position or name is invalid! Type player sapi to list all player items.", "CONSOLE_ERROR_MESSAGE");
        }
        else
        {
            Env_Item ItemScript = targetPlayerItem.GetComponent<Env_Item>();
            CreateNewConsoleLine("isProtected (not editable): " + ItemScript.isProtected + "", "CONSOLE_INFO_MESSAGE");
            CreateNewConsoleLine("isStackable (not editable): " + ItemScript.isStackable + "", "CONSOLE_INFO_MESSAGE");
            CreateNewConsoleLine("value: " + ItemScript.itemValue + "", "CONSOLE_INFO_MESSAGE");
            CreateNewConsoleLine("weight: " + ItemScript.itemWeight + "", "CONSOLE_INFO_MESSAGE");
            CreateNewConsoleLine("count: " + ItemScript.itemCount + "", "CONSOLE_INFO_MESSAGE");
            CreateNewConsoleLine("type (not editable): " + ItemScript.itemType.ToString() + "", "CONSOLE_INFO_MESSAGE");
            CreateNewConsoleLine("quality (not editable): " + ItemScript.itemQuality.ToString() + "", "CONSOLE_INFO_MESSAGE");
        }
    }
    //set item stat
    private void Command_SetItemStat()
    {
        GameObject targetPlayerItem = null;
        string itemName = separatedWords[2];

        bool isItemIndexInt = int.TryParse(separatedWords[3], out _);
        if (isItemIndexInt)
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
            CreateNewConsoleLine("Error: Item position or name is invalid! Type player sapi to list all player items.", "CONSOLE_ERROR_MESSAGE");
        }
        else
        {
            Env_Item ItemScript = targetPlayerItem.GetComponent<Env_Item>();
            bool isStatValueInt = int.TryParse(separatedWords[5], out _);

            if (separatedWords[4] != "value"
                && separatedWords[4] != "weight"
                && separatedWords[4] != "count")
            {
                CreateNewConsoleLine("Error: Stat name " + separatedWords[4] + " is invalid or not editable! Type player shis " + separatedWords[1] + " to list all stats for this item.", "CONSOLE_ERROR_MESSAGE");
            }
            else
            {
                if (!isStatValueInt)
                {
                    CreateNewConsoleLine("Error: Stat value " + separatedWords[4] + " must be an integer!", "CONSOLE_ERROR_MESSAGE");
                }
                else
                {
                    int statValue = int.Parse(separatedWords[5]);
                    if (separatedWords[4] == "value")
                    {
                        if (statValue >= 0 
                            && statValue <= 25000)
                        {
                            ItemScript.itemValue = statValue;
                            CreateNewConsoleLine("Successfully set " + separatedWords[2] + " item value to " + statValue + "!", "CONSOLE_SUCCESS_MESSAGE");
                        }
                        else
                        {
                            CreateNewConsoleLine("Error: Item value must be between 0 and 25000!", "CONSOLE_ERROR_MESSAGE");
                        }
                    }
                    else if (separatedWords[4] == "weight")
                    {
                        if (statValue >= 0
                            && statValue <= 100)
                        {
                            ItemScript.itemWeight = statValue;
                            CreateNewConsoleLine("Successfully set " + separatedWords[2] + " item weight to " + statValue + "!", "CONSOLE_SUCCESS_MESSAGE");
                        }
                        else
                        {
                            CreateNewConsoleLine("Error: Item weight must be between 0 and 100!", "CONSOLE_ERROR_MESSAGE");
                        }
                    }
                    else if (separatedWords[4] == "count")
                    {
                        if (ItemScript.isStackable)
                        {
                            if (statValue >= 1
                                && statValue <= 1000000)
                            {
                                int currentSingleWeight = ItemScript.itemWeight;
                                int currentTotalWeight = ItemScript.itemCount * ItemScript.itemWeight;
                                int currentTakenSpace = PlayerStatsScript.invSpace;
                                int maxSpace = PlayerStatsScript.maxInvSpace;

                                int availableSpaceWithoutItem = maxSpace - currentTakenSpace - currentTotalWeight;
                                int availableSpaceWithNewCount = maxSpace - availableSpaceWithoutItem + currentSingleWeight * statValue;

                                if (availableSpaceWithNewCount >= 0)
                                {
                                    ItemScript.itemCount = statValue;
                                    PlayerStatsScript.invSpace = availableSpaceWithNewCount;
                                    CreateNewConsoleLine("Successfully set " + separatedWords[2] + " item count to " + statValue + "!", "CONSOLE_SUCCESS_MESSAGE");
                                }
                                else
                                {
                                    CreateNewConsoleLine("Error: Cannot set item count to " + statValue + " because it would take more space than available space!", "CONSOLE_ERROR_MESSAGE");
                                }
                            }
                            else
                            {
                                CreateNewConsoleLine("Error: Item count must be between 0 and 1000000!", "CONSOLE_ERROR_MESSAGE");
                            }
                        }
                        else
                        {
                            CreateNewConsoleLine("Error: Cannot edit non-stackable item count!", "CONSOLE_ERROR_MESSAGE");
                        }
                    }
                }
            }
        }
    }

    //select target
    private void Command_SelectTarget()
    {
        target = null;
        isSelectingTarget = true;
        CreateNewConsoleLine("Selecting target...", "CONSOLE_INFO_MESSAGE");
    }
    //unlock selected target
    private void Command_UnlockTarget()
    {
        if (target == null)
        {
            CreateNewConsoleLine("Error: No target selected! Type st to select a target.", "CONSOLE_ERROR_MESSAGE");
        }
        else
        {
            //unlock respawnable container
            if (target.GetComponent<UI_Inventory>() 
                && target.GetComponent<UI_Inventory>().containerType
                == UI_Inventory.ContainerType.respawnable)
            {
                if (!target.GetComponent<Env_LockStatus>().isUnlocked)
                {
                    target.GetComponent<Env_LockStatus>().isUnlocked = true;
                    CreateNewConsoleLine("Successfully unlocked container " + target.GetComponent<UI_Inventory>().containerName + "!", "CONSOLE_SUCCESS_MESSAGE");
                }
                else
                {
                    CreateNewConsoleLine("Error: Selected container " + target.GetComponent<UI_Inventory>().containerName + " is already unlocked!", "CONSOLE_ERROR_MESSAGE");
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
                        CreateNewConsoleLine("Successfully unlocked " + DoorManagerScript.doorName + "!", "CONSOLE_SUCCESS_MESSAGE");
                    }
                    else
                    {
                        CreateNewConsoleLine("Error: Selected door " + DoorManagerScript.doorName + " is already unlocked!", "CONSOLE_ERROR_MESSAGE");
                    }
                }
                else if (DoorManagerScript.doorType == Manager_Door.DoorType.gate)
                {
                    DoorManagerScript.OpenDoor();
                    CreateNewConsoleLine("Successfully opened " + DoorManagerScript.doorName + "!", "CONSOLE_SUCCESS_MESSAGE");
                }
            }
            else
            {
                CreateNewConsoleLine("Error: Selected target is not an unlockable object!", "CONSOLE_ERROR_MESSAGE");
            }
        }
    }

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

    private void NewUnitylogMessage(string unusedStackString, LogType type)
    {
        string resultMessage;

        if (output.Contains("Exception")
            || output.Contains("CS")
            || output.Contains("Error"))
        {
            resultMessage = "UNITY_ERROR_MESSAGE] [" + unusedStackString + "] [" + type + "]";
        }
        else
        {
            resultMessage = "UNITY_LOG_MESSAGE";
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
            string msg = "[" + source + "] " + date + " - " + message;

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

    private IEnumerator ConsoleSetupWait()
    {
        yield return new WaitForSeconds(0.5f);
        startedConsoleSetupWait = false;
    }
}