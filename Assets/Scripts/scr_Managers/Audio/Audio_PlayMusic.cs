using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Audio_PlayMusic : MonoBehaviour
{
    [Header("Assignables")]
    [SerializeField] private float waitAfterNextSong;
    [SerializeField] private AudioSource audioSource;
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