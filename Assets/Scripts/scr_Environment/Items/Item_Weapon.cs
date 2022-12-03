using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Item_Weapon : MonoBehaviour
{
    [Range(0, 10000)]
    [Header("General")]
    public int baseDamage;
    [Range(0, 100)]
    public int weaponHealth;
    public EquipType equipType;
    public enum EquipType
    {
        singleHanded,
        doubleHanded
    }
    public WeaponType weaponType;
    public enum WeaponType
    {
        sword,
        mace,
        axe,
        staff,
        bow,
        crossbow
    }
    public Texture img_ItemLogo;
    public Transform pos_WeaponHold;

    [Header("Melee weapon variables")]
    [Range(0, 1)]
    [SerializeField] private float swingRotationOrigin;
    [Range(0, 1)]
    [SerializeField] private float swingRotationTarget;

    [Header("Scripts")]
    [SerializeField] private GameObject thePlayer;
    [SerializeField] private GameObject par_Managers;

    //public but hidden variables
    [HideInInspector] public bool isCallingMainAttack;
    [HideInInspector] public bool isUsing;
    [HideInInspector] public bool isAiming;
    [HideInInspector] public bool isBlocking;
    [HideInInspector] public bool isReloading;

    //private variables
    private bool calledResetOnce;
    private float step;

    //swing
    private bool swingStart;
    private bool returnWeapon;
    private bool swingEnd;
    private bool lightSwing;
    private bool heavySwing;
    private float swingTimer;
    private readonly float swingSpeed = 150;
    private readonly float timeToHeavyAttack = 0.2f;

    //scripts
    private Player_Stats PlayerStatsScript;
    private UI_PlayerMenu PlayerMenuScript;
    private Manager_KeyBindings KeyBindingsScript;
    private UI_PauseMenu PauseMenuScript;

    private void Awake()
    {
        PlayerStatsScript = thePlayer.GetComponent<Player_Stats>();
        PlayerMenuScript = par_Managers.GetComponent<UI_PlayerMenu>();
        KeyBindingsScript = par_Managers.GetComponent<Manager_KeyBindings>();
        PauseMenuScript = par_Managers.GetComponent<UI_PauseMenu>();
    }

    private void Update()
    {
        if (!PauseMenuScript.isPaused
            && !PlayerMenuScript.isPlayerInventoryOpen
            && PlayerStatsScript.currentHealth > 0
            && GetComponent<Env_Item>().isEquipped)
        {
            //all melee weapons
            if (weaponType == WeaponType.sword
                || weaponType == WeaponType.mace
                || weaponType == WeaponType.axe)
            {
                //melee attack
                if (!isBlocking)
                {
                    if (!isUsing)
                    {
                        if (calledResetOnce)
                        {
                            calledResetOnce = false;
                        }

                        isCallingMainAttack = KeyBindingsScript.GetKey("MainAttack");
                        if (isCallingMainAttack)
                        {
                            swingTimer += Time.deltaTime;
                            if (swingTimer >= timeToHeavyAttack)
                            {
                                heavySwing = true;
                                isUsing = true;
                                isCallingMainAttack = false;
                            }
                        }
                        else
                        {
                            if (swingTimer > 0)
                            {
                                lightSwing = true;
                                isUsing = true;
                            }
                        }
                    }
                    else
                    {
                        if (lightSwing)
                        {
                            step = swingSpeed * Time.deltaTime;
                        }
                        else if (heavySwing)
                        {
                            step = swingSpeed * 1.5f * Time.deltaTime;
                        }

                        MeleeAttackAnimation();
                    }
                }
                //can only block with singlehanded melee weapons
                else if (!isCallingMainAttack
                         && !isUsing
                         && equipType == EquipType.singleHanded)
                {
                    isBlocking = KeyBindingsScript.GetKey("SideAttack");
                    if (isBlocking)
                    {
                        if (calledResetOnce)
                        {
                            calledResetOnce = false;
                        }

                        Block();
                    }
                    else
                    {
                        ResetWeaponState();
                    }
                }
            }
        }
    }

    //melee weapon swing animation
    private void MeleeAttackAnimation()
    {
        float weaponRotation = pos_WeaponHold.transform.localRotation.z;

        //check if weapon can rotate towards target rotation
        if (!swingStart
            && !returnWeapon
            && !swingEnd)
        {
            swingStart = true;
        }

        //start rotating weapon towards target rotation
        if (swingStart)
        {
            pos_WeaponHold.Rotate(new Vector3(0, 0, 1) * step);

            if (weaponRotation >= swingRotationTarget)
            {
                returnWeapon = true;
            }
        }

        //check if weapon can rotate towards original rotation
        if (swingStart
            && returnWeapon
            && !swingEnd)
        {
            swingStart = false;
            returnWeapon = false;
            swingEnd = true;
        }

        //return weapon rotation to original rotation
        if (swingEnd)
        {
            pos_WeaponHold.Rotate(new Vector3(0, 0, -1) * step);

            if (weaponRotation <= swingRotationOrigin)
            {
                ResetWeaponState();
            }
        }
    }
    //block with the melee weapon
    private void Block()
    {
        
    }

    /*
    //shoot a bow or crossbow
    private void Shoot()
    {

    }
    //aim with a bow or crossbow
    private void Aim()
    {

    }
    //reload a crossbow
    private void Reload()
    {

    }

    //cast a spell with staff
    private void Cast()
    {

    }
    */

    //reset weapon state if user was
    //swinging/blocking/shooting/aiming/reloading or casting
    public void ResetWeaponState()
    {
        isCallingMainAttack = false;
        isUsing = false;
        isAiming = false;
        isBlocking = false;
        isReloading = false;

        swingStart = false;
        returnWeapon = false;
        swingEnd = false;
        lightSwing = false;
        heavySwing = false;

        swingTimer = 0;

        if (!calledResetOnce)
        {
            calledResetOnce = true;
        }

        transform.localRotation = Quaternion.Euler(Vector3.zero);
    }
}