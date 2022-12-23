using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Env_LockStatus : MonoBehaviour
{
    [Header("Is this lock meant to be locked at start?")]
    public bool lockedAtRestart;
    [Tooltip("Does this lock need a key?")]
    public bool needsKey;
    [Tooltip("Which key does this lock need?")]
    public GameObject key;
    [Tooltip("How hard is this lock to pick?")]
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
    [HideInInspector] public bool isUnlocked = true;
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

    //sets all tumbler positions and weights
    public void SetTumblerStatuses()
    {
        tumbler1Unlocked = false;
        tumbler2Unlocked = false;
        tumbler3Unlocked = false;
        tumbler4Unlocked = false;
        tumbler5Unlocked = false;
        tumbler1Weight = Random.Range(1, 5);
        tumbler2Weight = Random.Range(1, 5);
        tumbler3Weight = Random.Range(1, 5);
        tumbler4Weight = Random.Range(1, 5);
        tumbler5Weight = Random.Range(1, 5);

        if (lockDifficulty == LockDifficulty.Novice)
        {
            tumbler5Unlocked = true;
            tumbler4Unlocked = true;
            tumbler3Unlocked = true;
            tumbler2Unlocked = true;
        }
        else if (lockDifficulty == LockDifficulty.Apprentice)
        {
            tumbler5Unlocked = true;
            tumbler4Unlocked = true;
            tumbler3Unlocked = true;
        }
        else if (lockDifficulty == LockDifficulty.Journeyman)
        {
            tumbler5Unlocked = true;
            tumbler4Unlocked = true;
        }
        else if (lockDifficulty == LockDifficulty.Expert)
        {
            tumbler5Unlocked = true;
        }
    }
}