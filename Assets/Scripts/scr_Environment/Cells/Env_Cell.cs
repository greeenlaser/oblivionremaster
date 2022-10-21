using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Env_Cell : MonoBehaviour
{
    [Header("Assignables")]
    public bool isDiscovered;
    public string cellName;
    public List<GameObject> containers;
    public CellType cellType;
    public enum CellType
    {
        wilderness,
        dungeon,
        town
    }

    [Header("Scripts")]
    [SerializeField] private GameObject thePlayer;
    [SerializeField] private GameObject par_Managers;

    //reset all hostile AI and containers in this cell
    public void ResetCell()
    {
        foreach (GameObject container in containers)
        {
            UI_Inventory inventory = container.GetComponent<UI_Inventory>();
            Env_LootTable lootTable = container.GetComponent<Env_LootTable>();

            if (inventory.containerType == UI_Inventory.ContainerType.respawnable
                || inventory.containerType == UI_Inventory.ContainerType.store)
            {
                lootTable.RespawnContainer();
                Debug.Log("Respawned " + inventory.containerName + "...");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isDiscovered)
        {
            ResetCell();
            Debug.Log("Discovered " + cellName + "!");
            isDiscovered = true;
        }
    }
}