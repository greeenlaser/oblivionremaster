using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Manager_GlobalAudio;

public class Manager_Locations : MonoBehaviour
{
    [Header("Assignables")]
    public List<GameObject> locations = new();

    public List<GameObject> lootTables;

    //public but hidden variables
    [HideInInspector] public LocationType locationType;
    public enum LocationType
    {
        none,
        atmosphere,
        town,
        dungeon,
        battle
    }

    //private variables
    private Manager_GlobalAudio GlobalAudioScript;

    private void Awake()
    {
        GlobalAudioScript = GetComponentInChildren<Manager_GlobalAudio>();
    }

    //gets the current location type of the player
    public void UpdateCurrentLocation(string theLocationType)
    {
        if (locationType != (LocationType)Enum.Parse(typeof(LocationType), theLocationType))
        {
            locationType = (LocationType)Enum.Parse(typeof(LocationType), theLocationType);
            GlobalAudioScript.audioType = (UserDefined_AudioType)Enum.Parse(typeof(UserDefined_AudioType), theLocationType);
            GlobalAudioScript.PlayAudioType();
        }
    }
}