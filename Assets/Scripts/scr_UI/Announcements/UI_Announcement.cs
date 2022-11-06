using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Announcement : MonoBehaviour
{
    //public but hidden variables
    [HideInInspector] public bool isActivated;

    //private variables
    private float announcementLife = 5;

    private void Update()
    {
        announcementLife -= Time.unscaledDeltaTime;
        if (announcementLife <= 0)
        {
            Destroy(gameObject);
        }
    }
}