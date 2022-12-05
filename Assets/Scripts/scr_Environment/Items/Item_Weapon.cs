using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_Weapon : MonoBehaviour
{
    [Header("General")]
    public int baseDamage;
    [Range(0, 100)]
    public int maxWeaponHealth;
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
    [SerializeField] private float animationLength;
    [SerializeField] private Vector3 forcedRotation;

    //public but hidden variables
    [HideInInspector] public bool isCallingMainAttack;
    [HideInInspector] public bool isUsing;
    [HideInInspector] public bool isAiming;
    [HideInInspector] public bool isBlocking;
    [HideInInspector] public bool isReloading;
    [HideInInspector] public float weaponHealth;
    [HideInInspector] public GameObject instantiatedWeapon;

    //scripts
    private GameObject thePlayer;
    private GameObject par_Managers;
    private Player_Stats PlayerStatsScript;
    private UI_PlayerMenu PlayerMenuScript;
    private Manager_KeyBindings KeyBindingsScript;
    private UI_PauseMenu PauseMenuScript;

    //private variables
    private bool calledResetOnce;
    private readonly float timeToHeavyAttack = 0.2f;
    private float swingTimer;
    private Animator weaponAnimator;

    private void Awake()
    {
        thePlayer = GameObject.Find("Player");
        par_Managers = GameObject.Find("par_Managers");

        PlayerStatsScript = thePlayer.GetComponent<Player_Stats>();
        PlayerMenuScript = par_Managers.GetComponent<UI_PlayerMenu>();
        KeyBindingsScript = par_Managers.GetComponent<Manager_KeyBindings>();
        PauseMenuScript = par_Managers.GetComponent<UI_PauseMenu>();

        weaponHealth = maxWeaponHealth;
    }

    private void Update()
    {
        if (!PauseMenuScript.isPaused
            && !PlayerMenuScript.isPlayerInventoryOpen
            && PlayerStatsScript.currentHealth > 0
            && GetComponent<Env_Item>().isEquipped)
        {
            if (instantiatedWeapon != null
                && weaponAnimator == null)
            {
                weaponAnimator = instantiatedWeapon.GetComponentInChildren<Animator>();
            }
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
                                StartCoroutine(SwingWeapon("swing_Heavy"));
                            }
                        }
                        else if (!isCallingMainAttack
                                 && swingTimer > 0)
                        {
                            StartCoroutine(SwingWeapon("swing_Light"));
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

    //swing the weapon
    private IEnumerator SwingWeapon(string state)
    {
        isUsing = true;

        weaponAnimator.Play(state, 0, 0.0f);

        yield return new WaitForSeconds(animationLength);

        ResetWeaponState();
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

        swingTimer = 0;

        if (!calledResetOnce)
        {
            calledResetOnce = true;
        }
    }
}