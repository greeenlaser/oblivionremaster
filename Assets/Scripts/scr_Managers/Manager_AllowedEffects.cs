using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager_AllowedEffects : MonoBehaviour
{
    public List<string> allEffects = new();

    [Header("Enchantments for all equippable items")]
    public List<string> meleeEnchantments = new();
    public List<string> rangedEnchantments = new();
    public List<string> staffEnchantments = new();
    public List<string> helmetEnchantments = new();
    public List<string> cuirassEnchantments = new();
    public List<string> greavesEnchantments = new();
    public List<string> bootsEnchantments = new();
    public List<string> necklaceEnchantments = new();
    public List<string> ringEnchantments = new();

    [Header("Effects for all others")]
    public List<string> spellEffects = new();
    public List<string> potionEffects = new();

    [HideInInspector] public List<string> playerUnlockedEnchantments = new();
}