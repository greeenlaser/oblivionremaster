using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager_Locations : MonoBehaviour
{
    [Header("Assignables")]
    public LocationType locationType;
    public enum LocationType
    {
        outdoors,
        town,
        dungeon
    }

    //private variables
    private Manager_GlobalAudio GlobalAudioScript;

    private void Awake()
    {
        GlobalAudioScript = GetComponent<Manager_GlobalAudio>();
    }

    //gets the current location type of the player
    public void UpdateCurrentLocation()
    {

    }
}