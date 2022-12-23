using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Manager_GlobalAudio : MonoBehaviour
{
    [Header("Assignables")]
    [Tooltip("All scripts that play music in the game.")]
    [SerializeField] private List<Audio_PlayMusic> GlobalAudioPlayerScripts;

    //public but hidden variables
    [HideInInspector] public UserDefined_AudioType audioType;
    public enum UserDefined_AudioType
    {
        atmosphere,
        town,
        dungeon,
        battle,
        mainmenu
    }

    //private variables
    private int currentScene;

    private void Awake()
    {
        currentScene = SceneManager.GetActiveScene().buildIndex;
        if (currentScene == 0)
        {
            GlobalAudioPlayerScripts[0].NextTrack();
        }
    }

    public void PlayAudioType()
    {
        if (audioType == UserDefined_AudioType.atmosphere)
        {
            GlobalAudioPlayerScripts[0].NextTrack();
        }
        else if (audioType == UserDefined_AudioType.town)
        {
            GlobalAudioPlayerScripts[1].NextTrack();
        }
        else if (audioType == UserDefined_AudioType.dungeon)
        {
            GlobalAudioPlayerScripts[2].NextTrack();
        }
        else if (audioType == UserDefined_AudioType.battle)
        {
            GlobalAudioPlayerScripts[3].NextTrack();
        }
    }
}