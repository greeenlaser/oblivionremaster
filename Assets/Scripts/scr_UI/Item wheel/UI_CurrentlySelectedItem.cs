using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_CurrentlySelectedItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    //private variables
    private Manager_ItemWheel ItemWheelScript;

    void OnEnable()
    {
        Debug.Log(name);
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("hovering over " + name + "...");

        if (ItemWheelScript == null)
        {
            ItemWheelScript = GameObject.Find("par_Managers").GetComponent<Manager_ItemWheel>();
        }
        ItemWheelScript.assignableItemName = GetComponentInChildren<TMP_Text>().text;
    }
    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        if (ItemWheelScript == null)
        {
            ItemWheelScript = GameObject.Find("par_Managers").GetComponent<Manager_ItemWheel>();
        }
        ItemWheelScript.assignableItemName = null;
    }
}