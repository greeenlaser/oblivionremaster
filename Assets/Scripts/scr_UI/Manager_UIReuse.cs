using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Manager_UIReuse : MonoBehaviour
{
    [Header("Debug menu")]
    public TMP_Text txt_GameVersion;
    public TMP_Text txt_FPS;

    [Header("Confirmation UI")]
    public GameObject par_Confirmation;
    public TMP_Text txt_Confirmation;
    public Button btn_Confirm1;
    public Button btn_Cancel;

    //reset all confirmation UI elements
    public void ClearConfirmationUI()
    {
        btn_Confirm1.onClick.RemoveAllListeners();
        btn_Confirm1.gameObject.SetActive(false);

        par_Confirmation.SetActive(false);
    }
}