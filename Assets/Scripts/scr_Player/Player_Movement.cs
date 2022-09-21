using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Movement : MonoBehaviour
{
    [Header("Assignables")]
    [SerializeField] private LayerMask groundMask;

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
    private Player_Stats PlayerStatsScript;
    //private float minVelocity;

    private void Awake()
    {
        PlayerStatsScript = GetComponent<Player_Stats>();

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

        LoadPlayer();
    }

    //load player data at the beginning of the game
    private void LoadPlayer()
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

            PlayerRegularMovement();
        }
    }

    private void PlayerRegularMovement()
    {
        /*
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
            if (minVelocity <= -25f
                && PlayerHealthScript.canTakeDamage)
            {
                ApplyFallDamage();
                minVelocity = -2f;
            }

            velocity.y = -2f;
        }
        */

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

        //sprinting
        if (Input.GetKeyDown(KeyCode.LeftShift)
            && canSprint)
        {
            isSprinting = true;
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            isSprinting = false;
        }
        if (isSprinting
            && horizontalVelocity.magnitude > 0.3f)
        {
            //Debug.Log("Player is sprinting!");

            currentSpeed = PlayerStatsScript.sprintSpeed;

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
        else if (!isSprinting)
        {
            if (!isCrouching)
            {
                currentSpeed = PlayerStatsScript.walkSpeed;
            }
        }

        //jumping
        if (Input.GetKey(KeyCode.Space)
            && isGrounded
            && !isJumping
            && canJump)
        {
            velocity.y = Mathf.Sqrt(PlayerStatsScript.jumpHeight * -5.2f * gravity);
            CharacterController.stepOffset = 0;
            isJumping = true;
        }
        else if (isGrounded
                 && isJumping)
        {
            CharacterController.stepOffset = 0.3f;
            isJumping = false;
        }

        //crouching
        if (Input.GetKeyDown(KeyCode.LeftControl)
            && isGrounded
            && canCrouch)
        {
            isCrouching = !isCrouching;

            if (isSprinting)
            {
                isSprinting = false;
            }

            if (isCrouching)
            {
                //Debug.Log("Player is crouching!");

                currentSpeed = PlayerStatsScript.crouchSpeed;

                CharacterController.height = originalHeight / 2;

                PlayerCamera.transform.localPosition = PlayerStatsScript.cameraCrouchHeight;
            }
            else if (!isCrouching)
            {
                //Debug.Log("Player is no longer crouching...");

                currentSpeed = PlayerStatsScript.walkSpeed;

                CharacterController.height = originalHeight;

                PlayerCamera.transform.localPosition = PlayerStatsScript.cameraWalkHeight;
            }
        }
    }

    /*
    //deal damage based off of velocity when hitting ground
    private void ApplyFallDamage()
    {
        float damageDealt = Mathf.Round(Mathf.Abs(velocity.y * 1.2f) * 10) / 10;

        PlayerHealthScript.DealDamage("Ground", "gravity", damageDealt);
    }
    */
}