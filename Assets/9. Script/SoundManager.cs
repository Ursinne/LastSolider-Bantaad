using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio; // Lägg till för att kunna använda audiofunktioner



public class SoundManager : MonoBehaviour
{
    public AudioMixer mixer; // Referens till mixern
    public void SetVolum(float volume)
    {
        mixer.SetFloat("masterVolyme", volume); // Länka masterregeln till slidern 
    }
}
