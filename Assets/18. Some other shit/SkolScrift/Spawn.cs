using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Spawn : MonoBehaviour
{
    public GameObject cannonBall; // Referens till "cannonball" Prefab
    public Transform spawnpoint; // Referent till object
    public float speed = 5f;
    //public string[] vapen = new string[50]; // l�gger till vapen i inspectorn, new string l�gger till 50 direkt. Man kan beh�va reseta skriften i inspecktor f�r att det ska funka 
    public string[] vapen = new string[] { "Sv�rd", "Yxa", "Kniv" }; // Ett annat s�tt att l�gga till men d� kan man inte �ndra i inspecktorn

    void Start()
    {

        for (int i = 0; i < vapen.Length; i++)
        {
            if (vapen[i] == "Kniv")
            {
            Debug.Log("Kniven upplockad");
            }
            else
            {
            Debug.Log(" Kniven �r inte upplockad ");
            }
    }
    //Instantiate(cannonBall,spawnpoint.position,spawnpoint.rotation); // transform ist�llet f�r spawnpoint
    //int i = 0; //Loopar
    //if (i < 10);
    //i++;

    //Debug.Log(vapen[0]); // Printa namnet p� f�rsta elementet i arrayen

    //for(int i = 0; i < 10; i++)
    //{
    //    Debug.Log("Jag har k�rt bil" + i + " g�nger. ");
    //}
          
    }
    void Update()
    {
        // ta in input fr�n h�ger och v�nster piltangenterna
        //transform.position = transform.position + new Vector3(Input.GetAxis("Horizontal"), 0f, 0f);          // l�nga varianten
        transform.position += new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxis("Vertical")) * speed * Time.deltaTime;   // korta varianten
       

        //if(Input.GetKeyDown(KeyCode.H))    //
        //if(Input.GetButton("Fire2"))        // Skjuta med musen , button mus , Fire1, Fire2 osv
          if(Input.GetButtonDown("Fire1"))
        {
            Instantiate(cannonBall,spawnpoint.position,spawnpoint.rotation);
            Debug.Log("musmark�ren befinner sig p�: " + Input.GetAxis("Mouse X"));
            //Debug.Log(speed);
            //Instantiate(cannonBall,spawnpoint.transform,spawnpoint.transform);

        }
          /* Inledande
          anv�nd detta f�r att komentera ut allt under
          dsads
          Avslutande */          
    }
    private void OnCollisionEnter(Collision collision)
    {
        
    }
}