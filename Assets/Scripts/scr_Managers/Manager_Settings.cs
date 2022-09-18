using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager_Settings : MonoBehaviour
{
    [Header("Graphics settings")]
    [HideInInspector] public bool user_enableVSync;
    public bool def_EnableVsync = true;
    [HideInInspector] public Resolution user_Resolution;
    public Resolution def_Resolution = Resolution.res_1920x1080;
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

    //private variables
    private float timer;
    private float deltaTime;

    //scripts
    private Manager_UIReuse UIReuseScript;

    private void Awake()
    {
        UIReuseScript = GetComponent<Manager_UIReuse>();
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

    //sets the settings from the user settings file
    //TODO: create settings system and assign settings properly
    public void AssignSettings()
    {
        //set resolution to 1080p and enable fullscreen mode
        Screen.SetResolution(1920, 1080, FullScreenMode.FullScreenWindow);

        //enable or disable vsync
        if (def_EnableVsync)
        {
            QualitySettings.vSyncCount = 1;
        }
        else
        {
            QualitySettings.vSyncCount = 0;
        }
    }
}