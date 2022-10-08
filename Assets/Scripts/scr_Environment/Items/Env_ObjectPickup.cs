using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Env_ObjectPickup : MonoBehaviour
{
    [Tooltip("Max distance from where the object can be picked up.")]
    [Range(1f, 5f)]
    public float maxDistance = 3f;
    [Tooltip("How strong is the throwing force?")]
    [Range(10f, 30f)]
    [SerializeField] private float throwForce = 15f;
    [Range(1f, 10f)]
    public float speedLimit = 5;

    [Header("Assignables")]
    [SerializeField] private GameObject thePlayer;
    [SerializeField] private GameObject par_Managers;

    //public but hidden variables
    [HideInInspector] public bool isHolding;

    //private variables
    private bool isColliding;
    private bool isGrounded;
    private float distanceToGround;
    private Collider theCollider;
    private Rigidbody rb;
    private UI_Inventory PlayerInventoryScript;
    private Player_Raycast PlayerRaycastScript;

    private void Awake()
    {
        distanceToGround = GetComponent<Collider>().bounds.extents.y;
        rb = GetComponent<Rigidbody>();

        PlayerInventoryScript = thePlayer.GetComponent<UI_Inventory>();
        PlayerRaycastScript = thePlayer.GetComponentInChildren<Player_Raycast>();
    }

    private void FixedUpdate()
    {
        if (rb.velocity.magnitude > speedLimit)
        {
            rb.velocity = rb.velocity.normalized * speedLimit;
        }

        if (isHolding)
        {
            if (gameObject.layer != LayerMask.NameToLayer("LimitedCollision"))
            {
                isGrounded = false;

                gameObject.layer = LayerMask.NameToLayer("LimitedCollision");
            }

            PlayerInventoryScript.heldObject = gameObject;

            if (rb.useGravity)
            {
                rb.useGravity = false;
            }
            if (gameObject.GetComponent<Env_Item>().droppedObject)
            {
                gameObject.GetComponent<Env_Item>().droppedObject = false;
            }

            Vector3 targetPoint = PlayerRaycastScript.pos_HoldItem.transform.position;
            targetPoint += PlayerRaycastScript.pos_HoldItem.transform.forward;
            Vector3 force = targetPoint - rb.transform.position;
            rb.velocity = force.normalized * rb.velocity.magnitude;

            if (isColliding && theCollider != null)
            {
                rb.freezeRotation = false;
                //Debug.Log(name + " is colliding with " + theCollider.name + "!");
            }
            else if (!isColliding)
            {
                rb.freezeRotation = true;
            }

            rb.AddForce(force * 5000);

            rb.velocity *= Mathf.Min(1.0f, force.magnitude / 2);

            //drops held object if player is too far from it
            if (Vector3.Distance(transform.position, PlayerRaycastScript.pos_HoldItem.transform.position) > 3)
            {
                DropObject();
                //Debug.Log("Dropped " + gameObject.GetComponent<Env_Item>().str_fakeName + " because player went too far from it!");
            }
            //throws held object if player presses right mouse button
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                DropObject();
                rb.AddForce(200 * throwForce * PlayerRaycastScript.pos_HoldItem.forward);
            }
        }
        else if (!isHolding)
        {
            if (!isGrounded
                && Physics.Raycast(transform.position,
                                   -Vector3.up,
                                   distanceToGround + 0.1f))
            {
                isGrounded = true;
            }
            else
            {
                isGrounded = false;
            }

            if (gameObject.layer != LayerMask.NameToLayer("Ground")
                && isGrounded)
            {
                gameObject.layer = LayerMask.NameToLayer("Ground");
            }

            rb.useGravity = true;
            rb.freezeRotation = false;
            PlayerInventoryScript.heldObject = null;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isColliding)
        {
            theCollider = collision.collider;
            isColliding = true;
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        isColliding = false;
        theCollider = null;
    }

    public void DropObject()
    {
        rb.useGravity = true;
        isColliding = false;
        isHolding = false;
        GetComponent<Env_Item>().droppedObject = true;
    }
}