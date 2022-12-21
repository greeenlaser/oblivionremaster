using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Env_Effect : MonoBehaviour
{
    //effect variables
    [HideInInspector] public string theEffectName;
    [HideInInspector] public float theEffectValue;
    [HideInInspector] public float theEffectDuration;

    //private variables
    private bool activateEffect;
    private bool dealtFirstEffect;

    private float timer;

    //scripts
    private Player_Stats PlayerStatsScript;
    private GameObject par_Managers;
    private Manager_AllowedEffects EffectsScript;

    private void Awake()
    {
        par_Managers = GameObject.Find("par_Managers");
        EffectsScript = par_Managers.GetComponent<Manager_AllowedEffects>();
    }

    private void Update()
    {
        //a duration of -1 means this effect is permanent
        //or active until force-disabled
        if (activateEffect)
        {
            if (!dealtFirstEffect
                && theEffectDuration != -1)
            {
                DealEffect(theEffectName,
                           theEffectValue,
                           transform.parent.gameObject);
                dealtFirstEffect = true;
            }

            if (theEffectDuration == 0)
            {
                activateEffect = false;
            }
            else
            {
                timer -= Time.deltaTime;
                if (timer <= 0)
                {
                    DealEffect(theEffectName,
                               theEffectValue,
                               transform.parent.gameObject);
                    if (theEffectDuration > 0)
                    {
                        theEffectDuration -= 1;
                        timer = 1;
                    }
                    else if (theEffectDuration == -1)
                    {
                        timer = 1;
                    }
                    else
                    {
                        for (int i = 0; i < PlayerStatsScript.activeEffects.Count; i++)
                        {
                            GameObject effect = PlayerStatsScript.activeEffects[i];
                            if (effect == gameObject)
                            {
                                PlayerStatsScript.activeEffects.RemoveAt(i);
                                break;
                            }
                        }
                        Destroy(gameObject);
                    }
                }
            }
        }
    }

    //receive the effect values
    public void ReceiveEffect(string effectName, 
                              float effectValue,
                              float effectDuration)
    {
        theEffectName = effectName;
        theEffectValue = effectValue;
        theEffectDuration = effectDuration;

        timer = 1;
        activateEffect = true;
    }

    //deal the effect to the target
    public void DealEffect(string effectName,
                           float effectValue,
                           GameObject target)
    {
        //look through all effects
        bool foundEffect = false;
        foreach (string effect in EffectsScript.allEffects)
        {
            if (effectName == effect)
            {
                foundEffect = true;
                break;
            }
        }
        if (!foundEffect)
        {
            Debug.LogError("Error: " + effectName + " is not a valid effect! This should not be happening.");
        }
        else
        {
            //the player received an effect
            if (target.name == "Player")
            {
                PlayerStatsScript = target.GetComponent<Player_Stats>();
                PlayerReceivedEffect(effectName,
                                     effectValue);
            }
        }
    }

    //player successfully received the selected effect
    private void PlayerReceivedEffect(string effectName, float effectValue)
    {
        //player can only receive effects if god mode is not enabled
        if (PlayerStatsScript.isGodmodeEnabled)
        {
            Debug.LogWarning("Warning: Tried to apply " + effectName + " to player but this effect was not applied because god mode was enabled.");
        }
        else
        {
            if (effectName == "drainHealth")
            {
                PlayerStatsScript.currentHealth -= effectValue;
                if (PlayerStatsScript.currentHealth <= 0)
                {
                    PlayerStatsScript.PlayerDeath();
                }
                else
                {
                    PlayerStatsScript.UpdateBar(PlayerStatsScript.healthBar);
                }
            }
            else if (effectName == "drainMagicka")
            {
                PlayerStatsScript.currentMagicka -= effectValue;
                if (PlayerStatsScript.currentMagicka < 0)
                {
                    PlayerStatsScript.currentMagicka = 0;
                }
                PlayerStatsScript.UpdateBar(PlayerStatsScript.magickaBar);
            }
            else if (effectName == "drainFatigue")
            {
                PlayerStatsScript.currentStamina -= effectValue;
                if (PlayerStatsScript.currentStamina < 0)
                {
                    PlayerStatsScript.currentStamina = 0;
                }
                PlayerStatsScript.UpdateBar(PlayerStatsScript.staminaBar);
            }
            if (effectName == "addHealth")
            {
                PlayerStatsScript.currentHealth += effectValue;
                if (PlayerStatsScript.currentHealth
                    > PlayerStatsScript.maxHealth)
                {
                    PlayerStatsScript.currentHealth = PlayerStatsScript.maxHealth;
                }
                PlayerStatsScript.UpdateBar(PlayerStatsScript.healthBar);
            }
            else if (effectName == "addMagicka")
            {
                PlayerStatsScript.currentMagicka += effectValue;
                if (PlayerStatsScript.currentMagicka
                    > PlayerStatsScript.maxMagicka)
                {
                    PlayerStatsScript.currentMagicka = PlayerStatsScript.maxMagicka;
                }
                PlayerStatsScript.UpdateBar(PlayerStatsScript.magickaBar);
            }
            else if (effectName == "addFatigue")
            {
                PlayerStatsScript.currentStamina += effectValue;
                if (PlayerStatsScript.currentStamina
                    > PlayerStatsScript.maxStamina)
                {
                    PlayerStatsScript.currentStamina = PlayerStatsScript.maxStamina;
                }
                PlayerStatsScript.UpdateBar(PlayerStatsScript.staminaBar);
            }
        }
    }

    //an item successfully received an enchantment
    private void ItemReceivedEnchantment(string effectName, string effectValue)
    {

    }
}