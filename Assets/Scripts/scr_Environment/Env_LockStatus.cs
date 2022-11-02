using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Env_LockStatus : MonoBehaviour
{
    [Header("Lock status")]
    public bool lockedAtRestart;
    public bool needsKey;
    public GameObject key;
    public LockDifficulty lockDifficulty = LockDifficulty.Apprentice;
    public enum LockDifficulty
    {
        Novice,
        Apprentice,
        Journeyman,
        Expert,
        Master
    }

    //public but hidden variables
    [HideInInspector] public bool isUnlocked;
    [HideInInspector] public bool hasLoadedLock;
    [HideInInspector] public bool tumbler1Unlocked;
    [HideInInspector] public bool tumbler2Unlocked;
    [HideInInspector] public bool tumbler3Unlocked;
    [HideInInspector] public bool tumbler4Unlocked;
    [HideInInspector] public bool tumbler5Unlocked;
    [HideInInspector] public int tumbler1Weight;
    [HideInInspector] public int tumbler2Weight;
    [HideInInspector] public int tumbler3Weight;
    [HideInInspector] public int tumbler4Weight;
    [HideInInspector] public int tumbler5Weight;
}