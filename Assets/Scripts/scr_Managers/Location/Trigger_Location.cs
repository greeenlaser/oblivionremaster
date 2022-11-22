using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger_Location : MonoBehaviour
{
    [Header("General")]
    public string cellName;
    public LocationType locationType;
    public enum LocationType
    {
        atmosphere,
        town,
        dungeon,
        battle
    }
    [SerializeField] private float maxDistanceToEnable;
    [SerializeField] private float maxDistanceToDiscover;
    [SerializeField] private GameObject location;
    [SerializeField] private GameObject location_discovered;
    public List<GameObject> containers;
    public List<GameObject> doors;

    [Header("Scripts")]
    [SerializeField] private GameObject thePlayer;
    [SerializeField] private GameObject par_Managers;

    //public but hidden variables
    [HideInInspector] public bool wasDiscovered;

    //private variables
    private Vector3 undiscoveredStartScale;
    private Vector3 discoveredStartScale;
    private Manager_Locations LocationManagerScript;
    private Manager_Announcements AnnouncementScript;

    private void Awake()
    {
        LocationManagerScript = par_Managers.GetComponentInChildren<Manager_Locations>();
        AnnouncementScript = par_Managers.GetComponent<Manager_Announcements>();

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
                location.transform.LookAt(2 * location.transform.position - thePlayer.transform.position);

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
                location_discovered.transform.LookAt(2 * location_discovered.transform.position - thePlayer.transform.position);

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
                AnnouncementScript.CreateAnnouncement("Discovered " + cellName + "!");
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

    //reset all containers and doors
    public void ResetCell()
    {
        foreach (GameObject container in containers)
        {
            Env_LockStatus LockStatusScript = container.GetComponent<Env_LockStatus>();
            //all lockable containers are always locked again after a restart
            if (LockStatusScript.lockedAtRestart)
            {
                LockStatusScript.isUnlocked = false;
                LockStatusScript.SetTumblerStatuses();
            }

            UI_Inventory inventory = container.GetComponent<UI_Inventory>();
            if (inventory.containerType == UI_Inventory.ContainerType.respawnable
                || inventory.containerType == UI_Inventory.ContainerType.store)
            {
                foreach (GameObject lootTable in LocationManagerScript.lootTables)
                {
                    if (lootTable.name == "ContainerLootTable")
                    {
                        lootTable.GetComponent<Env_LootTable>().ResetContainer(container);
                        break;
                    }
                }
            }
        }
        foreach (GameObject door in doors)
        {
            Env_LockStatus LockStatusScript = door.GetComponent<Env_LockStatus>();
            //all lockable doors are always locked again after a restart
            if (LockStatusScript.lockedAtRestart)
            {
                LockStatusScript.isUnlocked = false;
                LockStatusScript.SetTumblerStatuses();
            }
        }
    }
}