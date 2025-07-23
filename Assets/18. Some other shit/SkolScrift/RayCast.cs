using UnityEngine;

public class RayCast : MonoBehaviour
{

    public Transform spawnPoint;
    public float force = 5f;      // Den lraft vi skickar till det tr�ffade objektets rigibody
    public float rayDistance = 100f;
    RaycastHit hitInfo; // Variable som inneh�ller information om vad raycasten tr�ffar

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void CastRay()
    {
     
        //Visa vart vi skjuter med hj�lp av en f�rgad linje
        Debug.DrawRay(spawnPoint.transform.position, spawnPoint.forward * rayDistance, Color.red);
        // Skjut en ray i spawnpointens framriktning och spara resultatet i hitinfo
        if (Physics.Raycast(spawnPoint.transform.position,spawnPoint.forward, out hitInfo, rayDistance));
        {
            if(hitInfo.rigidbody != null) // kolla om ridibodyn finns
            // L�gg i s� fall p� en kraft i den punkt d�r raycasten tr�ffar
            hitInfo.rigidbody.AddForceAtPosition(spawnPoint.forward * force,hitInfo.point);
        }
    }

    void jump()
    {

    }
}
