using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Env_ReceiveEffect : MonoBehaviour
{
    //public but hidden variables
    [HideInInspector] public Manager_UIReuse UIReuseScript;

    //private variables
    private bool activateEffect;
    private bool dealtFirstEffect;
    private float duration;
    private float timer;

    //effect variables
    private string theEffectName;
    private float theEffectValue;
    private EffectType effectType;
    private enum EffectType
    {
        fortifyHealth,     drainHealth,   addHealth,
        fortifyMagicka,    drainMagicka,  addMagicka,
        fortifyFatigue,    drainFatigue,  addFatigue,
        fortifyAttribute,  damageAbility,
        fortifySkill,      damageSkill,
        cureDisease,       resistDisease,
        fireShield,        fireDamage,
        frostShield,       frostDamage,
        shockShield,       shockDamage,
        waterBreathing,    waterWalking,
        resistParalysis,   paralyze,
        resistElement,
        resistPoison,
        shield,
        light,
        nightEye,
        feather,
        chameleon,
        detectLife,
        silence,
        disintegrateArmor
    }

    //scripts
    private Player_Stats PlayerStatsScript;

    private void Update()
    {
        if (activateEffect)
        {
            if (!dealtFirstEffect)
            {
                DealEffect(theEffectName, 
                           theEffectValue);
                dealtFirstEffect = false;
            }

            if (duration == 0)
            {
                activateEffect = false;
            }
            else
            {
                timer -= Time.deltaTime;
                if (timer <= 0)
                {
                    DealEffect(theEffectName,
                               theEffectValue);
                    duration -= 0.25f;
                    if (duration > 0)
                    {
                        timer = 0.25f;
                    }
                    else
                    {
                        activateEffect = false;
                    }
                }
            }
        }
    }

    //receive the effect
    public void ReceiveEffect(string effectName, 
                              float effectValue,
                              float effectDuration)
    {
        theEffectName = effectName;
        theEffectValue = effectValue;
        duration = effectDuration;

        timer = 0.25f;
        activateEffect = true;
    }

    //deal the effect to the target
    private void DealEffect(string effectName,
                            float effectValue)
    {
        effectType = (EffectType)Enum.Parse(typeof(EffectType), effectName);

        GameObject target = transform.parent.gameObject;
        if (target.GetComponent<Player_Stats>() != null)
        {
            PlayerStatsScript = target.GetComponent<Player_Stats>();
        }

        switch (effectType)
        {
            case EffectType.drainHealth:
                if (target.name == "Player")
                {
                    PlayerStatsScript.currentHealth -= effectValue;
                    if (PlayerStatsScript.currentHealth <= 0)
                    {
                        UIReuseScript.par_DeathUI.SetActive(true);
                        PlayerStatsScript.currentHealth = 0;
                    }
                    PlayerStatsScript.UpdateBar(PlayerStatsScript.healthBar);
                }
                break;
            case EffectType.drainMagicka:
                if (target.name == "Player")
                {
                    PlayerStatsScript.currentMagicka -= effectValue;
                    if (PlayerStatsScript.currentMagicka < 0)
                    {
                        PlayerStatsScript.currentMagicka = 0;
                    }
                    PlayerStatsScript.UpdateBar(PlayerStatsScript.magickaBar);
                }
                break;
            case EffectType.drainFatigue:
                if (target.name == "Player")
                {
                    PlayerStatsScript.currentStamina -= effectValue;
                    if (PlayerStatsScript.currentStamina < 0)
                    {
                        PlayerStatsScript.currentStamina = 0;
                    }
                    PlayerStatsScript.UpdateBar(PlayerStatsScript.staminaBar);
                }
                break;
            case EffectType.addHealth:
                if (target.name == "Player")
                {
                    PlayerStatsScript.currentHealth += effectValue;
                    if (PlayerStatsScript.currentStamina > PlayerStatsScript.maxHealth)
                    {
                        PlayerStatsScript.currentStamina = PlayerStatsScript.maxHealth;
                    }
                    PlayerStatsScript.UpdateBar(PlayerStatsScript.healthBar);
                }
                break;
            case EffectType.addMagicka:
                if (target.name == "Player")
                {
                    PlayerStatsScript.currentMagicka += effectValue;
                    if (PlayerStatsScript.currentMagicka > PlayerStatsScript.maxMagicka)
                    {
                        PlayerStatsScript.currentMagicka = PlayerStatsScript.maxMagicka;
                    }
                    PlayerStatsScript.UpdateBar(PlayerStatsScript.magickaBar);
                }
                break;
            case EffectType.addFatigue:
                if (target.name == "Player")
                {
                    PlayerStatsScript.currentStamina += effectValue;
                    if (PlayerStatsScript.currentStamina > PlayerStatsScript.maxStamina)
                    {
                        PlayerStatsScript.currentStamina = PlayerStatsScript.maxStamina;
                    }
                    PlayerStatsScript.UpdateBar(PlayerStatsScript.staminaBar);
                }
                break;
        }
    }
}