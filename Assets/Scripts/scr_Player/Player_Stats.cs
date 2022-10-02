using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player_Stats : MonoBehaviour
{
    [Header("Player movement")]
    [SerializeField] private int defaultWalkSpeed = 4;
    [HideInInspector] public int walkSpeed;
    [SerializeField] private int defaultSprintSpeed = 8;
    [HideInInspector] public int sprintSpeed;
    [SerializeField] private int defaultCrouchSpeed = 2;
    [HideInInspector] public int crouchSpeed;
    [SerializeField] private float defaultJumpHeight = 1.75f;
    [HideInInspector] public float jumpHeight;
    [HideInInspector] public Vector3 cameraWalkHeight = new(0, 0.6f, 0);
    [HideInInspector] public Vector3 cameraCrouchHeight = new(0, 0.3f, 0);

    [Header("Combat stats")]
    [SerializeField] private float defaultMaxHealth = 100;
    [HideInInspector] public float maxHealth;
    [HideInInspector] public float currentHealth;
    [SerializeField] private float defaultMaxStamina = 100;
    [HideInInspector] public float maxStamina;
    [HideInInspector] public float currentStamina;
    [SerializeField] private float defaultMaxMagicka = 100;
    [HideInInspector] public float maxMagicka;
    [HideInInspector] public float currentMagicka;

    [Header("Inventory stats")]
    [SerializeField] private int defaultMaxInvSpace = 100;
    [HideInInspector] public int maxInvSpace;
    [HideInInspector] public int invSpace;

    [Header("Main player UI")]
    public Slider healthBar;
    public Slider staminaBar;
    public Slider magickaBar;

    [Header("Scripts")]
    [SerializeField] private GameObject par_Managers;

    public void ResetStats()
    {
        walkSpeed = defaultWalkSpeed;
        sprintSpeed = defaultSprintSpeed;
        crouchSpeed = defaultCrouchSpeed;
        jumpHeight = defaultJumpHeight;

        maxHealth = defaultMaxHealth;
        currentHealth = defaultMaxHealth;
        maxStamina = defaultMaxStamina;
        currentStamina = defaultMaxStamina;
        maxMagicka = defaultMaxMagicka;
        currentMagicka = defaultMaxMagicka;

        maxInvSpace = defaultMaxInvSpace;
        invSpace = 0;

        UpdateBar(healthBar);
        UpdateBar(staminaBar);
        UpdateBar(magickaBar);
    }

    //update players health/stamina/magicka bar UI
    public void UpdateBar(Slider bar)
    {
        if (bar == healthBar)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }
        else if (bar == staminaBar)
        {
            staminaBar.maxValue = maxStamina;
            staminaBar.value = currentStamina;
        }
        else if (bar == magickaBar)
        {
            magickaBar.maxValue = maxMagicka;
            magickaBar.value = currentMagicka;
        }
    }
}