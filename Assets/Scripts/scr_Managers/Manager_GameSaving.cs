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
    [Header("Assignables")]
    [SerializeField] private GameObject thePlayer;
    [SerializeField] private Canvas canvas;

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
        if (currentScene == 1)
        {
            //save the game
            if (KeyBindingsScript.GetButtonDown("Save"))
            {
                CreateSaveFile("");
            }
            //load the latest save
            if (KeyBindingsScript.GetButtonDown("Load"))
            {
                CreateLoadFile("");
            }
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

                    //only show txt files as saves
                    if (fileName.Contains(".txt"))
                    {
                        DateTime creationDate = file.CreationTime;

                        Button btn_New = Instantiate(UIReuseScript.btn_SaveButtonTemplate);
                        btn_New.transform.SetParent(UIReuseScript.par_SaveButtons, false);

                        btn_New.GetComponentInChildren<TMP_Text>().text = fileName.Replace(".txt", "");

                        btn_New.onClick.AddListener(delegate { ShowSaveData(fileName); });

                        saveButtons.Add(btn_New);
                    }
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
        UIReuseScript.btn_LoadSave.interactable = true;
        UIReuseScript.btn_LoadSave.onClick.RemoveAllListeners();

        //always loads the game in main menu scene
        //or loads the game in game scene if confirmation bypass is allowed
        if (currentScene == 0)
        {
            UIReuseScript.btn_LoadSave.onClick.AddListener(delegate { CreateLoadFile(saveName); });
        }
        //asks for confirmation before loading a save in game scene
        else if (currentScene == 1)
        {
            UIReuseScript.btn_LoadSave.onClick.AddListener(delegate {
                GetComponent<UI_Confirmation>().RecieveData(gameObject,
                                                            "saveScript",
                                                            saveName,
                                                            "load"); });
        }

        //delete save button
        UIReuseScript.btn_DeleteSave.interactable = true;
        UIReuseScript.btn_DeleteSave.onClick.RemoveAllListeners();

        UIReuseScript.btn_DeleteSave.onClick.AddListener(delegate {
            GetComponent<UI_Confirmation>().RecieveData(gameObject,
                                                        "saveScript",
                                                        saveName,
                                                        "delete");});

        //show save name
        UIReuseScript.txt_SaveName.text = saveName.Replace(".txt", "");

        //show save creation date
        DateTime str_SaveDate = File.GetLastWriteTime(GameManagerScript.savePath + @"\" + saveName);
        UIReuseScript.txt_SaveDate.text = str_SaveDate.ToString("d");

        //find the screenshot from saves directory
        //and apply it to the rawimage texture slot
        DirectoryInfo info = new(GameManagerScript.savePath);
        FileInfo[] files = info.GetFiles().OrderBy(p => p.CreationTime).ToArray();
        //loop through all files
        foreach (FileInfo file in files)
        {
            string fileName = Path.GetFileName(file.Name);

            //find the png image with the same name as the save
            if (fileName.Contains(saveName.Replace(".txt", ".png")))
            {
                string path = GameManagerScript.savePath + @"\" + fileName;

                //convert the image to a texture
                //and apply it to the raw image texture slot
                Texture2D screenshot = new(1, 1);
                byte[] data = File.ReadAllBytes(path);
                screenshot.LoadImage(data);
                UIReuseScript.saveImage.texture = screenshot;
                UIReuseScript.saveImage.gameObject.SetActive(true);

                break;
            }
        }
    }

    //delete selected save
    public void DeleteSave(string saveName)
    {
        bool foundSave = false;
        //add .txt to saveName if it is missing
        if (!saveName.Contains(".txt"))
        {
            saveName += ".txt";
        }

        DirectoryInfo info = new(GameManagerScript.savePath);
        FileInfo[] files = info.GetFiles().OrderBy(p => p.CreationTime).ToArray();
        //loop through all files
        for (int i = 0; i < files.Length; i++)
        {
            string fileName = Path.GetFileName(files[i].Name);
            //find the file to delete
            if (fileName == saveName)
            {
                foundSave = true;

                break;
            }
        }

        if (foundSave)
        {
            File.Delete(GameManagerScript.savePath + @"\" + saveName);
            File.Delete(GameManagerScript.savePath + @"\" + saveName.Replace(".txt", ".png"));
            UIReuseScript.ClearSaveData();

            ShowGameSaves();

            Debug.Log("Successfully deleted save " + saveName.Replace(".txt", "") + ".");
        }
        else
        {
            Debug.LogError("Error: Did not find save " + saveName + "!");
        }
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
                || saveName == "reset")
            {
                Debug.LogError("Error: Invalid name " + saveName + " for game save. Save name is not allowed!");
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
        //save a screenshot of the game
        StartCoroutine(TakeScreenshot());

        //create a new save file and fill it with data
        using StreamWriter saveFile = File.CreateText(str_SaveFilePath);

        saveFile.WriteLine("Save file for " + UIReuseScript.txt_GameVersion.text + ".");
        saveFile.WriteLine("WARNING - Invalid values will break the game - edit at your own risk!");
        saveFile.WriteLine("");

        saveFile.WriteLine("---GLOBAL VALUES---");
        saveFile.WriteLine("Time: <UNASSIGNED VALUE>");
        saveFile.WriteLine("");

        saveFile.WriteLine("---PLAYER VALUES---");
        float posX = thePlayer.transform.position.x;
        float posY = thePlayer.transform.position.y;
        float posZ = thePlayer.transform.position.z;
        saveFile.WriteLine("PlayerPosition: " + posX + ", " + posY + ", " + posZ);

        float rotX = thePlayer.transform.rotation.x;
        float rotY = thePlayer.transform.rotation.y;
        float rotZ = thePlayer.transform.rotation.z;
        saveFile.WriteLine("PlayerRotation: " + rotX + ", " + rotY + ", " + rotZ);

        float camRotX = thePlayer.GetComponentInChildren<Camera>().transform.rotation.x;
        float camRotY = thePlayer.GetComponentInChildren<Camera>().transform.rotation.y;
        float camRotZ = thePlayer.GetComponentInChildren<Camera>().transform.rotation.z;
        saveFile.WriteLine("PlayerCameraRotation: " + camRotX + ", " + camRotY + ", " + camRotZ);
        saveFile.WriteLine("");

        saveFile.WriteLine("---PLAYER STATS---");
        saveFile.WriteLine("MaxHealth: " + PlayerStatsScript.maxHealth);
        saveFile.WriteLine("Health: " + PlayerStatsScript.currentHealth);
        saveFile.WriteLine("MaxStamina: " + PlayerStatsScript.maxStamina);
        saveFile.WriteLine("Stamina: " + PlayerStatsScript.currentStamina);
        saveFile.WriteLine("MaxMagicka: " + PlayerStatsScript.maxMagicka);
        saveFile.WriteLine("Magicka: " + PlayerStatsScript.currentMagicka);
        saveFile.WriteLine("MaxInvSpace: " + PlayerStatsScript.maxInvSpace);

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
                if (line.Contains(':'))
                {
                    //initial value split
                    string[] valueSplit = line.Split(':');
                    string[] values = valueSplit[1].Split(',');
                    string type = valueSplit[0];

                    //sorting between ints and floats
                    bool isFloat = float.TryParse(values[0], out _);
                    bool isInt = int.TryParse(values[0], out _);
                    float floatVal = 0;
                    float floatVal2 = 0;
                    float floatVal3 = 0;
                    int intVal = 0;
                    if (isFloat)
                    {
                        floatVal = float.Parse(values[0]);
                        if (values.Length > 1)
                        {
                            floatVal2 = float.Parse(values[1]);
                            floatVal3 = float.Parse(values[2]);     
                        }
                    }
                    else if (isInt)
                    {
                        intVal = int.Parse(values[0]);
                    }

                    //load global values
                    if (type == "Time")
                    {
                        //TODO: save and load game time
                        //saveFile.WriteLine("Time: <UNASSIGNED VALUE>");
                    }
                    //load player values
                    else if (type == "PlayerPosition")
                    {
                        thePlayer.transform.position = new(floatVal, floatVal2, floatVal3);
                    }
                    else if (type == "PlayerRotation")
                    {
                        Vector3 rot = new(floatVal, floatVal2, floatVal3);
                        thePlayer.transform.rotation = Quaternion.Euler(rot);
                    }
                    else if (type == "PlayerCameraRotation")
                    {
                        Vector3 camRot = new(floatVal, floatVal2, floatVal3);
                        thePlayer.GetComponentInChildren<Camera>().transform.rotation = Quaternion.Euler(camRot);
                    }
                    //load player stats
                    else if (type == "MaxHealth")
                    {
                        PlayerStatsScript.maxHealth = floatVal;
                    }
                    else if (type == "Health")
                    {
                        PlayerStatsScript.currentHealth = floatVal;
                    }
                    else if (type == "MaxStamina")
                    {
                        PlayerStatsScript.maxStamina = floatVal;
                    }
                    else if (type == "Stamina")
                    {
                        PlayerStatsScript.currentStamina = floatVal;
                    }
                    else if (type == "MaxMagicka")
                    {
                        PlayerStatsScript.maxMagicka = floatVal;
                    }
                    else if (type == "Magicka")
                    {
                        PlayerStatsScript.currentMagicka = floatVal;
                    }
                    else if (type == "MaxInvSpace")
                    {
                        PlayerStatsScript.maxInvSpace = intVal;
                    }

                    PlayerStatsScript.UpdateBar(PlayerStatsScript.healthBar);
                    PlayerStatsScript.UpdateBar(PlayerStatsScript.staminaBar);
                    PlayerStatsScript.UpdateBar(PlayerStatsScript.magickaBar);
                }
            }

            Debug.Log("Sucessfully loaded save file " + saveFileName.Replace(".txt", "") + " from " + GameManagerScript.savePath + "!");
        }
    }

    private IEnumerator TakeScreenshot()
    {
        string screenshotPath = str_SaveFilePath.Replace(".txt", ".png");

        canvas.enabled = false;

        ScreenCapture.CaptureScreenshot(screenshotPath);

        yield return new WaitForEndOfFrame();
        canvas.enabled = true;
    }
}