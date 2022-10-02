using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private string str_GameVersion;

    [Header("Scripts")]
    public GameObject thePlayer;

    //public but hidden variables
    [HideInInspector] public string parentPath;
    [HideInInspector] public string gamePath;
    [HideInInspector] public string savePath;
    [HideInInspector] public string settingsPath;
    [HideInInspector] public string debugFilePath;

    //private variables
    private int currentScene;
    private string day;
    private string month;
    private string year;
    private DateTime now;

    //scripts
    private Manager_Settings SettingsScript;
    private Manager_GameSaving SavingScript;
    private UI_PauseMenu PauseMenuScript;
    private Manager_KeyBindings KeyBindingsScript;
    private Manager_Console ConsoleScript;
    private Player_Stats PlayerStatsScript;

    private void Awake()
    {
        SettingsScript = GetComponent<Manager_Settings>();
        KeyBindingsScript = GetComponent<Manager_KeyBindings>();
        SavingScript = GetComponent<Manager_GameSaving>();
        ConsoleScript = GetComponent<Manager_Console>();

        //start recieving unity logs
        Application.logMessageReceived += GetComponent<Manager_Console>().HandleLog;

        parentPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\My Games";
        gamePath = parentPath + @"\OblivionUnity";
        savePath = gamePath + @"\Game saves";
        settingsPath = gamePath + @"\Settings";

        //create game directories
        CreatePaths();

        //get debug file path
        DirectoryInfo dir = new(gamePath);
        FileInfo[] files = dir.GetFiles();
        foreach (FileInfo file in files)
        {
            if (file.Name.Contains("DebugFile_"))
            {
                debugFilePath = file.FullName;
                break;
            }
        }

        currentScene = SceneManager.GetActiveScene().buildIndex;
        if (currentScene == 0)
        {
            //create debug file
            CreateDebugFile();
        }
        else if (currentScene == 1)
        {
            PauseMenuScript = GetComponent<UI_PauseMenu>();
            PlayerStatsScript = thePlayer.GetComponent<Player_Stats>();

            PauseMenuScript.canUnpause = true;
            PauseMenuScript.canTogglePMUI = true;
        }
    }

    private void Start()
    {
        SettingsScript.LoadSettings();
        KeyBindingsScript.LoadKeyBindings();

        //automatically update game version name
        UpdateVersionDate();

        if (currentScene == 1)
        {
            //player stats are always set to default before save is loaded
            PlayerStatsScript.ResetStats();
            //load expected save if any exists
            SavingScript.ReadLoadFile();

            //quickly pause and unpause to prevent game pause issues
            PauseMenuScript.PauseWithoutUI();
            PauseMenuScript.UnpauseGame();
        }

        ConsoleScript.canToggleConsole = true;
    }

    public void CreatePaths()
    {
        //create My Games folder
        if (!Directory.Exists(parentPath))
        {
            Directory.CreateDirectory(parentPath);
        }
        //create game folder
        if (!Directory.Exists(gamePath))
        {
            Directory.CreateDirectory(gamePath);
        }
        //create save path folder
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }
        //create settings path folder
        if (!Directory.Exists(settingsPath))
        {
            Directory.CreateDirectory(settingsPath);
        }
    }

    private void UpdateVersionDate()
    {
        //gets the current date if application is in editor
        if (Application.isEditor)
        {
            now = DateTime.Now;

            day = "";
            month = "";
            if (now.Date.Day.ToString().Length < 2)
            {
                day = "0" + now.Date.Day.ToString();
            }
            else
            {
                day = now.Date.Day.ToString();
            }
            if (now.Date.Month.ToString().Length < 2)
            {
                month = "0" + now.Date.Month.ToString();
            }
            else
            {
                month = now.Date.Month.ToString();
            }
            year = now.Date.Year.ToString();
            string[] splitYear = year.Split('0');
            string date = day + month + splitYear[1];

            GetComponent<Manager_UIReuse>().txt_GameVersion.text = str_GameVersion + "_" + date;
        }
        //otherwise gets the game doc folder last update date
        else
        {
            string path = Application.dataPath;
            string datefilepath = path + @"\datefile.txt";

            if (!File.Exists(datefilepath))
            {
                //using a text editor to write text to the game graphics file
                using StreamWriter dateFile = File.CreateText(datefilepath);

                dateFile.WriteLine("WARNING: This file is not meant to be edited! This is used to confirm the creation date of the game.");
                dateFile.WriteLine("");

                now = DateTime.Now;

                day = "";
                month = "";
                if (now.Date.Day.ToString().Length < 2)
                {
                    day = "0" + now.Date.Day.ToString();
                }
                else
                {
                    day = now.Date.Day.ToString();
                }
                if (now.Date.Month.ToString().Length < 2)
                {
                    month = "0" + now.Date.Month.ToString();
                }
                else
                {
                    month = now.Date.Month.ToString();
                }
                year = now.Date.Year.ToString();
                string[] splitYear = year.Split('0');
                string date = day + month + splitYear[1];

                dateFile.WriteLine("creationDate: " + date);
                GetComponent<Manager_UIReuse>().txt_GameVersion.text = str_GameVersion + "_" + date;
                File.SetAttributes(datefilepath, FileAttributes.Hidden);
            }
            else
            {
                foreach (string line in File.ReadLines(datefilepath))
                {
                    if (line.Contains("creationDate: "))
                    {
                        string[] splitText = line.Split(' ');
                        string creationDate = splitText[1];
                        GetComponent<Manager_UIReuse>().txt_GameVersion.text = str_GameVersion + "_" + creationDate;
                    }
                }
            }
        }
    }

    //creates the debug file
    public void CreateDebugFile()
    {
        //delete old debug file if player switched to main menu scene
        if (currentScene == 0)
        {
            string[] files = Directory.GetFiles(gamePath);
            foreach (string file in files)
            {
                if (file.Contains("DebugFile_"))
                {
                    File.Delete(file);
                    break;
                }
            }
        }

        string date = DateTime.Now.ToString();
        string replaceSlash = date.Replace('/', '_');
        string replaceColon = replaceSlash.Replace(':', '_');
        string replaceEmpty = replaceColon.Replace(' ', '_');
        debugFilePath = gamePath + @"\DebugFile_" + replaceEmpty + ".txt";

        //using a text editor to write new text to new debug file in the debug file path
        using StreamWriter debugFile = File.CreateText(debugFilePath);

        debugFile.WriteLine("Debug information file for " + str_GameVersion);
        debugFile.WriteLine("");

        //add user cpu
        string processorType = SystemInfo.processorType;
        int processorThreadCount = SystemInfo.processorCount;
        int processorFrequency = SystemInfo.processorFrequency;

        debugFile.WriteLine("CPU: " + processorType + "with " + processorThreadCount + " threads at " + processorFrequency + "mhz");
        //add user gpu
        string gpuName = SystemInfo.graphicsDeviceName;
        int gpuMemory = SystemInfo.graphicsMemorySize / 1000;

        debugFile.WriteLine("GPU: " + gpuName + " with " + gpuMemory + "gb memory");
        //add user ram
        int ramSize = SystemInfo.systemMemorySize / 1000;

        debugFile.WriteLine("RAM: " + ramSize + "gb");
        //add user OS
        string osVersion = SystemInfo.operatingSystem;

        debugFile.WriteLine("OS: " + osVersion);

        debugFile.WriteLine("");
    }
}