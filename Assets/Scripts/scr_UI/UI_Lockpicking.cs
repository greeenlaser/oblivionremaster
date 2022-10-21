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
    [SerializeField] private GameObject par_LockModel;
    [SerializeField] private GameObject pick;
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

    [Header("Tumbler 1")]
    [SerializeField] private GameObject tumbler1;
    [SerializeField] private GameObject spring1;
    [SerializeField] private Transform pos_Unlocked1;
    [SerializeField] private Transform pos_Locked1;

    [Header("Tumbler 2")]
    [SerializeField] private GameObject tumbler2;
    [SerializeField] private GameObject spring2;
    [SerializeField] private Transform pos_Unlocked2;
    [SerializeField] private Transform pos_Locked2;

    [Header("Tumbler 3")]
    [SerializeField] private GameObject tumbler3;
    [SerializeField] private GameObject spring3;
    [SerializeField] private Transform pos_Unlocked3;
    [SerializeField] private Transform pos_Locked3;

    [Header("Tumbler 4")]
    [SerializeField] private GameObject tumbler4;
    [SerializeField] private GameObject spring4;
    [SerializeField] private Transform pos_Unlocked4;
    [SerializeField] private Transform pos_Locked4;

    [Header("Tumbler 5")]
    [SerializeField] private GameObject tumbler5;
    [SerializeField] private GameObject spring5;
    [SerializeField] private Transform pos_Unlocked5;
    [SerializeField] private Transform pos_Locked5;

    [Header("Scripts")]
    [SerializeField] private GameObject thePlayer;

    //public but hidden variables
    [HideInInspector] public UI_Inventory TargetContainerScript;

    //private variables
    private bool goingLeft;
    private bool goingRight;
    private bool goingUp;
    private bool goingDown;
    private bool tumbler1Unlocked;
    private bool tumbler2Unlocked;
    private bool tumbler3Unlocked;
    private bool tumbler4Unlocked;
    private bool tumbler5Unlocked;
    private bool succeeded;
    private bool calledChanceOnce;
    private int finalSuccessChance;
    private int tumbler = 1;
    private int pickStep;
    private float timer;
    private Transform targetPosition;

    //scripts
    private Player_Stats PlayerStatsScript;
    private UI_Inventory PlayerInventoryScript;
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
                && !goingDown)
            {
                if (KeyBindingsScript.GetButtonDown("WalkLeft"))
                {
                    SetPickDirection("left");
                }
                else if (KeyBindingsScript.GetButtonDown("WalkRight"))
                {
                    SetPickDirection("right");
                }
                else if (KeyBindingsScript.GetButtonDown("WalkForwards"))
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
                                                              pick.transform.position.y + 0.003f,
                                                              pick.transform.position.z);

                                MoveTumblerUp();
                            }
                            else if (goingDown)
                            {
                                pick.transform.position = new(pick.transform.position.x,
                                                              pick.transform.position.y - 0.003f,
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

    //reset tumblers back to their original positions
    private void ResetTumblers()
    {
        tumbler1.transform.position = pos_Locked1.position;
        spring1.transform.localScale = new(1, 1, 1);
        tumbler1Unlocked = false;

        tumbler2.transform.position = pos_Locked2.position;
        spring2.transform.localScale = new(1, 1, 1);
        tumbler2Unlocked = false;

        tumbler3.transform.position = pos_Locked3.position;
        spring3.transform.localScale = new(1, 1, 1);
        tumbler3Unlocked = false;

        tumbler4.transform.position = pos_Locked4.position;
        spring4.transform.localScale = new(1, 1, 1);
        tumbler4Unlocked = false;

        tumbler5.transform.position = pos_Locked5.position;
        spring5.transform.localScale = new(1, 1, 1);
        tumbler5Unlocked = false;
    }

    //set some tumblers to unlocked status depeding on lock difficulty
    private void SetTumblerUnlockStatus()
    {
        int containerDifficulty = (int)TargetContainerScript.lockDifficulty;
        if (containerDifficulty == 0)
        {
            tumbler2Unlocked = true;
            tumbler2.transform.position = pos_Unlocked2.position;
            tumbler3Unlocked = true;
            tumbler3.transform.position = pos_Unlocked3.position;
            tumbler4Unlocked = true;
            tumbler4.transform.position = pos_Unlocked4.position;
            tumbler5Unlocked = true;
            tumbler5.transform.position = pos_Unlocked5.position;
        }
        else if (containerDifficulty == 1)
        {
            tumbler3Unlocked = true;
            tumbler3.transform.position = pos_Unlocked3.position;
            tumbler4Unlocked = true;
            tumbler4.transform.position = pos_Unlocked4.position;
            tumbler5Unlocked = true;
            tumbler5.transform.position = pos_Unlocked5.position;
        }
        else if (containerDifficulty == 2)
        {
            tumbler4Unlocked = true;
            tumbler4.transform.position = pos_Unlocked4.position;
            tumbler5Unlocked = true;
            tumbler5.transform.position = pos_Unlocked5.position;
        }
        else if (containerDifficulty == 3)
        {
            tumbler5Unlocked = true;
            tumbler5.transform.position = pos_Unlocked5.position;
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
        calledChanceOnce = false;
        succeeded = false;

        if (tumbler == 1
            && !tumbler1Unlocked)
        {
            tumbler1.transform.position = new(tumbler1.transform.position.x,
                                              tumbler1.transform.position.y + 0.003f,
                                              tumbler1.transform.position.z);
            spring1.transform.localScale = new(spring1.transform.localScale.x,
                                               spring1.transform.localScale.y - 0.042f,
                                               spring1.transform.localScale.z);
        }
        else if (tumbler == 2
                 && !tumbler2Unlocked)
        {
            tumbler2.transform.position = new(tumbler2.transform.position.x,
                                              tumbler2.transform.position.y + 0.003f,
                                              tumbler2.transform.position.z);
            spring2.transform.localScale = new(spring2.transform.localScale.x,
                                               spring2.transform.localScale.y - 0.042f,
                                               spring2.transform.localScale.z);
        }
        else if (tumbler == 3
                 && !tumbler3Unlocked)
        {
            tumbler3.transform.position = new(tumbler3.transform.position.x,
                                              tumbler3.transform.position.y + 0.003f,
                                              tumbler3.transform.position.z);
            spring3.transform.localScale = new(spring3.transform.localScale.x,
                                               spring3.transform.localScale.y - 0.042f,
                                               spring3.transform.localScale.z);
        }
        else if (tumbler == 4
                 && !tumbler4Unlocked)
        {
            tumbler4.transform.position = new(tumbler4.transform.position.x,
                                              tumbler4.transform.position.y + 0.003f,
                                              tumbler4.transform.position.z);
            spring4.transform.localScale = new(spring4.transform.localScale.x,
                                               spring4.transform.localScale.y - 0.042f,
                                               spring4.transform.localScale.z);
        }
        else if (tumbler == 5
                 && !tumbler5Unlocked)
        {
            tumbler5.transform.position = new(tumbler5.transform.position.x,
                                              tumbler5.transform.position.y + 0.003f,
                                              tumbler5.transform.position.z);
            spring5.transform.localScale = new(spring5.transform.localScale.x,
                                               spring5.transform.localScale.y - 0.042f,
                                               spring5.transform.localScale.z);
        }
    }
    private void MoveTumblerDown()
    {
        if (!calledChanceOnce)
        {
            finalSuccessChance = Random.Range(1, 80) + PlayerStatsScript.Skills["Security"];
            if (finalSuccessChance >= 75)
            {
                Debug.Log("lockpicking succeedeed with tumbler nr " + tumbler + " at chance of " + finalSuccessChance + " out of 75!");
                succeeded = true;
            }
            else
            {
                Debug.Log("lockpicking failed with tumbler nr " + tumbler + " at chance of " + finalSuccessChance + " out of 75...");
                FailedAttempt();
            }
            calledChanceOnce = true;
        }

        if (tumbler == 1
            && !tumbler1Unlocked)
        {
            if (succeeded)
            {
                tumbler1Unlocked = true;

                CheckLockStatus();
            }
            else
            {
                tumbler1.transform.position = new(tumbler1.transform.position.x,
                                                  tumbler1.transform.position.y - 0.003f,
                                                  tumbler1.transform.position.z);
                spring1.transform.localScale = new(spring1.transform.localScale.x,
                                                   spring1.transform.localScale.y + 0.042f,
                                                   spring1.transform.localScale.z);
            }
        }
        else if (tumbler == 2
                 && !tumbler2Unlocked)
        {
            if (succeeded)
            {
                tumbler2Unlocked = true;

                CheckLockStatus();
            }
            else
            {
                tumbler2.transform.position = new(tumbler2.transform.position.x,
                                                  tumbler2.transform.position.y - 0.003f,
                                                  tumbler2.transform.position.z);
                spring2.transform.localScale = new(spring2.transform.localScale.x,
                                                   spring2.transform.localScale.y + 0.042f,
                                                   spring2.transform.localScale.z);
            }
        }
        else if (tumbler == 3
                 && !tumbler3Unlocked)
        {
            if (succeeded)
            {
                tumbler3Unlocked = true;

                CheckLockStatus();
            }
            else
            {
                tumbler3.transform.position = new(tumbler3.transform.position.x,
                                                  tumbler3.transform.position.y - 0.003f,
                                                  tumbler3.transform.position.z);
                spring3.transform.localScale = new(spring3.transform.localScale.x,
                                                   spring3.transform.localScale.y + 0.042f,
                                                   spring3.transform.localScale.z);
            }
        }
        else if (tumbler == 4
                 && !tumbler4Unlocked)
        {
            if (succeeded)
            {
                tumbler4Unlocked = true;

                CheckLockStatus();
            }
            else
            {
                tumbler4.transform.position = new(tumbler4.transform.position.x,
                                                  tumbler4.transform.position.y - 0.003f,
                                                  tumbler4.transform.position.z);
                spring4.transform.localScale = new(spring4.transform.localScale.x,
                                                   spring4.transform.localScale.y + 0.042f,
                                                   spring4.transform.localScale.z);
            }
        }
        else if (tumbler == 5
                 && !tumbler5Unlocked)
        {
            if (succeeded)
            {
                tumbler5Unlocked = true;

                CheckLockStatus();
            }
            else
            {
                tumbler5.transform.position = new(tumbler5.transform.position.x,
                                                  tumbler5.transform.position.y - 0.003f,
                                                  tumbler5.transform.position.z);
                spring5.transform.localScale = new(spring5.transform.localScale.x,
                                                   spring5.transform.localScale.y + 0.042f,
                                                   spring5.transform.localScale.z);
            }
        }
    }

    //auto-attempt to unlock the lock
    public void AutoAttempt()
    {
        
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
    private void CheckLockStatus()
    {
        if (tumbler1Unlocked
            && tumbler2Unlocked
            && tumbler3Unlocked
            && tumbler4Unlocked
            && tumbler5Unlocked)
        {
            if (TargetContainerScript != null)
            {
                Debug.Log("Successfully unlocked " + TargetContainerScript.name.Replace("_", " ") + "!");

                TargetContainerScript.isLocked = false;
                CloseLockpickUI();
            }
        }
    }

    //opens the lockpick UI
    public void OpenLockpickUI(string targetContainerName, string lockDifficulty)
    {
        ResetTumblers();
        SetTumblerUnlockStatus();
        ResetPickPosition();

        par_Lockpicking.SetActive(true);
        par_LockModel.SetActive(true);
        txt_LockpickingTitle.text = "Unlocking " + targetContainerName.Replace("_", " ");
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
        PauseMenuScript.isPlayerMenuOpen = false;

        par_Lockpicking.SetActive(false);
        par_LockModel.SetActive(false);
    }

    //when the player successfully unlocks the target container
    public void UnlockContainer()
    {
        CloseLockpickUI();
    }
}