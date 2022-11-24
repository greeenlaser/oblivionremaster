using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_AssignSettings : MonoBehaviour
{
    //public but hidden variables
    [HideInInspector] public string info;
    [HideInInspector] public TMP_Text txt_SliderText;

    private void Awake()
    {
        info = transform.parent.name.Replace("par_", "");
        if (name == "slider_Settings")
        {
            foreach (Transform child in transform)
            {
                if (child.name == "txt_SliderValue")
                {
                    txt_SliderText = child.GetComponent<TMP_Text>();
                    break;
                }
            }
        }
    }
}