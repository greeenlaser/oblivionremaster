using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_Lockpicking : MonoBehaviour
{
    [Header("Lockpick UI")]
    [SerializeField] private GameObject par_Lockpicking;
    [SerializeField] private TMP_Text txt_LockpickingTitle;
    [SerializeField] private TMP_Text txt_LockLevel;
    [SerializeField] private TMP_Text txt_LockpickCount;
    [SerializeField] private TMP_Text txt_PlayerLockPickingLevel;
    [SerializeField] private Button btn_AutoAttempt;
    [SerializeField] private Button btn_CloseLockpickUI;

    [Header("Lock model")]
    [SerializeField] private Transform pick;
    [SerializeField] private Transform pos_PickDown1;
    [SerializeField] private Transform pos_PickDown2;
    [SerializeField] private Transform pos_PickDown3;
    [SerializeField] private Transform pos_PickDown4;
    [SerializeField] private Transform pos_PickDown5;
    [SerializeField] private Transform pos_PickUp1;
    [SerializeField] private Transform pos_PickUp2;
    [SerializeField] private Transform pos_PickUp3;
    [SerializeField] private Transform pos_PickUp4;
    [SerializeField] private Transform pos_PickUp5;

    [Header("Lock")]
    [SerializeField] private Transform gear1;
    [SerializeField] private Transform gear2;
    [SerializeField] private Transform theLock;
    [SerializeField] private Transform pos_LockLocked;

    [Header("Tumbler 1")]
    [SerializeField] private GameObject tumbler1;
    [SerializeField] private GameObject spring1;
    [SerializeField] private Transform pos_Unlocked1;
    [SerializeField] private Transform pos_Locked1;

    [Header("Tumbler 2")]
    [SerializeField] private Transform tumbler2;
    [SerializeField] private Transform spring2;
    [SerializeField] private Transform pos_Unlocked2;
    [SerializeField] private Transform pos_Locked2;

    [Header("Tumbler 3")]
    [SerializeField] private Transform tumbler3;
    [SerializeField] private Transform spring3;
    [SerializeField] private Transform pos_Unlocked3;
    [SerializeField] private Transform pos_Locked3;

    [Header("Tumbler 4")]
    [SerializeField] private Transform tumbler4;
    [SerializeField] private Transform spring4;
    [SerializeField] private Transform pos_Unlocked4;
    [SerializeField] private Transform pos_Locked4;

    [Header("Tumbler 5")]
    [SerializeField] private Transform tumbler5;
    [SerializeField] private Transform spring5;
    [SerializeField] private Transform pos_Unlocked5;
    [SerializeField] private Transform pos_Locked5;

    [Header("Scripts")]
    [SerializeField] private GameObject thePlayer;

    //public but hidden variables
    [HideInInspector] public GameObject targetLock;

    //private variables
    private bool goingLeft;
    private bool goingRight;
    private bool goingUp;
    private bool goingDown;
    private bool movingLock;
    private bool confirmedUnlockAttempt;
    private int tumbler = 1;
    private int pickStep;
    private float timer;
    private Vector3 gearOriginalRotation;
    private Transform targetPosition;

    //scripts
    private Player_Stats PlayerStatsScript;
    private UI_Inventory PlayerInventoryScript;
    private Env_LockStatus LockStatusScript;
    private UI_PauseMenu PauseMenuScript;
    private Manager_KeyBindings KeyBindingsScript;

    private void Awake()
    {
        PlayerStatsScript = thePlayer.GetComponent<Player_Stats>();
        PlayerInventoryScript = thePlayer.GetComponent<UI_Inventory>();

        PauseMenuScript = GetComponent<UI_PauseMenu>();
        KeyBindingsScript = GetComponent<Manager_KeyBindings>();

        btn_AutoAttempt.onClick.AddListener(AutoAttempt);
        btn_CloseLockpickUI.onClick.AddListener(CloseLockpickUI);

        gearOriginalRotation = gear1.localEulerAngles;
    }

    private void Start()
    {
        CloseLockpickUI();
    }

    private void Update()
    {
        if (par_Lockpicking.activeInHierarchy)
        {
            //detects key input to start pick movement
            if (!goingLeft
                && !goingRight
                && !goingUp
                && !goingDown
                && !movingLock
                && (!LockStatusScript.tumbler1Unlocked
                || !LockStatusScript.tumbler2Unlocked
                || !LockStatusScript.tumbler3Unlocked
                || !LockStatusScript.tumbler4Unlocked
                || !LockStatusScript.tumbler5Unlocked))
            {
                if (KeyBindingsScript.GetKeyDown("WalkLeft"))
                {
                    SetPickDirection("left");
                }
                else if (KeyBindingsScript.GetKeyDown("WalkRight"))
                {
                    SetPickDirection("right");
                }
                else if (KeyBindingsScript.GetKeyDown("WalkForwards"))
                {
                    SetPickDirection("up");
                }
            }

            //moves pick towards destination after input
            //has been recieved and direction is allowed
            if (goingLeft
                || goingRight
                || (goingUp
                && !goingDown)
                || (!goingUp
                && goingDown))
            {
                if (goingLeft
                    || goingRight)
                {
                    pick.transform.position = targetPosition.position;
                    goingLeft = false;
                    goingRight = false;
                }
                else
                {
                    if (pickStep >= 1)
                    {
                        timer -= Time.unscaledDeltaTime;
                        if (timer <= 0)
                        {
                            if (goingUp)
                            {
                                pick.transform.position = new(pick.transform.position.x,
                                                              pick.transform.position.y + 3f,
                                                              pick.transform.position.z);
                                MoveTumblerUp();
                            }
                            else if (goingDown)
                            {
                                pick.transform.position = new(pick.transform.position.x,
                                                              pick.transform.position.y - 3f,
                                                              pick.transform.position.z);
                                MoveTumblerDown();
                            }

                            pickStep--;
                            timer = 0.01f;
                        }
                    }
                    else if (pickStep == 0)
                    {
                        if (goingUp)
                        {
                            SetPickDirection("down");
                        }
                        else if (goingDown)
                        {
                            goingDown = false;
                        }
                    }
                }
            }

            //moves the lock itself and rotates the gears after all tumblers are unlocked
            if (movingLock)
            {
                if (pickStep >= 1)
                {
                    timer -= Time.unscaledDeltaTime;
                    if (timer <= 0)
                    {
                        theLock.transform.localPosition = new(theLock.transform.localPosition.x - 3f,
                                                              theLock.transform.localPosition.y,
                                                              theLock.transform.localPosition.z);

                        gear1.transform.Rotate(gear1.localRotation.x,
                                               gear1.localRotation.y,
                                               gear1.localRotation.z + 2f, 
                                               Space.Self);

                        gear2.transform.Rotate(gear2.localRotation.x,
                                               gear2.localRotation.y,
                                               gear2.localRotation.z + 2f,
                                               Space.Self);

                        pickStep--;
                        timer = 0.01f;
                    }
                }
                else if (pickStep == 0)
                {
                    if (targetLock != null)
                    {
                        CloseLockpickUI();
                    }

                    movingLock = false;
                }
            }
        }
    }

    //reset the pick to its original location
    private void ResetPickPosition()
    {
        goingLeft = false;
        goingRight = false;
        goingUp = false;
        goingDown = false;

        tumbler = 1;

        pick.transform.localPosition = pos_PickDown1.localPosition;
    }

    public void SetTumblerPositions(Env_LockStatus lockStatusScript)
    {
        targetLock = lockStatusScript.gameObject;
        LockStatusScript = targetLock.GetComponent<Env_LockStatus>();

        Debug.Log("Interacting with " + targetLock.name + "...");
             
        if (targetLock.GetComponent<Env_LockStatus>() != null
            && !targetLock.GetComponent<Env_LockStatus>().hasLoadedLock)
        {
            int containerDifficulty = (int)targetLock.GetComponent<Env_LockStatus>().lockDifficulty;
            if (containerDifficulty == 0)
            {
                lockStatusScript.tumbler1Weight = Random.Range(1, 5);
                tumbler2.transform.position = pos_Unlocked2.position;
                spring2.transform.localScale = new(1, 0.46f, 1);
                LockStatusScript.tumbler2Unlocked = true;
                tumbler3.transform.position = pos_Unlocked3.position;
                spring3.transform.localScale = new(1, 0.46f, 1);
                LockStatusScript.tumbler3Unlocked = true;
                tumbler4.transform.position = pos_Unlocked4.position;
                spring4.transform.localScale = new(1, 0.46f, 1);
                LockStatusScript.tumbler4Unlocked = true;
                tumbler5.transform.position = pos_Unlocked5.position;
                spring5.transform.localScale = new(1, 0.46f, 1);
                LockStatusScript.tumbler5Unlocked = true;
            }
            else if (containerDifficulty == 1)
            {
                lockStatusScript.tumbler1Weight = Random.Range(1, 5);
                lockStatusScript.tumbler2Weight = Random.Range(1, 5);
                tumbler3.transform.position = pos_Unlocked3.position;
                spring3.transform.localScale = new(1, 0.46f, 1);
                LockStatusScript.tumbler3Unlocked = true;
                tumbler4.transform.position = pos_Unlocked4.position;
                spring4.transform.localScale = new(1, 0.46f, 1);
                LockStatusScript.tumbler4Unlocked = true;
                tumbler5.transform.position = pos_Unlocked5.position;
                spring5.transform.localScale = new(1, 0.46f, 1);
                LockStatusScript.tumbler5Unlocked = true;
            }
            else if (containerDifficulty == 2)
            {
                lockStatusScript.tumbler1Weight = Random.Range(1, 5);
                lockStatusScript.tumbler2Weight = Random.Range(1, 5);
                lockStatusScript.tumbler3Weight = Random.Range(1, 5);
                tumbler4.transform.position = pos_Unlocked4.position;
                spring4.transform.localScale = new(1, 0.46f, 1);
                LockStatusScript.tumbler4Unlocked = true;
                tumbler5.transform.position = pos_Unlocked5.position;
                spring5.transform.localScale = new(1, 0.46f, 1);
                LockStatusScript.tumbler5Unlocked = true;
            }
            else if (containerDifficulty == 3)
            {
                lockStatusScript.tumbler1Weight = Random.Range(1, 5);
                lockStatusScript.tumbler2Weight = Random.Range(1, 5);
                lockStatusScript.tumbler3Weight = Random.Range(1, 5);
                lockStatusScript.tumbler4Weight = Random.Range(1, 5);
                tumbler5.transform.position = pos_Unlocked5.position;
                spring5.transform.localScale = new(1, 0.46f, 1);
                LockStatusScript.tumbler5Unlocked = true;
            }
        }
        else
        {
            if (LockStatusScript.tumbler1Unlocked)
            {
                tumbler1.transform.position = pos_Unlocked1.position;
                spring1.transform.localScale = new(1, 0.46f, 1);
            }
            if (LockStatusScript.tumbler2Unlocked)
            {
                tumbler2.transform.position = pos_Unlocked2.position;
                spring2.transform.localScale = new(1, 0.46f, 1);
            }
            if (LockStatusScript.tumbler3Unlocked)
            {
                tumbler3.transform.position = pos_Unlocked3.position;
                spring3.transform.localScale = new(1, 0.46f, 1);
            }
            if (LockStatusScript.tumbler4Unlocked)
            {
                tumbler4.transform.position = pos_Unlocked4.position;
                spring4.transform.localScale = new(1, 0.46f, 1);
            }
            if (LockStatusScript.tumbler5Unlocked)
            {
                tumbler5.transform.position = pos_Unlocked5.position;
                spring5.transform.localScale = new(1, 0.46f, 1);
            }
        }
    }

    //check if requested pick movement direction is allowed
    private void SetPickDirection(string direction)
    {
        if (direction == "left")
        {
            if (tumbler == 2)
            {
                targetPosition = pos_PickDown1;
                goingLeft = true;

                tumbler--;
            }
            else if (tumbler == 3)
            {
                targetPosition = pos_PickDown2;
                goingLeft = true;

                tumbler--;
            }
            else if (tumbler == 4)
            {
                targetPosition = pos_PickDown3;
                goingLeft = true;

                tumbler--;
            }
            else if (tumbler == 5)
            {
                targetPosition = pos_PickDown4;
                goingLeft = true;

                tumbler--;
            }
        }
        else if (direction == "right")
        {
            if (tumbler == 1)
            {
                targetPosition = pos_PickDown2;
                goingRight = true;

                tumbler++;
            }
            else if (tumbler == 2)
            {
                targetPosition = pos_PickDown3;
                goingRight = true;

                tumbler++;
            }
            else if (tumbler == 3)
            {
                targetPosition = pos_PickDown4;
                goingRight = true;

                tumbler++;
            }
            else if (tumbler == 4)
            {
                targetPosition = pos_PickDown5;
                goingRight = true;

                tumbler++;
            }
        }
        else if (direction == "up")
        {
            pickStep = 10;
            timer = 0.01f;

            if (tumbler == 1)
            {
                targetPosition = pos_PickUp1;
                goingUp = true;
            }
            else if (tumbler == 2)
            {
                targetPosition = pos_PickUp2;
                goingUp = true;
            }
            else if (tumbler == 3)
            {
                targetPosition = pos_PickUp3;
                goingUp = true;
            }
            else if (tumbler == 4)
            {
                targetPosition = pos_PickUp4;
                goingUp = true;
            }
            else if (tumbler == 5)
            {
                targetPosition = pos_PickUp5;
                goingUp = true;
            }
        }
        else if (direction == "down")
        {
            pickStep = 10;
            timer = 0.01f;
            goingUp = false;

            if (tumbler == 1)
            {
                targetPosition = pos_PickDown1;
                goingDown = true;
            }
            else if (tumbler == 2)
            {
                targetPosition = pos_PickDown2;
                goingDown = true;
            }
            else if (tumbler == 3)
            {
                targetPosition = pos_PickDown3;
                goingDown = true;
            }
            else if (tumbler == 4)
            {
                targetPosition = pos_PickDown4;
                goingDown = true;
            }
            else if (tumbler == 5)
            {
                targetPosition = pos_PickDown5;
                goingDown = true;
            }
        }
    }

    private void MoveTumblerUp()
    {
        if (tumbler == 1
            && !LockStatusScript.tumbler1Unlocked)
        {
            tumbler1.transform.position = new(tumbler1.transform.position.x,
                                              tumbler1.transform.position.y + 3,
                                              tumbler1.transform.position.z);
            spring1.transform.localScale = new(spring1.transform.localScale.x,
                                               spring1.transform.localScale.y - 0.042f,
                                               spring1.transform.localScale.z);

            if (Input.GetKeyDown(KeyCode.Mouse0)
                && !confirmedUnlockAttempt)
            {
                CheckAttempt(tumbler);
            }
        }
        else if (tumbler == 2
                 && !LockStatusScript.tumbler2Unlocked)
        {
            tumbler2.transform.position = new(tumbler2.transform.position.x,
                                              tumbler2.transform.position.y + 3,
                                              tumbler2.transform.position.z);
            spring2.transform.localScale = new(spring2.transform.localScale.x,
                                               spring2.transform.localScale.y - 0.042f,
                                               spring2.transform.localScale.z);

            if (Input.GetKeyDown(KeyCode.Mouse0)
                && !confirmedUnlockAttempt)
            {
                CheckAttempt(tumbler);
            }
        }
        else if (tumbler == 3
                 && !LockStatusScript.tumbler3Unlocked)
        {
            tumbler3.transform.position = new(tumbler3.transform.position.x,
                                              tumbler3.transform.position.y + 3,
                                              tumbler3.transform.position.z);
            spring3.transform.localScale = new(spring3.transform.localScale.x,
                                               spring3.transform.localScale.y - 0.042f,
                                               spring3.transform.localScale.z);

            if (Input.GetKeyDown(KeyCode.Mouse0)
                && !confirmedUnlockAttempt)
            {
                CheckAttempt(tumbler);
            }
        }
        else if (tumbler == 4
                 && !LockStatusScript.tumbler4Unlocked)
        {
            tumbler4.transform.position = new(tumbler4.transform.position.x,
                                              tumbler4.transform.position.y + 3,
                                              tumbler4.transform.position.z);
            spring4.transform.localScale = new(spring4.transform.localScale.x,
                                               spring4.transform.localScale.y - 0.042f,
                                               spring4.transform.localScale.z);

            if (Input.GetKeyDown(KeyCode.Mouse0)
                && !confirmedUnlockAttempt)
            {
                CheckAttempt(tumbler);
            }
        }
        else if (tumbler == 5
                 && !LockStatusScript.tumbler5Unlocked)
        {
            tumbler5.transform.position = new(tumbler5.transform.position.x,
                                              tumbler5.transform.position.y + 3,
                                              tumbler5.transform.position.z);
            spring5.transform.localScale = new(spring5.transform.localScale.x,
                                               spring5.transform.localScale.y - 0.042f,
                                               spring5.transform.localScale.z);

            if (Input.GetKeyDown(KeyCode.Mouse0)
                && !confirmedUnlockAttempt)
            {
                CheckAttempt(tumbler);
            }
        }
    }
    private void MoveTumblerDown()
    {
        if (tumbler == 1
            && !LockStatusScript.tumbler1Unlocked)
        {
            tumbler1.transform.position = new(tumbler1.transform.position.x,
                                              tumbler1.transform.position.y - 3,
                                              tumbler1.transform.position.z);
            spring1.transform.localScale = new(spring1.transform.localScale.x,
                                               spring1.transform.localScale.y + 0.042f,
                                               spring1.transform.localScale.z);

            if (Input.GetKeyDown(KeyCode.Mouse0)
                && !confirmedUnlockAttempt)
            {
                CheckAttempt(tumbler);
            }
        }
        else if (tumbler == 2
                 && !LockStatusScript.tumbler2Unlocked)
        {
            tumbler2.transform.position = new(tumbler2.transform.position.x,
                                              tumbler2.transform.position.y - 3,
                                              tumbler2.transform.position.z);
            spring2.transform.localScale = new(spring2.transform.localScale.x,
                                               spring2.transform.localScale.y + 0.042f,
                                               spring2.transform.localScale.z);

            if (Input.GetKeyDown(KeyCode.Mouse0)
                && !confirmedUnlockAttempt)
            {
                CheckAttempt(tumbler);
            }
        }
        else if (tumbler == 3
                 && !LockStatusScript.tumbler3Unlocked)
        {
            tumbler3.transform.position = new(tumbler3.transform.position.x,
                                              tumbler3.transform.position.y - 3,
                                              tumbler3.transform.position.z);
            spring3.transform.localScale = new(spring3.transform.localScale.x,
                                               spring3.transform.localScale.y + 0.042f,
                                               spring3.transform.localScale.z);

            if (Input.GetKeyDown(KeyCode.Mouse0)
                && !confirmedUnlockAttempt)
            {
                CheckAttempt(tumbler);
            }
        }
        else if (tumbler == 4
                 && !LockStatusScript.tumbler4Unlocked)
        {
            tumbler4.transform.position = new(tumbler4.transform.position.x,
                                              tumbler4.transform.position.y - 3,
                                              tumbler4.transform.position.z);
            spring4.transform.localScale = new(spring4.transform.localScale.x,
                                               spring4.transform.localScale.y + 0.042f,
                                               spring4.transform.localScale.z);

            if (Input.GetKeyDown(KeyCode.Mouse0)
                && !confirmedUnlockAttempt)
            {
                CheckAttempt(tumbler);
            }
        }
        else if (tumbler == 5
                 && !LockStatusScript.tumbler5Unlocked)
        {
            tumbler5.transform.position = new(tumbler5.transform.position.x,
                                              tumbler5.transform.position.y - 3,
                                              tumbler5.transform.position.z);
            spring5.transform.localScale = new(spring5.transform.localScale.x,
                                               spring5.transform.localScale.y + 0.042f,
                                               spring5.transform.localScale.z);

            if (Input.GetKeyDown(KeyCode.Mouse0)
                && !confirmedUnlockAttempt)
            {
                CheckAttempt(tumbler);
            }
        }
    }

    //successful user unlock attempt unlocks the selected tumbler
    private void CheckAttempt(int tumbler)
    {
        bool succeeded = false;
        int finalSuccessChance = Random.Range(1, 100) + PlayerStatsScript.Skills["Security"] * PlayerStatsScript.Attributes["Luck"];
        if (finalSuccessChance >= 50)
        {
            succeeded = true;
        }
        else
        {
            FailedAttempt();
        }

        if (succeeded)
        {
            if (tumbler == 1)
            {
                LockStatusScript.tumbler1Unlocked = true;
                tumbler1.transform.position = pos_Unlocked1.position;
                spring1.transform.localScale = new(1, 0.46f, 1);
            }
            if (tumbler == 2)
            {
                LockStatusScript.tumbler2Unlocked = true;
                tumbler2.transform.position = pos_Unlocked2.position;
                spring2.transform.localScale = new(1, 0.46f, 1);
            }
            if (tumbler == 3)
            {
                LockStatusScript.tumbler3Unlocked = true;
                tumbler3.transform.position = pos_Unlocked3.position;
                spring3.transform.localScale = new(1, 0.46f, 1);
            }
            if (tumbler == 4)
            {
                LockStatusScript.tumbler4Unlocked = true;
                tumbler4.transform.position = pos_Unlocked4.position;
                spring4.transform.localScale = new(1, 0.46f, 1);
            }
            if (tumbler == 5)
            {
                LockStatusScript.tumbler5Unlocked = true;
                tumbler5.transform.position = pos_Unlocked5.position;
                spring5.transform.localScale = new(1, 0.46f, 1);
            }

            CheckLockStatus("regular");
        }

        confirmedUnlockAttempt = false;
    }

    //auto-attempt to unlock the lock
    public void AutoAttempt()
    {
        if (!LockStatusScript.tumbler1Unlocked
            || !LockStatusScript.tumbler2Unlocked
            || !LockStatusScript.tumbler3Unlocked
            || !LockStatusScript.tumbler4Unlocked
            || !LockStatusScript.tumbler5Unlocked)
        {
            float finalSuccessChance = Random.Range(1, 100);
            if (finalSuccessChance >= 95)
            {
                LockStatusScript.tumbler1Unlocked = true;
                tumbler1.transform.position = pos_Unlocked1.position;
                spring1.transform.localScale = new(1, 0.46f, 1);

                LockStatusScript.tumbler2Unlocked = true;
                tumbler2.transform.position = pos_Unlocked2.position;
                spring2.transform.localScale = new(1, 0.46f, 1);

                LockStatusScript.tumbler3Unlocked = true;
                tumbler3.transform.position = pos_Unlocked3.position;
                spring3.transform.localScale = new(1, 0.46f, 1);

                LockStatusScript.tumbler4Unlocked = true;
                tumbler4.transform.position = pos_Unlocked4.position;
                spring4.transform.localScale = new(1, 0.46f, 1);

                LockStatusScript.tumbler5Unlocked = true;
                tumbler5.transform.position = pos_Unlocked5.position;
                spring5.transform.localScale = new(1, 0.46f, 1);

                CheckLockStatus("autoattempt");
            }
            else
            {
                FailedAttempt();
            }
        }
    }

    //some tumblers will reset to their original positions after a failed lock picking attempt
    //depending on the players lockpinging level and the container lock level
    private void FailedAttempt()
    {
        GameObject lockpick = null;
        foreach (GameObject item in PlayerInventoryScript.playerItems)
        {
            if (item.name == "Lockpick")
            {
                lockpick = item;
                break;
            }
        }
        if (lockpick != null)
        {
            lockpick.GetComponent<Env_Item>().itemCount--;

            int lockpickCount = lockpick.GetComponent<Env_Item>().itemCount;
            if (lockpickCount > 0)
            {
                if (lockpick.GetComponent<Env_Item>().itemCount > 99)
                {
                    txt_LockpickCount.text = "Picks left: 99+";
                }
                else
                {
                    txt_LockpickCount.text = "Picks left: " + lockpick.GetComponent<Env_Item>().itemCount;
                }
            }
            else
            {
                CloseLockpickUI();
                PauseMenuScript.isPlayerMenuOpen = false;
            }
        } 
        else
        {
            CloseLockpickUI();
            PauseMenuScript.isPlayerMenuOpen = false;
        }
    }

    //check if all tumblers have been successfully unlocked
    private void CheckLockStatus(string caller)
    {
        if (LockStatusScript.tumbler1Unlocked
            && LockStatusScript.tumbler2Unlocked
            && LockStatusScript.tumbler3Unlocked
            && LockStatusScript.tumbler4Unlocked
            && LockStatusScript.tumbler5Unlocked)
        {
            if (targetLock != null)
            {
                pick.gameObject.SetActive(false);

                timer = 0.01f;
                if (caller == "regular")
                {
                    pickStep = 100;
                }
                else if (caller == "autoattempt")
                {
                    pickStep = 50;
                }

                movingLock = true;

                LockStatusScript.isUnlocked = true;

                if (targetLock.GetComponent<UI_Inventory>() != null)
                {
                    Debug.Log("Successfully unlocked " + targetLock.GetComponent<UI_Inventory>().containerName + "!");
                }
                else if (targetLock.GetComponent<Manager_Door>() != null)
                {
                    Debug.Log("Successfully unlocked " + targetLock.GetComponent<Manager_Door>().doorType.ToString().Replace("_", " ") + "!");
                }
            }
        }
    }

    //opens the lockpick UI
    public void OpenLockpickUI(GameObject targetContainer, string lockDifficulty)
    {
        Time.timeScale = 1;

        par_Lockpicking.SetActive(true);
        pick.gameObject.SetActive(true);

        ResetPickPosition();
        SetTumblerPositions(targetContainer.GetComponent<Env_LockStatus>());

        theLock.transform.localPosition = pos_LockLocked.localPosition;
        gear1.transform.localEulerAngles = gearOriginalRotation;
        gear2.transform.localEulerAngles = gearOriginalRotation;

        txt_LockpickingTitle.text = "Unlocking " + targetContainer.name.Replace("_", " ");
        txt_LockLevel.text = "Lock level:\n" + lockDifficulty;

        int pickCount = 0;
        foreach (GameObject item in PlayerInventoryScript.playerItems)
        {
            if (item.name == "Lockpick")
            {
                pickCount = item.GetComponent<Env_Item>().itemCount;
                break;
            }
        }
        if (pickCount <= 99)
        {
            txt_LockpickCount.text = "Picks left: " + pickCount;
        }
        else
        {
            txt_LockpickCount.text = "Picks left: 99+";
        }

        int securitySkill = PlayerStatsScript.Skills["Security"];
        txt_PlayerLockPickingLevel.text = "Skill: " + securitySkill.ToString();
    }

    //closes the lockpick UI
    public void CloseLockpickUI()
    {
        if (!movingLock)
        {
            PauseMenuScript.isPlayerMenuOpen = false;

            par_Lockpicking.SetActive(false);
            par_Lockpicking.SetActive(false);
        }
    }
}