using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float speed = 2f;       // Kulans hastighet
    public float damage = 10f;      // Skada kulan orsakar
    public float lifetime = 200f;     // Hur länge kulan finns kvar
    public float maxDistance = 200f; // Maximal distans kulan kan färdas

    [Header("Physics")]
    public Rigidbody rb;
    public LayerMask hitLayers;     // Lager som kulan kan träffa

    [Header("Effects")]
    public GameObject hitEffect;     // Effekt som visas vid träff
    public AudioClip hitSound;      // Ljud som spelas vid träff

    private Vector3 startPosition;   // Startposition för att beräkna distans

    private void Awake()
    {
        // Säkerställ att Rigidbody finns
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        startPosition = transform.position;
    }

    private void Start()
    {
        // Förstör kulan efter en viss tid oavsett vad som händer
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        // Kontrollera om kulan har färdats maximalt avstånd
        if (Vector3.Distance(startPosition, transform.position) > maxDistance)
        {
            Destroy(gameObject);
        }
    }

    // Använd physics för att sätta hastighet istället för att ändra position direkt
    public void SetupVelocity(Vector3 direction)
    {
        if (rb != null)
        {
            rb.linearVelocity = direction * speed;
        }
    }

    // Du kan också ha en metod som tar en exakt målpunkt
    public void SetupTargetPoint(Vector3 targetPoint)
    {
        if (rb != null)
        {
            Vector3 direction = (targetPoint - transform.position).normalized;
            rb.linearVelocity = direction * speed;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandleCollision(collision.gameObject, collision.contacts[0].point, collision.contacts[0].normal);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Om vi använder triggers istället för colliders
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, 0.5f))
        {
            HandleCollision(other.gameObject, hit.point, hit.normal);
        }
        else
        {
            HandleCollision(other.gameObject, transform.position, -transform.forward);
        }
    }

    private void HandleCollision(GameObject hitObject, Vector3 hitPoint, Vector3 hitNormal)
    {
        // Skada fiender om vi träffar dem
        Enemy enemy = hitObject.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }

        // Kolla även efter andra träffbara objekt
        HitboxEnemy hitbox = hitObject.GetComponent<HitboxEnemy>();              // ------------------------------
        if (hitbox != null)
        {
            hitbox.OnRaycastHit(damage);
        }

        // Skapa träffeffekt om vi har en
        if (hitEffect != null)
        {
            GameObject effect = Instantiate(hitEffect, hitPoint, Quaternion.LookRotation(hitNormal));
            Destroy(effect, 2f); // Ta bort effekten efter 2 sekunder
        }

        // Spela ljud om vi har ett
        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, hitPoint);
        }

        // Ta bort kulan när den träffar något
        Destroy(gameObject);
    }
}
