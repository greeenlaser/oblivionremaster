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
    [HideInInspector] public List<GameObject> targets;

    //private variables
    private bool canInteract;
    private float timer;
    private GameObject target;
    //private GameObject deadAIContainer;
    private LayerMask IgnoredLayermask;

    //scripts
    private UI_PauseMenu PauseMenuScript;
    private Manager_KeyBindings KeyBindingsScript;
    //private Player_Movement PlayerMovementScript;
    private UI_Inventory PlayerInventoryScript;
    private Player_Stats PlayerStatsScript;

    private void Awake()
    {
        PauseMenuScript = par_Managers.GetComponent<UI_PauseMenu>();
        KeyBindingsScript = par_Managers.GetComponent<Manager_KeyBindings>();
        //PlayerMovementScript = transform.parent.GetComponent<Player_Movement>();
        PlayerInventoryScript = transform.parent.GetComponent<UI_Inventory>();
        PlayerStatsScript = transform.parent.GetComponent<Player_Stats>();

        IgnoredLayermask = LayerMask.NameToLayer("Player");

        cursor.gameObject.SetActive(true);
        interactIcon.gameObject.SetActive(false);
    }

    //checking what collides with the visionCone mesh
    private void OnTriggerEnter(Collider other)
    {
        if (!targets.Contains(other.gameObject)
            && other.GetComponent<Env_Item>() != null)
        {
            targets.Add(other.gameObject);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (targets.Contains(other.gameObject))
        {
            targets.Remove(other.gameObject);
        }
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
                && PlayerStatsScript.currentHealth > 0)
            {
                //all other layers except this one
                IgnoredLayermask = ~IgnoredLayermask;

                if (Physics.Raycast(transform.position,
                                    transform.TransformDirection(Vector3.forward),
                                    out RaycastHit hitTarget,
                                    3,
                                    IgnoredLayermask,
                                    QueryTriggerInteraction.Ignore))
                {
                    /*
                    //if the target is AI and it doesn't have a health script
                    //or it has a health script and it is alive
                    if (hitTarget.transform.GetComponent<UI_AIContent>() != null
                        && hitTarget.transform.GetComponent<AI_Health>() == null
                        || (hitTarget.transform.GetComponent<AI_Health>() != null
                        && hitTarget.transform.GetComponent<AI_Health>().isAlive))
                    {
                        if (target != hitTarget.transform.gameObject)
                        {
                            target = hitTarget.transform.gameObject;
                            //Debug.Log(target.name);
                        }

                        if (QuickLootScript.targetContainer != null)
                        {
                            QuickLootScript.targetContainer = null;
                        }

                        par_Managers.GetComponent<Manager_UIReuse>().txt_HoverItemCount.text = "";
                        par_Managers.GetComponent<Manager_UIReuse>().bgr_HoverItemCountBackground.gameObject.SetActive(false);

                        timer = 0;
                        canInteract = true;
                    }
                    //if the target is an AI but it has a health script and it is dead
                    else if (hitTarget.transform.GetComponent<UI_AIContent>() != null
                             && hitTarget.transform.GetComponent<AI_Health>() != null
                             && !hitTarget.transform.GetComponent<AI_Health>().isAlive)
                    {
                        target = hitTarget.transform.gameObject;

                        //gets the parent of the target and looks for the correct child
                        //which has the dead AI loot script
                        GameObject par = target.transform.parent.gameObject;
                        foreach (Transform child in par.transform)
                        {
                            if (child.name.Contains("Dead")
                                && child.GetComponent<Inv_Container>() != null)
                            {
                                deadAIContainer = child.transform.gameObject;
                                //Debug.Log(deadAIContainer.name);

                                par_Managers.GetComponent<Manager_UIReuse>().txt_HoverItemCount.text = "";
                                par_Managers.GetComponent<Manager_UIReuse>().bgr_HoverItemCountBackground.gameObject.SetActive(false);

                                timer = 0;
                                canInteract = true;
                            }
                        }
                    }
                    */

                    //if the target is an item
                    if (hitTarget.transform.GetComponent<Env_Item>() != null)
                    {
                        target = hitTarget.transform.gameObject;

                        timer = 0;
                        canInteract = true;
                    }

                    
                    //if the target is a container
                    else if (hitTarget.transform.GetComponent<UI_Inventory>() != null)
                    {
                        if (target != hitTarget.transform.gameObject)
                        {
                            target = hitTarget.transform.gameObject;
                            //Debug.Log(target.name);
                        }

                        timer = 0;
                        canInteract = true;
                    }

                    /*
                    //if the target is a workbench
                    else if (hitTarget.transform.GetComponent<Env_Workbench>() != null)
                    {
                        if (target != hitTarget.transform.gameObject)
                        {
                            target = hitTarget.transform.gameObject;
                            //Debug.Log(target.name);
                        }

                        if (QuickLootScript.targetContainer != null)
                        {
                            QuickLootScript.targetContainer = null;
                        }

                        timer = 0;
                        canInteract = true;
                    }

                    //if the target is a waitable
                    else if (hitTarget.transform.GetComponent<Env_Wait>() != null)
                    {
                        if (target != hitTarget.transform.gameObject)
                        {
                            target = hitTarget.transform.gameObject;
                            //Debug.Log(target.name);
                        }

                        if (QuickLootScript.targetContainer != null)
                        {
                            QuickLootScript.targetContainer = null;
                        }

                        timer = 0;
                        canInteract = true;
                    }

                    //special case where we need to get the locked doors trigger
                    //which the raycast is actually ignoring by default
                    //to allow interacting with other gameobjects
                    //if theyre inside a trigger we dont want to interact with
                    else if (hitTarget.transform.name == "door_interactable")
                    {
                        Transform doorParent = hitTarget.transform.parent.parent;
                        foreach (Transform child in doorParent)
                        {
                            if (child.GetComponent<Env_Door>() != null
                                && child.GetComponent<Env_Door>().isLocked
                                && !child.GetComponent<Env_Door>().controlledByComputer)
                            {
                                if (target != child.gameObject)
                                {
                                    target = child.gameObject;
                                    //Debug.Log(target.name);
                                }

                                if (QuickLootScript.targetContainer != null)
                                {
                                    QuickLootScript.targetContainer = null;
                                }

                                timer = 0;
                                canInteract = true;
                            }
                        }
                    }
                    */
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
            && PlayerStatsScript.currentHealth > 0)
        {
            //timer is used to "restart" loop
            //which re-checks if we are still looking at the same target
            timer += Time.deltaTime;
            if (timer > 0.05f)
            {
                canInteract = false;
            }

            if (cursor.gameObject.activeInHierarchy)
            {
                cursor.gameObject.SetActive(false);
            }
            if (!interactIcon.gameObject.activeInHierarchy)
            {
                interactIcon.gameObject.SetActive(true);
            }

            //interacting with an object
            if (KeyBindingsScript.GetButtonDown("PickUpOrInteract"))
            {
                /*
                //hit alive npc
                if (target.GetComponent<UI_AIContent>() != null
                    && target.GetComponent<UI_AIContent>().hasDialogue)
                {
                    target.GetComponent<UI_AIContent>().OpenNPCDialogue();
                }
                //hit dead npc
                else if (deadAIContainer != null
                         && deadAIContainer.GetComponent<Inv_Container>() != null
                         && deadAIContainer.GetComponent<Env_Follow>() != null)
                {
                    deadAIContainer.GetComponent<Inv_Container>().CheckIfLocked();
                }
                */
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
                /*
                //hit workbench
                else if (target.GetComponent<Env_Workbench>() != null)
                {
                    target.GetComponent<Env_Workbench>().OpenWorkbenchUI();
                }
                //hit waitable
                else if (target.GetComponent<Env_Wait>() != null)
                {
                    target.GetComponent<Env_Wait>().OpenTimeSlider();
                }
                //hit door
                else if (target.GetComponent<Env_Door>() != null
                         && target.GetComponent<Env_Door>().isLocked)
                {
                    target.GetComponent<Env_Door>().CheckIfKeyIsNeeded();
                }
                */
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