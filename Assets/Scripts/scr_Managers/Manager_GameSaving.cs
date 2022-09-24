using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Linq;

public class Manager_GameSaving : MonoBehaviour
{
    [Header("Scripts")]
    [SerializeField] private GameObject thePlayer;

    //hidden but public variables
    [HideInInspector] public bool allowGameLoadBypass;

    //private variables
    private string str_SaveName;
    private string str_SaveFilePath;
    private int currentScene;
    private readonly List<Button> saveButtons = new();
    private GameManager GameManagerScript;
    private Manager_KeyBindings KeyBindingsScript;
    private Manager_UIReuse UIReuseScript;
    private Player_Stats PlayerStatsScript;

    private void Awake()
    {
        GameManagerScript = GetComponent<GameManager>();
        KeyBindingsScript = GetComponent<Manager_KeyBindings>();
        UIReuseScript = GetComponent<Manager_UIReuse>();

        currentScene = SceneManager.GetActiveScene().buildIndex;

        if (currentScene == 1)
        {
            PlayerStatsScript = thePlayer.GetComponent<Player_Stats>();
        }
    }

    private void Update()
    {
        //save the game
        if (KeyBindingsScript.GetButtonDown("Save")
            && currentScene == 1)
        {
            CreateSaveFile("");
        }
        //load the latest save
        if (KeyBindingsScript.GetButtonDown("Load")
            && currentScene == 1)
        {
            CreateLoadFile("");
        }
    }

    //lists all game saves
    public void ShowGameSaves()
    {
        //only lists game saves if the path exists
        if (GameManagerScript.savePath != "")
        {
            //destroy all previous save buttons if there are any
            if (saveButtons.Count > 0)
            {
                foreach (Button button in saveButtons)
                {
                    Destroy(button.gameObject);
                }
                saveButtons.Clear();
            }

            //get game saves directory
            DirectoryInfo info = new(GameManagerScript.savePath);
            FileInfo[] files = info.GetFiles().OrderBy(p => p.CreationTime).ToArray();
            //if we have any game saves
            if (files.Length > 0)
            {
                //reset previously assigned data
                UIReuseScript.ClearSaveData();
                //load all buttons and their names
                foreach (FileInfo file in files)
                {
                    string filePath = file.Directory.FullName;

                    string fileName = Path.GetFileName(file.Name);
                    DateTime creationDate = file.CreationTime;

                    Button btn_New = Instantiate(UIReuseScript.btn_SaveButtonTemplate);
                    btn_New.transform.SetParent(UIReuseScript.par_SaveButtons, false);

                    btn_New.GetComponentInChildren<TMP_Text>().text = fileName.Replace(".txt", "");

                    btn_New.onClick.AddListener(delegate { ShowSaveData(fileName); });

                    saveButtons.Add(btn_New);
                }
            }
        }
        else
        {
            Debug.LogWarning("Error: Did not find game save path!");
        }
    }
    //shows a few details of the game save
    public void ShowSaveData(string saveName)
    {
        UIReuseScript.btn_LoadGame.interactable = true;
        UIReuseScript.btn_LoadGame.onClick.RemoveAllListeners();

        //always loads the game in main menu scene
        //or loads the game in game scene if confirmation bypass is allowed
        if (currentScene == 0
            || (currentScene == 1
            && allowGameLoadBypass))
        {
            UIReuseScript.btn_LoadGame.onClick.AddListener(delegate { CreateLoadFile(saveName); });
        }
        //asks for confirmation before loading a save in game scene
        else if (currentScene == 1)
        {
            UIReuseScript.btn_LoadGame.onClick.AddListener(delegate {
                GetComponent<UI_Confirmation>().RecieveData(gameObject,
                                                            "saveScript",
                                                            saveName); });
        }

        UIReuseScript.txt_SaveName.text = saveName.Replace(".txt", "");

        DateTime str_SaveDate = File.GetLastWriteTime(GameManagerScript.savePath + @"\" + saveName);
        UIReuseScript.txt_SaveDate.text = str_SaveDate.ToString("d");

        //TODO: save and show save screenshot
    }

    //set up game save file creation
    public void CreateSaveFile(string saveName)
    {
        //creating a regular save file
        if (saveName == "")
        {
            //get all game saves
            string[] saves = Directory.GetFiles(GameManagerScript.savePath);
            //if we have any save files
            if (saves.Length > 0)
            {
                int highestIndex = 1;
                foreach (string file in saves)
                {
                    string fileName = Path.GetFileName(file);
                    string[] splitParts = fileName.Split('.');
                    fileName = splitParts[0];

                    //look for only save files with the correct name
                    if (fileName.Contains("Save_")
                        && fileName.Length > 4
                        && Char.IsDigit(fileName[5]))
                    {
                        //split the file name again with _
                        string[] fileNameSplit = fileName.Split('_');
                        //get the number from the split file name
                        int num = int.Parse(fileNameSplit[1]);

                        //update the highest index to this
                        //if it is higher than the last highest index
                        if (num > highestIndex)
                        {
                            highestIndex = num;
                        }
                    }
                }
                //increase highest index by 1
                highestIndex++;
                //create new file name with new highest index
                str_SaveName = "Save_" + highestIndex;
                //set save path
                str_SaveFilePath = GameManagerScript.savePath + @"\" + str_SaveName + ".txt";
                //create a new save file and fill it with data
                SaveGame();
            }
            //if we dont have any save files
            else
            {
                str_SaveName = "Save_1";
                //set save path
                str_SaveFilePath = GameManagerScript.savePath + @"\" + str_SaveName + ".txt";
                //create a new save file and fill it with data
                SaveGame();
            }
        }
        //creating a save file with a custom name
        else
        {
            bool foundBadSymbol = false;

            //loop through all characters in saveName
            foreach (char c in saveName)
            {
                //if character is illegal
                if (!Char.IsLetterOrDigit(c))
                {
                    foundBadSymbol = true;
                    break;
                }
            }

            if (foundBadSymbol
                || saveName == "reset"
                || saveName.Length >= 11)
            {
                Debug.LogError("Error: Invalid name " + saveName + " for game save. Save name is too long or not allowed!");
            }
            else
            {
                str_SaveName = saveName;

                //set save path
                str_SaveFilePath = GameManagerScript.savePath + @"\" + str_SaveName + ".txt";
                //create a new save file and fill it with data
                SaveGame();
            }
        }
    }
    //create the actual game save file
    private void SaveGame()
    {
        //create a new save file and fill it with data
        using StreamWriter saveFile = File.CreateText(str_SaveFilePath);

        saveFile.WriteLine("Save file for " + UIReuseScript.txt_GameVersion.text + ".");
        saveFile.WriteLine("WARNING: Invalid values will break the game - edit at your own risk!");
        saveFile.WriteLine("");

        saveFile.WriteLine("---GLOBAL VALUES---");
        saveFile.WriteLine("gv_Time: <UNASSIGNED VALUE>");
        saveFile.WriteLine("gv_Difficulty: <UNASSIGNED VALUE>");
        saveFile.WriteLine("");

        saveFile.WriteLine("---PLAYER VALUES---");
        float posX = thePlayer.transform.position.x;
        float posY = thePlayer.transform.position.y;
        float posZ = thePlayer.transform.position.z;
        saveFile.WriteLine("pv_PlayerPosition: " + posX + ", " + posY + ", " + posZ);

        float rotX = thePlayer.transform.rotation.x;
        float rotY = thePlayer.transform.rotation.y;
        float rotZ = thePlayer.transform.rotation.z;
        saveFile.WriteLine("pv_PlayerRotation: " + rotX + ", " + rotY + ", " + rotZ);

        float camRotX = thePlayer.GetComponentInChildren<Camera>().transform.rotation.x;
        float camRotY = thePlayer.GetComponentInChildren<Camera>().transform.rotation.y;
        float camRotZ = thePlayer.GetComponentInChildren<Camera>().transform.rotation.z;
        saveFile.WriteLine("pv_PlayerCameraRotation: " + camRotX + ", " + camRotY + ", " + camRotZ);
        saveFile.WriteLine("");

        saveFile.WriteLine("pv_MouseSpeed: " + PlayerStatsScript.cameraMoveSpeed);
        saveFile.WriteLine("pv_FieldOfView: " + PlayerStatsScript.fieldOfView);
        saveFile.WriteLine("");

        saveFile.WriteLine("---PLAYER STATS---");
        saveFile.WriteLine("ps_MaxHealth: " + PlayerStatsScript.maxHealth);
        saveFile.WriteLine("ps_Health: " + PlayerStatsScript.currentHealth);
        saveFile.WriteLine("ps_MaxStamina: " + PlayerStatsScript.maxStamina);
        saveFile.WriteLine("ps_Stamina: " + PlayerStatsScript.currentStamina);
        saveFile.WriteLine("ps_MaxMagicka: " + PlayerStatsScript.maxMagicka);
        saveFile.WriteLine("ps_Magicka: " + PlayerStatsScript.currentMagicka);
        saveFile.WriteLine("ps_MaxInvSpace: " + PlayerStatsScript.maxInvSpace);

        Debug.Log("Sucessfully saved game to " + str_SaveFilePath + "!");
    }

    //creates a load file where the game scene
    //reads the load file from to load the correct game save
    public void CreateLoadFile(string saveName)
    {
        str_SaveName = "";

        //load newest save
        if (saveName == "")
        {
            //get game saves directory
            DirectoryInfo info = new(GameManagerScript.savePath);
            FileInfo[] files = info.GetFiles().OrderBy(p => p.CreationTime).ToArray();
            //if we have any game saves
            if (files.Length > 0)
            {
                //get newest save file name
                str_SaveName = files.Last().Name;
                Debug.Log(str_SaveName);
            }
            else
            {
                Debug.LogWarning("Error: Cannot load game because no saves were found at " + GameManagerScript.savePath + "!");
            }
        }
        //load custom save
        else
        {
            //get game saves directory
            DirectoryInfo info = new(GameManagerScript.savePath);
            FileInfo[] files = info.GetFiles().OrderBy(p => p.CreationTime).ToArray();
            //if we have any game saves
            if (files.Length > 0)
            {
                foreach (FileInfo file in files)
                {
                    if (saveName == file.Name)
                    {
                        //get custom save file name if it exists
                        str_SaveName = saveName;
                        break;
                    }
                }
            }
            else
            {
                Debug.LogWarning("Error: Cannot load game because no saves were found at " + GameManagerScript.savePath + "!");
            }
        }

        if (str_SaveName != "")
        {
            //create a new load file and add which save file we want to load
            using StreamWriter loadFile = File.CreateText(GameManagerScript.gamePath + @"\loadfile.txt");
            loadFile.WriteLine(str_SaveName);

            //switch to the game scene
            SceneManager.LoadScene(1);
        }
        else
        {
            Debug.LogWarning("Error: Did not find save from " + GameManagerScript.savePath + ". Save name is invalid or not found!");
        }
    }
    //find the correct save file to load from the load file
    public void ReadLoadFile()
    {
        string str_loadFileName = "";

        //get game path
        DirectoryInfo info = new(GameManagerScript.gamePath);
        FileInfo[] files = info.GetFiles().OrderBy(p => p.CreationTime).ToArray();
        //if we have any game saves
        if (files.Length > 0)
        {
            //look through all files
            foreach (FileInfo file in files)
            {
                //if we found the load file
                if (file.Name == "loadfile.txt")
                {
                    //read the first line in the load file
                    str_loadFileName = File.ReadLines(GameManagerScript.gamePath + @"\loadfile.txt").First();
                    break;
                }
            }

            //loads game data if game isnt restarting
            if (str_loadFileName != "restart")
            {
                LoadGame(str_loadFileName);
            }

            //delete load file
            File.Delete(GameManagerScript.gamePath + @"\loadfile.txt");
        }
    }
    //load game data from save file
    private void LoadGame(string saveFileName)
    {
        string str_SaveFileName = "";

        string[] files = Directory.GetFiles(GameManagerScript.savePath);
        foreach (string file in files)
        {
            if (saveFileName != ""
                && file.Contains(saveFileName))
            {
                str_SaveFileName = GameManagerScript.savePath + @"\" + saveFileName;
                break;
            }
        }

        if (str_SaveFileName != "")
        {
            foreach (string line in File.ReadLines(str_SaveFileName))
            {
                //split full line between :
                if (line.Contains(':')
                    && line.Contains('_'))
                {
                    string[] valueSplit = line.Split(':');
                    string[] values = valueSplit[1].Split(',');
                    //split type between _
                    string[] typeSplit = valueSplit[0].Split('_');
                    string type = typeSplit[0];
                    string typeName = typeSplit[1];

                    //load global values
                    if (type == "gv")
                    {
                        //TODO: save and load game time
                        //saveFile.WriteLine("gv_Time: <UNASSIGNED VALUE>");
                    }
                    //load player values
                    else if (type == "pv")
                    {
                        float valX = float.Parse(values[0]);
                        float valY = 0;
                        float valZ = 0;
                        if (values.Length > 1)
                        {
                            valY = float.Parse(values[1]);
                            valZ = float.Parse(values[2]);
                        }

                        if (typeName == "PlayerPosition")
                        {
                            thePlayer.transform.position = new(valX, valY, valZ);
                        }
                        else if (typeName == "PlayerRotation")
                        {
                            Vector3 rot = new(valX, valY, valZ);
                            thePlayer.transform.rotation = Quaternion.Euler(rot);
                        }
                        else if (typeName == "PlayerCameraRotation")
                        {
                            Vector3 camRot = new(valX, valY, valZ);
                            thePlayer.GetComponentInChildren<Camera>().transform.rotation = Quaternion.Euler(camRot);
                        }
                        else if (typeName == "FieldOfView")
                        {
                            float fov = float.Parse(values[0]);
                            PlayerStatsScript.fieldOfView = fov;
                            thePlayer.GetComponentInChildren<Camera>().fieldOfView = fov;
                        }
                        else if (typeName == "MouseSpeed")
                        {
                            float mouseSpeed = float.Parse(values[0]);
                            PlayerStatsScript.cameraMoveSpeed = (int)mouseSpeed;
                            thePlayer.GetComponentInChildren<Player_Camera>().sensX = mouseSpeed;
                            thePlayer.GetComponentInChildren<Player_Camera>().sensY = mouseSpeed;
                        }
                    }
                    //load player stats
                    else if (type == "ps")
                    {
                        float floatVal = float.Parse(values[0]);
                        int intVal = int.Parse(values[0]);

                        if (typeName == "MaxHealth")
                        {
                            PlayerStatsScript.maxHealth = floatVal;
                        }
                        else if (typeName == "Health")
                        {
                            PlayerStatsScript.currentHealth = floatVal;
                        }
                        else if (typeName == "MaxStamina")
                        {
                            PlayerStatsScript.maxStamina = floatVal;
                        }
                        else if (typeName == "Stamina")
                        {
                            PlayerStatsScript.currentStamina = floatVal;
                        }
                        else if (typeName == "MaxMagicka")
                        {
                            PlayerStatsScript.maxMagicka = floatVal;
                        }
                        else if (typeName == "Magicka")
                        {
                            PlayerStatsScript.currentMagicka = floatVal;
                        }
                        else if (typeName == "MaxInvSpace")
                        {
                            PlayerStatsScript.maxInvSpace = intVal;
                        }

                        PlayerStatsScript.UpdateBar(PlayerStatsScript.healthBar,
                                                   (int)PlayerStatsScript.currentHealth,
                                                   (int)PlayerStatsScript.maxHealth);
                        PlayerStatsScript.UpdateBar(PlayerStatsScript.staminaBar,
                                                   (int)PlayerStatsScript.currentStamina,
                                                   (int)PlayerStatsScript.maxStamina);
                        PlayerStatsScript.UpdateBar(PlayerStatsScript.magickaBar,
                                                   (int)PlayerStatsScript.currentMagicka,
                                                   (int)PlayerStatsScript.maxMagicka);
                    }
                }
            }

            Debug.Log("Sucessfully loaded save file " + saveFileName.Replace(".txt", "") + " from " + GameManagerScript.savePath + "!");
        }
    }
}