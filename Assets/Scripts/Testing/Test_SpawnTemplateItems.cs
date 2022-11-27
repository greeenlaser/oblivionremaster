using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_SpawnTemplateItems : MonoBehaviour
{
    [Header("Assignables")]
    [SerializeField] private GameObject spawnedItem;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            SpawnItem(spawnedItem);
        }
    }

    private void SpawnItem(GameObject item)
    {
        GameObject newItem = Instantiate(item,
                                         transform.transform.position,
                                         Quaternion.identity,
                                         transform);
        newItem.name = newItem.GetComponent<Env_Item>().itemName;
        newItem.SetActive(true);

        Debug.Log("Info: Spawned " + newItem.name.Replace("_", " ") + " at " + transform.position.ToString() + ".");
    }
}