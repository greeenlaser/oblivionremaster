using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Compass : MonoBehaviour
{
    [Header("Assignables")]
    [SerializeField] private GameObject thePlayer;
    public List<GameObject> locations = new();

    private void Update()
    {
        transform.position = thePlayer.transform.position;
    }
}