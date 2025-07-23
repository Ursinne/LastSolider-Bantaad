using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    private Vector3 startPos;
    private Vector3 targetPos;
    public float moveSpeed = 100f;
    public float damage = 10f;
    public float lifetime = 3f;

    // Lägg till en arc-rörelse
    public float arcHeight = 1f; // Höjd på bågen

    private float journeyLength;
    private float startTime;

    private void Start()
    {
        startPos = transform.position;
        startTime = Time.time;

        // Debug-logg för startposition
        Debug.Log($"Bullet started at: {startPos}");

        // Beräkna total sträcka
        journeyLength = Vector3.Distance(startPos, targetPos);

        Destroy(gameObject, lifetime);
    }

    public void Setup(Vector3 targetPos)
    {
        this.targetPos = targetPos;

        // Debug-logg för Setup
        Debug.Log($"Bullet Setup - Start Pos: {transform.position}, Target: {targetPos}");

        // Rotera projektilen mot målet
        transform.LookAt(targetPos);
    }

    void Update()
    {
        // Beräkna färdprocent
        float distCovered = (Time.time - startTime) * moveSpeed;
        float fractionOfJourney = distCovered / journeyLength;

        // Skapa en båge
        //Vector3 arc = Vector3.up * arcHeight * Mathf.Sin(fractionOfJourney * Mathf.PI);

        // Linjär interpolation mellan start och mål
        Vector3 newPos = Vector3.Lerp(startPos, targetPos, fractionOfJourney);

        // Lägg till bågen
        //newPos += arc;

        // Uppdatera position
        transform.position = newPos;

        // Rotera mot rörelseriktningen
        transform.LookAt(newPos + (targetPos - startPos).normalized);

        // Förstör om vi nått målet
        if (fractionOfJourney >= 1)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        HitboxPlayer hitbox = other.GetComponent<HitboxPlayer>();
        if (hitbox != null && hitbox.playerHealth != null)
        {
            hitbox.OnRaycastHit(damage);
            Destroy(gameObject);
            return;
        }

        PlayerHealth health = other.GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}