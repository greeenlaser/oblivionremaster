using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Env_LockStatus : MonoBehaviour
{
    [Header("Assignables")]
    public bool isUnlocked = true;

    //public but hidden variables
    [HideInInspector] public bool hasLoadedLock;
    [HideInInspector] public bool tumbler1Unlocked;
    [HideInInspector] public bool tumbler2Unlocked;
    [HideInInspector] public bool tumbler3Unlocked;
    [HideInInspector] public bool tumbler4Unlocked;
    [HideInInspector] public bool tumbler5Unlocked;
}