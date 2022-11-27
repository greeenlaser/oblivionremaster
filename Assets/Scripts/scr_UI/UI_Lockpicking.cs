using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_Lockpicking : MonoBehaviour
{
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
    private float lockpickStep;
    private float tumblerStep;
    private float lockpickMoveSpeed;
    private float tumblerMoveSpeed;
    private float lockpickTimer;
    private float tumblerTimer;
    private readonly float[] slotPositions =
    {
        -360,
        -302,
        -244,
        -186,
        -128
    };
    private Vector3 lockpickStartPosition;

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
            if (!isMovingPick
                && lockpickTimer <= 0
                && tumblerTimer <= 0)
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
                if (KeyBindingsScript.GetKeyDown("MainAttack"))
                {
                    Attempt();
                }

                lockpickStep = lockpickMoveSpeed * Time.unscaledDeltaTime;
                tumblerStep = tumblerMoveSpeed * Time.unscaledDeltaTime;
                if (isPickGoingUp)
                {
                    lockpickTimer -= Time.unscaledDeltaTime;
                    if (lockpickTimer > 0)
                    {
                        lockpick.transform.localPosition = new Vector3(lockpick.transform.localPosition.x,
                                                                       lockpick.transform.localPosition.y + 0.15f * lockpickStep,
                                                                       lockpick.transform.localPosition.z);
                        if (currentTumblerSlot == 0
                            && !LockStatusScript.tumbler1Unlocked)
                        {
                            tumbler1.transform.localPosition = new Vector3(tumbler1.transform.localPosition.x,
                                                                           tumbler1.transform.localPosition.y + 0.15f * tumblerStep,
                                                                           tumbler1.transform.localPosition.z);
                            spring1.transform.localScale = new Vector3(spring1.transform.localScale.x,
                                                                       spring1.transform.localScale.y - 0.0025f * tumblerStep,
                                                                       spring1.transform.localScale.z);
                        }
                        else if (currentTumblerSlot == 1
                                 && !LockStatusScript.tumbler2Unlocked)
                        {
                            tumbler2.transform.localPosition = new Vector3(tumbler2.transform.localPosition.x,
                                                                           tumbler2.transform.localPosition.y + 0.15f * tumblerStep,
                                                                           tumbler2.transform.localPosition.z);
                            spring2.transform.localScale = new Vector3(spring2.transform.localScale.x,
                                                                       spring2.transform.localScale.y - 0.0025f * tumblerStep,
                                                                       spring2.transform.localScale.z);
                        }
                        else if (currentTumblerSlot == 2
                                 && !LockStatusScript.tumbler3Unlocked)
                        {
                            tumbler3.transform.localPosition = new Vector3(tumbler3.transform.localPosition.x,
                                                                           tumbler3.transform.localPosition.y + 0.15f * tumblerStep,
                                                                           tumbler3.transform.localPosition.z);
                            spring3.transform.localScale = new Vector3(spring3.transform.localScale.x,
                                                                       spring3.transform.localScale.y - 0.0025f * tumblerStep,
                                                                       spring3.transform.localScale.z);
                        }
                        else if (currentTumblerSlot == 3
                                 && !LockStatusScript.tumbler4Unlocked)
                        {
                            tumbler4.transform.localPosition = new Vector3(tumbler4.transform.localPosition.x,
                                                                           tumbler4.transform.localPosition.y + 0.15f * tumblerStep,
                                                                           tumbler4.transform.localPosition.z);
                            spring4.transform.localScale = new Vector3(spring4.transform.localScale.x,
                                                                       spring4.transform.localScale.y - 0.0025f * tumblerStep,
                                                                       spring4.transform.localScale.z);
                        }
                        else if (currentTumblerSlot == 4 
                                 && !LockStatusScript.tumbler5Unlocked)
                        {
                            tumbler5.transform.localPosition = new Vector3(tumbler5.transform.localPosition.x,
                                                                           tumbler5.transform.localPosition.y + 0.15f * tumblerStep,
                                                                           tumbler5.transform.localPosition.z);
                            spring5.transform.localScale = new Vector3(spring5.transform.localScale.x,
                                                                       spring5.transform.localScale.y - 0.0025f * tumblerStep,
                                                                       spring5.transform.localScale.z);
                        }
                    }
                    else
                    {
                        MovePick("down");
                    }
                }
                else if (isPickGoingDown)
                {
                    lockpickTimer -= Time.unscaledDeltaTime;
                    tumblerTimer -= Time.unscaledDeltaTime;
                    if (lockpickTimer > 0)
                    {
                        lockpick.transform.localPosition = new Vector3(lockpick.transform.localPosition.x,
                                                                       lockpick.transform.localPosition.y - 0.15f * lockpickStep,
                                                                       lockpick.transform.localPosition.z);
                    }
                    if (tumblerTimer > 0)
                    {
                        if (currentTumblerSlot == 0
                            && !LockStatusScript.tumbler1Unlocked)
                        {
                            tumbler1.transform.localPosition = new Vector3(tumbler1.transform.localPosition.x,
                                                                           tumbler1.transform.localPosition.y - 0.15f * tumblerStep,
                                                                           tumbler1.transform.localPosition.z);
                            spring1.transform.localScale = new Vector3(spring1.transform.localScale.x,
                                                                       spring1.transform.localScale.y + 0.0025f * tumblerStep,
                                                                       spring1.transform.localScale.z);
                        }
                        else if (currentTumblerSlot == 1
                                 && !LockStatusScript.tumbler2Unlocked)
                        {
                            tumbler2.transform.localPosition = new Vector3(tumbler2.transform.localPosition.x,
                                                                           tumbler2.transform.localPosition.y - 0.15f * tumblerStep,
                                                                           tumbler2.transform.localPosition.z);
                            spring2.transform.localScale = new Vector3(spring2.transform.localScale.x,
                                                                       spring2.transform.localScale.y + 0.0025f * tumblerStep,
                                                                       spring2.transform.localScale.z);
                        }
                        else if (currentTumblerSlot == 2
                                 && !LockStatusScript.tumbler3Unlocked)
                        {
                            tumbler3.transform.localPosition = new Vector3(tumbler3.transform.localPosition.x,
                                                                           tumbler3.transform.localPosition.y - 0.15f * tumblerStep,
                                                                           tumbler3.transform.localPosition.z);
                            spring3.transform.localScale = new Vector3(spring3.transform.localScale.x,
                                                                       spring3.transform.localScale.y + 0.0025f * tumblerStep,
                                                                       spring3.transform.localScale.z);
                        }
                        else if (currentTumblerSlot == 3
                                 && !LockStatusScript.tumbler4Unlocked)
                        {
                            tumbler4.transform.localPosition = new Vector3(tumbler4.transform.localPosition.x,
                                                                           tumbler4.transform.localPosition.y - 0.15f * tumblerStep,
                                                                           tumbler4.transform.localPosition.z);
                            spring4.transform.localScale = new Vector3(spring4.transform.localScale.x,
                                                                       spring4.transform.localScale.y + 0.0025f * tumblerStep,
                                                                       spring4.transform.localScale.z);
                        }
                        else if (currentTumblerSlot == 4
                                 && !LockStatusScript.tumbler5Unlocked)
                        {
                            tumbler5.transform.localPosition = new Vector3(tumbler5.transform.localPosition.x,
                                                                           tumbler5.transform.localPosition.y - 0.15f * tumblerStep,
                                                                           tumbler5.transform.localPosition.z);
                            spring5.transform.localScale = new Vector3(spring5.transform.localScale.x,
                                                                       spring5.transform.localScale.y + 0.0025f * tumblerStep,
                                                                       spring5.transform.localScale.z);
                        }
                    }
                    else if (lockpickTimer <= 0
                             && tumblerTimer <= 0)
                    {
                        SetAllPositions();
                    }
                }
            }
        }
    }

    //start pick movement after a key was pressed
    private void MovePick(string direction)
    {
        float reducedAmount = 0;
        if (currentTumblerSlot == 0)
        {
            reducedAmount = LockStatusScript.tumbler1Weight;
        }
        else if (currentTumblerSlot == 1)
        {
            reducedAmount = LockStatusScript.tumbler2Weight;
        }
        else if (currentTumblerSlot == 2)
        {
            reducedAmount = LockStatusScript.tumbler3Weight;
        }
        else if (currentTumblerSlot == 3)
        {
            reducedAmount = LockStatusScript.tumbler4Weight;
        }
        else if (currentTumblerSlot == 4)
        {
            reducedAmount = LockStatusScript.tumbler5Weight;
        }

        isMovingPick = true;

        lockpickMoveSpeed = 1000;
        lockpickTimer = 0.25f;

        if (direction == "up")
        {
            tumblerMoveSpeed = 1000;
            tumblerTimer = 0.25f;

            isPickGoingUp = true;
        }
        else if (direction == "down")
        {
            tumblerMoveSpeed = 1000 / reducedAmount; 
            tumblerTimer = 0.25f * reducedAmount;

            isPickGoingUp = false;
            isPickGoingDown = true;
        }
        else if (direction == "left"
                 && currentTumblerSlot >= 1)
        {
            MovePickSideways(currentTumblerSlot - 1);
        }
        else if (direction == "right"
                 && currentTumblerSlot <= 3)
        {
            MovePickSideways(currentTumblerSlot + 1);
        }
        else
        {
            isMovingPick = false;
        }
    }

    //move the pick left or right towards direction, to position targetSlot
    private void MovePickSideways(int targetSlot)
    {
        currentTumblerSlot = targetSlot;

        lockpick.transform.localPosition = new(slotPositions[currentTumblerSlot],
                                               lockpick.transform.localPosition.y,
                                               lockpick.transform.localPosition.z);

        SetAllPositions();
    }

    //attempt to pick currently selected tumbler
    public void Attempt()
    {
        float tumblerWeight = 0;
        if (currentTumblerSlot == 0)
        {
            tumblerWeight = LockStatusScript.tumbler1Weight;
        }
        else if (currentTumblerSlot == 1)
        {
            tumblerWeight = LockStatusScript.tumbler2Weight;
        }
        else if (currentTumblerSlot == 2)
        {
            tumblerWeight = LockStatusScript.tumbler3Weight;
        }
        else if (currentTumblerSlot == 3)
        {
            tumblerWeight = LockStatusScript.tumbler4Weight;
        }
        else if (currentTumblerSlot == 4)
        {
            tumblerWeight = LockStatusScript.tumbler5Weight;
        }

        float determinator = 10000 * tumblerWeight;
        float unlockChance = Random.Range(0, determinator / PlayerStatsScript.Skills["Security"] / PlayerStatsScript.Attributes["Luck"] * 10);
        float requiredUnlockChance = determinator / PlayerStatsScript.Skills["Security"] / PlayerStatsScript.Attributes["Luck"] * 10 * 0.9f;

        if (unlockChance >= requiredUnlockChance)
        {
            if (currentTumblerSlot == 0)
            {
                LockStatusScript.tumbler1Unlocked = true;
            }
            else if (currentTumblerSlot == 1)
            {
                LockStatusScript.tumbler2Unlocked = true;
            }
            else if (currentTumblerSlot == 2)
            {
                LockStatusScript.tumbler3Unlocked = true;
            }
            else if (currentTumblerSlot == 3)
            {
                LockStatusScript.tumbler4Unlocked = true;
            }
            else if (currentTumblerSlot == 4)
            {
                LockStatusScript.tumbler5Unlocked = true;
            }

            if (LockStatusScript.tumbler1Unlocked
                && LockStatusScript.tumbler2Unlocked
                && LockStatusScript.tumbler3Unlocked
                && LockStatusScript.tumbler4Unlocked
                && LockStatusScript.tumbler5Unlocked)
            {
                Unlock();
            }
            else
            {
                SetAllPositions();
            }
        }
        //failed lockpick attempt or rushing lockpicking too fast
        else if (unlockChance < requiredUnlockChance)
        {
            if (LockpickScript.itemCount - 1 == 0)
            {
                LockpickScript.itemCount = 0;
                CloseLockpickUI();
            }
            else
            {
                LockpickScript.itemCount--;
                txt_LockpickCount.text = "Lockpicks: " + LockpickScript.itemCount.ToString();
            }

            SetAllPositions();
        }
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
                txt_LockpickCount.text = "Lockpicks: " + LockpickScript.itemCount.ToString();
            }
        }
    }

    //once lock has successfully been unlocked
    private void Unlock()
    {
        if (LockStatusScript.GetComponent<Manager_Door>() != null)
        {
            Debug.Log("Info: Unlocked " + LockStatusScript.GetComponent<Manager_Door>().doorName + ".");
        }
        else if (LockStatusScript.GetComponent<UI_Inventory>() != null)
        {
            Debug.Log("Info: Unlocked " + LockStatusScript.GetComponent<UI_Inventory>().containerName + ".");
        }

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

        SetAllPositions();
        currentTumblerSlot = 0;
    }
    public void CloseLockpickUI()
    {
        SetAllPositions();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        par_LockpickUI.SetActive(false);

        PauseMenuScript.isLockpickUIOpen = false;
        PauseMenuScript.UnpauseGame();
    }

    //sets tumbler positions and spring scales
    private void SetAllPositions()
    {
        isMovingPick = false;
        isPickGoingUp = false;
        isPickGoingDown = false;

        lockpickTimer = 0;
        tumblerTimer = 0;

        lockpick.transform.localPosition = new(slotPositions[currentTumblerSlot],
                                               lockpickStartPosition.y,
                                               lockpickStartPosition.z);

        if (!LockStatusScript.tumbler1Unlocked)
        {
            tumbler1.transform.localPosition = new Vector3(tumbler1.transform.localPosition.x,
                                                           0,
                                                           tumbler1.transform.localPosition.z);
            spring1.transform.localScale = new Vector3(spring1.transform.localScale.x,
                                                       1,
                                                       spring1.transform.localScale.z);
        }
        else
        {
            tumbler1.transform.localPosition = new Vector3(tumbler1.transform.localPosition.x,
                                                           35,
                                                           tumbler1.transform.localPosition.z);
            spring1.transform.localScale = new Vector3(spring1.transform.localScale.x,
                                                       0.5f,
                                                       spring1.transform.localScale.z);
        }

        if (!LockStatusScript.tumbler2Unlocked)
        {
            tumbler2.transform.localPosition = new Vector3(tumbler2.transform.localPosition.x,
                                                           0,
                                                           tumbler2.transform.localPosition.z);
            spring2.transform.localScale = new Vector3(spring2.transform.localScale.x,
                                                       1,
                                                       spring2.transform.localScale.z);
        }
        else
        {
            tumbler2.transform.localPosition = new Vector3(tumbler2.transform.localPosition.x,
                                                           35,
                                                           tumbler2.transform.localPosition.z);
            spring2.transform.localScale = new Vector3(spring2.transform.localScale.x,
                                                       0.5f,
                                                       spring2.transform.localScale.z);
        }

        if (!LockStatusScript.tumbler3Unlocked)
        {
            tumbler3.transform.localPosition = new Vector3(tumbler3.transform.localPosition.x,
                                                           0,
                                                           tumbler3.transform.localPosition.z);
            spring3.transform.localScale = new Vector3(spring3.transform.localScale.x,
                                                       1,
                                                       spring3.transform.localScale.z);
        }
        else
        {
            tumbler3.transform.localPosition = new Vector3(tumbler3.transform.localPosition.x,
                                                           35,
                                                           tumbler3.transform.localPosition.z);
            spring3.transform.localScale = new Vector3(spring3.transform.localScale.x,
                                                       0.5f,
                                                       spring3.transform.localScale.z);
        }

        if (!LockStatusScript.tumbler4Unlocked)
        {
            tumbler4.transform.localPosition = new Vector3(tumbler4.transform.localPosition.x,
                                                           0,
                                                           tumbler4.transform.localPosition.z);
            spring4.transform.localScale = new Vector3(spring4.transform.localScale.x,
                                                       1,
                                                       spring4.transform.localScale.z);
        }
        else
        {
            tumbler4.transform.localPosition = new Vector3(tumbler4.transform.localPosition.x,
                                                           35,
                                                           tumbler4.transform.localPosition.z);
            spring4.transform.localScale = new Vector3(spring4.transform.localScale.x,
                                                       0.5f,
                                                       spring4.transform.localScale.z);
        }

        if (!LockStatusScript.tumbler5Unlocked)
        {
            tumbler5.transform.localPosition = new Vector3(tumbler5.transform.localPosition.x,
                                                           0,
                                                           tumbler5.transform.localPosition.z);
            spring5.transform.localScale = new Vector3(spring5.transform.localScale.x,
                                                       1,
                                                       spring5.transform.localScale.z);
        }
        else
        {
            tumbler5.transform.localPosition = new Vector3(tumbler5.transform.localPosition.x,
                                                      35,
                                                      tumbler5.transform.localPosition.z);
            spring5.transform.localScale = new Vector3(spring5.transform.localScale.x,
                                                       0.5f,
                                                       spring5.transform.localScale.z);
        }
    }
}