using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MusicManager : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip musicStop;
    public AudioClip[] transitionMusic;
    public float currentClipLength;
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }
    public void StartMusic()
    {
        if (audioSource.isPlaying == false)
        {
            audioSource.clip = transitionMusic[Random.Range(0, transitionMusic.Length)];
            audioSource.Play();
            Invoke("SequenceMusic", currentClipLength);
        }
    }
    public void StopMusic()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
            audioSource.clip = musicStop;
            audioSource.PlayOneShot(musicStop);
        }
        CancelInvoke("SequenceMusic");
    }
    public void SequenceMusic()
    {
        audioSource.clip = transitionMusic[Random.Range(0, transitionMusic.Length)];
        currentClipLength = audioSource.clip.length;
        audioSource.Play();
        Invoke("SequenceMusic", currentClipLength);
    }
}