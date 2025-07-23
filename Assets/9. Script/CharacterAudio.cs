using UnityEditor.PackageManager;
using UnityEngine;

public class CharacterAudio : MonoBehaviour
{
    public AudioSource FootstepAudio;               // Refrens till ljudk�llan

    public AudioClip[] woodSteps;                   // [] Detta �r en array
    public AudioClip[] grassSteps;                  // Array ineh�llande ljudk�llan
    public AudioClip[] snowSteps;
    public AudioClip[] sandSteps;
    public AudioClip[] waterSteps;
    public float distanceToGround = 1f;             // Karakt�rens avst�nd till marken. Avg�r fotstegsljud ska spelas upp eller ej

    

    RaycastHit hitinfo;                             // Variabel som innneh�ller infprmation om underlagets tag

    void Start()
    {
        FootstepAudio = GetComponent<AudioSource>();
    }

    bool Grounded()
    {
        return Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out hitinfo, distanceToGround); // Skjut en ray mot golve6t och lagra informationen om vad vi tr�ffar i hitinfo.
    }

    public void Foosteep()
    {
        if (Grounded())                                             // Kolla om vi st�r p� marken.
        {
            //int r = Random.Range(0, woodSteps.Length);            // Slumpa ett v�rde mellan 0 och antalet element i arayen
            switch(hitinfo.transform.tag)
            {
                case ("WoodFloor"):
                    FootstepAudio.PlayOneShot(woodSteps[0]);                // Spela upp slumpm�ssigt ljud.
                    break;
                case ("GrassFloor"):
                    FootstepAudio.PlayOneShot(grassSteps[0]);               // Spela upp slumpm�ssigt ljud.
                    break;
                case ("SnowFloor"):
                    FootstepAudio.PlayOneShot(snowSteps[0]);                // Spela upp slumpm�ssigt ljud.
                    break;
                case ("SandFloor"):
                    FootstepAudio.PlayOneShot(sandSteps[0]);                // Spela upp slumpm�ssigt ljud.
                    break;
                case ("WaterFloor"):
                    FootstepAudio.PlayOneShot(waterSteps[0]);               // Spela upp slumpm�ssigt ljud.
                    break;
                default:
                    FootstepAudio.PlayOneShot(woodSteps[0]);                 // Spela upp slumpm�ssigt ljud
                    break;
            }
            
            
            
            
            
        }

    }

}
