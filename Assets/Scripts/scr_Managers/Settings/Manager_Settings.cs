using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Manager_Settings : MonoBehaviour
{
    [Header("Assignables")]
    [SerializeField] private GameObject playerMainCamera; 

    [Header("General settings")]
    public int def_Difficulty = 0;
    [HideInInspector] public int user_Difficulty;
    public int def_MouseSpeed = 50;
    [HideInInspector] public int user_MouseSpeed;

    [Header("Graphics settings")]
    public UserDefined_Preset def_Preset = UserDefined_Preset.medium;
    [HideInInspector] public UserDefined_Preset user_Preset;
    public enum UserDefined_Preset
    {
        low,
        medium,
        high,
        ultra,
        custom
    }
    public UserDefined_Resolution def_Resolution = UserDefined_Resolution.res_1920x1080;
    [HideInInspector] public UserDefined_Resolution user_Resolution;
    public enum UserDefined_Resolution
    {
        res_1280x720,
        res_1366x768,
        res_1920x1080,
        res_2160x1440,
        res_2560x1080,
        res_3440x1440,
        res_3840x1080,
        res_3840x2160,
        res_5120x1440
    }
    public UserDefined_FullScreenMode def_FullScreenMode = UserDefined_FullScreenMode.FullScreenWindow;
    [HideInInspector] public UserDefined_FullScreenMode user_FullScreenMode;
    public enum UserDefined_FullScreenMode
    {
        ExclusiveFullScreen,
        FullScreenWindow,
        MaximizedWindow,
        Windowed
    }
    public int def_FieldOfView = 90;
    [HideInInspector] public int user_FieldOfView;
    public string def_EnableVsync = "true";
    [HideInInspector] public string user_EnableVSync;
    public UserDefined_TextureQuality def_TextureQuality = UserDefined_TextureQuality.medium;
    [HideInInspector] public UserDefined_TextureQuality user_TextureQuality;
    public enum UserDefined_TextureQuality
    {
        low,
        medium,
        high,
        ultra
    }
    public int def_LightDistance = 250;
    [HideInInspector] public int user_LightDistance;
    public int def_ShadowDistance = 250;
    [HideInInspector] public int user_ShadowDistance;
    public UserDefined_ShadowQuality def_ShadowQuality = UserDefined_ShadowQuality.medium;
    [HideInInspector] public UserDefined_ShadowQuality user_ShadowQuality;
    public enum UserDefined_ShadowQuality
    {
        low,
        medium,
        high,
        ultra
    }
    public int def_TreeDistance = 1000;
    [HideInInspector] public int user_TreeDistance;
    public int def_GrassDistance = 100;
    [HideInInspector] public int user_GrassDistance;
    public int def_ObjectDistance = 1000;
    [HideInInspector] public int user_ObjectDistance;
    public int def_ItemDistance = 100;
    [HideInInspector] public int user_ItemDistance;
    public int def_AIDistance = 100;
    [HideInInspector] public int user_AIDistance;

    [Header("Audio settings")]
    public int def_MasterVolume = 50;
    [HideInInspector] public int user_MasterVolume;
    public int def_MusicVolume = 50;
    [HideInInspector] public int user_MusicVolume;
    public int def_SFXVolume = 50;
    [HideInInspector] public int user_SFXVolume;
    public int def_NPCVolume = 50;
    [HideInInspector] public int user_NPCVolume;

    //private variables
    private int currentScene;

    //scripts
    private UI_Confirmation ConfirmationScript;
    private GameManager GameManagerScript;
    private Manager_UIReuse UIReuseScript;
    private Player_Camera PlayerCameraScript;

    private void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        ConfirmationScript = GetComponent<UI_Confirmation>();
        GameManagerScript = GetComponent<GameManager>();
        UIReuseScript = GetComponent<Manager_UIReuse>();

        SetSliderLimits();

        currentScene = SceneManager.GetActiveScene().buildIndex;

        if (currentScene == 1)
        {
            PlayerCameraScript = playerMainCamera.GetComponent<Player_Camera>();
        }
    }

    //set slider limits to all sliders
    private void SetSliderLimits()
    {
        List<GameObject> parents = new();
        foreach (GameObject par in UIReuseScript.generalSettingsParents)
        {
            parents.Add(par);
        }
        foreach (GameObject par in UIReuseScript.graphicsSettingsParents)
        {
            parents.Add(par);
        }
        foreach (GameObject par in UIReuseScript.audioSettingsParents)
        {
            parents.Add(par);
        }

        foreach (GameObject par in parents)
        {
            if (par.GetComponentInChildren<Slider>() != null)
            {
                Slider slider = par.GetComponentInChildren<Slider>();
                UI_AssignSettings AssignScript = slider.GetComponent<UI_AssignSettings>();
                string info = AssignScript.info;

                if (info == "Difficulty")
                {
                    slider.minValue = -100;
                    slider.maxValue = 100;
                }
                else if (info == "MouseSpeed")
                {
                    slider.minValue = 1;
                    slider.maxValue = 100;
                }
                else if (info == "FieldOfView")
                {
                    slider.minValue = 70;
                    slider.maxValue = 110;
                }
                else if (info.Contains("Volume"))
                {
                    slider.minValue = 0;
                    slider.maxValue = 100;
                }
                else
                {
                    slider.minValue = 15;
                    slider.maxValue = 5000;
                }
            }
        }
    }

    //rebuild selected settings list
    public void RebuildSettingsList(string targetSettingsParentName)
    {
        UIReuseScript.ClearSettingsUI();

        //only lists all settings from selected key bindings parent
        List<GameObject> settingsParents = null;
        if (targetSettingsParentName == "general")
        {
            UIReuseScript.par_MainGeneralSettingsParent.SetActive(true);
            UIReuseScript.btn_ShowGeneralSettings.interactable = false;

            settingsParents = UIReuseScript.generalSettingsParents;
        }
        else if (targetSettingsParentName == "graphics")
        {
            UIReuseScript.par_MainGraphicsSettingsParent.SetActive(true);
            UIReuseScript.btn_ShowGraphicsSettings.interactable = false;

            settingsParents = UIReuseScript.graphicsSettingsParents;
        }
        else if (targetSettingsParentName == "audio")
        {
            UIReuseScript.par_MainAudioSettingsParent.SetActive(true);
            UIReuseScript.btn_ShowAudioSettings.interactable = false;

            settingsParents = UIReuseScript.audioSettingsParents;
        }

        foreach (GameObject buttonUIParent in settingsParents)
        {
            TMP_Dropdown dropdown;
            Slider slider;
            Button button;

            foreach (Transform child in buttonUIParent.transform)
            {
                if (child.name == "dropdown_Settings"
                    && child.gameObject.activeInHierarchy)
                {
                    dropdown = buttonUIParent.GetComponentInChildren<TMP_Dropdown>();
                    dropdown.ClearOptions();
                    AssignEvent(dropdown.gameObject, "dropdown");
                }
                else if (child.name == "slider_Settings"
                         && child.gameObject.activeInHierarchy)
                {
                    slider = buttonUIParent.GetComponentInChildren<Slider>();
                    slider.onValueChanged.RemoveAllListeners();
                    AssignEvent(slider.gameObject, "slider");
                }
                else if (child.name == "button_Settings"
                         && child.gameObject.activeInHierarchy)
                {
                    button = buttonUIParent.GetComponentInChildren<Button>();
                    button.onClick.RemoveAllListeners();
                    AssignEvent(button.gameObject, "button");
                }
            }
        }
    }

    //assign event to all targets
    public void AssignEvent(GameObject target, string targetType)
    {
        UI_AssignSettings AssignScript = target.GetComponent<UI_AssignSettings>();
        string info = AssignScript.info;

        if (targetType == "dropdown")
        {
            TMP_Dropdown dropdown = target.GetComponent<TMP_Dropdown>();

            //assign preset choices, current choice and event
            if (info == "Preset")
            {
                List<string> values = new(Enum.GetNames(typeof(UserDefined_Preset)));
                dropdown.AddOptions(values);
                foreach (string res in values)
                {
                    dropdown.onValueChanged.AddListener(delegate { DropdownEvent(dropdown); });
                }

                for (int i = 0; i < values.Count; i++)
                {
                    string selectedValue = values[i];
                    string userDefinedValue = user_Preset.ToString();

                    if (userDefinedValue == selectedValue)
                    {
                        dropdown.value = i;
                        break;
                    }
                }
            }
            //assign resolution choices, current choice and event
            else if (info == "Resolution")
            {
                List<string> values = new(Enum.GetNames(typeof(UserDefined_Resolution)));
                dropdown.AddOptions(values);
                foreach (string res in values)
                {
                    dropdown.onValueChanged.AddListener(delegate { DropdownEvent(dropdown); });
                }

                for (int i = 0; i < values.Count; i++)
                {
                    string selectedValue = values[i];
                    string userDefinedValue = user_Resolution.ToString();

                    if (userDefinedValue == selectedValue)
                    {
                        dropdown.value = i;
                        break;
                    }
                }
            }
            //assign fullscreen mode choices, current choice and event
            else if (info == "FullScreenMode")
            {
                List<string> values = new(Enum.GetNames(typeof(FullScreenMode)));
                dropdown.AddOptions(values);
                foreach (string res in values)
                {
                    dropdown.onValueChanged.AddListener(delegate { DropdownEvent(dropdown); });
                }

                for (int i = 0; i < values.Count; i++)
                {
                    string selectedValue = values[i];
                    string userDefinedValue = user_FullScreenMode.ToString();

                    if (userDefinedValue == selectedValue)
                    {
                        dropdown.value = i;
                        break;
                    }
                }
            }
            //assign texture quality choices, current choice and event
            else if (info == "TextureQuality")
            {
                List<string> values = new(Enum.GetNames(typeof(UserDefined_TextureQuality)));
                dropdown.AddOptions(values);
                foreach (string res in values)
                {
                    dropdown.onValueChanged.AddListener(delegate { DropdownEvent(dropdown); });
                }

                for (int i = 0; i < values.Count; i++)
                {
                    string selectedValue = values[i];
                    string userDefinedValue = user_TextureQuality.ToString();

                    if (userDefinedValue == selectedValue)
                    {
                        dropdown.value = i;
                        break;
                    }
                }
            }
            //assign shadow quality choices, current choice and event
            else if (info == "ShadowQuality")
            {
                List<string> values = new(Enum.GetNames(typeof(UserDefined_ShadowQuality)));
                dropdown.AddOptions(values);
                foreach (string res in values)
                {
                    dropdown.onValueChanged.AddListener(delegate { DropdownEvent(dropdown); });
                }

                for (int i = 0; i < values.Count; i++)
                {
                    string selectedValue = values[i];
                    string userDefinedValue = user_ShadowQuality.ToString();

                    if (userDefinedValue == selectedValue)
                    {
                        dropdown.value = i;
                        break;
                    }
                }
            }
        }
        else if (targetType == "slider")
        {
            Slider slider = target.GetComponent<Slider>();
            TMP_Text sliderText = AssignScript.txt_SliderText;

            //general settings
            if (info == "Difficulty")
            {
                //get user defined value
                slider.value = user_Difficulty;
                sliderText.text = user_Difficulty.ToString();

                //create a new slider event
                slider.onValueChanged.AddListener(delegate { SliderEvent(slider, sliderText); });
            }
            else if (info == "MouseSpeed")
            {
                //get user defined value
                slider.value = user_MouseSpeed;
                sliderText.text = user_MouseSpeed.ToString();

                //create a new slider event
                slider.onValueChanged.AddListener(delegate { SliderEvent(slider, sliderText); });
            }

            //graphics settings
            else if (info == "FieldOfView")
            {
                //get user defined value
                slider.value = user_FieldOfView;
                sliderText.text = user_FieldOfView.ToString();

                //create a new slider event
                slider.onValueChanged.AddListener(delegate { SliderEvent(slider, sliderText); });
            }
            else if (info == "LightDistance")
            {
                //get user defined value
                slider.value = user_LightDistance;
                sliderText.text = user_LightDistance.ToString();

                //create a new slider event
                slider.onValueChanged.AddListener(delegate { SliderEvent(slider, sliderText); });
            }
            else if (info == "ShadowDistance")
            {
                //get user defined value
                slider.value = user_ShadowDistance;
                sliderText.text = user_ShadowDistance.ToString();

                //create a new slider event
                slider.onValueChanged.AddListener(delegate { SliderEvent(slider, sliderText); });
            }
            else if (info == "TreeDistance")
            {
                //get user defined value
                slider.value = user_TreeDistance;
                sliderText.text = user_TreeDistance.ToString();

                //create a new slider event
                slider.onValueChanged.AddListener(delegate { SliderEvent(slider, sliderText); });
            }
            else if (info == "GrassDistance")
            {
                //get user defined value
                slider.value = user_GrassDistance;
                sliderText.text = user_GrassDistance.ToString();

                //create a new slider event
                slider.onValueChanged.AddListener(delegate { SliderEvent(slider, sliderText); });
            }
            else if (info == "ObjectDistance")
            {
                //get user defined value
                slider.value = user_ObjectDistance;
                sliderText.text = user_ObjectDistance.ToString();

                //create a new slider event
                slider.onValueChanged.AddListener(delegate { SliderEvent(slider, sliderText); });
            }
            else if (info == "ItemDistance")
            {
                //get user defined value
                slider.value = user_ItemDistance;
                sliderText.text = user_ItemDistance.ToString();

                //create a new slider event
                slider.onValueChanged.AddListener(delegate { SliderEvent(slider, sliderText); });
            }
            else if (info == "AIDistance")
            {
                //get user defined value
                slider.value = user_AIDistance;
                sliderText.text = user_AIDistance.ToString();

                //create a new slider event
                slider.onValueChanged.AddListener(delegate { SliderEvent(slider, sliderText); });
            }

            //audio settings
            else if (info == "MasterVolume")
            {
                //get user defined value
                slider.value = user_MasterVolume;
                sliderText.text = user_MasterVolume.ToString();

                //create a new slider event
                slider.onValueChanged.AddListener(delegate { SliderEvent(slider, sliderText); });
            }
            else if (info == "MusicVolume")
            {
                //get user defined value
                slider.value = user_MusicVolume;
                sliderText.text = user_MusicVolume.ToString();

                //create a new slider event
                slider.onValueChanged.AddListener(delegate { SliderEvent(slider, sliderText); });
            }
            else if (info == "SFXVolume")
            {
                //get user defined value
                slider.value = user_SFXVolume;
                sliderText.text = user_SFXVolume.ToString();

                //create a new slider event
                slider.onValueChanged.AddListener(delegate { SliderEvent(slider, sliderText); });
            }
            else if (info == "NPCVolume")
            {
                //get user defined value
                slider.value = user_NPCVolume;
                sliderText.text = user_NPCVolume.ToString();

                //create a new slider event
                slider.onValueChanged.AddListener(delegate { SliderEvent(slider, sliderText); });
            }
        }
        else if (targetType == "button")
        {
            Button button = target.GetComponent<Button>();

            if (info == "VSyncState")
            {
                //get user defined vsync state
                target.GetComponent<Button>().GetComponentInChildren<TMP_Text>().text = user_EnableVSync;

                //create a new toggle event for vsync state
                target.GetComponent<Button>().onClick.AddListener(delegate { ButtonEvent(button); });
            }
        }
    }

    //the individual dropdown event for every selected dropdown choice
    public void DropdownEvent(TMP_Dropdown target)
    {
        UI_AssignSettings SettingsScript = target.GetComponent<UI_AssignSettings>();
        string info = SettingsScript.info;

        string newValue = target.value.ToString();

        if (info == "Preset")
        {
            user_Preset = (UserDefined_Preset)Enum.Parse(typeof(UserDefined_Preset), newValue);
        }
        else if (info == "Resolution")
        {
            user_Resolution = (UserDefined_Resolution)Enum.Parse(typeof(UserDefined_Resolution), newValue);
        }
        else if (info == "FullScreenMode")
        {
            user_FullScreenMode = (UserDefined_FullScreenMode)Enum.Parse(typeof(UserDefined_FullScreenMode), newValue);
        }
        else if (info == "TextureQuality")
        {
            user_TextureQuality = (UserDefined_TextureQuality)Enum.Parse(typeof(UserDefined_TextureQuality), newValue);
        }
        else if (info == "ShadowQuality")
        {
            user_ShadowQuality = (UserDefined_ShadowQuality)Enum.Parse(typeof(UserDefined_ShadowQuality), newValue);
        }
    }
    //the individual slider event for every slider
    public void SliderEvent(Slider target, TMP_Text sliderText)
    {
        UI_AssignSettings SettingsScript = target.GetComponent<UI_AssignSettings>();
        string info = SettingsScript.info;

        if (info == "Difficulty"         //general settings
            || info == "MouseSpeed"
            || info == "FieldOfView"     //graphics settings 
            || info == "LightDistance"
            || info == "ShadowDistance"
            || info == "TreeDistance"
            || info == "GrassDistance"
            || info == "ObjectDistance"
            || info == "ItemDistance"
            || info == "AIDistance"
            || info == "MasterVolume"    //audio settings
            || info == "MusicVolume"
            || info == "SFXVolume"
            || info == "NPCVolume")
        {
            sliderText.text = target.value.ToString();
        }
    }
    //the individual button event for every button
    public void ButtonEvent(Button target)
    { 
        UI_AssignSettings SettingsScript = target.GetComponent<UI_AssignSettings>();
        string info = SettingsScript.info;
        if (info == "VSyncState")
        {
            if (target.GetComponentInChildren<TMP_Text>().text == "true")
            {
                target.GetComponentInChildren<TMP_Text>().text = "false";
            }
            else if (target.GetComponentInChildren<TMP_Text>().text == "false")
            {
                target.GetComponentInChildren<TMP_Text>().text = "true";
            }
            user_EnableVSync = target.GetComponentInChildren<TMP_Text>().text;
        }
    }

    public void ResetSettings(bool resetSettings)
    {
        if (!resetSettings)
        {
            ConfirmationScript.RecieveData(gameObject,
                                           "settingsScript",
                                           "",
                                           "");
        }
        else
        {
            //deleted the settings file because it is no longer needed
            if (File.Exists(GameManagerScript.settingsPath + @"\Settings.txt"))
            {
                File.Delete(GameManagerScript.settingsPath + @"\Settings.txt");
            }

            //general settings

            //reset difficulty
            user_Difficulty = def_Difficulty;
            //reset mouse speed
            user_MouseSpeed = def_MouseSpeed;
            if (currentScene == 1)
            {
                PlayerCameraScript.sensX = user_MouseSpeed;
                PlayerCameraScript.sensY = user_MouseSpeed;
            }

            if (currentScene == 1
                && UIReuseScript.par_GeneralSettingsParent.activeInHierarchy)
            {
                RebuildSettingsList("general");
            }

            //graphics settings

            //reset graphics preset
            user_Preset = def_Preset;

            //reset resolution and fullscreen mode
            user_Resolution = def_Resolution;
            user_FullScreenMode = def_FullScreenMode;
            //get default x and y resolution values
            string resolution = def_Resolution.ToString();
            string[] splitResolutionString = resolution.Split('_');
            string[] resolutionValues = splitResolutionString[1].Split('x');
            int defaultX = int.Parse(resolutionValues[0]);
            int defaultY = int.Parse(resolutionValues[1]);
            //get default fullscreen mode
            int fullScreenMode = (int)def_FullScreenMode;
            //set default resolution and fullscreen mode
            if (fullScreenMode == 0)
            {
                Screen.SetResolution(defaultX, defaultY, FullScreenMode.ExclusiveFullScreen);
            }
            else if (fullScreenMode == 1)
            {
                Screen.SetResolution(defaultX, defaultY, FullScreenMode.FullScreenWindow);
            }
            else if (fullScreenMode == 2)
            {
                Screen.SetResolution(defaultX, defaultY, FullScreenMode.Windowed);
            }
            else if (fullScreenMode == 3)
            {
                Screen.SetResolution(defaultX, defaultY, FullScreenMode.MaximizedWindow);
            }

            //reset field of view
            user_FieldOfView = def_FieldOfView;
            if (currentScene == 1)
            {
                playerMainCamera.GetComponent<Camera>().fieldOfView = user_FieldOfView;
            }

            //reset vsync
            user_EnableVSync = def_EnableVsync;
            if (user_EnableVSync == "true")
            {
                Application.targetFrameRate = 60;
            }
            else if (user_EnableVSync == "false")
            {
                Application.targetFrameRate = 999;
            }

            //reset texture quality
            user_TextureQuality = def_TextureQuality;

            //reset light distance
            user_LightDistance = def_LightDistance;

            //reset shadow distance
            user_ShadowDistance = def_ShadowDistance;

            //reset shadow quality
            user_ShadowQuality = def_ShadowQuality;

            //reset tree distance
            user_TreeDistance = def_TreeDistance;

            //reset grass distance
            user_GrassDistance = def_GrassDistance;

            //reset object distance
            user_ObjectDistance = def_ObjectDistance;

            //reset item distance
            user_ItemDistance = def_ItemDistance;

            //reset ai distance
            user_AIDistance = def_AIDistance;

            if (currentScene == 1
                && UIReuseScript.par_GraphicsSettingsParent.activeInHierarchy)
            {
                RebuildSettingsList("graphics");
            }

            //audio settings

            //reset master volume
            user_MasterVolume = def_MasterVolume;

            //reset music volume
            user_MusicVolume = def_MusicVolume;

            //reset sfx volume
            user_SFXVolume = def_MusicVolume;

            //reset npc volume
            user_NPCVolume = def_NPCVolume;

            if (currentScene == 1 
                && UIReuseScript.par_AudioSettingsParent.activeInHierarchy)
            {
                RebuildSettingsList("audio");
            }
        }
    }

    //save settings to Settings.txt,
    //delete old file if it exists
    public void SaveSettings()
    {
        string settingsPath = GameManagerScript.settingsPath;
        string filePath = settingsPath + @"\Settings.txt";

        //delete the old settings file if it exists
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        using StreamWriter settingsFile = File.CreateText(filePath);

        settingsFile.WriteLine("Settings file for " + UIReuseScript.txt_GameVersion.text + ".");
        settingsFile.WriteLine("WARNING - Invalid values will break the game - edit at your own risk!");
        settingsFile.WriteLine("");

        List<GameObject> parents = new();
        foreach (GameObject par in UIReuseScript.generalSettingsParents)
        {
            parents.Add(par);
        }
        foreach (GameObject par in UIReuseScript.graphicsSettingsParents)
        {
            parents.Add(par);
        }
        foreach (GameObject par in UIReuseScript.audioSettingsParents)
        {
            parents.Add(par);
        }

        foreach (GameObject par in parents)
        {
            Dropdown dropDown = null;
            Slider slider = null;
            Button button = null;

            UI_AssignSettings AssignScript;
            string info = "";

            if (par.GetComponentInChildren<Dropdown>() != null
                && par.GetComponentInChildren<Dropdown>().gameObject.activeInHierarchy)
            {
                dropDown = par.GetComponentInChildren<Dropdown>();
                AssignScript = dropDown.GetComponent<UI_AssignSettings>();
                info = AssignScript.info;
            }
            else if (par.GetComponentInChildren<Slider>() != null
                     && par.GetComponentInChildren<Slider>().gameObject.activeInHierarchy)
            {
                slider = par.GetComponentInChildren<Slider>();
                AssignScript = slider.GetComponent<UI_AssignSettings>();
                info = AssignScript.info;
            }
            else if (par.GetComponentInChildren<Button>() != null
                     && par.GetComponentInChildren<Button>().gameObject.activeInHierarchy)
            {
                button = par.GetComponentInChildren<Button>();
                AssignScript = button.GetComponent<UI_AssignSettings>();
                info = AssignScript.info;
            }

            //general settings

            //apply difficulty
            if (info == "Difficulty")
            {
                //TODO: assign difficulty value
                user_Difficulty = (int)slider.value;
            }
            //apply mouse speed
            else if (info == "MouseSpeed")
            {
                user_MouseSpeed = (int)slider.value;
                PlayerCameraScript.sensX = user_MouseSpeed;
                PlayerCameraScript.sensY = user_MouseSpeed;
            }

            //graphics settings

            //apply preset
            else if (info == "Preset")
            {
                //TODO: assign preset
                string dropDownValue = dropDown.options[dropDown.value].ToString();
                Debug.Log(dropDownValue);
                user_Preset = (UserDefined_Preset)Enum.Parse(typeof(UserDefined_Preset), dropDownValue);
            }
            //apply resolution and fullscreen mode
            else if (info == "FullScreenMode")
            {
                //apply resolution and fullscreen mode
                string resolution = user_Resolution.ToString();
                string[] splitResolutionString = resolution.Split('_');
                string[] resolutionValues = splitResolutionString[1].Split('x');
                int defaultX = int.Parse(resolutionValues[0]);
                int defaultY = int.Parse(resolutionValues[1]);
                //get user defined fullscreen mode
                int fullScreenMode = (int)user_FullScreenMode;
                //set user defined resolution and fullscreen mode
                if (fullScreenMode == 0)
                {
                    Screen.SetResolution(defaultX, defaultY, FullScreenMode.ExclusiveFullScreen);
                }
                else if (fullScreenMode == 1)
                {
                    Screen.SetResolution(defaultX, defaultY, FullScreenMode.FullScreenWindow);
                }
                else if (fullScreenMode == 2)
                {
                    Screen.SetResolution(defaultX, defaultY, FullScreenMode.Windowed);
                }
                else if (fullScreenMode == 3)
                {
                    Screen.SetResolution(defaultX, defaultY, FullScreenMode.MaximizedWindow);
                }
            }
            else if (info == "FieldOfView")
            {
                user_FieldOfView = (int)slider.value;
                playerMainCamera.GetComponent<Camera>().fieldOfView = user_FieldOfView;
            }
            else if (info == "VSyncState")
            {
                user_EnableVSync = button.GetComponentInChildren<TMP_Text>().text;
                if (user_EnableVSync == "true")
                {
                    Application.targetFrameRate = 60;
                }
                else if (user_EnableVSync == "false")
                {
                    Application.targetFrameRate = 999;
                }
            }
            else if (info == "TextureQuality")
            {
                //TODO: assign texture quality
                string dropDownValue = dropDown.options[dropDown.value].ToString();
                user_TextureQuality = (UserDefined_TextureQuality)Enum.Parse(typeof(UserDefined_TextureQuality), dropDownValue);
            }
            else if (info == "LightDistance")
            {
                //TODO: assign light distance
                user_LightDistance = (int)slider.value;
            }
            else if (info == "ShadowDistance")
            {
                //TODO: assign shadow distance
                user_ShadowDistance = (int)slider.value;
            }
            else if (info == "ShadowQuality")
            {
                //TODO: assign shadow quality
                string dropDownValue = dropDown.options[dropDown.value].ToString();
                user_ShadowQuality = (UserDefined_ShadowQuality)Enum.Parse(typeof(UserDefined_ShadowQuality), dropDownValue);
            }
            else if (info == "TreeDistance")
            {
                //TODO: assign tree distance
                user_TreeDistance = (int)slider.value;
            }
            else if (info == "GrassDistance")
            {
                //TODO: assign grass distance
                user_GrassDistance = (int)slider.value;
            }
            else if (info == "ObjectDistance")
            {
                //TODO: assign object distance
                user_ObjectDistance = (int)slider.value;
            }
            else if (info == "ItemDistance")
            {
                //TODO: assign item distance
                user_ItemDistance = (int)slider.value;
            }
            else if (info == "AIDistance")
            {
                //TODO: assign AI distance
                user_AIDistance = (int)slider.value;
            }

            //audio settings

            else if (info == "MasterVolume")
            {
                //TODO: assign master volume
                user_MasterVolume = (int)slider.value;
            }
            else if (info == "MusicVolume")
            {
                //TODO: assign music volume
                user_MusicVolume = (int)slider.value;
            }
            else if (info == "SFXVolume")
            {
                //TODO: assign SFX volume
                user_SFXVolume = (int)slider.value;
            }
            else if (info == "NPCVolume")
            {
                //TODO: assign NPC volume
                user_NPCVolume = (int)slider.value;
            }
        }

        settingsFile.WriteLine("---GENERAL SETTINGS---");
        settingsFile.WriteLine("Difficulty: " + user_Difficulty);
        settingsFile.WriteLine("MouseSpeed: " + user_MouseSpeed);
        settingsFile.WriteLine("");

        settingsFile.WriteLine("---GRAPHICS SETTINGS---");
        settingsFile.WriteLine("Preset: " + user_Preset);
        string res = user_Resolution.ToString().Replace("res_", "");
        settingsFile.WriteLine("Resolution: " + res);
        settingsFile.WriteLine("FullScreenMode: " + user_FullScreenMode);
        settingsFile.WriteLine("FieldOfView: " + user_FieldOfView);
        settingsFile.WriteLine("VSyncState: " + user_EnableVSync);
        settingsFile.WriteLine("TextureQuality: " + user_TextureQuality);
        settingsFile.WriteLine("LightDistance: " + user_LightDistance);
        settingsFile.WriteLine("ShadowDistance: " + user_ShadowDistance);
        settingsFile.WriteLine("ShadowQuality: " + user_ShadowQuality);
        settingsFile.WriteLine("TreeDistance: " + user_TreeDistance);
        settingsFile.WriteLine("GrassDistance: " + user_GrassDistance);
        settingsFile.WriteLine("ObjectDistance: " + user_ObjectDistance);
        settingsFile.WriteLine("ItemDistance: " + user_ItemDistance);
        settingsFile.WriteLine("AIDistance: " + user_AIDistance);
        settingsFile.WriteLine("");

        settingsFile.WriteLine("---AUDIO SETTINGS---");
        settingsFile.WriteLine("MasterVolume: " + user_MasterVolume);
        settingsFile.WriteLine("MusicVolume: " + user_MusicVolume);
        settingsFile.WriteLine("SFXVolume: " + user_SFXVolume);
        settingsFile.WriteLine("NPCVolume: " + user_NPCVolume);

        Debug.Log("Successfully saved settings to " + filePath + ".");
    }

    //load settings from Settings.txt,
    //reset settings if load settings file was not found
    public void LoadSettings()
    {
        string settingsFilePath = GameManagerScript.settingsPath + @"\Settings.txt";

        if (!File.Exists(settingsFilePath))
        {
            ResetSettings(true);
            Debug.Log("Loaded default settings.");
        }
        else
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

            foreach (Transform parent in parents)
            {
                UI_AssignSettings AssignScript = parent.transform.GetComponentInChildren<UI_AssignSettings>();
                string info = AssignScript.info;

                foreach (string line in File.ReadLines(settingsFilePath))
                {
                    if (line.Contains(':'))
                    {
                        string[] valueSplit = line.Split(':');
                        string type = valueSplit[0];
                        string value = valueSplit[1].Replace(" ", string.Empty);

                        if (info == type)
                        {
                            //general settings
                            if (type == "Difficulty")
                            {
                                //TODO: assign difficulty
                                int insertedValue;
                                bool isInt = int.TryParse(value, out _);
                                if (isInt)
                                {
                                    insertedValue = int.Parse(value);
                                    if (insertedValue >= -100
                                        && insertedValue < 100)
                                    {
                                        user_Difficulty = insertedValue;
                                    }
                                    else
                                    {
                                        Debug.LogError("Error: Inserted " + type + " value in settings file is out of range! Resetting to default value.");
                                        user_Difficulty = def_Difficulty;
                                    }
                                }
                                else
                                {
                                    Debug.LogError("Error: Inserted " + type + " value in settings file is invalid! Resetting to default value.");
                                    user_Difficulty = def_Difficulty;
                                }
                            }
                            else if (type == "MouseSpeed")
                            {
                                int insertedValue;
                                bool isInt = int.TryParse(value, out _);
                                if (isInt)
                                {
                                    insertedValue = int.Parse(value);
                                    if (insertedValue >= 1
                                        && insertedValue <= 100)
                                    {
                                        user_MouseSpeed = insertedValue;
                                    }
                                    else
                                    {
                                        Debug.LogError("Error: Inserted " + type + " value in settings file is invalid! Resetting to default value.");
                                        user_MouseSpeed = def_MouseSpeed;
                                        PlayerCameraScript.sensX = user_MouseSpeed;
                                        PlayerCameraScript.sensY = user_MouseSpeed;
                                    }
                                }
                                else
                                {
                                    Debug.LogError("Error: Inserted " + type + " value in settings file is invalid! Resetting to default value.");
                                    user_MouseSpeed = def_MouseSpeed;
                                    PlayerCameraScript.sensX = user_MouseSpeed;
                                    PlayerCameraScript.sensY = user_MouseSpeed;
                                }
                            }

                            //graphics settings
                            else if (type == "Preset")
                            {
                                //TODO: apply preset

                                string[] presetNames = Enum.GetNames(typeof(UserDefined_Preset));
                                bool foundCorrectPresetName = false;
                                foreach (string presetName in presetNames)
                                {
                                    if (presetName == value)
                                    {
                                        foundCorrectPresetName = true;
                                        break;
                                    }
                                }
                                if (foundCorrectPresetName)
                                {
                                    user_Preset = (UserDefined_Preset)Enum.Parse(typeof(UserDefined_Preset), value);
                                }
                                else
                                {
                                    Debug.LogError("Error: Inserted " + type + " value in settings file is invalid! Resetting to default value.");
                                    user_Preset = def_Preset;
                                }
                            }
                            else if (info == "Resolution")
                            {
                                string[] resolutions = Enum.GetNames(typeof(UserDefined_Resolution));
                                bool foundCorrectResolution = false;
                                foreach (string res in resolutions)
                                {
                                    if (res.Replace("res_", "") == value)
                                    {
                                        foundCorrectResolution = true;
                                        break;
                                    }
                                }
                                if (foundCorrectResolution)
                                {
                                    user_Resolution = (UserDefined_Resolution)Enum.Parse(typeof(UserDefined_Resolution), "res_" + value);
                                }
                                else
                                {
                                    Debug.LogError("Error: Inserted " + type + " value in settings file is invalid! Resetting to default value.");
                                    user_Resolution = def_Resolution;
                                }
                            }
                            else if (info == "FullScreenMode")
                            {
                                string[] fullscreenModes = Enum.GetNames(typeof(UserDefined_FullScreenMode));
                                bool foundCorrectFullscreenMode = false;
                                foreach (string mode in fullscreenModes)
                                {
                                    if (mode == value)
                                    {
                                        foundCorrectFullscreenMode = true;
                                        break;
                                    }
                                }
                                if (foundCorrectFullscreenMode)
                                {
                                    user_FullScreenMode = (UserDefined_FullScreenMode)Enum.Parse(typeof(UserDefined_FullScreenMode), value);
                                }
                                else
                                {
                                    Debug.LogError("Error: Inserted " + type + " value in settings file is invalid! Resetting to default value.");
                                    user_FullScreenMode = def_FullScreenMode;
                                }

                                //apply resolution and fullscreen mode
                                string resolution = user_Resolution.ToString();
                                string[] splitResolutionString = resolution.Split('_');
                                string[] resolutionValues = splitResolutionString[1].Split('x');
                                int defaultX = int.Parse(resolutionValues[0]);
                                int defaultY = int.Parse(resolutionValues[1]);
                                //get user defined fullscreen mode
                                int fullScreenMode = (int)user_FullScreenMode;
                                //set user defined resolution and fullscreen mode
                                if (fullScreenMode == 0)
                                {
                                    Screen.SetResolution(defaultX, defaultY, FullScreenMode.ExclusiveFullScreen);
                                }
                                else if (fullScreenMode == 1)
                                {
                                    Screen.SetResolution(defaultX, defaultY, FullScreenMode.FullScreenWindow);
                                }
                                else if (fullScreenMode == 2)
                                {
                                    Screen.SetResolution(defaultX, defaultY, FullScreenMode.Windowed);
                                }
                                else if (fullScreenMode == 3)
                                {
                                    Screen.SetResolution(defaultX, defaultY, FullScreenMode.MaximizedWindow);
                                }
                            }
                            else if (type == "FieldOfView")
                            {
                                int insertedValue;
                                bool isInt = int.TryParse(value, out _);
                                if (isInt)
                                {
                                    insertedValue = int.Parse(value);
                                    if (insertedValue >= 70
                                        && insertedValue <= 110)
                                    {
                                        user_FieldOfView = insertedValue;
                                        playerMainCamera.GetComponent<Camera>().fieldOfView = user_FieldOfView;
                                    }
                                    else
                                    {
                                        Debug.LogError("Error: Inserted " + type + " value in settings file is out of range! Resetting to default value.");
                                        user_FieldOfView = def_FieldOfView;
                                        playerMainCamera.GetComponent<Camera>().fieldOfView = user_FieldOfView;
                                    }
                                }
                                else
                                {
                                    Debug.LogError("Error: Inserted " + type + " value in settings file is invalid! Resetting to default value.");
                                    user_FieldOfView = def_FieldOfView;
                                    playerMainCamera.GetComponent<Camera>().fieldOfView = user_FieldOfView;
                                }
                            }
                            else if (type == "VSyncState")
                            {
                                if (value == "true"
                                    || value == "false")
                                {
                                    user_EnableVSync = value;
                                }
                                else
                                {
                                    Debug.LogError("Error: Inserted " + type + " value in settings file is invalid! Resetting to default value.");
                                    user_EnableVSync = def_EnableVsync;
                                }

                                if (user_EnableVSync == "true")
                                {
                                    Application.targetFrameRate = 60;
                                }
                                else if (user_EnableVSync == "false")
                                {
                                    Application.targetFrameRate = 999;
                                }
                            }
                            else if (type == "TextureQuality")
                            {
                                //TODO: apply texture quality

                                string[] textureQualityValues = Enum.GetNames(typeof(UserDefined_TextureQuality));
                                bool foundCorrectTextureQuality = false;
                                foreach (string tex in textureQualityValues)
                                {
                                    if (tex == value)
                                    {
                                        foundCorrectTextureQuality = true;
                                        break;
                                    }
                                }
                                if (foundCorrectTextureQuality)
                                {
                                    user_TextureQuality = (UserDefined_TextureQuality)Enum.Parse(typeof(UserDefined_TextureQuality), value);
                                }
                                else
                                {
                                    Debug.LogError("Error: Inserted " + type + " value in settings file is invalid! Resetting to default value.");
                                    user_TextureQuality = def_TextureQuality;
                                }
                            }
                            else if (type == "LightDistance")
                            {
                                //TODO: apply light distance

                                int insertedValue;
                                bool isInt = int.TryParse(value, out _);
                                if (isInt)
                                {
                                    insertedValue = int.Parse(value);
                                    if (insertedValue >= 15
                                        && insertedValue <= 5000)
                                    {
                                        user_LightDistance = insertedValue;
                                    }
                                    else
                                    {
                                        Debug.LogError("Error: Inserted " + type + " value in settings file is out of range! Resetting to default value.");
                                        user_LightDistance = def_LightDistance;
                                    }
                                }
                                else
                                {
                                    Debug.LogError("Error: Inserted " + type + " value in settings file is invalid! Resetting to default value.");
                                    user_LightDistance = def_LightDistance;
                                }
                            }
                            else if (type == "ShadowDistance")
                            {
                                //TODO: apply shadow distance

                                int insertedValue;
                                bool isInt = int.TryParse(value, out _);
                                if (isInt)
                                {
                                    insertedValue = int.Parse(value);
                                    if (insertedValue >= 15
                                        && insertedValue <= 5000)
                                    {
                                        user_ShadowDistance = insertedValue;
                                    }
                                    else
                                    {
                                        Debug.LogError("Error: Inserted " + type + " value in settings file is out of range! Resetting to default value.");
                                        user_ShadowDistance = def_ShadowDistance;
                                    }
                                }
                                else
                                {
                                    Debug.LogError("Error: Inserted " + type + " value in settings file is invalid! Resetting to default value.");
                                    user_ShadowDistance = def_ShadowDistance;
                                }
                            }
                            else if (type == "ShadowQuality")
                            {
                                //TODO: apply shadow quality

                                string[] shadowQualityValues = Enum.GetNames(typeof(UserDefined_ShadowQuality));
                                bool foundCorrectShadowValue = false;
                                foreach (string shadow in shadowQualityValues)
                                {
                                    if (shadow == value)
                                    {
                                        foundCorrectShadowValue = true;
                                        break;
                                    }
                                }
                                if (foundCorrectShadowValue)
                                {
                                    user_ShadowQuality = (UserDefined_ShadowQuality)Enum.Parse(typeof(UserDefined_ShadowQuality), value);
                                }
                                else
                                {
                                    Debug.LogError("Error: Inserted " + type + " value in settings file is invalid! Resetting to default value.");
                                    user_ShadowQuality = def_ShadowQuality;
                                }
                            }
                            else if (type == "TreeDistance")
                            {
                                //TODO: apply tree distance

                                int insertedValue;
                                bool isInt = int.TryParse(value, out _);
                                if (isInt)
                                {
                                    insertedValue = int.Parse(value);
                                    if (insertedValue >= 15
                                        && insertedValue <= 5000)
                                    {
                                        user_TreeDistance = insertedValue;
                                    }
                                    else
                                    {
                                        Debug.LogError("Error: Inserted " + type + " value in settings file is out of range! Resetting to default value.");
                                        user_TreeDistance = def_TreeDistance;
                                    }
                                }
                                else
                                {
                                    Debug.LogError("Error: Inserted " + type + " value in settings file is invalid! Resetting to default value.");
                                    user_TreeDistance = def_TreeDistance;
                                }
                            }
                            else if (type == "GrassDistance")
                            {
                                //TODO: apply grass distance

                                int insertedValue;
                                bool isInt = int.TryParse(value, out _);
                                if (isInt)
                                {
                                    insertedValue = int.Parse(value);
                                    if (insertedValue >= 15
                                        && insertedValue <= 5000)
                                    {
                                        user_GrassDistance = insertedValue;
                                    }
                                    else
                                    {
                                        Debug.LogError("Error: Inserted " + type + " value in settings file is out of range! Resetting to default value.");
                                        user_GrassDistance = def_GrassDistance;
                                    }
                                }
                                else
                                {
                                    Debug.LogError("Error: Inserted " + type + " value in settings file is invalid! Resetting to default value.");
                                    user_GrassDistance = def_GrassDistance;
                                }
                            }
                            else if (type == "ObjectDistance")
                            {
                                //TODO: apply object distance

                                int insertedValue;
                                bool isInt = int.TryParse(value, out _);
                                if (isInt)
                                {
                                    insertedValue = int.Parse(value);
                                    if (insertedValue >= 15
                                        && insertedValue <= 5000)
                                    {
                                        user_ObjectDistance = insertedValue;
                                    }
                                    else
                                    {
                                        Debug.LogError("Error: Inserted " + type + " value in settings file is out of range! Resetting to default value.");
                                        user_ObjectDistance = def_ObjectDistance;
                                    }
                                }
                                else
                                {
                                    Debug.LogError("Error: Inserted " + type + " value in settings file is invalid! Resetting to default value.");
                                    user_ObjectDistance = def_ObjectDistance;
                                }
                            }
                            else if (type == "ItemDistance")
                            {
                                //TODO: apply item distance

                                int insertedValue;
                                bool isInt = int.TryParse(value, out _);
                                if (isInt)
                                {
                                    insertedValue = int.Parse(value);
                                    if (insertedValue >= 15
                                        && insertedValue <= 5000)
                                    {
                                        user_ItemDistance = insertedValue;
                                    }
                                    else
                                    {
                                        Debug.LogError("Error: Inserted " + type + " value in settings file is out of range! Resetting to default value.");
                                        user_ItemDistance = def_ItemDistance;
                                    }
                                }
                                else
                                {
                                    Debug.LogError("Error: Inserted " + type + " value in settings file is invalid! Resetting to default value.");
                                    user_ItemDistance = def_ItemDistance;
                                }
                            }
                            else if (type == "AIDistance")
                            {
                                //TODO: apply AI distance

                                int insertedValue;
                                bool isInt = int.TryParse(value, out _);
                                if (isInt)
                                {
                                    insertedValue = int.Parse(value);
                                    if (insertedValue >= 15
                                        && insertedValue <= 5000)
                                    {
                                        user_AIDistance = insertedValue;
                                    }
                                    else
                                    {
                                        Debug.LogError("Error: Inserted " + type + " value in settings file is out of range! Resetting to default value.");
                                        user_AIDistance = def_AIDistance;
                                    }
                                }
                                else
                                {
                                    Debug.LogError("Error: Inserted " + type + " value in settings file is invalid! Resetting to default value.");
                                    user_AIDistance = def_AIDistance;
                                }
                            }

                            //audio settings
                            else if (type == "MasterVolume")
                            {
                                //TODO: apply master volume

                                int insertedValue;
                                bool isInt = int.TryParse(value, out _);
                                if (isInt)
                                {
                                    insertedValue = int.Parse(value);
                                    if (insertedValue >= 0
                                        && insertedValue <= 100)
                                    {
                                        user_MasterVolume = insertedValue;
                                    }
                                    else
                                    {
                                        Debug.LogError("Error: Inserted " + type + " value in settings file is out of range! Resetting to default value.");
                                        user_MasterVolume = def_MasterVolume;
                                    }
                                }
                                else
                                {
                                    Debug.LogError("Error: Inserted " + type + " value in settings file is invalid! Resetting to default value.");
                                    user_MasterVolume = def_MasterVolume;
                                }
                            }
                            else if (type == "MusicVolume")
                            {
                                //TODO: apply music volume

                                int insertedValue;
                                bool isInt = int.TryParse(value, out _);
                                if (isInt)
                                {
                                    insertedValue = int.Parse(value);
                                    if (insertedValue >= 0
                                        && insertedValue <= 100)
                                    {
                                        user_MusicVolume = insertedValue;
                                    }
                                    else
                                    {
                                        Debug.LogError("Error: Inserted " + type + " value in settings file is out of range! Resetting to default value.");
                                        user_MusicVolume = def_MusicVolume;
                                    }
                                }
                                else
                                {
                                    Debug.LogError("Error: Inserted " + type + " value in settings file is invalid! Resetting to default value.");
                                    user_MusicVolume = def_MusicVolume;
                                }
                            }
                            else if (type == "SFXVolume")
                            {
                                //TODO: apply SFX volume

                                int insertedValue;
                                bool isInt = int.TryParse(value, out _);
                                if (isInt)
                                {
                                    insertedValue = int.Parse(value);
                                    if (insertedValue >= 0
                                        && insertedValue <= 100)
                                    {
                                        user_SFXVolume = insertedValue;
                                    }
                                    else
                                    {
                                        Debug.LogError("Error: Inserted " + type + " value in settings file is out of range! Resetting to default value.");
                                        user_SFXVolume = def_SFXVolume;
                                    }
                                }
                                else
                                {
                                    Debug.LogError("Error: Inserted " + type + " value in settings file is invalid! Resetting to default value.");
                                    user_SFXVolume = def_SFXVolume;
                                }
                            }
                            else if (type == "NPCVolume")
                            {
                                //TODO: apply NPC volume

                                int insertedValue;
                                bool isInt = int.TryParse(value, out _);
                                if (isInt)
                                {
                                    insertedValue = int.Parse(value);
                                    if (insertedValue >= 0
                                        && insertedValue <= 100)
                                    {
                                        user_NPCVolume = insertedValue;
                                    }
                                    else
                                    {
                                        Debug.LogError("Error: Inserted " + type + " value in settings file is out of range! Resetting to default value.");
                                        user_NPCVolume = def_NPCVolume;
                                    }
                                }
                                else
                                {
                                    Debug.LogError("Error: Inserted " + type + " value in settings file is invalid! Resetting to default value.");
                                    user_NPCVolume = def_NPCVolume;
                                }
                            }
                        }
                    }
                }
            }

            Debug.Log("Successfully loaded game settings from " + GameManagerScript.settingsPath + @"\Settings.txt" + "!");
        }
    }
}