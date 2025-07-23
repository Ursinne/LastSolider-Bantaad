using UnityEngine;

public class RayCast : MonoBehaviour
{

    public Transform spawnPoint;
    public float force = 5f;      // Den lraft vi skickar till det träffade objektets rigibody
    public float rayDistance = 100f;
    RaycastHit hitInfo; // Variable som innehåller information om vad raycasten träffar

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void CastRay()
    {
     
        //Visa vart vi skjuter med hjälp av en färgad linje
        Debug.DrawRay(spawnPoint.transform.position, spawnPoint.forward * rayDistance, Color.red);
        // Skjut en ray i spawnpointens framriktning och spara resultatet i hitinfo
        if (Physics.Raycast(spawnPoint.transform.position,spawnPoint.forward, out hitInfo, rayDistance));
        {
            if(hitInfo.rigidbody != null) // kolla om ridibodyn finns
            // Lägg i så fall på en kraft i den punkt där raycasten träffar
            hitInfo.rigidbody.AddForceAtPosition(spawnPoint.forward * force,hitInfo.point);
        }
    }

    void jump()
    {

    }
}
