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
    private Manager_DateAndTime DateAndTimeScript;

    private void Awake()
    {
        GlobalAudioScript = GetComponentInChildren<Manager_GlobalAudio>();
        DateAndTimeScript = GetComponent<Manager_DateAndTime>();
    }

    //gets the current location type of the player
    public void UpdateCurrentLocation(string theLocationType)
    {
        locationType = (LocationType)Enum.Parse(typeof(LocationType), theLocationType);
        GlobalAudioScript.audioType = (UserDefined_AudioType)Enum.Parse(typeof(UserDefined_AudioType), theLocationType);
        GlobalAudioScript.PlayAudioType();
    }

    //used automatically after 3 in-game days or when game is first opened
    public void ResetAllLocations()
    {
        foreach (GameObject location in locations)
        {
            Trigger_Location LocationScript = location.GetComponent<Trigger_Location>();
            LocationScript.ResetCell();
        }

        DateAndTimeScript.daysSinceLastRestart = 3;

        Debug.Log("Successfully reset all locations!");
    }
}