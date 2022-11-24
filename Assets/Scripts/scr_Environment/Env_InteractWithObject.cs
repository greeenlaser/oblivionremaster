using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Env_InteractWithObject : MonoBehaviour
{
    [Header("General assignables")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private ObjectType objectType;
    [SerializeField] private enum ObjectType
    {
        button,
        lever
    }

    [Header("Button")]
    [SerializeField] private Transform par_ButtonCenter;
    [SerializeField] private Transform pos_ButtonDisabled;
    [SerializeField] private Transform pos_ButtonEnabled;

    [Header("Lever")]
    [SerializeField] private Transform par_RotationCenter;

    [Header("Scripts")]
    [SerializeField] private Env_Door DoorScript;

    //private variables
    private bool isObjectEnabled;
    private bool isObjectMoving;
    private readonly float openAngle = -0.4f;
    private readonly float closedAngle = 0.4f;

    private void Update()
    {
        if (isObjectMoving)
        {
            if (!isObjectEnabled)
            {
                float step = moveSpeed * Time.deltaTime;
                switch (objectType)
                {
                    case ObjectType.button:
                        float buttonDistance = Vector3.Distance(pos_ButtonEnabled.position, par_ButtonCenter.position);
                        par_ButtonCenter.position = Vector3.MoveTowards(par_ButtonCenter.position, par_ButtonCenter.position + new Vector3(0.001f, 0, 0), step);

                        if (buttonDistance <= 0.001f)
                        {
                            ObjectIsEnabled();
                        }
                        break;
                    case ObjectType.lever:
                        par_RotationCenter.Rotate(new Vector3(-1, 0, 0) * step);

                        if (par_RotationCenter.transform.localRotation.x <= openAngle)
                        {
                            ObjectIsEnabled();
                        }
                        break;
                }
            }
            else
            {
                float step = moveSpeed * Time.deltaTime;
                switch (objectType)
                {
                    case ObjectType.button:
                        float buttonDistance = Vector3.Distance(pos_ButtonDisabled.position, par_ButtonCenter.position);
                        par_ButtonCenter.position = Vector3.MoveTowards(par_ButtonCenter.position, par_ButtonCenter.position - new Vector3(0.001f, 0, 0), step);

                        if (buttonDistance <= 0.001f)
                        {
                            ObjectIsDisabled();
                        }
                        break;
                    case ObjectType.lever:
                        par_RotationCenter.Rotate(new Vector3(1, 0, 0) * step);

                        if (par_RotationCenter.transform.localRotation.x >= closedAngle)
                        {
                            ObjectIsDisabled();
                        }
                        break;
                }
            }
        }
    }

    public void Interact()
    {
        if (DoorScript != null)
        {
            DoorScript.Interact();
            isObjectMoving = true;
        }
    }

    //this is a child method of the door management script
    //and it is called if the parent script called the same door open method
    public void EnableObject()
    {
        isObjectMoving = true;
    }

    private void ObjectIsEnabled()
    {
        isObjectEnabled = true;
        isObjectMoving = false;
    }
    private void ObjectIsDisabled()
    {
        isObjectEnabled = false;
        isObjectMoving = false;
    }
}