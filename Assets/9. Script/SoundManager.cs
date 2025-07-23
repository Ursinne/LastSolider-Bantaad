using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio; // L�gg till f�r att kunna anv�nda audiofunktioner



public class SoundManager : MonoBehaviour
{
    public AudioMixer mixer; // Referens till mixern
    public void SetVolum(float volume)
    {
        mixer.SetFloat("masterVolyme", volume); // L�nka masterregeln till slidern 
    }
}
