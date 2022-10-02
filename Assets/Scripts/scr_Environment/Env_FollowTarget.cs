using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Env_FollowTarget : MonoBehaviour
{
    [Header("Assignables")]
    [SerializeField] private bool followHeightOnly;
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