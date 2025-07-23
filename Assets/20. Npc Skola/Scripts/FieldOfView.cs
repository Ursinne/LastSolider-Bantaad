using UnityEngine;
using System.Collections;

public class FieldOfView : MonoBehaviour
{
    public float viewRadius = 8f; //Synf�ltets radie
    [Range(0, 360)]
    public float viewAngle = 130f; //Max synf�ltsvinkel
    public Transform target; //Referens till m�let
    public bool canSeePlayer; //Boolean som �r true om agenten kan se spelaren
    public LayerMask targetMask;
    public LayerMask obstructionMask; //Layermasks som inneh�ller spelaren respektive alla hinder som blockerar synf�ltet
    [HideInInspector]
    public Collider[] objectsInView; //H�r samlas colliders som finns inom agentens synf�lt

    void Start()
    {
        StartCoroutine(FOVtimer()); //Starta Field Of View-funktionen
    }

    private IEnumerator FOVtimer() //Hj�lpcoroutine som startar FOV-funktionen med f�rdr�jning
    {
        FieldOfViewCheck(); //Kalla p� FOV-funktionen utan f�rdr�jning
        yield return new WaitForSeconds(0.5f); //Timerfunktion
        StartCoroutine(FOVtimer()); //Starta denna coroutine p� nytt
    }

    //void FieldOfViewCheck()
    //{
    //    //canSeePlayer = false;
    //    //Skapa en overlapsf�r och lagra samtliga colliders som befinner sig inom denna i en array
    //    objectsInView = Physics.OverlapSphere(transform.position, viewRadius, targetMask);

    //    //Kolla om spelaren befinner sig inom sf�rens omkrets (teoretiskt synf�lt).
    //    if (objectsInView.Length != 0)
    //    {
    //        //R�kna ut riktningen till spelaren
    //        Vector3 directionToTarget = (target.position - transform.position).normalized;
    //        if (Vector3.Angle(transform.forward, directionToTarget) < viewAngle / 2)
    //        {
    //            //R�kna ut avst�ndet till spelaren
    //            float distanceToTarget = Vector3.Distance(transform.position, target.position);
    //            //Skjut en raycast mot spelaren
    //            if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstructionMask))
    //            {
    //                canSeePlayer = true; //N�got objekt i obstructionmask blockerar synf�ltet
    //            }
    //        }
    //    }
    //    else
    //    {
    //        canSeePlayer = false;
    //    }
    //}

    private void FieldOfViewCheck()
    {
        objectsInView = Physics.OverlapSphere(transform.position, viewRadius, targetMask);
        if (objectsInView.Length != 0)
        {
            Vector3 directionToTarget = (target.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, directionToTarget) < viewAngle / 2)
            {
                float distanceToTarget = Vector3.Distance(transform.position,
                target.position);
                if (Physics.Raycast(transform.position, directionToTarget, distanceToTarget,
                obstructionMask))
                    canSeePlayer = false;
                else
                    canSeePlayer = true;
            }
            else
                canSeePlayer = false;
        }
        else if (canSeePlayer)
            canSeePlayer = false;
    }
}

