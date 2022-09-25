using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Globalization;

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
    public Preset def_Preset = Preset.medium;
    [HideInInspector] public Preset user_Preset;
    public enum Preset
    {
        low,
        medium,
        high,
        ultra,
        custom
    }
    public Resolution def_Resolution = Resolution.res_1920x1080;
    [HideInInspector] public Resolution user_Resolution;
    public enum Resolution
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
    public UserDefined_FullScreenMode def_FullScreenMode;
    [HideInInspector] public UserDefined_FullScreenMode user_FullScreenMode;
    public enum UserDefined_FullScreenMode
    {
        MaximizedWindow,
        ExclusiveFullScreen,
        Windowed,
        FullScreenWindow
    }
    public int def_FieldOfView = 90;
    [HideInInspector] public int user_FieldOfView;
    public bool def_EnableVsync = true;
    [HideInInspector] public bool user_EnableVSync;
    public TextureQuality def_TextureQuality = TextureQuality.medium;
    [HideInInspector] public TextureQuality user_TextureQuality;
    public enum TextureQuality
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
    public ShadowQuality def_ShadowQuality = ShadowQuality.medium;
    [HideInInspector] public ShadowQuality user_ShadowQuality;
    public enum ShadowQuality
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

    //public but hidden variables
    [HideInInspector] public bool savedSettings;

    //private variables
    private float timer;
    private float deltaTime;
    private int currentScene;

    //scripts
    private UI_Confirmation ConfirmationScript;
    private GameManager GameManagerScript;
    private Manager_UIReuse UIReuseScript;
    private Player_Camera PlayerCameraScript;

    private void Awake()
    {
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

    private void Update()
    {
        //framerate block beginning
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        float msec = Mathf.FloorToInt(deltaTime * 1000.0f);
        float fps = Mathf.FloorToInt(1.0f / deltaTime);

        timer += Time.unscaledDeltaTime;
        if (timer > 0.1f)
        {
            UIReuseScript.txt_FPS.text = fps + " (" + msec + ")";
            timer = 0;
        }
        //framerate block ending
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
                string info = AssignScript.str_Info;

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
            foreach (Transform child in UIReuseScript.par_MainGeneralSettingsParent.transform)
            {
                if (child.GetComponentInChildren<Scrollbar>() != null)
                {
                    child.GetComponentInChildren<Scrollbar>().value = 1;
                    break;
                }
            }

            settingsParents = UIReuseScript.generalSettingsParents;
        }
        else if (targetSettingsParentName == "graphics")
        {
            UIReuseScript.par_MainGraphicsSettingsParent.SetActive(true);
            UIReuseScript.btn_ShowGraphicsSettings.interactable = false;
            foreach (Transform child in UIReuseScript.par_MainGraphicsSettingsParent.transform)
            {
                if (child.GetComponentInChildren<Scrollbar>() != null)
                {
                    child.GetComponentInChildren<Scrollbar>().value = 1;
                    break;
                }
            }

            settingsParents = UIReuseScript.graphicsSettingsParents;
        }
        else if (targetSettingsParentName == "audio")
        {
            UIReuseScript.par_MainAudioSettingsParent.SetActive(true);
            UIReuseScript.btn_ShowAudioSettings.interactable = false;
            foreach (Transform child in UIReuseScript.par_MainAudioSettingsParent.transform)
            {
                if (child.GetComponentInChildren<Scrollbar>() != null) 
                {
                    child.GetComponentInChildren<Scrollbar>().value = 1;
                    break;
                }
            }

            settingsParents = UIReuseScript.audioSettingsParents;
        }

        foreach (GameObject buttonUIParent in settingsParents)
        {
            TMP_Dropdown dropdown;
            Slider slider;
            Toggle toggle;

            if (buttonUIParent.GetComponentInChildren<TMP_Dropdown>() != null
                && buttonUIParent.GetComponentInChildren<TMP_Dropdown>().gameObject.activeInHierarchy)
            {
                dropdown = buttonUIParent.GetComponentInChildren<TMP_Dropdown>();
                dropdown.ClearOptions();
                AssignEvent(dropdown.gameObject, "dropdown");
            }
            else if (buttonUIParent.GetComponentInChildren<Slider>() != null
                     && buttonUIParent.GetComponentInChildren<Slider>().gameObject.activeInHierarchy)
            {
                slider = buttonUIParent.GetComponentInChildren<Slider>();
                slider.onValueChanged.RemoveAllListeners();
                AssignEvent(slider.gameObject, "slider");
            }
            else if (buttonUIParent.GetComponentInChildren<Toggle>() != null
                     && buttonUIParent.GetComponentInChildren<Toggle>().gameObject.activeInHierarchy)
            {
                toggle = buttonUIParent.GetComponentInChildren<Toggle>();
                toggle.onValueChanged.RemoveAllListeners();
                AssignEvent(toggle.gameObject, "toggle");
            }
        }
    }

    //assign event to all targets
    public void AssignEvent(GameObject target, string targetType)
    {
        UI_AssignSettings AssignScript = target.GetComponent<UI_AssignSettings>();
        string info = AssignScript.str_Info;

        if (targetType == "dropdown")
        {
            TMP_Dropdown dropdown = target.GetComponent<TMP_Dropdown>();

            //assign preset choices, current choice and event
            if (info == "Preset")
            {
                List<string> values = new(Enum.GetNames(typeof(Preset)));
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
                List<string> values = new(Enum.GetNames(typeof(Resolution)));
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
                List<string> values = new(Enum.GetNames(typeof(TextureQuality)));
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
                List<string> values = new(Enum.GetNames(typeof(ShadowQuality)));
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
        else if (targetType == "toggle")
        {
            Toggle toggle = target.GetComponent<Toggle>();

            if (info == "VSyncState")
            {
                //get user defined vsync state
                target.GetComponent<Toggle>().enabled = user_EnableVSync;

                //create a new toggle event for vsync state
                target.GetComponent<Toggle>().onValueChanged.AddListener(delegate { ToggleEvent(toggle); });
            }
        }
    }

    //the individual dropdown event for every selected dropdown choice
    public void DropdownEvent(TMP_Dropdown target)
    {
        UI_AssignSettings SettingsScript = target.GetComponent<UI_AssignSettings>();
        string info = SettingsScript.str_Info;

        string newValue = target.value.ToString();

        if (info == "Preset")
        {
            user_Preset = (Preset)Enum.Parse(typeof(Preset), newValue);
        }
        else if (info == "Resolution")
        {
            user_Resolution = (Resolution)Enum.Parse(typeof(Resolution), newValue);
        }
        else if (info == "FullScreenMode")
        {
            user_FullScreenMode = (UserDefined_FullScreenMode)Enum.Parse(typeof(UserDefined_FullScreenMode), newValue);
        }
        else if (info == "TextureQuality")
        {
            user_TextureQuality = (TextureQuality)Enum.Parse(typeof(TextureQuality), newValue);
        }
        else if (info == "ShadowQuality")
        {
            user_ShadowQuality = (ShadowQuality)Enum.Parse(typeof(ShadowQuality), newValue);
        }
    }
    //the individual slider event for every slider
    public void SliderEvent(Slider target, TMP_Text sliderText)
    {
        UI_AssignSettings SettingsScript = target.GetComponent<UI_AssignSettings>();
        string info = SettingsScript.str_Info;

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
    //the individual toggle event for every toggle
    public void ToggleEvent(Toggle target)
    { 
        UI_AssignSettings SettingsScript = target.GetComponent<UI_AssignSettings>();
        string info = SettingsScript.str_Info;
        if (info == "VSyncState")
        {
            user_EnableVSync = target.GetComponent<Toggle>().isOn;
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
            savedSettings = false;

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

            if (UIReuseScript.par_GeneralSettingsParent.activeInHierarchy)
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
            if (user_EnableVSync)
            {
                QualitySettings.vSyncCount = 1;
            }
            else
            {
                QualitySettings.vSyncCount = 0;
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

            if (UIReuseScript.par_GraphicsSettingsParent.activeInHierarchy)
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

            if (UIReuseScript.par_AudioSettingsParent.activeInHierarchy)
            {
                RebuildSettingsList("audio");
            }
        }
    }

    //save settings to Settings.txt,
    //delete old file if it exists
    public void SaveSettings()
    {
        savedSettings = true;

        string settingsPath = GameManagerScript.settingsPath;
        string filePath = settingsPath + @"\Settings.txt";

        //delete the old settings file if it exists
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        using StreamWriter settingsFile = File.CreateText(filePath);

        settingsFile.WriteLine("Settings file for " + UIReuseScript.txt_GameVersion.text + ".");
        settingsFile.WriteLine("WARNING: Invalid values will break the game - edit at your own risk!");
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
            Transform target = par.transform.GetChild(0);
            Slider slider = null;
            UI_AssignSettings AssignScript;
            string info = "";
            if (target.GetComponentInChildren<Slider>() != null
                && target.GetComponentInChildren<Slider>().gameObject.activeInHierarchy)
            {
                slider = target.GetComponentInChildren<Slider>();
                AssignScript = slider.GetComponent<UI_AssignSettings>();
                info = AssignScript.str_Info;
            }

            //general settings

            //apply difficulty
            if (info == "Difficulty")
            {
                //TODO: assign difficulty value
            }
            //apply mouse speed
            else if (info == "MouseSpeed")
            {
                PlayerCameraScript.sensX = user_MouseSpeed;
                PlayerCameraScript.sensY = user_MouseSpeed;
            }

            //graphics settings

            //apply preset
            else if (info == "Preset")
            {
                //TODO: assign preset
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
                playerMainCamera.GetComponent<Camera>().fieldOfView = user_FieldOfView;
            }
            else if (info == "VSyncState")
            {
                if (user_EnableVSync)
                {
                    QualitySettings.vSyncCount = 1;
                }
                else
                {
                    QualitySettings.vSyncCount = 0;
                }
            }
            else if (info == "TextureQuality")
            {
                //TODO: assign texture quality
            }
            else if (info == "LightDistance")
            {
                //TODO: assign light distance
            }
            else if (info == "ShadowDistance")
            {
                //TODO: assign shadow distance
            }
            else if (info == "ShadowQuality")
            {
                //TODO: assign shadow quality
            }
            else if (info == "TreeDistance")
            {
                //TODO: assign tree distance
            }
            else if (info == "GrassDistance")
            {
                //TODO: assign grass distance
            }
            else if (info == "ObjectDistance")
            {
                //TODO: assign object distance
            }
            else if (info == "ItemDistance")
            {
                //TODO: assign item distance
            }
            else if (info == "AIDistance")
            {
                //TODO: assign AI distance
            }

            //audio settings

            else if (info == "MasterVolume")
            {
                //TODO: assign master volume
            }
            else if (info == "MusicVolume")
            {
                //TODO: assign music volume
            }
            else if (info == "SFXVolume")
            {
                //TODO: assign SFX volume
            }
            else if (info == "NPCVolume")
            {
                //TODO: assign NPC volume
            }
        }

        settingsFile.WriteLine("---GENERAL SETTINGS---");
        settingsFile.WriteLine("ge_Difficulty: " + user_Difficulty);
        settingsFile.WriteLine("ge_MouseSpeed: " + user_MouseSpeed);
        settingsFile.WriteLine("");

        settingsFile.WriteLine("---GRAPHICS SETTINGS---");
        settingsFile.WriteLine("gr_Preset: " + user_Preset);
        string res = user_Resolution.ToString().Replace("res_", "");
        settingsFile.WriteLine("gr_Resolution: " + res);
        settingsFile.WriteLine("gr_FullScreenMode: " + user_FullScreenMode);
        settingsFile.WriteLine("gr_FieldOfView: " + user_FieldOfView);
        settingsFile.WriteLine("gr_VSyncState: " + user_EnableVSync);
        settingsFile.WriteLine("gr_TextureQuality: " + user_TextureQuality);
        settingsFile.WriteLine("gr_LightDistance: " + user_LightDistance);
        settingsFile.WriteLine("gr_ShadowDistance: " + user_ShadowDistance);
        settingsFile.WriteLine("gr_ShadowQuality: " + user_ShadowQuality);
        settingsFile.WriteLine("gr_TreeDistance: " + user_TreeDistance);
        settingsFile.WriteLine("gr_GrassDistance: " + user_GrassDistance);
        settingsFile.WriteLine("gr_ObjectDistance: " + user_ObjectDistance);
        settingsFile.WriteLine("gr_ItemDistance: " + user_ItemDistance);
        settingsFile.WriteLine("gr_AIDistance: " + user_AIDistance);
        settingsFile.WriteLine("");

        settingsFile.WriteLine("---AUDIO SETTINGS---");
        settingsFile.WriteLine("au_MasterVolume: " + user_MasterVolume);
        settingsFile.WriteLine("au_MusicVolume: " + user_MusicVolume);
        settingsFile.WriteLine("au_SFXVolume: " + user_SFXVolume);
        settingsFile.WriteLine("au_NPCVolume: " + user_NPCVolume);

        Debug.Log("Successfully saved settings to " + filePath + ".");
    }

    //load settings from Settings.txt,
    //reset settings if load settings file was not found
    public void LoadSettings()
    {
        //temporarily always reset settings
        ResetSettings(true);

        //Debug.Log("Successfully loaded settings from ");
    }
}