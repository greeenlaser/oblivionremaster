using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger_Location : MonoBehaviour
{
    [Header("Assignables")]
    public LocationType locationType;
    public enum LocationType
    {
        outdoors,
        town,
        dungeon
    }
    [SerializeField] private GameObject par_Managers;

    //private variables
    private Manager_Locations LocationManagerScript;

    private void Awake()
    {
        LocationManagerScript = par_Managers.GetComponentInChildren<Manager_Locations>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")
            && LocationManagerScript.locationType.ToString()
            != locationType.ToString())
        {
            LocationManagerScript.locationType = (Manager_Locations.LocationType)(LocationType)Enum.Parse(typeof(LocationType), locationType.ToString());
        }
    }
}