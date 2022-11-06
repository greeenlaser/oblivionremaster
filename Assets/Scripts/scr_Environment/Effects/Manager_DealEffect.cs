using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager_DealEffect : MonoBehaviour
{
    //private variables
    private Env_ReceiveEffect EffectScript;

    //deal the selected effect to the target
    public void DealEffect(GameObject dealer,
                           GameObject receiver, 
                           string effectName, 
                           float effectValue, 
                           float effectDuration)
    {
        string dealerName;
        if (dealer == null)
        {
            dealerName = "World";
        }
        else
        {
            dealerName = dealer.name;
        }

        GameObject effect = Instantiate(new GameObject(),
                                        receiver.transform.position,
                                        Quaternion.identity,
                                        receiver.transform);

        effect.AddComponent<Env_ReceiveEffect>();
        EffectScript = effect.GetComponent<Env_ReceiveEffect>();
        EffectScript.UIReuseScript = GetComponent<Manager_UIReuse>();
        EffectScript.ReceiveEffect(effectName,
                                   effectValue,
                                   effectDuration);

        Debug.Log(dealerName + " dealt " + effectValue + " " + effectName + " to " + receiver.name + " for " + effectDuration + " seconds.");
    }
}