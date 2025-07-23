using UnityEngine;

public class DripScript : MonoBehaviour
{
    private AudioSource audioSource;                    // Referens till ljudkällan
    float randomDrip = 1f;                              // Tidsintervall för dropljuden
    void Start()
    {
        audioSource = GetComponent<AudioSource>(); 
        Invoke("PlayDripSound", randomDrip);
    }


void PlayDripSound()
    {
        audioSource.Play();
        //randomDrip = Random.Range(2f, 10f);           // Slupma tidsibtervaller
        Invoke("PlayDripSound", randomDrip);

    }
}
