using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Anim_Weapon : MonoBehaviour
{
    //public but hidden variables
    [HideInInspector] public GameObject targetWeapon;
    [HideInInspector] public Animator targetController;

    //scripts
    private UI_Inventory PlayerInventoryScript;
    private Item_Weapon WeaponScript;

    //private variables
    private GameObject thePlayer;

    private void Start()
    {
        thePlayer = GameObject.Find("Player");
        PlayerInventoryScript = thePlayer.GetComponent<UI_Inventory>();
        WeaponScript = PlayerInventoryScript.equippedWeapon.GetComponent<Item_Weapon>();
        targetWeapon = WeaponScript.instantiatedWeapon;
        targetController = targetWeapon.GetComponentInChildren<Animator>();
    }

    //start this script ienumerator from outside this script
    public void StartAnimation(string animation, float length)
    {
        StartCoroutine(Animation(animation, length));
    }

    //play selected animation
    private IEnumerator Animation(string animation, float length)
    {
        //block animation does not trigger the isUsing bool
        if (animation != "block"
            && animation != "unBlock"
            && animation != "unsheath"
            && animation != "sheath")
        {
            WeaponScript.isUsing = true;

            targetController.Play(animation, 0, 0.0f);

            yield return new WaitForSeconds(length);

            WeaponScript.ResetWeaponState();
        }
        else if (animation == "block"
                 || animation == "unBlock")
        {
            if (animation == "block")
            {
                WeaponScript.isBlocking = true;

                targetController.Play(animation, 0, 0.0f);
            }
            else if (animation == "unblock")
            {
                targetController.Play(animation, 0, 0.0f);

                WeaponScript.isBlocking = false;
            }
        }
        else if (animation == "unsheath"
                 || animation == "sheath")
        {
            PlayerInventoryScript.isSheathingUnsheathingWeapon = true;

            if (animation == "unsheath")
            {
                targetController.Play(animation, 0, 0.0f);

                yield return new WaitForSeconds(length);

                PlayerInventoryScript.UseItem(PlayerInventoryScript.lastEquippedWeapon);

                PlayerInventoryScript.isWeaponUnsheathed = true;
            }
            else if (animation == "sheath")
            {
                targetController.Play(animation, 0, 0.0f);

                yield return new WaitForSeconds(length);

                PlayerInventoryScript.isWeaponUnsheathed = false;

                PlayerInventoryScript.UseItem(PlayerInventoryScript.lastEquippedWeapon);
            }

            PlayerInventoryScript.isSheathingUnsheathingWeapon = false;
        }
    }
}