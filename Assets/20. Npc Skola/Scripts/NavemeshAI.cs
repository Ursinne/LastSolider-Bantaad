using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations.Rigging;
using UnityEngine.UI;

public class NavemeshAI : MonoBehaviour
{
    NavMeshAgent agent;                         // Referens för navmesh-komponenten
    public Transform target;                    // Referens till spelaren
    public float pathUpdateTime = 1f;           // Det tidsintervall som avgör hur ofta en path skapas till målet
    float timer = 0f;                           // Timervärde
    Animator animator;                          // Referens till animatorkomponenten
    public Transform[] waypoints;               // Array innehållande våra waypoints
    public int waypointIndex;                   // Nuvarande patrullmål för agenten
    public bool randomWp;                       // Bool som avgör om patrullvägen ska vara slumpmässig eller ej
    private FieldOfView fieldOfView;            // Referens till field of view-scriptet
    public MultiAimConstraint bodyAimRig;       // Referens för ryggradsriggen
    public MultiAimConstraint RhandAimRig;      // Referens för högerhandsriggen
    public bool aimModeActive;                  // Bool som är sann när agenten siktar                                       

    // FSM
    public enum STATE { IDLE, PATROL, CHASE, ATTACK, DEAD } // De states FSM kan befinna sig i
    public STATE state; // Variabel som innehåller valet av state
    public float waypointDistance = 0.5f; // Stopping distancevärde som används i patrolstate
    public float attackDistance = 5f; // Stopping distancevärde som används i attackstate
    public float escapeDistance = 24f; // Det avstånd där agenten "glömmer" spelaren

    // Shoot
    private RaycastHit hitInfo; // variabel som lagrar info om vad vi träffar när vi skjuter
    public Transform bulletPrefab; // Projektil
    public ParticleSystem muzzleFlash; // Mynningsflamma
    public ParticleSystem hitEffect; // Partiklar som alstras när man träffar ett objekt
    public Transform bulletSpawnpoint; // Den punkt där projektilerna skjuts ifrån
    float shootTimer = 0f; // Timervärde för skjutning
    public float shootFreq = 0.15f; // Skjutfrekvens
    public Transform shootTarget; // Den punkt där npc'n egentligen siktar
    //public float weaponamage = 19f; 

    // Ljudhantering för fotsteg
    public AudioClip[] footstepSounds;
    private AudioSource audioSource;
    public float footstepVolume = 0.5f;

    //Annat
    //public PlayerHealth playerHealth;  // Refrens till spelarens healthscript   -------------------

    public Slider healthSlider;
    public Canvas healthBar;
    public EnemyHealth healthScript;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        fieldOfView = GetComponent<FieldOfView>();
        waypointIndex = 0; // Nollställ patrullmålet
        healthBar.enabled = true;
        healthScript = GetComponent<EnemyHealth>();


        // Konfigurera ljudkälla
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1.0f;       // 3D ljud
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.maxDistance = 20f;         // Max avstånd för att höra ljudet
            audioSource.volume = footstepVolume;
        }

        // Konfigurera mynningselden att INTE spela automatiskt
        if (muzzleFlash != null)
        {
            var main = muzzleFlash.main;
            main.loop = false;          // Stäng av looping
            main.playOnAwake = false;   // Spela inte vid start
            muzzleFlash.Stop();         // Försäkra oss om att den är stoppad vid start
        }

        // Kontrollera om waypoints finns innan vi sätter destination
        if (waypoints != null && waypoints.Length > 0)
        {
            agent.SetDestination(waypoints[0].position); // Sätt första mål till första wp
        }

        state = STATE.PATROL; // Sätt utgångsstate till patrol
    }

    void Update()
    {
        Aim(); // Kalla på aimfunktionen

        switch (state) // Finite statemachine som switchar mellan olika states
        {
            case STATE.IDLE:
                Idle();
                break;
            case STATE.PATROL:
                FollowWaypoints();
                break;
            case STATE.CHASE:
                ChasePlayer();
                break;
            case STATE.ATTACK:
                Attack();
                break;
            case STATE.DEAD:
                Dead();
                break;
        }
        healthSlider.value = healthScript.currentHealth / 10;
        if (healthSlider != null && healthScript != null)
        {
            healthSlider.value = healthScript.currentHealth / 10;
        }
    }

    void ChasePlayer()
    {
        aimModeActive = false; // Sluta sikta
        float distanceToPlayer = Vector3.Distance(transform.position,
        target.transform.position); // Räkna ut avstånd mellan agent och spelare

        if (fieldOfView.canSeePlayer) // Kolla om agenten kan se spelaren
        {
            // Sätt stoppingdistance till attackdistance för att kunna skjuta på längre avstånd
            agent.stoppingDistance = attackDistance;
        }
        else
        {
            agent.stoppingDistance = 1.6f; // Sätt stopping till lågt värde för att följa spelaren tätt inpå
        }

        timer -= Time.deltaTime; // Baklängestimer för att optimera pathgeneretion
        if (timer < 0.0f)
        {
            agent.destination = target.position;
            timer = pathUpdateTime; // Resetta timern
        }

        animator.SetFloat("Speed", agent.velocity.magnitude); // Sätt animationshastigheten
        transform.LookAt(target.transform.position); // Rotera npc mot spelaren

        // Gå in i attackstate om npc är nära spelaren och kan se denne
        if ((agent.remainingDistance <= attackDistance) && fieldOfView.canSeePlayer)
        {
            state = STATE.ATTACK;
        }

        if (distanceToPlayer > escapeDistance) // "Glöm" spelaren om npc befinner sig längre bort än esc-distance
        {
            state = STATE.PATROL;
        }
    }

    void FollowWaypoints()
    {
        // Säkerställ att mynningselden är stoppad i detta state
        if (muzzleFlash != null && muzzleFlash.isPlaying)
        {
            muzzleFlash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        // Kontrollera om waypoints finns
        if (waypoints == null || waypoints.Length == 0)
        {
            state = STATE.IDLE;
            return;
        }

        agent.stoppingDistance = waypointDistance;

        if (randomWp)
        {
            if (agent.remainingDistance < agent.stoppingDistance)
            {
                int newIndex = Random.Range(0, waypoints.Length);
                agent.SetDestination(waypoints[newIndex].position);
            }
        }
        else
        {
            if (agent.remainingDistance < agent.stoppingDistance)
            {
                waypointIndex++;
                if (waypointIndex >= waypoints.Length)
                {
                    waypointIndex = 0;
                }
                agent.SetDestination(waypoints[waypointIndex].position);
            }
        }

        animator.SetFloat("Speed", agent.velocity.magnitude);

        if (fieldOfView.canSeePlayer)
        {
            state = STATE.CHASE;
        }
    }

    void Idle()
    {
        animator.SetBool("isShooting", false);   // Växla aniamationen till rifle idle
        aimModeActive = false;                   // sluta sikta
        // Säkerställ att mynningselden är stoppad i detta state
        if (muzzleFlash != null && muzzleFlash.isPlaying)
        {
            muzzleFlash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        // Stå stilla och vänta
        animator.SetFloat("Speed", 0);

        // Om agenten kan se spelaren, börja jaga
        if (fieldOfView.canSeePlayer)
        {
            state = STATE.CHASE;
        }
    }

    void Attack()
    {
        aimModeActive = true; // Slå på siktfunktionen
        transform.LookAt(target.transform.position); // Titta alltid mot spelaren
        animator.SetFloat("Speed", agent.velocity.magnitude); // Uppdatera speedparametern i animatorn

        timer -= Time.deltaTime; // Baklängestimer för pathfinding
        if (timer < 0.0f)
        {
            agent.destination = target.position; // Räkna ut en ny path till spelaren
            timer = pathUpdateTime; // Resetta timern
        }

        // Skjut-logik - Enbart när shootTimer når noll
        shootTimer -= Time.deltaTime;
        if (shootTimer <= 0)
        {
            Shoot(); // Skjut och spela mynningseld enbart här
            shootTimer = shootFreq; // Reset timer med skjutfrekvensen
        }

        // Kolla om spelaren lämnat attackzonen och vi fortfarande kan se denne
        if ((agent.remainingDistance > attackDistance) && fieldOfView.canSeePlayer)
        {
            state = STATE.CHASE; // Sätt i så fall state till chase
        }

        // Om vi inte längre kan se spelaren, återgå till patrullering
        if (!fieldOfView.canSeePlayer)
        {
            state = STATE.PATROL;
        }
    }



    void Shoot()
    {
        // Lägg till dessa debug-loggar
        Debug.Log($"bulletSpawnpoint: {bulletSpawnpoint}");
        Debug.Log($"bulletSpawnpoint position: {bulletSpawnpoint.position}");
        Debug.Log($"Fiende position: {transform.position}");

        if (aimModeActive)
        {
            // Kontrollera att bulletSpawnpoint är giltig
            if (bulletSpawnpoint == null)
            {
                Debug.LogError("KRITISK: Ingen bulletSpawnpoint hittad!");
                return;
            }

            Vector3 targetPosition = target.position + Vector3.up * 1.5f;

            Transform bulletTransform = Instantiate(
                bulletPrefab,
                bulletSpawnpoint.position,  // Använd ALLTID bulletSpawnpoint.position
                bulletSpawnpoint.rotation   // Och bulletSpawnpoint.rotation
            );

            bulletTransform.tag = "EnemyBullet";
            bulletTransform.GetComponent<EnemyBullet>().Setup(targetPosition);

            muzzleFlash.Emit(1);
            animator.SetBool("isShooting", true);
        }
    }

    //Skapa en kula om bulletPrefab är satt
    //if (bulletPrefab != null && bulletSpawnpoint != null)
    //{
    //    Transform bullet = Instantiate(bulletPrefab, bulletSpawnpoint.position, bulletSpawnpoint.rotation);

    //    Lägg till kraft på kulan om den har en Rigidbody
    //    Rigidbody rb = bullet.GetComponent<Rigidbody>();
    //    if (rb != null)
    //    {
    //        rb.AddForce(bulletSpawnpoint.forward * 40f, ForceMode.Impulse);
    //    }

    //    Förstör kulan efter några sekunder om den inte redan förstörts
    //    Destroy(bullet.gameObject, 3f);
    //}

    //Utför raycast för att se om vi träffar något
    //if (Physics.Raycast(bulletSpawnpoint.position, bulletSpawnpoint.forward, out hitInfo, 100f))
    //{
    //    Skapa träffeffekt
    //    if (hitEffect != null)
    //    {
    //        Instantiate(hitEffect, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
    //    }

    //    Skada spelaren eller annat objekt som träffas
    //    if (hitInfo.transform != null && hitInfo.transform.CompareTag("Player"))
    //    {
    //        Skada spelaren
    //         hitInfo.transform.GetComponent<PlayerHealth>().TakeDamage(10);
    //    }
    //}


    void Dead()
    {
        // Implementera dödslogik här
        agent.isStopped = true;
        animator.SetTrigger("Death");
        //ragdollSetup.enabled = false;
        this.enabled = false;
        agent.enabled = false;

        // Inaktivera skript efter en viss tid
        // Destroy(this, 3f);
    }

    void Aim()
    {
        if (aimModeActive) // Agenten ska skjuta
        {
            animator.SetLayerWeight(1, Mathf.Lerp(animator.GetLayerWeight(1), 1f,
            Time.deltaTime * 10f)); // Blenda in skjutanimationslagret 100%

            //if (RhandAimRig != null)
            RhandAimRig.weight = 1f; // Aktivera riggarna

            //  if (bodyAimRig != null)
            bodyAimRig.weight = 1f;
            if (Physics.Raycast(bulletSpawnpoint.transform.position, bulletSpawnpoint.transform.forward, out hitInfo, 1000f))
            {
                //Debug.DrawRay(bulletSpawnpoint.transform.position, bulletSpawnpoint.transform.forward *
                //hitInfo.distance);
            }
            else
            {
                animator.SetLayerWeight(1, Mathf.Lerp(animator.GetLayerWeight(1), 0f,
                Time.deltaTime * 10f)); // Blenda ut skjutanimationslagret

                //if (RhandAimRig != null)
                RhandAimRig.weight = 0f; // Inaktivera riggarna

                // if (bodyAimRig != null)
                bodyAimRig.weight = 0f;
            }
        }
    }

    // Metod som anropas av animationshändelsen 'Footstep'
    public void Footstep()
    {
        if (footstepSounds != null && footstepSounds.Length > 0)
        {
            int index = Random.Range(0, footstepSounds.Length);
            audioSource.PlayOneShot(footstepSounds[index], footstepVolume);
        }
    }
}

//using UnityEngine;
//using UnityEngine.AI;
//using UnityEngine.Animations.Rigging;
//using UnityEngine.UI;


//public class NavemeshAI : MonoBehaviour
//{
//    NavMeshAgent agent; //Referens för navmesh-komponenten
//    public Transform target; //Referens till spelaren
//    public float pathUpdateTime = 1f; //Det tidsintervall som avgör hur ofta en path skapas till målet
//    float timer = 0f; //Timervärde
//    Animator animator; //Referens till animatorkomponenten
//    public Transform[] waypoints; // Array innehållande våra waypoints
//    int waypointIndex; //Nuvarande patrullmål för agenten
//    public bool randomWp; //Bool som avgör om patrullvägen ska vara slumpmässig eller ej
//    private FieldOfView fieldOfView; //Referens till field of view-scriptet
//    public MultiAimConstraint bodyAimRig; //Referens för ryggradsriggen
//    public MultiAimConstraint RhandAimRig; //Referens för högerhandsriggen
//    public bool aimModeActive; //Bool som är sann när agenten siktar
//    public PlayerHealth playerHealth; //Referens till spelarens healthscript
//    public EnemyHealth enemyHealth; //Referens till det egna healthscriptet
//    public RagdollSetup ragdollSetup; //Referens till ragdollscriptet
//    public Slider healthSlider; //Referens till healthbar
//    public Canvas healthBar; //Referens till healthbarens canvasobjekt


//    //FSM
//    public enum STATE { IDLE, PATROL, CHASE, ATTACK, DEAD } //De states FSM kan befinna sig i
//    public STATE state; //Variabel som innehåller valet av state
//    public float waypointDistance = 0.5f; //Stopping distancevärde som används i patrolstate
//    public float attackDistance = 5f; //Stopping distancevärde som används i attackstate
//    public float escapeDistance = 24f; //Det avstånd där agenten "glömmer" spelaren

//    //Shoot
//    private RaycastHit hitInfo; //variabel som lagrar info om vad vi träffar när vi skjuter
//    public Transform bulletPrefab; //Projektil
//    public ParticleSystem muzzleFlash; //Mynningsflamma
//    public ParticleSystem hitEffect; //Partiklar som alstras när man träffar ett objekt
//    public Transform bulletSpawnpoint; //Den punkt där projektilerna skjuts ifrån
//    float shootTimer = 0.5f; //Timervärde
//    float shootFreq = 0.15f; //Skjutfrekvens
//    public Transform shootTarget; //Den punkt där npc'n egentligen siktar
//    public float weaponDamage = 10f; //Den skada en träff ger
//    public float shootInaccuracy = 50f; //Variabel som inför slumpen i träffsäkerheten
//    public Vector3 shootOffset; //Roterar shootspawn med hjälp av ovanstående värde


//    void Start()
//    {
//        agent = GetComponent<NavMeshAgent>();
//        animator = GetComponent<Animator>();
//        fieldOfView = GetComponent<FieldOfView>();
//        waypointIndex = 0; //Nollställ patrullmålet
//        playerHealth = target.gameObject.GetComponent<PlayerHealth>(); //Sök reda på spelarens healthscript
//        enemyHealth = GetComponent<EnemyHealth>(); //Hämta fiendens healthscript
//        ragdollSetup = GetComponent<RagdollSetup>();

//        if (randomWp)
//        {
//            agent.SetDestination(waypoints[Random.Range(0, waypoints.Length)].transform.position); //Sätt första mål random wp 
//        }
//        else
//        {
//            agent.SetDestination(waypoints[0].transform.position); //Sätt första mål till första wp
//        }
//        state = STATE.PATROL; //Sätt utgångsstate till patrol

//    }


//    void Update()
//    {
//        Aim(); //Kalla på aimfunktionen

//        switch (state) //Finite statemachine som switchar mellan olika states
//        {
//            case STATE.IDLE:
//                Idle();
//                break;
//            case STATE.PATROL:
//                FollowWaypoints();
//                break;
//            case STATE.CHASE:
//                ChasePlayer();
//                break;
//            case STATE.ATTACK:
//                Attack();
//                break;
//            case STATE.DEAD:
//                Dead();
//                break;
//        }

//        healthSlider.value = enemyHealth.currentHealth / 10;
//    }


//    void ChasePlayer()
//    {
//        aimModeActive = false; //Sluta sikta
//        float distanceToPlayer = Vector3.Distance(transform.position,
//        target.transform.position); //Räkna ut avstånd mellan agent och spelare
//        if (fieldOfView.canSeePlayer) //Kolla om agenten kan se spelaren
//        {
//            //Sätt stoppingdistance till attackdistance för att kunna skjuta på längre avstånd
//            agent.stoppingDistance = attackDistance;
//        }
//        else
//            agent.stoppingDistance = 1.6f; //Sätt stopping till lågt värde för att följa spelaren tätt inpå

//        timer -= Time.deltaTime; //Baklängestimer för att optimera pathgeneretion
//        if (timer < 0.0f)
//        {
//            agent.destination = target.position; //Följ spelaren
//            timer = pathUpdateTime; //Resetta timern
//        }
//        animator.SetFloat("Speed", agent.velocity.magnitude); //Sätt animationshastigheten
//        transform.LookAt(target.transform.position); //Rotera npc mot spelaren

//        //Gå in i attackstate om npc är nära spelaren och kan se denne
//        if ((agent.remainingDistance <= attackDistance) && fieldOfView.canSeePlayer)
//        {
//            state = STATE.ATTACK;
//        }
//        if (distanceToPlayer > escapeDistance) //"Glöm" spelaren om npc befinner sig längre bort än esc-distance
//        {
//            state = STATE.PATROL; //Återgå till patrolstate
//        }

//        if (enemyHealth.isDead) //Kolla om agenten är död
//        {
//            Dead(); //Byt state till dead
//        }
//    }

//    void FollowWaypoints()
//    {
//        agent.stoppingDistance = waypointDistance;//Ställ in stopping distance till ett lågt värde

//        if (randomWp) //Bestämmer om patrullvägen ska vara slumpmässig
//        {
//            if (agent.remainingDistance < agent.stoppingDistance)
//            {
//                agent.SetDestination(waypoints[Random.Range(0,
//                    waypoints.Length)].transform.position); //Slumpa en waypoint som patrullmål
//            }
//        }
//        else
//        {
//            if (agent.remainingDistance < agent.stoppingDistance) //Kolla om agenten befinner sig nära aktuell wp
//            {
//                waypointIndex++; //Öka is så fall index med 1
//                if (waypointIndex >= waypoints.Length)
//                    waypointIndex = 0; //Resetta index för att skapa en loopande patrullering
//            }
//            agent.SetDestination(waypoints[waypointIndex].transform.position); //Sätt agentens mål till aktuell wp
//        }
//        animator.SetFloat("Speed", agent.velocity.magnitude); //Sätt agentens animationshastighet

//        if (fieldOfView.canSeePlayer) //Kolla om agenten kan se spelaren
//        {
//            state = STATE.CHASE; //Gå in i chase-state
//        }

//        if (enemyHealth.isDead) //Kolla om agenten är död
//        {
//            Dead(); //Byt state till dead
//        }
//    }

//    void Idle()
//    {
//        animator.SetBool("isShooting", false); //Växla animation till rifle idle
//        aimModeActive = false; //Sluta sikta på spelaren

//        if (enemyHealth.isDead) //Kolla om agenten är död
//        {
//            Dead(); //Byt state till dead
//        }
//    }

//    void Attack()
//    {
//        shootTimer -= Time.deltaTime; //Baklängestimer
//        if (shootTimer <= 0f)
//        {
//            Shoot(); //Skjut
//            shootTimer = shootFreq; //Resetta timern
//        }

//        aimModeActive = true; //Slå på siktfunktionen
//        transform.LookAt(target.transform.position); //Titta alltid mot spelaren
//        animator.SetFloat("Speed", agent.velocity.magnitude); //Uppdatera speedparametern i animatorn
//        timer -= Time.deltaTime; //Baklängestimer
//        if (timer < 0.0f)
//        {
//            agent.destination = target.position; //Räkna ut en ny path till spelaren
//            timer = pathUpdateTime; //Resetta timern
//        }
//        //Kolla om spelaren lämnat attackzonen och vi fortfarande kan se denne
//        if ((agent.remainingDistance > attackDistance) && fieldOfView.canSeePlayer)
//        {
//            state = STATE.CHASE; //Sätt i så fall state till chase
//        }

//        if (playerHealth.isDead) //Kolla om spelaren är död
//        {
//            state = STATE.IDLE; //Byt state till idle
//        }

//        if (enemyHealth.isDead) //Kolla om agenten är död
//        {
//            Dead(); //Byt state till dead
//        }
//    }

//    void Dead()
//    {
//        animator.enabled = false; //Slå av animatorn
//        ragdollSetup.ActivateRagdoll(); //Aktivera ragdoll
//        this.enabled = false; //Slå av detta script
//        agent.enabled = false; //Slå av navmesh agent-komponenten för att omöjliggöra förflyttning
//        fieldOfView.enabled = false;
//        healthBar.enabled = false; //Slå av healthbar
//    }

//    void Aim()
//    {
//        if (aimModeActive) //Agenten ska skjuta
//        {
//            animator.SetLayerWeight(1, Mathf.Lerp(animator.GetLayerWeight(1), 1f,
//            Time.deltaTime * 10f)); //Blenda in skjutanimationslagret 100%
//            RhandAimRig.weight = 1f; //Aktivera riggarna
//            bodyAimRig.weight = 1f;
//            Vector3 shootRand = Random.insideUnitSphere * shootInaccuracy; //Slumpa skottets träffpunkt
//            //Konvertera localspace till worldspace för att undvika problem vid raycasting
//            Vector3 shootDir = transform.TransformPoint(shootOffset + shootRand);

//            Ray ray = new Ray(bulletSpawnpoint.transform.position, shootDir * 1000f); //Ray som används vid raycast
//            //Skjut en ray mot målet och lagra info om träffat objekt i hitInfo
//            if (Physics.Raycast(ray, out hitInfo))
//            {
//                //Rita ut kulans väg mot målet
//                //Debug.DrawRay(bulletSpawnpoint.transform.position, shootDir * 1000f);
//            }
//        }
//        else
//        {
//            animator.SetLayerWeight(1, Mathf.Lerp(animator.GetLayerWeight(1), 0f,
//            Time.deltaTime * 10f)); //Blenda ut skjutanimationslagret
//            RhandAimRig.weight = 0f; //Inaktivera riggarna
//            bodyAimRig.weight = 0f;
//        }
//    }

//    void Shoot()
//    {
//        if (aimModeActive) //Kolla om aim-mode är aktivt och agenten kan se spelaren
//        {
//            //Instansiera ett skott
//            Transform bulletTransform = Instantiate(bulletPrefab, bulletSpawnpoint.position, Quaternion.identity);
//            bulletTransform.GetComponent<EnemyBullet>().Setup(shootTarget.position); //Skjut projektilen mot målet
//            if (hitInfo.collider != null && hitInfo.collider.tag != "NotShootable")
//            {
//                hitEffect.transform.position = hitInfo.point; //Placera hiteffekten där skottet träffar
//                hitEffect.transform.forward = hitInfo.normal; //Rotera hiteffekten i samma riktining som den träffade ytans normal
//                hitEffect.Emit(1); //Starta hiteffekten 
//            }
//            muzzleFlash.Play(); //Starta muzzleflash-effekt
//            animator.SetBool("isShooting", true); //Starta shoot-animationen

//            if (hitInfo.transform != null && hitInfo.transform.GetComponent<HitboxPlayer>()) //Kolla om det träffade objektet har ett hitboxscript
//            {
//                hitInfo.transform.GetComponent<HitboxPlayer>().OnRaycastHit(weaponDamage); //Dra skadevärdet från health                                                                     
//            }
//        }
//        else
//        {
//            animator.SetBool("isShooting", false); //Stoppa shoot-animationen
//            muzzleFlash.Stop(); //Stäng av muzzleflash
//            hitEffect.Emit(0); //Stäng av hiteffect
//        }
//    }
//}
