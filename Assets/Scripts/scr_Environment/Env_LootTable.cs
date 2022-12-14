using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Env_LootTable : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private List<GameObject> spawnableItems;

    [Header("Scripts")]
    [SerializeField] private GameObject thePlayer;
    [SerializeField] private GameObject par_Managers;

    //scripts
    private Player_Stats PlayerStatsScript;
    private UI_Inventory TargetInventory;
    private UI_PlayerMenu PlayerMenuScript;

    private void Awake()
    {
        PlayerStatsScript = thePlayer.GetComponent<Player_Stats>();
        PlayerMenuScript = par_Managers.GetComponent<UI_PlayerMenu>();
    }

    //used for restocking stores and refilling refillable containers
    public void ResetContainer(GameObject targetContainer)
    {
        TargetInventory = targetContainer.GetComponent<UI_Inventory>();

        if (TargetInventory.containerItems.Count > 0)
        {
            ClearTargetInventory();
        }

        foreach (GameObject item in spawnableItems)
        {
            foreach (Transform templateItem in PlayerMenuScript.par_TemplateItems.transform)
            {
                if (templateItem.name == item.name)
                {
                    int randomChance = 0;

                    Env_Item itemScript = templateItem.GetComponent<Env_Item>();
                    string itemQuality = itemScript.itemQuality.ToString();
                    if (itemQuality == "trash")
                    {
                        randomChance = Random.Range(25, 60);
                    }
                    else if (itemQuality == "common")
                    {
                        randomChance = Random.Range(20, 50);
                    }
                    else if (itemQuality == "rare")
                    {
                        randomChance = Random.Range(15, 40);
                    }
                    else if (itemQuality == "legendary")
                    {
                        randomChance = Random.Range(10, 30);
                    }

                    int luck = PlayerStatsScript.Attributes["Luck"];
                    int luckChance = 0;
                    if (luck <= 5)
                    {
                        luckChance = 1;
                    }
                    else if (luckChance > 5
                             && luckChance <= 8)
                    {
                        luckChance = 2;
                    }
                    else if (luck > 8)
                    {
                        luckChance = 3;
                    }

                    int chanceToSpawn = randomChance * luckChance;

                    if (chanceToSpawn >= 50)
                    {
                        GameObject realItem = Instantiate(templateItem.gameObject,
                                                          TargetInventory.par_ContainerItems.transform.position,
                                                          Quaternion.identity,
                                                          TargetInventory.par_ContainerItems.transform);

                        Env_Item realItemScript = realItem.GetComponent<Env_Item>();
                        realItem.name = realItemScript.itemName;
                        realItem.layer = LayerMask.NameToLayer("LimitedCollision");
                        TargetInventory.containerItems.Add(realItem);

                        if (realItemScript.isStackable)
                        {
                            int count = Random.Range(3, 15);

                            if (realItemScript.itemType == Env_Item.ItemType.consumable
                                || realItemScript.itemType == Env_Item.ItemType.alchemyIngredient
                                || realItemScript.itemType == Env_Item.ItemType.ammo
                                || realItemScript.itemType == Env_Item.ItemType.misc)
                            {
                                count *= luckChance;
                                if (count > 50)
                                {
                                    count = 50;
                                }
                            }

                            realItemScript.itemCount = count;
                        }
                    }
                    break;
                }
            }
        }
    }

    //clears target inventory of any remaining items
    private void ClearTargetInventory()
    {
        foreach (Transform item in TargetInventory.par_ContainerItems.transform)
        {
            Destroy(item.gameObject);
        }
        TargetInventory.containerItems.Clear();
    }
}