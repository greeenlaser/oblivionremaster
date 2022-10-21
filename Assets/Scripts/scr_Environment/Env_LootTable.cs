using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Env_LootTable : MonoBehaviour
{
    [Header("General")]
    [Range(1, 10)]
    [SerializeField] private int lootQuality = 2;
    [SerializeField] private GameObject par_RealSpawnableItems;
    [SerializeField] private List<GameObject> spawnableItems;

    [Header("Scripts")]
    [SerializeField] private GameObject thePlayer;
    [SerializeField] private GameObject par_Managers;

    //scripts
    private UI_Inventory TargetInventory;
    private UI_PlayerMenu PlayerMenuScript;

    private void Awake()
    {
        TargetInventory = GetComponent<UI_Inventory>();
        PlayerMenuScript = par_Managers.GetComponent<UI_PlayerMenu>();
    }

    //used for restocking stores and refilling refillable containers
    public void RespawnContainer()
    {
        //ClearTargetInventory();

        List<GameObject> realSpawnableItems = new();
        foreach (GameObject item in spawnableItems)
        {
            foreach (Transform templateItem in PlayerMenuScript.par_TemplateItems.transform)
            {
                if (templateItem.name == item.name)
                {
                    int chanceToSpawn = 10;
                    Env_Item itemScript = templateItem.GetComponent<Env_Item>();

                    if (itemScript.itemQuality == Env_Item.ItemQuality.trash)
                    {
                        chanceToSpawn *= Random.Range(1, 10) * lootQuality;
                    }
                    else if (itemScript.itemQuality == Env_Item.ItemQuality.trash)
                    {
                        chanceToSpawn *= Random.Range(2, 6) * lootQuality;
                    }
                    else if (itemScript.itemQuality == Env_Item.ItemQuality.trash)
                    {
                        chanceToSpawn *= Random.Range(1, 3) * lootQuality;
                    }
                    else if (itemScript.itemQuality == Env_Item.ItemQuality.trash)
                    {
                        chanceToSpawn *= lootQuality;
                    }

                    if (chanceToSpawn > 50)
                    {
                        GameObject realItem = Instantiate(templateItem.gameObject,
                                                          par_RealSpawnableItems.transform.position,
                                                          Quaternion.identity,
                                                          par_RealSpawnableItems.transform);

                        Env_Item realItemScript = realItem.GetComponent<Env_Item>();
                        realItem.name = realItemScript.str_ItemName;
                        realSpawnableItems.Add(realItem);

                        if (realItemScript.isStackable)
                        {
                            int minCount = Random.Range(1, 15);
                            int maxCount = Random.Range(25, 100);

                            if (realItemScript.itemType == Env_Item.ItemType.consumable
                                || realItemScript.itemType == Env_Item.ItemType.alchemyIngredient)
                            {
                                if (minCount >= 5)
                                {
                                    minCount = Mathf.FloorToInt(minCount / Random.Range(3, 8));
                                }
                                if (maxCount >= 35)
                                {
                                    maxCount = Mathf.FloorToInt(maxCount / Random.Range(4, 10));
                                }
                            }

                            realItemScript.itemCount = Random.Range(minCount * lootQuality,
                                                                    maxCount * lootQuality);
                        }

                        Debug.Log("Spawned " + realItemScript.itemCount + " " + realItem.name + "(s) to " + TargetInventory.containerName + "...");
                    }
                    break;
                }
            }
        }
    }

    //clears target inventory of any remaining items
    private void ClearTargetInventory()
    {
        foreach (Transform item in par_RealSpawnableItems.transform)
        {
            Destroy(item.gameObject);
        }

        foreach (Transform item in TargetInventory.transform)
        {
            Destroy(item.gameObject);
        }
        TargetInventory.containerItems.Clear();
    }
}