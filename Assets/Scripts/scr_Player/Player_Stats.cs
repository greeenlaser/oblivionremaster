using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    [Header("Player camera")]
    [SerializeField] private int defaultCameraMoveSpeed = 50;
    [HideInInspector] public int cameraMoveSpeed;
    [SerializeField] private float defaultFieldOfView = 90;
    [HideInInspector] public float fieldOfView;

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

    [Header("Scripts")]
    [SerializeField] private GameObject par_Managers;

    public void ResetStats()
    {
        walkSpeed = defaultWalkSpeed;
        sprintSpeed = defaultSprintSpeed;
        crouchSpeed = defaultCrouchSpeed;
        jumpHeight = defaultJumpHeight;
        cameraMoveSpeed = defaultCameraMoveSpeed;
        fieldOfView = defaultFieldOfView;
        GetComponentInChildren<Camera>().fieldOfView = fieldOfView;
        GetComponentInChildren<Player_Camera>().sensX = cameraMoveSpeed;
        GetComponentInChildren<Player_Camera>().sensY = cameraMoveSpeed;

        maxHealth = defaultMaxHealth;
        currentHealth = defaultMaxHealth;
        maxStamina = defaultMaxStamina;
        currentStamina = defaultMaxStamina;
        maxMagicka = defaultMaxMagicka;
        currentMagicka = defaultMaxMagicka;

        maxInvSpace = defaultMaxInvSpace;
        invSpace = 0;
    }
}