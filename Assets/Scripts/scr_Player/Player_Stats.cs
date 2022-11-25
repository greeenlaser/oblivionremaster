using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player_Stats : MonoBehaviour
{
    [Header("Player movement main values")]
    [SerializeField] private int defaultWalkSpeed = 4;
    [HideInInspector] public int walkSpeed;
    [SerializeField] private int defaultSprintSpeed = 8;
    [HideInInspector] public int sprintSpeed;
    [SerializeField] private int defaultCrouchSpeed = 2;
    [HideInInspector] public int crouchSpeed;
    [SerializeField] private float defaultJumpHeight = 1.75f;
    [HideInInspector] public float jumpHeight;
    [HideInInspector] public Vector3 cameraWalkHeight = new(0, 0.6f, 0);
    [HideInInspector] public Vector3 cameraCrouchHeight = new(0, 0.3f, 0);

    [Header("Combat main values")]
    [HideInInspector] public bool isGodmodeEnabled;
    [SerializeField] private float defaultMaxHealth = 100;
    [HideInInspector] public float maxHealth;
    [HideInInspector] public float currentHealth;
    [SerializeField] private float defaultMaxStamina = 100;
    [HideInInspector] public float maxStamina;
    [HideInInspector] public float currentStamina;
    [SerializeField] private float defaultMaxMagicka = 100;
    [HideInInspector] public float maxMagicka;
    [HideInInspector] public float currentMagicka;

    [Header("Inventory main values")]
    [SerializeField] private int defaultMaxInvSpace = 100;
    [HideInInspector] public int maxInvSpace;
    [HideInInspector] public int invSpace;

    //player level
    [HideInInspector] public int level;
    [HideInInspector] public int level_PointsToNextLevel;

    //attributes and skills
    [HideInInspector] public Dictionary<string, int> Attributes = new();
    [HideInInspector] public Dictionary<string, int> Skills = new();
    [HideInInspector] public Dictionary<string, int> SkillPoints = new();

    [Header("Main player UI")]
    public Slider healthBar;
    public Slider staminaBar;
    public Slider magickaBar;

    [Header("Scripts")]
    [SerializeField] private GameObject par_Managers;

    //scripts
    private Manager_UIReuse UIReuseScript;

    private void Awake()
    {
        //debugging method for checking what the max possible achievable
        //level is with the current level and skill levelup system
        //StartCoroutine(GetMaxAchievableLevel());

        UIReuseScript = par_Managers.GetComponent<Manager_UIReuse>();
    }

    public void ResetStats()
    {
        walkSpeed = defaultWalkSpeed;
        sprintSpeed = defaultSprintSpeed;
        crouchSpeed = defaultCrouchSpeed;
        jumpHeight = defaultJumpHeight;

        level = 1;
        level_PointsToNextLevel = level * 500;

        maxHealth = defaultMaxHealth;
        currentHealth = defaultMaxHealth;
        maxStamina = defaultMaxStamina;
        currentStamina = defaultMaxStamina;
        maxMagicka = defaultMaxMagicka;
        currentMagicka = defaultMaxMagicka;

        maxInvSpace = defaultMaxInvSpace;
        invSpace = 0;

        UpdateBar(healthBar);
        UpdateBar(staminaBar);
        UpdateBar(magickaBar);

        //assign default attribute levels
        Attributes["Strength"] = 1;
        Attributes["Intelligence"] = 1;
        Attributes["Willpower"] = 1;
        Attributes["Agility"] = 1;
        Attributes["Speed"] = 1;
        Attributes["Endurance"] = 1;
        Attributes["Personality"] = 1;
        Attributes["Luck"] = 1;

        //assign default skill levels   //assign default skill point requirements
        Skills["Blade"] = 5; SkillPoints["Blade"] = 750;
        Skills["Blunt"] = 5; SkillPoints["Blunt"] = 750;
        Skills["HandToHand"] = 5; SkillPoints["HandToHand"] = 750;
        Skills["Armorer"] = 5; SkillPoints["Armorer"] = 750;
        Skills["Block"] = 5; SkillPoints["Block"] = 750;
        Skills["HeavyArmor"] = 5; SkillPoints["HeavyArmor"] = 750;
        Skills["Athletics"] = 5; SkillPoints["Athletics"] = 750;
        Skills["Alteration"] = 5; SkillPoints["Alteration"] = 750;
        Skills["Destruction"] = 5; SkillPoints["Destruction"] = 750;
        Skills["Restoration"] = 5; SkillPoints["Restoration"] = 750;
        Skills["Alchemy"] = 5; SkillPoints["Alchemy"] = 750;
        Skills["Conjuration"] = 5; SkillPoints["Conjuration"] = 750;
        Skills["Mysticism"] = 5; SkillPoints["Mysticism"] = 750;
        Skills["Illusion"] = 5; SkillPoints["Illusion"] = 750;
        Skills["Security"] = 5; SkillPoints["Security"] = 750;
        Skills["Sneak"] = 5; SkillPoints["Sneak"] = 750;
        Skills["Marksman"] = 5; SkillPoints["Marksman"] = 750;
        Skills["Acrobatics"] = 5; SkillPoints["Acrobatics"] = 750;
        Skills["LightArmor"] = 5; SkillPoints["LightArmor"] = 750;
        Skills["Mercantile"] = 5; SkillPoints["Mercantile"] = 750;
        Skills["Speechcraft"] = 5; SkillPoints["Speechcraft"] = 750;
    }

    //update players health/stamina/magicka bar UI
    public void UpdateBar(Slider bar)
    {
        if (bar == healthBar)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }
        else if (bar == staminaBar)
        {
            staminaBar.maxValue = maxStamina;
            staminaBar.value = currentStamina;
        }
        else if (bar == magickaBar)
        {
            magickaBar.maxValue = maxMagicka;
            magickaBar.value = currentMagicka;
        }
    }

    //if player dies then the death UI is opened
    public void PlayerDeath()
    {
        if (currentHealth < 0)
        {
            currentHealth = 0;
        }
        UpdateBar(healthBar);

        UIReuseScript.par_DeathUI.SetActive(true);
    }

    //level up a skill if player gained more points than the skill required
    //otherwise remove points required to level up this skill
    public void UpdateSkillValue(string skill, int gainedPoints)
    {
        foreach (KeyValuePair<string, int> skills in Skills)
        {
            string skillName = skills.Key;
            int skillLevel = skills.Value;

            foreach (KeyValuePair<string, int> skillpoints in SkillPoints)
            {
                string skillPointName = skillpoints.Key;
                int skillPointsToNextLevel = skillpoints.Value;

                if (skillName == skillPointName
                    && skill == skillName
                    && skillLevel < 100)
                {
                    bool leveledUpSkill = false;

                    int savedPoints = 0;
                    if (gainedPoints > skillPointsToNextLevel)
                    {
                        savedPoints = gainedPoints - skillPointsToNextLevel;

                        leveledUpSkill = true;
                    }
                    else if (skillPointsToNextLevel - gainedPoints == 0)
                    {
                        leveledUpSkill = true;
                    }
                    else
                    {
                        savedPoints = gainedPoints;
                    }

                    if (leveledUpSkill)
                    {
                        if (skillLevel != 99)
                        {
                            skillPointsToNextLevel = savedPoints;
                        }
                        else
                        {
                            skillPointsToNextLevel = 0;
                        }

                        skillLevel++;

                        SkillPoints[skill] = skillLevel * 150 - savedPoints;
                        GivePlayerExperience(skillLevel * 25);

                        Debug.Log("Leveled up " + skill + " to " + skillLevel + "!");
                        leveledUpSkill = false;
                    }
                    else
                    {
                        skillPointsToNextLevel -= savedPoints;
                    }

                    SkillPoints[skill] = skillPointsToNextLevel;
                    break;
                }
            }
        }
    }

    //give the player exp after a successful quest
    //or increased skill level
    public void GivePlayerExperience(int gainedExperience)
    {
        int extraExp = 0;

        bool leveledUp = false;
        if (gainedExperience > level_PointsToNextLevel)
        {
            extraExp = gainedExperience - level_PointsToNextLevel;

            leveledUp = true;
        }
        else if (level_PointsToNextLevel - gainedExperience == 0)
        {
            leveledUp = true;
        }
        else
        {
            level_PointsToNextLevel -= gainedExperience;
        }

        if (leveledUp)
        {
            level++;
            level_PointsToNextLevel = level * 500 - extraExp;

            Debug.Log("Player leveled up to " + level + "!");
        }
    }

    /*
    private IEnumerator GetMaxAchievableLevel()
    {
        yield return null;

        int level = 1;
        int expRequired = 250;

        int skillCount = 0;
        while (skillCount < 21)
        {
            int skillLevel = 5;
            while (skillLevel < 101)
            {
                int gainedExp = skillLevel * 5;
                int extraExp = 0;

                bool leveledUp = false;
                if (gainedExp > expRequired)
                {
                    extraExp = gainedExp - expRequired;

                    leveledUp = true;
                }
                else if (expRequired - gainedExp == 0)
                {
                    leveledUp = true;
                }
                else
                {
                    expRequired -= gainedExp;
                }

                if (leveledUp)
                {
                    level++;
                    expRequired = level * 500 - extraExp;
                }

                skillLevel++;
            }

            skillCount++;
        }

        Debug.Log("Player reached level " + level + " with " + expRequired + " exp remaining until next level...");
    }
    */
}