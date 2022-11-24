using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player_Raycast : MonoBehaviour
{
    [Header("Assignables")]
    [SerializeField] private RawImage cursor;
    [SerializeField] private RawImage interactIcon;
    public Transform pos_HoldItem;
    [SerializeField] private GameObject par_Managers;

    //public but hidden variables
    [HideInInspector] public GameObject heldObject;
    [HideInInspector] public bool isColliding;

    //private variables
    private bool canInteract;
    private float timer;
    private GameObject target;
    private LayerMask IgnoredLayermask;

    //scripts
    private UI_PauseMenu PauseMenuScript;
    private Manager_KeyBindings KeyBindingsScript;
    private UI_Inventory PlayerInventoryScript;
    private Player_Stats PlayerStatsScript;

    private void Awake()
    {
        PauseMenuScript = par_Managers.GetComponent<UI_PauseMenu>();
        KeyBindingsScript = par_Managers.GetComponent<Manager_KeyBindings>();
        PlayerInventoryScript = transform.parent.GetComponent<UI_Inventory>();
        PlayerStatsScript = transform.parent.GetComponent<Player_Stats>();

        IgnoredLayermask = LayerMask.NameToLayer("Player");

        cursor.gameObject.SetActive(true);
        interactIcon.gameObject.SetActive(false);
    }

    private void FixedUpdate()
    {
        if (heldObject == null)
        {
            if (!PauseMenuScript.isPaused
                && !PauseMenuScript.isConsoleOpen
                && !PauseMenuScript.isPlayerMenuOpen
                && !PauseMenuScript.isConfirmationUIOpen
                && !PauseMenuScript.isKeyAssignUIOpen
                && !PauseMenuScript.isLockpickUIOpen
                && PlayerStatsScript.currentHealth > 0)
            {
                //all other layers except this one
                IgnoredLayermask = ~IgnoredLayermask;

                if (Physics.Raycast(transform.position,
                                    transform.TransformDirection(Vector3.forward),
                                    out RaycastHit hitTarget,
                                    2,
                                    IgnoredLayermask,
                                    QueryTriggerInteraction.Ignore))
                {
                    if (hitTarget.transform.GetComponent<Env_Item>() != null                   //item
                        || hitTarget.transform.GetComponent<UI_Inventory>() != null            //container
                        || (hitTarget.transform.GetComponent<Env_Door>() != null               //door, gates are not allowed to be opened directly
                        && hitTarget.transform.GetComponent<Env_Door>().DoorManagerScript.doorType  
                        != Manager_Door.DoorType.gate)
                        || hitTarget.transform.GetComponent<Env_InteractWithObject>() != null) //button or lever
                    {
                        if (target != hitTarget.transform.gameObject)
                        {
                            target = hitTarget.transform.gameObject;

                            //Debug.Log("looking at " + target.name + "...");
                        }

                        timer = 0;
                        canInteract = true;
                    }
                }
            }
        }

        //simple override to always show interact UI if player is currently holding an object
        //no matter if the raycast is hitting the held object or not
        else if (heldObject != null)
        {
            timer = 0;
            canInteract = true;
        }
    }

    private void Update()
    {
        if (canInteract
            && !PauseMenuScript.isPaused
            && !PauseMenuScript.isPlayerMenuOpen
            && !PauseMenuScript.isConsoleOpen
            && !PauseMenuScript.isConfirmationUIOpen
            && !PauseMenuScript.isKeyAssignUIOpen
            && !PauseMenuScript.isLockpickUIOpen
            && PlayerStatsScript.currentHealth > 0)
        {
            //timer is used to "restart" loop
            //which re-checks if we are still looking at the same target
            timer += Time.deltaTime;
            if (timer > 0.05f)
            {
                canInteract = false;
            }

            //disables cursor and re-enables interact icon
            if (cursor.gameObject.activeInHierarchy)
            {
                cursor.gameObject.SetActive(false);
            }
            if (!interactIcon.gameObject.activeInHierarchy)
            {
                interactIcon.gameObject.SetActive(true);
            }

            //interacting with an object
            if (KeyBindingsScript.GetKeyDown("PickUpOrInteract"))
            {
                //hit item
                if (target.GetComponent<Env_Item>() != null)
                {
                    PlayerInventoryScript.TakeItem(target, null);
                }
                
                //hit container
                else if (target.GetComponent<UI_Inventory>() != null)
                {
                    target.GetComponent<UI_Inventory>().CheckIfLocked();
                }

                //hit door, gates are not allowed to be opened directly
                else if (target.GetComponent<Env_Door>() != null)
                {
                    target.GetComponent<Env_Door>().Interact();
                }

                //hit button or lever
                else if (target.GetComponent<Env_InteractWithObject>() != null)
                {
                    target.GetComponent<Env_InteractWithObject>().Interact();
                }
            }

            //holding an item
            if (Input.GetKeyDown(KeyCode.Mouse0)
                && target.GetComponent<Env_Item>() != null
                && PlayerInventoryScript.equippedWeapon == null
                && heldObject == null)
            {
                target.GetComponent<Env_ObjectPickup>().isHolding = true;
                target.GetComponent<Rigidbody>().useGravity = false;
                heldObject = target;
            }
            //dropping the held item
            if ((Input.GetKeyUp(KeyCode.Mouse0)
                && heldObject != null
                && heldObject.GetComponent<Env_ObjectPickup>().isHolding)
                || (heldObject != null
                && !heldObject.GetComponent<Env_ObjectPickup>().isHolding))
            {
                heldObject.GetComponent<Env_ObjectPickup>().DropObject();
                heldObject = null;
            }
        }
        //re-enables cursor and disables interact icon
        else if (!canInteract
                 && (timer > 0
                 || interactIcon.gameObject.activeInHierarchy))
        {
            cursor.gameObject.SetActive(true);
            interactIcon.gameObject.SetActive(false);

            timer = 0;
        }
    }
}