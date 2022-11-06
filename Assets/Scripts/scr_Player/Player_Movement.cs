using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Movement : MonoBehaviour
{
    [Header("Assignables")]
    [SerializeField] private LayerMask groundMask;

    [Header("Scripts")]
    [SerializeField] private GameObject par_Managers;

    //public but hidden variables
    [HideInInspector] public bool canMove;
    [HideInInspector] public bool canSprint;
    [HideInInspector] public bool isSprinting;
    [HideInInspector] public bool isGrounded;
    [HideInInspector] public bool canJump;
    [HideInInspector] public bool isJumping;
    [HideInInspector] public bool canCrouch;
    [HideInInspector] public bool isCrouching;
    [HideInInspector] public Vector3 velocity;

    //private variables
    private readonly float gravity = -9.81f;
    private float originalHeight;
    private float currentSpeed;
    private Camera PlayerCamera;
    private CharacterController CharacterController;
    private GameObject checkSphere;
    private float minVelocity;

    //scripts
    private Player_Stats PlayerStatsScript;
    private Manager_KeyBindings KeyBindingsScript;
    private Manager_DealEffect EffectManagerScript;

    private void Awake()
    {
        PlayerStatsScript = GetComponent<Player_Stats>();
        KeyBindingsScript = par_Managers.GetComponent<Manager_KeyBindings>();
        EffectManagerScript = par_Managers.GetComponent<Manager_DealEffect>();

        PlayerCamera = GetComponentInChildren<Camera>();
        CharacterController = GetComponent<CharacterController>();
        foreach (Transform child in transform)
        {
            if (child.name == "pos_CheckSphere")
            {
                checkSphere = child.gameObject;
                break;
            }
        }
    }

    //load player data at the beginning of the game
    public void LoadPlayer()
    {
        currentSpeed = PlayerStatsScript.walkSpeed;
        canMove = true;
        canSprint = true;
        canCrouch = true;
        canJump = true;

        originalHeight = CharacterController.height;
        PlayerCamera.transform.localPosition = PlayerStatsScript.cameraWalkHeight;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (canMove)
        {
            //check if player is grounded
            if (Physics.CheckSphere(checkSphere.transform.position,
                                    0.4f,
                                    groundMask))
            {
                isGrounded = true;
            }
            else
            {
                isGrounded = false;
            }

            if (PlayerStatsScript.currentHealth > 0)
            {
                PlayerRegularMovement();
            }
        }
    }

    private void PlayerRegularMovement()
    {
        //gravity if player is grounded
        if (velocity.y < 0
            && isGrounded)
        {
            //get smallest velocity
            if (velocity.y < minVelocity)
            {
                minVelocity = velocity.y;
            }

            //check if smallest velocity is less than or equal to -25f
            if (minVelocity <= -25f)
                //&& PlayerHealthScript.canTakeDamage)
            {
                ApplyFallDamage();
                minVelocity = -2f;
            }

            velocity.y = -2f;
        }

        //gravity if player isnt grounded
        if (!isGrounded)
        {
            velocity.y += gravity * Time.deltaTime * 4f;
        }

        //movement input
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        move = Vector3.ClampMagnitude(move, 1);

        //first movement update based on speed and input
        CharacterController.Move(currentSpeed * Time.deltaTime * move);

        //final movement update based on velocity
        CharacterController.Move(velocity * Time.deltaTime);

        //get all velocity of the controller
        Vector3 horizontalVelocity = transform.right * x + transform.forward * z;

        //enable/disable sprinting
        if (KeyBindingsScript.GetButtonDown("Sprint")
            && PlayerStatsScript.currentStamina >= Time.deltaTime * 10)
        {
            isSprinting = true;
        }
        if (isSprinting
            && horizontalVelocity.magnitude > 0.3f)
        {
            //Debug.Log("Player is sprinting!");

            currentSpeed = PlayerStatsScript.sprintSpeed;
            PlayerStatsScript.currentStamina -= Time.deltaTime * 10;
            PlayerStatsScript.UpdateBar(PlayerStatsScript.staminaBar);

            if (isCrouching)
            {
                isCrouching = false;

                CharacterController.height = originalHeight;

                PlayerCamera.transform.localPosition = PlayerStatsScript.cameraWalkHeight;
            }
        }
        //force-disables sprinting if the player is no longer moving but still holding down sprint key
        else if (isSprinting
                 && horizontalVelocity.magnitude < 0.3f)
        {
            isSprinting = false;
        }
        else if (!isSprinting
                 && !isJumping)
        {
            if (PlayerStatsScript.currentStamina <= PlayerStatsScript.maxStamina)
            {
                if (horizontalVelocity.magnitude < 0.3f)
                {
                    PlayerStatsScript.currentStamina += Time.deltaTime * 2;
                }
                else
                {
                    PlayerStatsScript.currentStamina += Time.deltaTime * 0.5f;
                }
                PlayerStatsScript.UpdateBar(PlayerStatsScript.staminaBar);
            }

            if (!isCrouching)
            {
                currentSpeed = PlayerStatsScript.walkSpeed;
            }
        }

        //enable/disable jumping
        if (KeyBindingsScript.GetButtonDown("Jump")
            && isGrounded
            && !isJumping
            && canJump
            && PlayerStatsScript.currentStamina >= 10)
        {
            PlayerStatsScript.currentStamina -= 10;
            PlayerStatsScript.UpdateBar(PlayerStatsScript.staminaBar);

            velocity.y = Mathf.Sqrt(PlayerStatsScript.jumpHeight * -5.2f * gravity);
            CharacterController.stepOffset = 0;
            isJumping = true;
        }
        //stop jumping
        if (isGrounded
             && isJumping)
        {
            CharacterController.stepOffset = 0.3f;
            isJumping = false;
        }

        //enable/disable crouching
        if (KeyBindingsScript.GetButtonDown("Crouch"))
        {
            if (isGrounded
                && canCrouch)
            {
                isCrouching = !isCrouching;

                if (isSprinting)
                {
                    isSprinting = false;
                }

                if (isCrouching)
                {
                    currentSpeed = PlayerStatsScript.crouchSpeed;

                    CharacterController.height = originalHeight / 2;

                    PlayerCamera.transform.localPosition = PlayerStatsScript.cameraCrouchHeight;
                }
                else if (!isCrouching)
                {
                    currentSpeed = PlayerStatsScript.walkSpeed;

                    CharacterController.height = originalHeight;

                    PlayerCamera.transform.localPosition = PlayerStatsScript.cameraWalkHeight;
                }
            }
        }
    }

    //deal damage based off of velocity when hitting ground
    private void ApplyFallDamage()
    {
        float damageDealt = Mathf.Round(Mathf.Abs(velocity.y * 1.2f) * 10) / 10;

        EffectManagerScript.DealEffect(null,
                                       gameObject,
                                       "drainHealth",
                                       damageDealt,
                                       0);
    }
}