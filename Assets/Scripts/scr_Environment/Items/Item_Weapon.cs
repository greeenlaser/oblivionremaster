using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_Weapon : MonoBehaviour
{
    [Header("Default values")]
    public int damage_Default;

    [Header("General")]
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
    public GameObject templateAnimatedWeapon;
    [SerializeField] private Vector3 forcedRotation;

    //public but hidden variables
    [HideInInspector] public bool isCallingMainAttack;
    [HideInInspector] public bool isUsing;
    [HideInInspector] public bool isAiming;
    [HideInInspector] public bool isBlocking;
    [HideInInspector] public bool isReloading;
    [HideInInspector] public bool isCasting;
    [HideInInspector] public GameObject instantiatedWeapon;
    [HideInInspector] public Anim_Weapon WeaponAnimationScript;
    [HideInInspector] public Animator weaponAnimator;
    [HideInInspector] public Env_Effect AppliedEnchantmentScript;

    //current values
    [HideInInspector] public int damage_Current;

    //scripts
    private Player_Stats PlayerStatsScript;
    private UI_PlayerMenu PlayerMenuScript;
    private Manager_KeyBindings KeyBindingsScript;
    private UI_PauseMenu PauseMenuScript;

    //private variables
    private GameObject thePlayer;
    private GameObject par_Managers;
    private bool calledResetOnce;
    private readonly float timeToHeavyAttack = 0.2f;
    private float swingTimer;

    private void Awake()
    {
        thePlayer = GameObject.Find("Player");
        par_Managers = GameObject.Find("par_Managers");

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
            if (instantiatedWeapon.transform.localRotation != Quaternion.Euler(forcedRotation))
            {
                instantiatedWeapon.transform.localRotation = Quaternion.Euler(forcedRotation);
            }

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

                        isCallingMainAttack = Input.GetKey(KeyCode.Mouse0);
                        if (isCallingMainAttack)
                        {
                            swingTimer += Time.deltaTime;
                            if (swingTimer >= timeToHeavyAttack)
                            {
                                WeaponAnimationScript.StartAnimation("swing_Heavy", 2);
                            }
                        }
                        else if (!isCallingMainAttack
                                 && swingTimer > 0)
                        {
                            WeaponAnimationScript.StartAnimation("swing_Light", 2);
                        }
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

                        WeaponAnimationScript.StartAnimation("block", 0.5f);
                    }
                    else
                    {
                        WeaponAnimationScript.StartAnimation("unblock", 0.5f);
                        ResetWeaponState();
                    }
                }
            }
        }
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

        swingTimer = 0;

        if (!calledResetOnce)
        {
            calledResetOnce = true;
        }
    }
}