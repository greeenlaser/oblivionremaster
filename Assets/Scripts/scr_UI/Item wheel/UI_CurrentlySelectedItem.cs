using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_CurrentlySelectedItem : MonoBehaviour, IPointerEnterHandler
{
    //private variables
    private Manager_ItemWheel ItemWheelScript;

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        if (ItemWheelScript == null)
        {
            ItemWheelScript = GameObject.Find("par_Managers").GetComponent<Manager_ItemWheel>();
        }
        ItemWheelScript.assignableItemName = GetComponentInChildren<TMP_Text>().text;
    }
}