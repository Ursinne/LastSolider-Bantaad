using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorChanger : MonoBehaviour // denna fil kan ligga p� en fiende eller vilket objekt som helst
{
    public GameObject[] cubes; // Refereans till kuberna, [] kallas array
    public int index = 1;   

    void Start()
    {
        //Rigibody = GetComponent<Rigidbody>().// efter punkt kommer man �t en massa inst�llningar  .Addforce();f�r att skjuta n�t tex
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) // h�r g�r vi f�r att byta f�rg p� object, men alla i samma f�rg.
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
        //        Debug.Log("test med f�rger"); 
        //    }
        //    else
        //    {
        //        break;
        //    }
    }

}
