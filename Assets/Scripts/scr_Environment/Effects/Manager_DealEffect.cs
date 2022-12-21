using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager_DealEffect : MonoBehaviour
{
    [Header("Scripts")]
    [SerializeField] private GameObject thePlayer;

    //private variables
    private Env_Effect EffectScript;
    private Player_Stats PlayerStatsScript;

    private void Awake()
    {
        PlayerStatsScript = thePlayer.GetComponent<Player_Stats>();
    }

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

        effect.AddComponent<Env_Effect>();
        EffectScript = effect.GetComponent<Env_Effect>();
        EffectScript.ReceiveEffect(effectName,
                                   effectValue,
                                   effectDuration);
        PlayerStatsScript.activeEffects.Add(effect);

        Debug.Log("Info: " + dealerName + " dealt " + effectValue + " " + effectName + " to " + receiver.name + " for " + effectDuration + " seconds.");
    }
}