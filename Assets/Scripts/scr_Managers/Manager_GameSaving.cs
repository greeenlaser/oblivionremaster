using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Diagnostics.CodeAnalysis;
using Unity.VisualScripting;

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

    //scripts
    private UI_Inventory PlayerInventoryScript;
    private Player_Stats PlayerStatsScript;
    private UI_PlayerMenu PlayerMenuScript;
    private GameManager GameManagerScript;
    private Manager_KeyBindings KeyBindingsScript;
    private UI_LoadingScreen LoadingScreenScript;
    private Manager_DateAndTime DateAndTimeScript;
    private Manager_Locations LocationsScript;
    private Manager_UIReuse UIReuseScript;

    private void Awake()
    {
        GameManagerScript = GetComponent<GameManager>();
        KeyBindingsScript = GetComponent<Manager_KeyBindings>();
        LoadingScreenScript = GetComponent<UI_LoadingScreen>();
        UIReuseScript = GetComponent<Manager_UIReuse>();

        currentScene = SceneManager.GetActiveScene().buildIndex;

        if (currentScene == 0)
        {
            UIReuseScript.btn_DeleteSave.gameObject.SetActive(false);
        }
        else if (currentScene == 1)
        {
            PlayerInventoryScript = thePlayer.GetComponent<UI_Inventory>();
            PlayerStatsScript = thePlayer.GetComponent<Player_Stats>();
            PlayerMenuScript = GetComponent<UI_PlayerMenu>();
            DateAndTimeScript = GetComponent<Manager_DateAndTime>();
            LocationsScript = GetComponent<Manager_Locations>();
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

            UIReuseScript.btn_DeleteSave.interactable = true;
            UIReuseScript.btn_DeleteSave.onClick.RemoveAllListeners();

            UIReuseScript.btn_DeleteSave.onClick.AddListener(delegate {
                GetComponent<UI_Confirmation>().RecieveData(gameObject,
                                                            "saveScript",
                                                            saveName,
                                                            "delete");});
        }

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
        saveFile.WriteLine("DaysSinceReset: " + DateAndTimeScript.daysSinceLastRestart);
        saveFile.WriteLine("TimeAndDate: " + Mathf.Floor(DateAndTimeScript.second) + ", " 
                                             + DateAndTimeScript.minute + ", " 
                                             + DateAndTimeScript.hour + ", "
                                             + DateAndTimeScript.dayName + ", " 
                                             + DateAndTimeScript.monthName);
        saveFile.WriteLine("");

        saveFile.WriteLine("---PLAYER POSITION AND ROTATION---");
        float posX = Mathf.Round(thePlayer.transform.position.x * 100f) / 100f;
        float posY = Mathf.Round(thePlayer.transform.position.y * 100f) / 100f;
        float posZ = Mathf.Round(thePlayer.transform.position.z * 100f) / 100f;
        saveFile.WriteLine("PlayerPosition: " + posX + ", " + posY + ", " + posZ);

        float rotX = Mathf.Round(thePlayer.transform.rotation.x * 100f) / 100f;
        float rotY = Mathf.Round(thePlayer.transform.rotation.y * 100f) / 100f;
        float rotZ = Mathf.Round(thePlayer.transform.rotation.z * 100f) / 100f;
        saveFile.WriteLine("PlayerRotation: " + rotX + ", " + rotY + ", " + rotZ);

        float camRotX = Mathf.Round(thePlayer.GetComponentInChildren<Camera>().transform.rotation.x * 100f) / 100f;
        float camRotY = Mathf.Round(thePlayer.GetComponentInChildren<Camera>().transform.rotation.y * 100f) / 100f;
        float camRotZ = Mathf.Round(thePlayer.GetComponentInChildren<Camera>().transform.rotation.z * 100f) / 100f;
        saveFile.WriteLine("PlayerCameraRotation: " + camRotX + ", " + camRotY + ", " + camRotZ);
        saveFile.WriteLine("");

        saveFile.WriteLine("---PLAYER MAIN VALUES---");
        saveFile.WriteLine("Level: " + PlayerStatsScript.level + ", " + PlayerStatsScript.level_PointsToNextLevel);
        saveFile.WriteLine("MaxHealth: " + PlayerStatsScript.maxHealth);
        saveFile.WriteLine("Health: " + PlayerStatsScript.currentHealth);
        saveFile.WriteLine("MaxStamina: " + PlayerStatsScript.maxStamina);
        saveFile.WriteLine("Stamina: " + PlayerStatsScript.currentStamina);
        saveFile.WriteLine("MaxMagicka: " + PlayerStatsScript.maxMagicka);
        saveFile.WriteLine("Magicka: " + PlayerStatsScript.currentMagicka);
        saveFile.WriteLine("MaxInvSpace: " + PlayerStatsScript.maxInvSpace);
        saveFile.WriteLine("");

        saveFile.WriteLine("---PLAYER ATTRIBUTES---");
        saveFile.WriteLine("Strength: " + PlayerStatsScript.Attributes["Strength"]);
        saveFile.WriteLine("Intelligence: " + PlayerStatsScript.Attributes["Intelligence"]);
        saveFile.WriteLine("Willpower: " + PlayerStatsScript.Attributes["Willpower"]);
        saveFile.WriteLine("Agility: " + PlayerStatsScript.Attributes["Agility"]);
        saveFile.WriteLine("Speed: " + PlayerStatsScript.Attributes["Speed"]);
        saveFile.WriteLine("Endurance: " + PlayerStatsScript.Attributes["Endurance"]);
        saveFile.WriteLine("Personality: " + PlayerStatsScript.Attributes["Personality"]);
        saveFile.WriteLine("Luck: " + PlayerStatsScript.Attributes["Luck"]);
        saveFile.WriteLine("");

        saveFile.WriteLine("---PLAYER SKILLS---");
        saveFile.WriteLine("Blade: " + PlayerStatsScript.Skills["Blade"] + ", " + PlayerStatsScript.SkillPoints["Blade"]);
        saveFile.WriteLine("Blunt: " + PlayerStatsScript.Skills["Blunt"] + ", " + PlayerStatsScript.SkillPoints["Blunt"]);
        saveFile.WriteLine("HandToHand: " + PlayerStatsScript.Skills["HandToHand"] + ", " + PlayerStatsScript.SkillPoints["HandToHand"]);
        saveFile.WriteLine("Armorer: " + PlayerStatsScript.Skills["Armorer"] + ", " + PlayerStatsScript.SkillPoints["Armorer"]);
        saveFile.WriteLine("Block: " + PlayerStatsScript.Skills["Block"] + ", " + PlayerStatsScript.SkillPoints["Block"]);
        saveFile.WriteLine("HeavyArmor: " + PlayerStatsScript.Skills["HeavyArmor"] + ", " + PlayerStatsScript.SkillPoints["HeavyArmor"]);
        saveFile.WriteLine("Athletics: " + PlayerStatsScript.Skills["Athletics"] + ", " + PlayerStatsScript.SkillPoints["Athletics"]);
        saveFile.WriteLine("Alteration: " + PlayerStatsScript.Skills["Alteration"] + ", " + PlayerStatsScript.SkillPoints["Alteration"]);
        saveFile.WriteLine("Destruction: " + PlayerStatsScript.Skills["Destruction"] + ", " + PlayerStatsScript.SkillPoints["Destruction"]);
        saveFile.WriteLine("Restoration: " + PlayerStatsScript.Skills["Restoration"] + ", " + PlayerStatsScript.SkillPoints["Restoration"]);
        saveFile.WriteLine("Alchemy: " + PlayerStatsScript.Skills["Alchemy"] + ", " + PlayerStatsScript.SkillPoints["Alchemy"]);
        saveFile.WriteLine("Conjuration: " + PlayerStatsScript.Skills["Conjuration"] + ", " + PlayerStatsScript.SkillPoints["Conjuration"]);
        saveFile.WriteLine("Mysticism: " + PlayerStatsScript.Skills["Mysticism"] + ", " + PlayerStatsScript.SkillPoints["Mysticism"]);
        saveFile.WriteLine("Illusion: " + PlayerStatsScript.Skills["Illusion"] + ", " + PlayerStatsScript.SkillPoints["Illusion"]);
        saveFile.WriteLine("Security: " + PlayerStatsScript.Skills["Security"] + ", " + PlayerStatsScript.SkillPoints["Security"]);
        saveFile.WriteLine("Sneak: " + PlayerStatsScript.Skills["Sneak"] + ", " + PlayerStatsScript.SkillPoints["Sneak"]);
        saveFile.WriteLine("Marksman: " + PlayerStatsScript.Skills["Marksman"] + ", " + PlayerStatsScript.SkillPoints["Marksman"]);
        saveFile.WriteLine("Acrobatics: " + PlayerStatsScript.Skills["Acrobatics"] + ", " + PlayerStatsScript.SkillPoints["Acrobatics"]);
        saveFile.WriteLine("LightArmor: " + PlayerStatsScript.Skills["LightArmor"] + ", " + PlayerStatsScript.SkillPoints["LightArmor"]);
        saveFile.WriteLine("Mercantile: " + PlayerStatsScript.Skills["Mercantile"] + ", " + PlayerStatsScript.SkillPoints["Mercantile"]);
        saveFile.WriteLine("Speechcraft: " + PlayerStatsScript.Skills["Speechcraft"] + ", " + PlayerStatsScript.SkillPoints["Speechcraft"]);
        saveFile.WriteLine("");

        saveFile.WriteLine("---PLAYER ITEMS---");
        int itemCount = 0;
        foreach (GameObject item in PlayerInventoryScript.playerItems)
        {
            Env_Item ItemScript = item.GetComponent<Env_Item>();
            saveFile.WriteLine(item.name + ": " + ItemScript.itemCount);
            itemCount++;
        }
        if (itemCount == 0)
        {
            saveFile.WriteLine("No items were found in player inventory.");
        }

        //get each location in the game
        foreach (GameObject location in LocationsScript.locations)
        {
            Trigger_Location LocationScript = location.GetComponent<Trigger_Location>();

            saveFile.WriteLine("");
            saveFile.WriteLine("---" + LocationScript.cellName + " info---");
            saveFile.WriteLine("Cell_" + LocationScript.cellName + "_DiscoverStatus: " + LocationScript.wasDiscovered);

            int foundContainerCount = 0;
            //get each container in this location
            foreach (GameObject container in LocationScript.containers)
            {
                UI_Inventory ContainerScript = container.GetComponent<UI_Inventory>();

                saveFile.WriteLine("");
                saveFile.WriteLine("-" + ContainerScript.containerName + " container items-");

                Env_LockStatus LockStatusScript = container.GetComponent<Env_LockStatus>();
                string lockStatus = LockStatusScript.isUnlocked + ", " +
                                    LockStatusScript.tumbler1Unlocked + ", " +
                                    LockStatusScript.tumbler2Unlocked + ", " +
                                    LockStatusScript.tumbler3Unlocked + ", " +
                                    LockStatusScript.tumbler4Unlocked + ", " +
                                    LockStatusScript.tumbler5Unlocked;
                
                saveFile.WriteLine("Cell_" + ContainerScript.containerName + "_LockStatus: " + lockStatus);

                int foundItemCount = 0;
                foreach (GameObject item in ContainerScript.containerItems)
                {
                    Env_Item itemScript = item.GetComponent<Env_Item>();
                    string itemName = itemScript.str_ItemName;
                    int theItemCount = itemScript.itemCount;

                    saveFile.WriteLine("Cell_" + ContainerScript.containerName + "_" + itemName + ": " + theItemCount);
                    foundItemCount++;
                }
                if (foundItemCount == 0)
                {
                    saveFile.WriteLine("No items were found in " + ContainerScript.containerName + " container.");
                }
                foundContainerCount++;
            }
            if (foundContainerCount == 0)
            {
                saveFile.WriteLine("No containers were found in " + LocationScript.cellName + " cell.");
            }
        }

        Debug.Log("Successfully saved game to " + str_SaveFilePath + "!");
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
            LoadingScreenScript.OpenLoadingScreen();
            LoadingScreenScript.UpdateLoadingScreenBar(10);

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
            else
            {
                Debug.Log("Started new game.");
                LocationsScript.ResetAllLocations();
            }

            //delete load file
            File.Delete(GameManagerScript.gamePath + @"\loadfile.txt");
        }
    }
    //load game data from save file
    private void LoadGame(string saveFileName)
    {
        string str_SaveFileName = "";

        List<string> templateItemNames = new();
        foreach (GameObject item in PlayerMenuScript.templateItems)
        {
            templateItemNames.Add(item.name);
        }

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
                    string[] valueSplit = line.Split(':');
                    string[] values = valueSplit[1].Split(',');
                    string type = valueSplit[0];

                    //set reset time
                    if (type == "DaysSinceReset")
                    {
                        DateAndTimeScript.daysSinceLastRestart = int.Parse(values[0]);
                    }
                    //load time
                    else if (type == "TimeAndDate")
                    {
                        float sec = MathF.Floor(float.Parse(values[0]));
                        int min = int.Parse(values[1]);
                        int hr = int.Parse(values[2]);
                        string date = values[3];
                        string month = values[4];

                        DateAndTimeScript.SetDateAndTime(sec, min, hr, date, month);
                    }

                    //load player position and rotation
                    else if (type == "PlayerPosition")
                    {
                        thePlayer.transform.position = new(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]));
                    }
                    else if (type == "PlayerRotation")
                    {
                        Vector3 rot = new(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]));
                        thePlayer.transform.rotation = Quaternion.Euler(rot);
                    }
                    else if (type == "PlayerCameraRotation")
                    {
                        Vector3 camRot = new(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]));
                        thePlayer.GetComponentInChildren<Camera>().transform.rotation = Quaternion.Euler(camRot);
                    }

                    //load player main values
                    else if (type == "Level")
                    {
                        PlayerStatsScript.level = int.Parse(values[0]);
                        PlayerStatsScript.level_PointsToNextLevel = int.Parse(values[1]);
                    }
                    else if (type == "MaxHealth")
                    {
                        PlayerStatsScript.maxHealth = float.Parse(values[0]);
                    }
                    else if (type == "Health")
                    {
                        PlayerStatsScript.currentHealth = float.Parse(values[0]);
                    }
                    else if (type == "MaxStamina")
                    {
                        PlayerStatsScript.maxStamina = float.Parse(values[0]);
                    }
                    else if (type == "Stamina")
                    {
                        PlayerStatsScript.currentStamina = float.Parse(values[0]);
                    }
                    else if (type == "MaxMagicka")
                    {
                        PlayerStatsScript.maxMagicka = float.Parse(values[0]);
                    }
                    else if (type == "Magicka")
                    {
                        PlayerStatsScript.currentMagicka = float.Parse(values[0]);
                    }
                    else if (type == "MaxInvSpace")
                    {
                        PlayerStatsScript.maxInvSpace = int.Parse(values[0]);
                    }

                    //load player attributes
                    else if (type == "Strength"
                             || type == "Intelligence"
                             || type == "Willpower"
                             || type == "Agility"
                             || type == "Speed"
                             || type == "Endurance"
                             || type == "Personality"
                             || type == "Luck")
                    {
                        List<string> attributeNames = new();
                        List<int> attributeValues = new();
                        foreach (KeyValuePair<string, int> attributes in PlayerStatsScript.Attributes)
                        {
                            attributeNames.Add(attributes.Key);
                            attributeValues.Add(attributes.Value);
                        }

                        for (int i = 0; i < attributeNames.Count; i++)
                        {
                            string attributeName = attributeNames[i];

                            if (attributeName == type)
                            {
                                PlayerStatsScript.Attributes[type] = int.Parse(values[0]);
                            }
                        }
                    }

                    //load player skills
                    else if (type == "Blade"
                             || type == "Blunt"
                             || type == "HandToHand"
                             || type == "Armorer"
                             || type == "Block"
                             || type == "HeavyArmor"
                             || type == "Athletics"
                             || type == "Alteration"
                             || type == "Destruction"
                             || type == "Restoration"
                             || type == "Alchemy"
                             || type == "Conjuration"
                             || type == "Mysticism"
                             || type == "Illusion"
                             || type == "Security"
                             || type == "Sneak"
                             || type == "Marksman"
                             || type == "Acrobatics"
                             || type == "LightArmor"
                             || type == "Mercantile"
                             || type == "Speechcraft")
                    {
                        List<string> skillNames = new();
                        List<int> skillValues = new();
                        foreach (KeyValuePair<string, int> skills in PlayerStatsScript.Skills)
                        {
                            skillNames.Add(skills.Key);
                            skillValues.Add(skills.Value);
                        }

                        for (int i = 0; i < skillNames.Count; i++)
                        {
                            string skillName = skillNames[i];

                            if (skillName == type)
                            {
                                PlayerStatsScript.Skills[type] = int.Parse(values[0]);
                            }
                        }

                        List<string> skillPointNames = new();
                        List<int> skillPointValues = new();
                        foreach (KeyValuePair<string, int> skillPoints in PlayerStatsScript.SkillPoints)
                        {
                            skillPointNames.Add(skillPoints.Key);
                            skillPointValues.Add(skillPoints.Value);
                        }

                        for (int i = 0; i < skillPointNames.Count; i++)
                        {
                            string skillPointName = skillPointNames[i];

                            if (skillPointName == type)
                            {
                                PlayerStatsScript.SkillPoints[type] = int.Parse(values[0]);
                            }
                        }
                    }

                    //load player items
                    else if (templateItemNames.Contains(type))
                    {
                        GameObject templateItem = null;
                        foreach (GameObject item in PlayerMenuScript.templateItems)
                        {
                            if (item.name == type)
                            {
                                templateItem = item;
                                break;
                            }
                        }
                        GameObject spawnedItem = Instantiate(templateItem,
                                                             PlayerInventoryScript.par_PlayerItems.transform.position,
                                                             Quaternion.identity,
                                                             PlayerInventoryScript.par_PlayerItems.transform);
                        spawnedItem.SetActive(false);
                        spawnedItem.name = templateItem.name;
                        spawnedItem.GetComponent<Env_Item>().itemCount = int.Parse(values[0]);

                        PlayerInventoryScript.playerItems.Add(spawnedItem);
                        PlayerStatsScript.invSpace += spawnedItem.GetComponent<Env_Item>().itemCount * spawnedItem.GetComponent<Env_Item>().itemWeight;
                    }

                    //load locations, container lock states and container items
                    else if (type.Contains("Cell"))
                    {
                        foreach (GameObject location in LocationsScript.locations)
                        {
                            Trigger_Location LocationScript = location.GetComponent<Trigger_Location>();
                            string cellName = LocationScript.cellName;

                            if (type.Contains(cellName + "_DiscoverStatus"))
                            {
                                LocationScript.wasDiscovered = bool.Parse(values[0]);
                            }
                            else
                            {
                                foreach (GameObject container in LocationScript.containers)
                                {
                                    UI_Inventory ContainerScript = container.GetComponent<UI_Inventory>();
                                    string containerName = ContainerScript.containerName;

                                    if (type.Contains(containerName))
                                    {
                                        ContainerScript.GetComponent<Env_LockStatus>().hasLoadedLock = true;

                                        if (type.Contains("_LockStatus"))
                                        {
                                            Env_LockStatus LockStatusScript = container.GetComponent<Env_LockStatus>();

                                            LockStatusScript.isUnlocked = bool.Parse(values[0]);
                                            LockStatusScript.tumbler1Unlocked = bool.Parse(values[1]);
                                            LockStatusScript.tumbler2Unlocked = bool.Parse(values[2]);
                                            LockStatusScript.tumbler3Unlocked = bool.Parse(values[3]);
                                            LockStatusScript.tumbler4Unlocked = bool.Parse(values[4]);
                                            LockStatusScript.tumbler5Unlocked = bool.Parse(values[5]);
                                        }
                                        else
                                        {
                                            foreach (GameObject item in PlayerMenuScript.templateItems)
                                            {
                                                if (type.Contains(item.name))
                                                {
                                                    int itemCount = int.Parse(values[0]);

                                                    GameObject spawnedItem = Instantiate(item,
                                                                                         ContainerScript.par_ContainerItems.transform.position,
                                                                                         Quaternion.identity,
                                                                                         ContainerScript.par_ContainerItems.transform);
                                                    Env_Item SpawnedItemScript = spawnedItem.GetComponent<Env_Item>();
                                                    spawnedItem.name = SpawnedItemScript.str_ItemName;
                                                    SpawnedItemScript.itemCount = itemCount;
                                                    ContainerScript.containerItems.Add(spawnedItem);

                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            PlayerStatsScript.UpdateBar(PlayerStatsScript.healthBar);
            PlayerStatsScript.UpdateBar(PlayerStatsScript.staminaBar);
            PlayerStatsScript.UpdateBar(PlayerStatsScript.magickaBar);

            Debug.Log("Successfully loaded save file " + saveFileName.Replace(".txt", "") + " from " + GameManagerScript.savePath + "!");
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