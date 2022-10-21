using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_Rotate : MonoBehaviour
{
    [Header("Assignables")]
    [SerializeField] float rotationSpeed = 45;

    //private variables
    private Vector3 currentEulerAngles;
    private float x;
    private float y;
    private float timer;

    private void Awake()
    {
        x = 40;
        y = 0;
    }

    private void Update()
    {
        if (timer < 1)
        {
            timer += Time.deltaTime;
            currentEulerAngles += rotationSpeed * Time.deltaTime * new Vector3(1 - x, 1 - y, 0);
        }
        else
        {
            currentEulerAngles += rotationSpeed * Time.deltaTime * new Vector3(x, 1 - y, 0);
        }
        
        transform.eulerAngles = currentEulerAngles;
    }
}