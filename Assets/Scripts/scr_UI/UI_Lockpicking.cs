using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_Lockpicking : MonoBehaviour
{
    [Header("Lock values")]
    [SerializeField] private float lockpickMoveSpeed; 
    [SerializeField] private float lockpickDownPosition;
    [SerializeField] private float lockpickUpPosition;
    [SerializeField] private float tumblerUnlockedPosition;
    [SerializeField] private float tumblerLockedPosition;
    [SerializeField] private float springUnlockedScale;
    [SerializeField] private float springLockedScale;

    [Header("Main UI")]
    [SerializeField] private GameObject par_LockpickUI;
    [SerializeField] private RawImage lockpick;
    [SerializeField] private TMP_Text txt_LockName;
    [SerializeField] private TMP_Text txt_LockLevel;
    [SerializeField] private TMP_Text txt_LockpickCount;
    [SerializeField] private Button btn_AutoAttempt;
    [SerializeField] private Button btn_Close;

    [Header("Tumblers and springs")]
    [SerializeField] private RawImage tumbler1;
    [SerializeField] private RawImage tumbler2;
    [SerializeField] private RawImage tumbler3;
    [SerializeField] private RawImage tumbler4;
    [SerializeField] private RawImage tumbler5;
    [SerializeField] private GameObject spring1;
    [SerializeField] private GameObject spring2;
    [SerializeField] private GameObject spring3;
    [SerializeField] private GameObject spring4;
    [SerializeField] private GameObject spring5;

    [Header("Scripts")]
    [SerializeField] private GameObject thePlayer;

    //public but hidden variables
    [HideInInspector] public Env_LockStatus LockStatusScript;

    //private variables
    private bool isMovingPick;
    private bool isPickGoingUp;
    private bool isPickGoingDown;
    private int currentTumblerSlot;
    private float step;
    private float[] slotPositions =
    {
        -360,
        -302,
        -244,
        -186,
        -128
    };
    private Vector3 lockpickStartPosition;
    private Vector3 lockpickCurrentPosition;
    private Vector3 lockpickTargetPosition;

    //scripts
    private Env_Item LockpickScript;
    private UI_Inventory PlayerInventoryScript;
    private Player_Stats PlayerStatsScript;
    private Manager_Door DoorManagerScript;
    private UI_Inventory ContainerScript;
    private UI_PauseMenu PauseMenuScript;
    private Manager_KeyBindings KeyBindingsScript;

    private void Awake()
    {
        PlayerInventoryScript = thePlayer.GetComponent<UI_Inventory>();
        PlayerStatsScript = thePlayer.GetComponent<Player_Stats>();
        PauseMenuScript = GetComponent<UI_PauseMenu>();
        KeyBindingsScript = GetComponent<Manager_KeyBindings>();

        btn_AutoAttempt.onClick.AddListener(AutoAttempt);
        btn_Close.onClick.AddListener(CloseLockpickUI);

        lockpickStartPosition = lockpick.transform.localPosition;
    }

    private void Update()
    {
        if (PauseMenuScript.isLockpickUIOpen)
        {
            if (!isMovingPick)
            {
                if (KeyBindingsScript.GetKeyDown("WalkForwards"))
                {
                    MovePick("up");
                }
                else if (KeyBindingsScript.GetKeyDown("WalkLeft"))
                {
                    MovePick("left");
                }
                else if (KeyBindingsScript.GetKeyDown("WalkRight"))
                {
                    MovePick("right");
                }
            }
            else
            {
                step = lockpickMoveSpeed * Time.unscaledDeltaTime;
                lockpickCurrentPosition = lockpick.transform.localPosition;

                if (isPickGoingUp)
                {
                    float distance = Vector3.Distance(lockpickTargetPosition, lockpickCurrentPosition);
                    Debug.Log("distance to target is " + distance);

                    if (distance > 0.001f)
                    {
                        Debug.Log("going up");

                        Vector3.MoveTowards(lockpickTargetPosition, lockpickCurrentPosition, step);
                    }
                    else
                    {
                        MovePick("down");
                    }
                }
                else if (isPickGoingDown)
                {
                    float distance = Vector3.Distance(lockpickTargetPosition, lockpickCurrentPosition);
                    Debug.Log("distance to target is " + distance);

                    if (distance > 0.001f)
                    {
                        Debug.Log("going down");

                        Vector3.MoveTowards(lockpickTargetPosition, lockpickCurrentPosition, step);
                    }
                    else
                    {
                        StopMovement();
                    }
                }
            }
        }
    }

    //start pick movement after a key was pressed
    private void MovePick(string direction)
    {
        isMovingPick = true;

        if (direction == "up")
        {
            lockpickTargetPosition = new(lockpick.transform.localPosition.x,
                                         lockpickUpPosition,
                                         lockpick.transform.localPosition.z);

            isPickGoingUp = true;
        }
        else if (direction == "down")
        {
            lockpickTargetPosition = new(lockpick.transform.localPosition.x,
                                         lockpickDownPosition,
                                         lockpick.transform.localPosition.z);

            isPickGoingDown = true;
            isPickGoingUp = false;
        }
        else if (direction == "left"
                 && currentTumblerSlot >= 0)
        {
            MovePickSideways(currentTumblerSlot - 1);
        }
        else if (direction == "right"
                 && currentTumblerSlot <= 3)
        {
            MovePickSideways(currentTumblerSlot + 1);
        }
    }
    //moves tumbler up and down independently from the lockpick speed
    private void MoveTumbler(string direction, int slot)
    {

    }
    //move the pick left or right towards direction, to position targetSlot
    private void MovePickSideways(int targetSlot)
    {
        currentTumblerSlot = targetSlot;

        Debug.Log("moving pick to " + slotPositions[currentTumblerSlot] + "...");

        lockpick.transform.localPosition = new Vector3(slotPositions[currentTumblerSlot],
                                                       lockpick.transform.localPosition.y,
                                                       lockpick.transform.localPosition.z);

        StopMovement();
    }
    //stops all bools that trigger tumbler or pick movement
    private void StopMovement()
    {
        isMovingPick = false;
        isPickGoingUp = false;
        isPickGoingDown = false;
    }

    //try to pick the lock without manually moving any tumblers,
    //this has a much lower chance of successfully unlocking the door/container
    public void AutoAttempt()
    {
        float unlockChance = Random.Range(0, 10000 / PlayerStatsScript.Skills["Security"] / PlayerStatsScript.Attributes["Luck"] * 10);
        float requiredUnlockChance = 10000 / PlayerStatsScript.Skills["Security"] / PlayerStatsScript.Attributes["Luck"] * 10 * 0.9f;

        //if the unlock chance was high enough
        //with auto-attempt to unlock this lock
        if (unlockChance >= requiredUnlockChance)
        {
            Unlock();
        }
        //otherwise it breaks a lockpick
        else
        {
            if (LockpickScript.itemCount - 1 == 0)
            {
                LockpickScript.itemCount = 0;
                CloseLockpickUI();
            }
            else
            {
                LockpickScript.itemCount--;
                txt_LockpickCount.text = LockpickScript.itemCount.ToString();
            }
        }
    }

    //once lock has successfully been unlocked
    private void Unlock()
    {
        if (LockStatusScript.GetComponent<Manager_Door>() != null)
        {
            Debug.Log("Successfully unlocked " + LockStatusScript.GetComponent<Manager_Door>().doorName + "!");
        }
        else if (LockStatusScript.GetComponent<UI_Inventory>() != null)
        {
            Debug.Log("Successfully unlocked " + LockStatusScript.GetComponent<UI_Inventory>().containerName + "!");
        }

        LockStatusScript.tumbler1Unlocked = true;
        LockStatusScript.tumbler2Unlocked = true;
        LockStatusScript.tumbler3Unlocked = true;
        LockStatusScript.tumbler4Unlocked = true;
        LockStatusScript.tumbler5Unlocked = true;
        LockStatusScript.isUnlocked = true;

        CloseLockpickUI();
    }

    public void OpenlockpickUI()
    {
        PauseMenuScript.isLockpickUIOpen = true;
        PauseMenuScript.PauseWithoutUI();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        par_LockpickUI.SetActive(true);

        //set lock name
        if (LockStatusScript.GetComponent<Manager_Door>() != null)
        {
            DoorManagerScript = LockStatusScript.GetComponent<Manager_Door>();
            txt_LockName.text = DoorManagerScript.doorName + " lock";
        }
        else if (LockStatusScript.GetComponent<UI_Inventory>() != null)
        {
            ContainerScript = LockStatusScript.GetComponent<UI_Inventory>();
            txt_LockName.text = ContainerScript.containerName + " lock";
        }
        //set lock level text
        txt_LockLevel.text = "Lock difficulty: " + LockStatusScript.lockDifficulty.ToString();
        //set current lockpick count
        if (LockpickScript == null)
        {
            foreach (GameObject item in PlayerInventoryScript.playerItems)
            {
                if (item.name == "Lockpick")
                {
                    LockpickScript = item.GetComponent<Env_Item>();
                    break;
                }
            }
        }
        txt_LockpickCount.text = "Lockpicks: " + LockpickScript.itemCount.ToString();

        SetTumblerPositions();
    }
    public void CloseLockpickUI()
    {
        SetTumblerPositions();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        par_LockpickUI.SetActive(false);

        PauseMenuScript.isLockpickUIOpen = false;
        PauseMenuScript.UnpauseGame();
    }

    //sets tumbler positions and spring scales
    private void SetTumblerPositions()
    {
        StopMovement();

        lockpick.transform.localPosition = lockpickStartPosition;

        if (LockStatusScript.tumbler1Unlocked)
        {
            tumbler1.transform.localPosition = new Vector3(tumbler1.transform.localPosition.x,
                                                           tumblerUnlockedPosition,
                                                           tumbler1.transform.localPosition.z);
            spring1.transform.localScale = new Vector3(spring1.transform.localScale.x,
                                                       springUnlockedScale,
                                                       spring1.transform.localScale.z);
        }
        else
        {
            tumbler1.transform.localPosition = new Vector3(tumbler1.transform.localPosition.x,
                                                           tumblerLockedPosition,
                                                           tumbler1.transform.localPosition.z);
            spring1.transform.localScale = new Vector3(spring1.transform.localScale.x,
                                                       springLockedScale,
                                                       spring1.transform.localScale.z);
        }

        if (LockStatusScript.tumbler2Unlocked)
        {
            tumbler2.transform.localPosition = new Vector3(tumbler2.transform.localPosition.x,
                                                           tumblerUnlockedPosition,
                                                           tumbler2.transform.localPosition.z);
            spring2.transform.localScale = new Vector3(spring2.transform.localScale.x,
                                                       springUnlockedScale,
                                                       spring2.transform.localScale.z);
        }
        else
        {
            tumbler2.transform.localPosition = new Vector3(tumbler2.transform.localPosition.x,
                                                           tumblerLockedPosition,
                                                           tumbler2.transform.localPosition.z);
            spring2.transform.localScale = new Vector3(spring2.transform.localScale.x,
                                                       springLockedScale,
                                                       spring2.transform.localScale.z);
        }

        if (LockStatusScript.tumbler3Unlocked)
        {
            tumbler3.transform.localPosition = new Vector3(tumbler3.transform.localPosition.x,
                                                           tumblerUnlockedPosition,
                                                           tumbler3.transform.localPosition.z);
            spring3.transform.localScale = new Vector3(spring3.transform.localScale.x,
                                                       springUnlockedScale,
                                                       spring3.transform.localScale.z);
        }
        else
        {
            tumbler3.transform.localPosition = new Vector3(tumbler3.transform.localPosition.x,
                                                           tumblerLockedPosition,
                                                           tumbler3.transform.localPosition.z);
            spring3.transform.localScale = new Vector3(spring3.transform.localScale.x,
                                                       springLockedScale,
                                                       spring3.transform.localScale.z);
        }

        if (LockStatusScript.tumbler4Unlocked)
        {
            tumbler4.transform.localPosition = new Vector3(tumbler4.transform.localPosition.x,
                                                           tumblerUnlockedPosition,
                                                           tumbler4.transform.localPosition.z);
            spring4.transform.localScale = new Vector3(spring4.transform.localScale.x,
                                                       springUnlockedScale,
                                                       spring4.transform.localScale.z);
        }
        else
        {
            tumbler4.transform.localPosition = new Vector3(tumbler4.transform.localPosition.x,
                                                           tumblerLockedPosition,
                                                           tumbler4.transform.localPosition.z);
            spring4.transform.localScale = new Vector3(spring4.transform.localScale.x,
                                                       springLockedScale,
                                                       spring4.transform.localScale.z);
        }

        if (LockStatusScript.tumbler5Unlocked)
        {
            tumbler5.transform.localPosition = new Vector3(tumbler5.transform.localPosition.x,
                                                           tumblerUnlockedPosition,
                                                           tumbler5.transform.localPosition.z);
            spring5.transform.localScale = new Vector3(spring5.transform.localScale.x,
                                                       springUnlockedScale,
                                                       spring5.transform.localScale.z);
        }
        else
        {
            tumbler5.transform.localPosition = new Vector3(tumbler5.transform.localPosition.x,
                                                      tumblerLockedPosition,
                                                      tumbler5.transform.localPosition.z);
            spring5.transform.localScale = new Vector3(spring5.transform.localScale.x,
                                                       springLockedScale,
                                                       spring5.transform.localScale.z);
        }
    }
}