using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TriggerScript : MonoBehaviour
{
    public int score = 0;
    public AudioScript audioScript; // H�r fyller vi i referens till audioscriptet
    

    void Start()
    {
        score = 0;
        audioScript = GetComponent<AudioScript>(); 
    }

    private void OnCollisionEnter(Collision collision)
    {



        //private void OnTriggerEnter(Collider other)  // other �r triggersnamn, man kan byta ut det som man vill


        if (collision.gameObject.CompareTag("Ball"))  // om man vill utskilja ett objekt, eller aanv�nda friendley fire eller inte kunna skada sig sj�lv.
        {                                            // detta kan man skapa npc, fallluckor, scarejump mm.
            Debug.Log("Sound on ball!!");
            score++; // ++ �r lika med 1
            score += 1;
            Debug.Log(score);
            audioScript.PlayAudio1();
        }

        if (collision.gameObject.CompareTag("Dick"))  // om man vill utskilja ett objekt, eller aanv�nda friendley fire eller inte kunna skada sig sj�lv.
        {                                            // detta kan man skapa npc, fallluckor, scarejump mm.
            Debug.Log("sound on dick");
            score++; // ++ �r lika med 1
            score += 1;
            Debug.Log(score);
            audioScript.PlayAudio2();
        }
    } 
}