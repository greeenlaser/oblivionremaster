using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_DebugMenu : MonoBehaviour
{
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
    }
}