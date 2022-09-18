using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Manager_Console : MonoBehaviour
{
    [Header("Scripts")]
    [SerializeField] private GameObject par_Managers;
    [SerializeField] private GameObject thePlayer;

    //public but hidden variables
    [HideInInspector] public bool isConsoleOpen;

    //private variables
    private bool startedConsoleSetupWait;
    private string input;
    private string output;
    private string lastOutput;
    private readonly char[] separators = new char[] { ' ' };
    private int currentSelectedInsertedCommand;
    private int currentScene;
    private GameManager GameManagerScript;
    private UI_PauseMenu PauseMenuScript;
    private Manager_UIReuse UIReuseScript;
    private readonly List<string> separatedWords = new();
    private readonly List<GameObject> createdTexts = new();
    private readonly List<string> insertedCommands = new();

    //command variables
    private bool debugMenuEnabled;
    private Player_Stats PlayerStatsScript;

    private void Awake()
    {
        GameManagerScript = GetComponent<GameManager>();
        PauseMenuScript = GetComponent<UI_PauseMenu>();
        UIReuseScript = GetComponent<Manager_UIReuse>();

        currentScene = SceneManager.GetActiveScene().buildIndex;

        if (currentScene == 1)
        {
            PlayerStatsScript = thePlayer.GetComponent<Player_Stats>();
        }

        //debug menu is temporarily open when game is launched
        debugMenuEnabled = true;
    }

    private void Start()
    {
        CreateNewConsoleLine("Game version " + UIReuseScript.txt_GameVersion.text, "CONSOLE_INFO_MESSAGE");
        CreateNewConsoleLine("Type help to list all commands ", "CONSOLE_INFO_MESSAGE");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.PageUp))
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
        foreach (string word in input.Split(separators, StringSplitOptions.RemoveEmptyEntries))
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
                /*
                //show all saves
                else if (separatedWords[0] == "sas"
                         && separatedWords.Count == 1)
                {
                    Command_ShowAllSaves();
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
                //restart game from the beginning
                else if (separatedWords[0] == "restart"
                         && separatedWords.Count == 1)
                //&& PlayerHealthScript.isPlayerAlive)
                {
                    Command_Restart();
                }
                //delete all saves
                else if (separatedWords[0] == "das"
                         && separatedWords.Count == 1)
                {
                    Command_DeleteAllSaves();
                }
                */
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

                        CreateNewConsoleLine("Error: Unknown or incorrect command or command is disabled (either player is dead or this command is not allowed in main menu)! Type help to list all commands.", "CONSOLE_ERROR_MESSAGE");
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

                        CreateNewConsoleLine("Error: Unknown or incorrect command or command is disabled (either player is dead or this command is not allowed in main menu)! Type help to list all commands.", "CONSOLE_ERROR_MESSAGE");
                    }
                }

                else
                {
                    insertedCommands.Add(input);
                    currentSelectedInsertedCommand = insertedCommands.Count - 1;

                    CreateNewConsoleLine("Error: Unknown or incorrect command or command is disabled (either player is dead or this command is not allowed in main menu)! Type help to list all commands.", "CONSOLE_ERROR_MESSAGE");
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
            //CreateNewConsoleLine("sas - show all game saves.", "CONSOLE_INFO_MESSAGE");
            //CreateNewConsoleLine("save savename - save game with save name (GAME SCENE ONLY).", "CONSOLE_INFO_MESSAGE");
            //CreateNewConsoleLine("load loadname - load game with game save name.", "CONSOLE_INFO_MESSAGE");
            CreateNewConsoleLine("restart - restart the game from the beginning.", "CONSOLE_INFO_MESSAGE");
            CreateNewConsoleLine("das - delete all game saves.", "CONSOLE_INFO_MESSAGE");
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

    /*
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
                    else
                    {
                        CreateNewConsoleLine("Error: Invalid save file extention type found at " + path + "!", "CONSOLE_ERROR_MESSAGE");
                    }
                }
            }
            else
            {
                CreateNewConsoleLine("Save folder at path " + path + " is empty.", "CONSOLE_INFO_MESSAGE");
            }
        }
        else
        {
            CreateNewConsoleLine("Error: Cannot find game saves folder!", "CONSOLE_ERROR_MESSAGE");
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
    //restart game from beginning
    private void Command_Restart()
    {
        string loadFilePath = GameManagerScript.gamePath + @"\loadfile.txt";

        //using a text editor to write text to the game save file in the saved file path
        using StreamWriter loadFile = File.CreateText(loadFilePath);

        loadFile.WriteLine("restart");

        SceneManager.LoadScene(1);
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
    */

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
        CreateNewConsoleLine("cameramovespeed: " + PlayerStatsScript.cameraMoveSpeed, "CONSOLE_INFO_MESSAGE");
        CreateNewConsoleLine("fov: " + PlayerStatsScript.fieldOfView, "CONSOLE_INFO_MESSAGE");
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
                    PlayerStatsScript.cameraMoveSpeed = (int)statValue;
                    thePlayer.GetComponentInChildren<Player_Camera>().sensX = (int)statValue;
                    thePlayer.GetComponentInChildren<Player_Camera>().sensY = (int)statValue;
                }
                else if (statName == "fov")
                {
                    if (statValue >= 70
                        && statValue <= 110)
                    {
                        PlayerStatsScript.fieldOfView = (int)statValue;
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
                    CreateNewConsoleLine("Sucessfully set player max health to " + (int)statValue + "!", "CONSOLE_SUCCESS_MESSAGE");
                }
                else if (statName == "currenthealth")
                {
                    PlayerStatsScript.currentHealth = (int)statValue;
                    CreateNewConsoleLine("Sucessfully set player current health to " + (int)statValue + "!", "CONSOLE_SUCCESS_MESSAGE");
                }
                else if (statName == "maxstamina")
                {
                    PlayerStatsScript.maxStamina = (int)statValue;
                    CreateNewConsoleLine("Sucessfully set player max stamina to " + (int)statValue + "!", "CONSOLE_SUCCESS_MESSAGE");
                }
                else if (statName == "currentstamina")
                {
                    PlayerStatsScript.currentStamina = (int)statValue;
                    CreateNewConsoleLine("Sucessfully set player current stamina to " + (int)statValue + "!", "CONSOLE_SUCCESS_MESSAGE");
                }
                else if (statName == "maxmagicka")
                {
                    PlayerStatsScript.maxMagicka = (int)statValue;
                    CreateNewConsoleLine("Sucessfully set player max magicka to " + (int)statValue + "!", "CONSOLE_SUCCESS_MESSAGE");
                }
                else if (statName == "currentmagicka")
                {
                    PlayerStatsScript.currentMagicka = (int)statValue;
                    CreateNewConsoleLine("Sucessfully set player current magicka to " + (int)statValue + "!", "CONSOLE_SUCCESS_MESSAGE");
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

                NewUnitylogMessage();
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

    private void NewUnitylogMessage()
    {
        string resultMessage;

        if (output.Contains("Exception")
            || output.Contains("CS")
            || output.Contains("Error"))
        {
            resultMessage = "UNITY_ERROR_MESSAGE";
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