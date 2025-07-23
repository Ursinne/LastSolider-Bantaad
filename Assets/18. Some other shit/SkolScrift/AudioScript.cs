using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AudioScript : MonoBehaviour
{
    public AudioSource audioSource;
    //public AudioSource audioSource2; // Referens till ljudfilen
    public AudioClip clip1, clip2;
    //HealtScript.CalculateDamage(); elller HealthAcript.score +=1;

    void Start()
    {
        audioSource = GetComponent<AudioSource>(); // fyll referensen
        
      
}

    public void PlayAudio1()
    {
        //audioSource.Play();
        audioSource.PlayOneShot(clip1); // spela upp ljudet
    }

    public void PlayAudio2()
    {
        //audioSource.Play();
        audioSource.PlayOneShot(clip2); // spela upp ljudet
    }
}
