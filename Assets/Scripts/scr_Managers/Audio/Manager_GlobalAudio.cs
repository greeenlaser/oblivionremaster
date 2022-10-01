using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Manager_GlobalAudio : MonoBehaviour
{
    [Header("Assignables")]
    public AudioType audioType;
    public enum AudioType
    {
        atmosphere,
        town,
        dungeon,
        battle,
        mainmenu
    }
    [SerializeField] private List<Audio_PlayMusic> GlobalAudioPlayerScripts;

    //private variables
    private int currentScene;

    private void Awake()
    {
        currentScene = SceneManager.GetActiveScene().buildIndex;
        PlayAudioType();
    }

    public void PlayAudioType()
    {
        if (currentScene == 0)
        {
            //always starts main menu theme if the player is in main menu
            GlobalAudioPlayerScripts[0].NextTrack();
        }
        else if (currentScene == 1)
        {
            if (audioType == AudioType.atmosphere)
            {
                GlobalAudioPlayerScripts[0].NextTrack();
            }
            else if (audioType == AudioType.town)
            {
                GlobalAudioPlayerScripts[1].NextTrack();
            }
            else if (audioType == AudioType.dungeon)
            {
                GlobalAudioPlayerScripts[2].NextTrack();
            }
            else if (audioType == AudioType.battle)
            {
                GlobalAudioPlayerScripts[3].NextTrack();
            }
        }
    }
}