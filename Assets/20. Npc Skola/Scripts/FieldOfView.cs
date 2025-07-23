using UnityEngine;
using System.Collections;

public class FieldOfView : MonoBehaviour
{
    public float viewRadius = 8f; //Synfältets radie
    [Range(0, 360)]
    public float viewAngle = 130f; //Max synfältsvinkel
    public Transform target; //Referens till målet
    public bool canSeePlayer; //Boolean som är true om agenten kan se spelaren
    public LayerMask targetMask;
    public LayerMask obstructionMask; //Layermasks som innehåller spelaren respektive alla hinder som blockerar synfältet
    [HideInInspector]
    public Collider[] objectsInView; //Här samlas colliders som finns inom agentens synfält

    void Start()
    {
        StartCoroutine(FOVtimer()); //Starta Field Of View-funktionen
    }

    private IEnumerator FOVtimer() //Hjälpcoroutine som startar FOV-funktionen med fördröjning
    {
        FieldOfViewCheck(); //Kalla på FOV-funktionen utan fördröjning
        yield return new WaitForSeconds(0.5f); //Timerfunktion
        StartCoroutine(FOVtimer()); //Starta denna coroutine på nytt
    }

    //void FieldOfViewCheck()
    //{
    //    //canSeePlayer = false;
    //    //Skapa en overlapsfär och lagra samtliga colliders som befinner sig inom denna i en array
    //    objectsInView = Physics.OverlapSphere(transform.position, viewRadius, targetMask);

    //    //Kolla om spelaren befinner sig inom sfärens omkrets (teoretiskt synfält).
    //    if (objectsInView.Length != 0)
    //    {
    //        //Räkna ut riktningen till spelaren
    //        Vector3 directionToTarget = (target.position - transform.position).normalized;
    //        if (Vector3.Angle(transform.forward, directionToTarget) < viewAngle / 2)
    //        {
    //            //Räkna ut avståndet till spelaren
    //            float distanceToTarget = Vector3.Distance(transform.position, target.position);
    //            //Skjut en raycast mot spelaren
    //            if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstructionMask))
    //            {
    //                canSeePlayer = true; //Något objekt i obstructionmask blockerar synfältet
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

