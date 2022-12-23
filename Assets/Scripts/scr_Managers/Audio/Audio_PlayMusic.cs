using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Audio_PlayMusic : MonoBehaviour
{
    [Header("Assignables")]
    [Tooltip("How much time should pass before the next song starts?")]
    [SerializeField] private float waitAfterNextSong;
    [Tooltip("Which audio source plays the music?")]
    [SerializeField] private AudioSource audioSource;
    [Tooltip("What songs are played?")]
    [SerializeField] private List<AudioClip> songs = new();

    //private variables
    private float timer;

    private void Update()
    {
        if (audioSource.clip != null
            && !audioSource.isPlaying)
        {
            timer += Time.unscaledDeltaTime;
            if (timer >= waitAfterNextSong)
            {
                NextTrack();
            }
        }
    }

    public void NextTrack()
    {
        timer = 0;
        int random = Random.Range(0, songs.Count);
        audioSource.clip = songs[random];
        audioSource.Play();
    }
}