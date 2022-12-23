using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Env_FollowTarget : MonoBehaviour
{
    [Header("Assignables")]
    [Tooltip("Is it only following target in y axis?")]
    [SerializeField] private bool followHeightOnly;
    [Tooltip("Which target does this gameobject follow?")]
    [SerializeField] private GameObject target;

    private void Update()
    {
        if (!followHeightOnly)
        {
            transform.position = target.transform.position;
        }
        else
        {
            transform.position = new Vector3(transform.position.x,
                                             target.transform.position.y,
                                             transform.position.z);
        }
    }
}