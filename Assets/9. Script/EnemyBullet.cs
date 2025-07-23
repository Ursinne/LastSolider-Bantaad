using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    private Vector3 startPos;
    private Vector3 targetPos;
    public float moveSpeed = 100f;
    public float damage = 10f;
    public float lifetime = 3f;

    // L�gg till en arc-r�relse
    public float arcHeight = 1f; // H�jd p� b�gen

    private float journeyLength;
    private float startTime;

    private void Start()
    {
        startPos = transform.position;
        startTime = Time.time;

        // Debug-logg f�r startposition
        Debug.Log($"Bullet started at: {startPos}");

        // Ber�kna total str�cka
        journeyLength = Vector3.Distance(startPos, targetPos);

        Destroy(gameObject, lifetime);
    }

    public void Setup(Vector3 targetPos)
    {
        this.targetPos = targetPos;

        // Debug-logg f�r Setup
        Debug.Log($"Bullet Setup - Start Pos: {transform.position}, Target: {targetPos}");

        // Rotera projektilen mot m�let
        transform.LookAt(targetPos);
    }

    void Update()
    {
        // Ber�kna f�rdprocent
        float distCovered = (Time.time - startTime) * moveSpeed;
        float fractionOfJourney = distCovered / journeyLength;

        // Skapa en b�ge
        //Vector3 arc = Vector3.up * arcHeight * Mathf.Sin(fractionOfJourney * Mathf.PI);

        // Linj�r interpolation mellan start och m�l
        Vector3 newPos = Vector3.Lerp(startPos, targetPos, fractionOfJourney);

        // L�gg till b�gen
        //newPos += arc;

        // Uppdatera position
        transform.position = newPos;

        // Rotera mot r�relseriktningen
        transform.LookAt(newPos + (targetPos - startPos).normalized);

        // F�rst�r om vi n�tt m�let
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