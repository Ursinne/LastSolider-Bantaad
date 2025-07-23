using UnityEditor.PackageManager;
using UnityEngine;

public class CharacterAudio : MonoBehaviour
{
    public AudioSource FootstepAudio;               // Refrens till ljudkällan

    public AudioClip[] woodSteps;                   // [] Detta är en array
    public AudioClip[] grassSteps;                  // Array inehållande ljudkällan
    public AudioClip[] snowSteps;
    public AudioClip[] sandSteps;
    public AudioClip[] waterSteps;
    public float distanceToGround = 1f;             // Karaktärens avstånd till marken. Avgör fotstegsljud ska spelas upp eller ej

    

    RaycastHit hitinfo;                             // Variabel som innnehåller infprmation om underlagets tag

    void Start()
    {
        FootstepAudio = GetComponent<AudioSource>();
    }

    bool Grounded()
    {
        return Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out hitinfo, distanceToGround); // Skjut en ray mot golve6t och lagra informationen om vad vi träffar i hitinfo.
    }

    public void Foosteep()
    {
        if (Grounded())                                             // Kolla om vi står på marken.
        {
            //int r = Random.Range(0, woodSteps.Length);            // Slumpa ett värde mellan 0 och antalet element i arayen
            switch(hitinfo.transform.tag)
            {
                case ("WoodFloor"):
                    FootstepAudio.PlayOneShot(woodSteps[0]);                // Spela upp slumpmässigt ljud.
                    break;
                case ("GrassFloor"):
                    FootstepAudio.PlayOneShot(grassSteps[0]);               // Spela upp slumpmässigt ljud.
                    break;
                case ("SnowFloor"):
                    FootstepAudio.PlayOneShot(snowSteps[0]);                // Spela upp slumpmässigt ljud.
                    break;
                case ("SandFloor"):
                    FootstepAudio.PlayOneShot(sandSteps[0]);                // Spela upp slumpmässigt ljud.
                    break;
                case ("WaterFloor"):
                    FootstepAudio.PlayOneShot(waterSteps[0]);               // Spela upp slumpmässigt ljud.
                    break;
                default:
                    FootstepAudio.PlayOneShot(woodSteps[0]);                 // Spela upp slumpmässigt ljud
                    break;
            }
            
            
            
            
            
        }

    }

}
