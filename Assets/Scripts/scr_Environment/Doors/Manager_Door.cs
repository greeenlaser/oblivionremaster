using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager_Door : MonoBehaviour
{
    [Header("General assignables")]
    public string doorName;
    [SerializeField] private float moveSpeed;
    public DoorType doorType;
    public enum DoorType
    {
        door_Single,
        door_Double,
        gate
    }

    [Header("Single door")]
    [SerializeField] private Transform par_RotationCenter;

    [Header("Double doors")]
    [SerializeField] private Transform par_LeftRotationCenter;
    [SerializeField] private Transform par_RightRotationCenter;

    [Header("Gate")]
    [SerializeField] private Transform par_GateCenter;
    [SerializeField] private Transform pos_GateOpen;
    [SerializeField] private Transform pos_GateClosed;

    [Header("Scripts")]
    [SerializeField] private Env_InteractWithObject GateInteractScript;
    [SerializeField] private GameObject thePlayer;
    [SerializeField] private GameObject par_Managers;

    //public but hidden variables
    [HideInInspector] public bool isDoorOpen;

    //private variables
    private bool isDoorMoving;
    private Quaternion singleDoorOriginalAngle;
    private Quaternion leftDoorOriginalAngle;

    //scripts
    private Env_LockStatus LockStatusScript;
    private UI_Inventory PlayerInventoryScript;
    private UI_Lockpicking LockpickingScript;
    private UI_PauseMenu PauseMenuScript;

    private void Awake()
    {
        LockStatusScript = GetComponent<Env_LockStatus>();
        PlayerInventoryScript = thePlayer.GetComponent<UI_Inventory>();
        LockpickingScript = par_Managers.GetComponent<UI_Lockpicking>();
        PauseMenuScript = par_Managers.GetComponent<UI_PauseMenu>();

        if (par_RotationCenter != null)
        {
            singleDoorOriginalAngle = par_RotationCenter.transform.localRotation;
        }
        else if (par_LeftRotationCenter != null)
        {
            leftDoorOriginalAngle = par_LeftRotationCenter.transform.localRotation;
        }
    }

    private void Update()
    {
        if (isDoorMoving)
        {
            if (!isDoorOpen)
            {
                float step = moveSpeed * Time.deltaTime;
                switch (doorType)
                {
                    case DoorType.door_Single:
                        par_RotationCenter.Rotate(new Vector3(0, 1, 0) * step);

                        if (par_RotationCenter.transform.localRotation.y >= singleDoorOriginalAngle.y + 0.75f)
                        {
                            DoorIsOpen();
                        }
                        break;
                    case DoorType.door_Double:
                        par_LeftRotationCenter.Rotate(new Vector3(0, 1, 0) * step);
                        par_RightRotationCenter.Rotate(new Vector3(0, -1, 0) * step);

                        if (par_LeftRotationCenter.transform.localRotation.y >= leftDoorOriginalAngle.y + 0.75f)
                        {
                            DoorIsOpen();
                        }
                        break;
                    case DoorType.gate:
                        float gateDistance = Vector3.Distance(pos_GateOpen.position, par_GateCenter.position);
                        par_GateCenter.position = Vector3.MoveTowards(par_GateCenter.position, par_GateCenter.position + new Vector3(0, 0.01f, 0), step);

                        if (gateDistance <= 0.01f)
                        {
                            DoorIsOpen();
                        }
                        break;
                }
            }
            else
            {
                float step = moveSpeed * Time.deltaTime;
                switch (doorType)
                {
                    case DoorType.door_Single:
                        par_RotationCenter.Rotate(new Vector3(0, -1, 0) * step);

                        if (par_RotationCenter.transform.localRotation.y <= singleDoorOriginalAngle.y)
                        {
                            DoorIsClosed();
                        }
                        break;
                    case DoorType.door_Double:
                        par_LeftRotationCenter.Rotate(new Vector3(0, -1, 0) * step);
                        par_RightRotationCenter.Rotate(new Vector3(0, 1, 0) * step);

                        if (par_LeftRotationCenter.transform.localRotation.y <= leftDoorOriginalAngle.y)
                        {
                            DoorIsClosed();
                        }
                        break;
                    case DoorType.gate:
                        float gateDistance = Vector3.Distance(pos_GateClosed.position, par_GateCenter.position);
                        par_GateCenter.position = Vector3.MoveTowards(par_GateCenter.position, par_GateCenter.position - new Vector3(0, 0.01f, 0), step);

                        if (gateDistance <= 0.01f)
                        {
                            DoorIsClosed();
                        }
                        break;
                }
            }
        }
    }

    //used for checking whether this door/gate can be unlocked or not
    public void CheckIfLocked()
    {
        if (!isDoorMoving)
        {
            if (LockStatusScript.isUnlocked)
            {
                isDoorMoving = true;
            }
            else
            {
                if (!LockStatusScript.needsKey)
                {
                    bool hasLockpicks = false;
                    foreach (GameObject item in PlayerInventoryScript.playerItems)
                    {
                        if (item.name == "Lockpick")
                        {
                            PauseMenuScript.isPlayerMenuOpen = true;
                            hasLockpicks = true;

                            LockpickingScript.targetLock = gameObject;
                            LockpickingScript.OpenLockpickUI(doorType.ToString(),
                                                             LockStatusScript.lockDifficulty.ToString());
                            break;
                        }
                    }
                    if (!hasLockpicks)
                    {
                        Debug.Log("Player tried to pick " + doorName.Replace("_", " ") + " lock with no lockpicks.");
                    }
                }
                else
                {
                    bool foundKey = false;
                    foreach (GameObject item in PlayerInventoryScript.playerItems)
                    {
                        if (item == LockStatusScript.key)
                        {
                            foundKey = true;
                            break;
                        }
                    }

                    if (!foundKey)
                    {
                        Debug.Log("Player tried to unlock " + doorName.Replace("_", " ") + " lock without proper key.");
                    }
                    else
                    {
                        PlayerInventoryScript.playerItems.Remove(LockStatusScript.key);
                        foreach (Transform item in PlayerInventoryScript.par_PlayerItems.transform)
                        {
                            if (item.gameObject == LockStatusScript.key)
                            {
                                Destroy(item.gameObject);
                                LockStatusScript.isUnlocked = true;

                                isDoorMoving = true;

                                break;
                            }
                        }
                    }
                }
            }
        }
    }

    //opens the door if game was loaded and save file had this door open
    //or if door was unlocked through console
    public void OpenDoor()
    {
        isDoorMoving = true;

        if (doorType == DoorType.gate)
        {
            GateInteractScript.EnableObject();
        }
    }

    private void DoorIsOpen()
    {
        isDoorOpen = true;
        isDoorMoving = false;
    }
    private void DoorIsClosed()
    {
        isDoorOpen = false;
        isDoorMoving = false;

        if (par_RotationCenter != null)
        {
            par_RotationCenter.transform.rotation = new Quaternion(0, 0, 0, 0);
        }
        else if (par_LeftRotationCenter != null)
        {
            par_LeftRotationCenter.transform.rotation = new Quaternion(0, 0, 0, 0);
            par_RightRotationCenter.transform.rotation = new Quaternion(0, 0, 0, 0);
        }
    }
}