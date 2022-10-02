using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Camera : MonoBehaviour
{
    [Header("Assignables")]
    [SerializeField] private GameObject par_Managers;

    //public but hidden variables
    [HideInInspector] public bool isCamEnabled;
    [HideInInspector] public float sensX;
    [HideInInspector] public float sensY;

    //private variables
    private float mouseX;
    private float mouseY;
    private float xRot;
    private Manager_Settings SettingsScript;

    private void Awake()
    {
        SettingsScript = par_Managers.GetComponent<Manager_Settings>();

        StartCoroutine(Wait());
    }

    private void Update()
    {
        if (isCamEnabled)
        {
            mouseX = Input.GetAxis("Mouse X") * sensX * 6 * Time.deltaTime;
            mouseY = Input.GetAxis("Mouse Y") * sensY * 6 * Time.deltaTime;

            xRot -= mouseY;

            xRot = Mathf.Clamp(xRot, -90f, 90f);
            transform.localRotation = Quaternion.Euler(xRot, 0f, 0f);

            transform.parent.Rotate(Vector3.up * mouseX);
        }
    }

    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(0.2f);

        sensX = SettingsScript.user_MouseSpeed;
        sensY = SettingsScript.user_MouseSpeed;
    }
}