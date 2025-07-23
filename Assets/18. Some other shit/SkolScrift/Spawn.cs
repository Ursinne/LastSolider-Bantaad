using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Spawn : MonoBehaviour
{
    public GameObject cannonBall; // Referens till "cannonball" Prefab
    public Transform spawnpoint; // Referent till object
    public float speed = 5f;
    //public string[] vapen = new string[50]; // lägger till vapen i inspectorn, new string lägger till 50 direkt. Man kan behöva reseta skriften i inspecktor för att det ska funka 
    public string[] vapen = new string[] { "Svärd", "Yxa", "Kniv" }; // Ett annat sätt att lägga till men då kan man inte ändra i inspecktorn

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
            Debug.Log(" Kniven är inte upplockad ");
            }
    }
    //Instantiate(cannonBall,spawnpoint.position,spawnpoint.rotation); // transform istället för spawnpoint
    //int i = 0; //Loopar
    //if (i < 10);
    //i++;

    //Debug.Log(vapen[0]); // Printa namnet på första elementet i arrayen

    //for(int i = 0; i < 10; i++)
    //{
    //    Debug.Log("Jag har kört bil" + i + " gånger. ");
    //}
          
    }
    void Update()
    {
        // ta in input från höger och vänster piltangenterna
        //transform.position = transform.position + new Vector3(Input.GetAxis("Horizontal"), 0f, 0f);          // långa varianten
        transform.position += new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxis("Vertical")) * speed * Time.deltaTime;   // korta varianten
       

        //if(Input.GetKeyDown(KeyCode.H))    //
        //if(Input.GetButton("Fire2"))        // Skjuta med musen , button mus , Fire1, Fire2 osv
          if(Input.GetButtonDown("Fire1"))
        {
            Instantiate(cannonBall,spawnpoint.position,spawnpoint.rotation);
            Debug.Log("musmarkören befinner sig på: " + Input.GetAxis("Mouse X"));
            //Debug.Log(speed);
            //Instantiate(cannonBall,spawnpoint.transform,spawnpoint.transform);

        }
          /* Inledande
          använd detta för att komentera ut allt under
          dsads
          Avslutande */          
    }
    private void OnCollisionEnter(Collision collision)
    {
        
    }
}