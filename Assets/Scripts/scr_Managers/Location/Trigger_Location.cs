using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Trigger_Location : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private GameObject par_Managers;
    public LocationType locationType;
    public enum LocationType
    {
        atmosphere,
        town,
        dungeon,
        battle
    }

    [Header("Location variables")]
    [SerializeField] private float maxDistanceToEnable;
    [SerializeField] private float maxDistanceToDiscover;
    [SerializeField] private Transform thePlayer;
    [SerializeField] private GameObject location;
    [SerializeField] private GameObject location_discovered;

    //public but hidden variables
    [HideInInspector] public bool wasDiscovered;

    //private variables
    private Vector3 undiscoveredStartScale;
    private Vector3 discoveredStartScale;
    private Manager_Locations LocationManagerScript;

    private void Awake()
    {
        LocationManagerScript = par_Managers.GetComponentInChildren<Manager_Locations>();

        undiscoveredStartScale = location.transform.localScale;
        discoveredStartScale = location_discovered.transform.localScale;
        location.SetActive(false);
        location_discovered.SetActive(false);
    }

    private void Update()
    {
        if (Vector3.Distance(thePlayer.transform.position, transform.position) <= maxDistanceToEnable
            && Vector3.Distance(thePlayer.transform.position, transform.position) > maxDistanceToDiscover)
        {
            if (!wasDiscovered)
            {
                if (location_discovered.activeInHierarchy)
                {
                    location_discovered.SetActive(false);
                }

                location.SetActive(true);
                location.transform.LookAt(2 * location.transform.position - thePlayer.position);

                float distance = Vector3.Distance(thePlayer.transform.position, location.transform.position);
                location.transform.localScale = undiscoveredStartScale * distance / 3;
            }
            else
            {
                if (location.activeInHierarchy)
                {
                    location.SetActive(false);
                }

                location_discovered.SetActive(true);
                location_discovered.transform.LookAt(2 * location_discovered.transform.position - thePlayer.position);

                float distance = Vector3.Distance(thePlayer.transform.position, location_discovered.transform.position);
                location_discovered.transform.localScale = discoveredStartScale * distance / 3;
            }
        }
        else if (Vector3.Distance(thePlayer.transform.position, transform.position) <= maxDistanceToDiscover)
        {
            if (location.activeInHierarchy)
            {
                location.SetActive(false);
            }
            if (location_discovered.activeInHierarchy)
            {
                location_discovered.SetActive(false);
            }

            if (!wasDiscovered)
            {
                wasDiscovered = true;
            }
        }
        else
        {
            if (location.activeInHierarchy)
            {
                location.SetActive(false);
            }
            if (location_discovered.activeInHierarchy)
            {
                location_discovered.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            string theLocationType = locationType.ToString();
            string currentLocationType = LocationManagerScript.locationType.ToString();
            if (theLocationType != currentLocationType)
            {
                LocationManagerScript.UpdateCurrentLocation(theLocationType);
            }
        }
    }
}