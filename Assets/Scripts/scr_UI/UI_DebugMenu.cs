using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_DebugMenu : MonoBehaviour
{
    //private variables
    private float timer;
    private float deltaTime;
    private int currentScene;

    //scripts
    private Manager_UIReuse UIReuseScript;
    private Manager_DateAndTime DateAndTimeScript;

    private void Awake()
    {
        UIReuseScript = GetComponent<Manager_UIReuse>();

        currentScene = SceneManager.GetActiveScene().buildIndex;
        if (currentScene == 1)
        {
            DateAndTimeScript = GetComponent<Manager_DateAndTime>();
        }
    }

    private void Update()
    {
        //framerate
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        float msec = Mathf.FloorToInt(deltaTime * 1000.0f);
        float fps = Mathf.FloorToInt(1.0f / deltaTime);

        timer += Time.unscaledDeltaTime;
        if (timer > 0.1f)
        {
            UIReuseScript.txt_FPS.text = fps + " (" + msec + ")";
            timer = 0;
        }

        if (currentScene == 1)
        {
            UIReuseScript.txt_Time.text = "Time: " + DateAndTimeScript.hour + ":" + DateAndTimeScript.minute;
            UIReuseScript.txt_Date.text = "Date: " + DateAndTimeScript.dayName + " at " + DateAndTimeScript.monthName;
        }
    }
}