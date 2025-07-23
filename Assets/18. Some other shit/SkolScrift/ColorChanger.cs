using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorChanger : MonoBehaviour // denna fil kan ligga på en fiende eller vilket objekt som helst
{
    public GameObject[] cubes; // Refereans till kuberna, [] kallas array
    public int index = 1;   

    void Start()
    {
        //Rigibody = GetComponent<Rigidbody>().// efter punkt kommer man åt en massa inställningar  .Addforce();för att skjuta nåt tex
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) // här gör vi för att byta färg på object, men alla i samma färg.
        {
            foreach(var cube in cubes) // var = kopplat till gameobject
            {
                cube.GetComponent<MeshRenderer>().material.color = Color.blue; 
                index++;
            }
        }

        //for (int i = 0; i < cubes.Length; i++)
        //    if(i == 1)
        //    {
        //        Debug.Log("test med färger"); 
        //    }
        //    else
        //    {
        //        break;
        //    }
    }

}
