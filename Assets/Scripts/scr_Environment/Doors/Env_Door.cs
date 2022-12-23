using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Env_Door : MonoBehaviour
{
    [Header("Scripts")]
    public Manager_Door DoorManagerScript;

    //this is a child method of the real door interaction method
    //used for referencing the main method no matter
    //how the door is positioned or rotated. 
    //the main script gameobject is static while
    //this script gameobject moves with the trigger of the door
    public void Interact()
    {
        DoorManagerScript.CheckIfLocked();
    }
}