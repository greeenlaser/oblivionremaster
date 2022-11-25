using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class Manager_ForceQuit
{
    //force-quit the game. used in the game save file, keybindings and settings scripts
    //when invalid or out of range values were inserted
    public static void ForceQuit(string variableParent,
                                 string variableName,
                                 string scriptName,
                                 string condition)
    {
        Debug.LogError(variableParent + variableName + " in " + scriptName + " is " + condition + "! Quitting game...");

        #if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
        #endif

        #if UNITY_STANDALONE
        Application.Quit();
        #endif
    }
}