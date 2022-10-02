using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using static Manager_Settings;

public class Manager_Console : MonoBehaviour
{
    [Header("Scripts")]
    [SerializeField] private GameObject par_Managers;
    [SerializeField] private GameObject thePlayer;

    //public but hidden variables
    [HideInInspector] public bool canToggleConsole;
    [HideInInspector] public bool isConsoleOpen;

    //private variables
    private bool startedConsoleSetupWait;
    private string input;
    private string output;
    private string lastOutput;
    private int currentSelectedInsertedCommand;
    private int currentScene;
    private readonly List<string> separatedWords = new();
    private readonly List<GameObject> createdTexts = new();
    private readonly List<string> insertedCommands = new();

    //command variables
    private bool debugMenuEnabled;
    private Player_Stats PlayerStatsScript;

    //scripts
    private GameManager GameManagerScript;
    private UI_PauseMenu PauseMenuScript;
    private Manager_KeyBindings KeyBindingsScript;
    private Manager_Settings SettingsScript;
    private Manager_UIReuse UIReuseScript;

    private void Awake()
    {
        GameManagerScript = GetComponent<GameManager>();
        PauseMenuScript = GetComponent<UI_PauseMenu>();
        KeyBindingsScript = GetComponent<Manager_KeyBindings>();
        SettingsScript = GetComponent<Manager_Settings>();
        UIReuseScript = GetComponent<Manager_UIReuse>();

        currentScene = SceneManager.GetActiveScene().buildIndex;

        if (currentScene == 1)
        {
            PlayerStatsScript = thePlayer.GetComponent<Player_Stats>();
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
        if (KeyBindingsScript.GetButtonDown("ToggleConsole")
            && canToggleConsole)
        {
            isConsoleOpen = !isConsoleOpen;
        }

        //choose newer and older inserted commands with arrow keys
        if (isConsoleOpen
            && insertedCommands.Count > 0)
        {
            //always picks newest added console command if input field is empty
            if ((Input.GetKeyDown(KeyCode.UpArrow)
                || Input.GetKeyDown(KeyCode.DownArrow))
                && UIReuseScript.consoleInputField.text == "")
            {
                currentSelectedInsertedCommand = insertedCommands.Count - 1;
                UIReuseScript.consoleInputField.text = insertedCommands[^1];
                UIReuseScript.consoleInputField.MoveToEndOfLine(false, false);
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.UpArrow))
                {
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

        //open console
        if (isConsoleOpen
            && !UIReuseScript.par_Console.activeInHierarchy)
        {
            if (currentScene == 1)
            {
                //cannot unpause game while console is open
                PauseMenuScript.canUnpause = false;

                PauseMenuScript.PauseWithoutUI();
            }

            OpenConsole();
        }
        //close console
        else if (!isConsoleOpen
                 && UIReuseScript.par_Console.activeInHierarchy)
        {
            if (currentScene == 1)
            {
                //can unpause game once console is closed
                PauseMenuScript.canUnpause = true;

                if (!PauseMenuScript.isPaused)
                {
                    PauseMenuScript.UnpauseGame();
                }
            }

            CloseConsole();
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
        CreateNewConsoleLine("--" + input + "--", "CONSOLE_INFO_MESSAGE");

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

                //show all saves
                else if (separatedWords[0] == "showallsaves"
                         && separatedWords.Count == 1)
                {
                    Command_ShowAllSaves();
                }
                //delete all saves
                else if (separatedWords[0] == "deleteallsaves"
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
                else if (separatedWords[0] == "resetallkeybindings"
                         && separatedWords.Count == 1)
                {
                    Command_ResetAllKeyBindings();
                }
                //list all key bindings and their values
                else if (separatedWords[0] == "showallkeybindings"
                         && separatedWords.Count == 1)
                {
                    Command_ShowAllKeyBindings();
                }
                //set keybindingname to keybindingvalue
                else if (separatedWords[0] == "setkeybinding"
                         && separatedWords.Count == 3)
                {
                    Command_SetKeyBinding();
                }

                //reset all settings to default values
                else if (separatedWords[0] == "resetallsettings"
                         && separatedWords.Count == 1
                         && currentScene == 1)
                {
                    Command_ResetAllSettings();
                }
                //list all settings and their values
                else if (separatedWords[0] == "showallsettings"
                         && separatedWords.Count == 1
                         && currentScene == 1)
                {
                    Command_ShowAllSettings();
                }
                //set settingsname to settingsvalue
                else if (separatedWords[0] == "setsettings"
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
                    else
                    {
                        insertedCommands.Add(input);
                        currentSelectedInsertedCommand = insertedCommands.Count - 1;

                        CreateNewConsoleLine("Error: Unknown or incorrect command or command is disabled (either player is dead or this command is not allowed in this scene)! Type help to list all commands.", "CONSOLE_ERROR_MESSAGE");
                    }
                }

                //list all target commands
                else if (separatedWords[0] == "help"
                         && separatedWords[1] == "target"
                         && separatedWords.Count == 2
                         && currentScene == 1)
                {
                    Command_Help();
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
                    //disable target
                    if (separatedWords[1] == "disable"
                        && separatedWords.Count == 2)
                    {
                        Command_DisableTarget();
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

            CreateNewConsoleLine("showallsaves - show all game saves.", "CONSOLE_INFO_MESSAGE");
            CreateNewConsoleLine("deleteallsaves - delete all game saves.", "CONSOLE_INFO_MESSAGE");
            CreateNewConsoleLine("save savename - save game with save name (GAME SCENE ONLY).", "CONSOLE_INFO_MESSAGE");
            CreateNewConsoleLine("load loadname - load game with game save name.", "CONSOLE_INFO_MESSAGE");

            CreateNewConsoleLine("resetallkeybindings - reset all key bindings to default values.", "CONSOLE_INFO_MESSAGE");
            CreateNewConsoleLine("showallkeybindings - list all key bindings and their current values.", "CONSOLE_INFO_MESSAGE");
            CreateNewConsoleLine("setkeybinding keybindingname keybindingvalue - set keybindingname to keybindingvalue.", "CONSOLE_INFO_MESSAGE");

            CreateNewConsoleLine("resetallsettings - reset all settings to default values.", "CONSOLE_INFO_MESSAGE");
            CreateNewConsoleLine("showallsettings - list all settings and their current values.", "CONSOLE_INFO_MESSAGE");
            CreateNewConsoleLine("setsettings settingsname settingsvalue - set settingsname to settingsvalue.", "CONSOLE_INFO_MESSAGE");

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
                CreateNewConsoleLine("player tp valx valy valz - teleport player to xyz coordinates.", "CONSOLE_INFO_MESSAGE");
                CreateNewConsoleLine("player showstats - list all player stats.", "CONSOLE_INFO_MESSAGE");
                CreateNewConsoleLine("player resetstats - reset all player stats.", "CONSOLE_INFO_MESSAGE");
                CreateNewConsoleLine("player setstat statName statValue - set player stat statName to statValue.", "CONSOLE_INFO_MESSAGE");
            }
            else if (separatedWords[1] == "target")
            {
                CreateNewConsoleLine("---TARGET COMMANDS---.", "CONSOLE_INFO_MESSAGE");
                CreateNewConsoleLine("st - select target.", "CONSOLE_INFO_MESSAGE");
                CreateNewConsoleLine("target disable - disable selected target.", "CONSOLE_INFO_MESSAGE");
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
            GetComponent<Manager_UIReuse>().par_DebugMenu.transform.position -= new Vector3(0, 114, 0);

            CreateNewConsoleLine("Enabled debug menu.", "CONSOLE_INFO_MESSAGE");
            debugMenuEnabled = true;
        }
        else
        {
            GetComponent<Manager_UIReuse>().par_DebugMenu.transform.position += new Vector3(0, 114, 0);

            CreateNewConsoleLine("Disabled debug menu.", "CONSOLE_INFO_MESSAGE");
            debugMenuEnabled = false;
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
        GetComponent<Manager_GameSaving>().CreateSaveFile(saveName);
    }
    //load game save with name
    private void Command_LoadWithName()
    {
        string path = GameManagerScript.savePath;
        if (File.Exists(path + @"\" + separatedWords[1] + ".txt"))
        {
            GetComponent<Manager_GameSaving>().CreateLoadFile(separatedWords[1] + ".txt");
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

                if (AssignScript.str_Info == userKey)
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
            string info = par.GetComponentInChildren<UI_AssignSettings>().str_Info;

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
        Application.Quit();
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

                CreateNewConsoleLine("Sucessfully teleported player to " + teleportPos + "!", "CONSOLE_SUCCESS_MESSAGE");

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

        CreateNewConsoleLine("---PLAYER STATS---", "CONSOLE_INFO_MESSAGE");
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
    }
    //reset all player stats
    private void Command_ResetPlayerStats()
    {
        PlayerStatsScript.ResetStats();
        CreateNewConsoleLine("Sucessfully reset all player stats to their default values!", "CONSOLE_SUCCESS_MESSAGE");
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
                    CreateNewConsoleLine("Sucessfully set player walk speed to " + (int)statValue + "!", "CONSOLE_SUCCESS_MESSAGE");
                }
                else if (statName == "sprintspeed")
                {
                    PlayerStatsScript.sprintSpeed = (int)statValue;
                    CreateNewConsoleLine("Sucessfully set player sprint speed to " + (int)statValue + "!", "CONSOLE_SUCCESS_MESSAGE");
                }
                else if (statName == "crouchspeed")
                {
                    PlayerStatsScript.crouchSpeed = (int)statValue;
                    CreateNewConsoleLine("Sucessfully set player crouch speed to " + (int)statValue + "!", "CONSOLE_SUCCESS_MESSAGE");
                }
                else if (statName == "jumpheight")
                {
                    PlayerStatsScript.jumpHeight = (int)statValue;
                    CreateNewConsoleLine("Sucessfully set player jump height to " + (int)statValue + "!", "CONSOLE_SUCCESS_MESSAGE");
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
                        CreateNewConsoleLine("Sucessfully set player field of view to " + (int)statValue + "!", "CONSOLE_SUCCESS_MESSAGE");
                    }
                    else
                    {
                        CreateNewConsoleLine("Error: Camera field of view must be between 70 and 110!", "CONSOLE_ERROR_MESSAGE");
                    }
                }
                else if (statName == "maxhealth")
                {
                    PlayerStatsScript.maxHealth = (int)statValue;
                    PlayerStatsScript.UpdateBar(PlayerStatsScript.healthBar);
                    CreateNewConsoleLine("Sucessfully set player max health to " + (int)statValue + "!", "CONSOLE_SUCCESS_MESSAGE");
                }
                else if (statName == "currenthealth")
                {
                    if (statValue > PlayerStatsScript.maxHealth)
                    {
                        CreateNewConsoleLine("Error: Player health must be less than or equal to max health!", "CONSOLE_ERROR_MESSAGE");
                    }
                    else
                    {
                        PlayerStatsScript.currentHealth = (int)statValue;
                        PlayerStatsScript.UpdateBar(PlayerStatsScript.healthBar);
                        CreateNewConsoleLine("Sucessfully set player current health to " + (int)statValue + "!", "CONSOLE_SUCCESS_MESSAGE");
                    }
                }
                else if (statName == "maxstamina")
                {
                    PlayerStatsScript.maxStamina = (int)statValue;
                    PlayerStatsScript.UpdateBar(PlayerStatsScript.staminaBar);
                    CreateNewConsoleLine("Sucessfully set player max stamina to " + (int)statValue + "!", "CONSOLE_SUCCESS_MESSAGE");
                }
                else if (statName == "currentstamina")
                {
                    if (statValue > PlayerStatsScript.maxStamina)
                    {
                        CreateNewConsoleLine("Error: Player stamina must be less than or equal to max stamina!", "CONSOLE_ERROR_MESSAGE");
                    }
                    else
                    {
                        PlayerStatsScript.currentStamina = (int)statValue;
                        PlayerStatsScript.UpdateBar(PlayerStatsScript.staminaBar);
                        CreateNewConsoleLine("Sucessfully set player current stamina to " + (int)statValue + "!", "CONSOLE_SUCCESS_MESSAGE");
                    }
                }
                else if (statName == "maxmagicka")
                {
                    PlayerStatsScript.maxMagicka = (int)statValue;
                    PlayerStatsScript.UpdateBar(PlayerStatsScript.magickaBar);
                    CreateNewConsoleLine("Sucessfully set player max magicka to " + (int)statValue + "!", "CONSOLE_SUCCESS_MESSAGE");
                }
                else if (statName == "currentmagicka")
                {
                    if (statValue > PlayerStatsScript.maxMagicka)
                    {
                        CreateNewConsoleLine("Error: Player magicka must be less than or equal to max magicka!", "CONSOLE_ERROR_MESSAGE");
                    }
                    else
                    {
                        PlayerStatsScript.currentMagicka = (int)statValue;
                        PlayerStatsScript.UpdateBar(PlayerStatsScript.magickaBar);
                        CreateNewConsoleLine("Sucessfully set player current magicka to " + (int)statValue + "!", "CONSOLE_SUCCESS_MESSAGE");
                    }
                }
                else if (statName == "maxinvspace")
                {
                    PlayerStatsScript.maxInvSpace = (int)statValue;
                    CreateNewConsoleLine("Sucessfully set player max inventory space to " + (int)statValue + "!", "CONSOLE_SUCCESS_MESSAGE");
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

    //select target
    private void Command_SelectTarget()
    {
        CreateNewConsoleLine("Command does not yet have any functions...", "CONSOLE_INFO_MESSAGE");
    }
    //disable selected target
    private void Command_DisableTarget()
    {
        CreateNewConsoleLine("Command does not yet have any functions...", "CONSOLE_INFO_MESSAGE");
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